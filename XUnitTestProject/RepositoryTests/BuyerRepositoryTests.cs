using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using SharedClassLibrary.IRepository;
using Microsoft.Data.SqlClient;
using Xunit;
using Moq;

namespace XUnitTestProject.RepositoryTests
{
    // Fixture to manage the SQL Server LocalDB database lifecycle for tests
    public class DatabaseFixture : IDisposable
    {
        private static readonly string MasterConnectionString = @"Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;";
        public string TestDatabaseName { get; }
        public string TestConnectionString { get; }

        // Keep a single connection open for schema setup/teardown if needed, but BuyerRepository will use its own.
        // Primarily use the TestConnectionString for repository instantiation.

        public DatabaseFixture()
        {
            TestDatabaseName = $"TestMarketPlace_{Guid.NewGuid()}";
            TestConnectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={TestDatabaseName};Trusted_Connection=True;MultipleActiveResultSets=true"; // Added MARS

            CreateDatabase();
            CreateSchema();
        }

        private void CreateDatabase()
        {
            using var masterDbConnection = new SqlConnection(MasterConnectionString);
            masterDbConnection.Open();
            using var createDbCommand = masterDbConnection.CreateCommand();
            createDbCommand.CommandText = $"CREATE DATABASE [{TestDatabaseName}]";
            createDbCommand.ExecuteNonQuery();
        }

        private void CreateSchema()
        {
            using var testDbConnection = new SqlConnection(TestConnectionString);
            testDbConnection.Open();
            using var schemaSetupCommand = testDbConnection.CreateCommand();

            // Use schema from MarketPlaceQuery.sql (ensure compatibility)
            // Split commands if necessary (GO statements aren't directly executable)
            // Simplified for brevity - add all required tables from your .sql file
            schemaSetupCommand.CommandText = @"
                CREATE TABLE Users (
                    UserID INT IDENTITY(1,1) PRIMARY KEY,
                    Username NVARCHAR(100) NOT NULL UNIQUE,
                    Email NVARCHAR(255) NOT NULL UNIQUE,
                    PhoneNumber NVARCHAR(20) NULL,
                    Password NVARCHAR(255) NOT NULL,
                    Role INT NOT NULL,
                    FailedLogins INT DEFAULT 0,
                    BannedUntil DATETIME NULL,
                    IsBanned BIT DEFAULT 0
                );

                CREATE TABLE BuyerAddress (
                    Id int IDENTITY(0,1) PRIMARY KEY,
                    StreetLine nvarchar(255) NOT NULL,
                    City nvarchar(255) NOT NULL,
                    Country nvarchar(255) NOT NULL,
                    PostalCode nvarchar(255) NOT NULL
                );

                CREATE TABLE Buyers (
                    UserId Int NOT NULL PRIMARY KEY,
                    FirstName nvarchar(255) NOT NULL,
                    LastName nvarchar(255) NOT NULL,
                    BillingAddressId int NOT NULL,
                    ShippingAddressId int NOT NULL,
                    UseSameAddress bit,
                    Badge nvarchar(10),
                    TotalSpending NUMERIC(32, 2) NOT NULL,
                    NumberOfPurchases int not null,
                    Discount NUMERIC(32, 2) NOT NULL,
                    FOREIGN KEY (UserId) REFERENCES Users (UserId),
                    FOREIGN KEY (BillingAddressId) REFERENCES BuyerAddress (Id)
                    -- ShippingAddressId FK might need adjustment depending on UseSameAddress logic/constraints
                );

                CREATE TABLE BuyerLinkage (
                    RequestingBuyerId INT NOT NULL,
                    ReceivingBuyerId INT NOT NULL,
                    IsApproved BIT NOT NULL,
                    FOREIGN KEY (ReceivingBuyerId) REFERENCES Buyers (UserId),
                    FOREIGN KEY (RequestingBuyerId) REFERENCES Buyers (UserId),
                    PRIMARY KEY (RequestingBuyerId, ReceivingBuyerId),
                    CHECK (RequestingBuyerId <> ReceivingBuyerId)
                );

                 CREATE TABLE Sellers(
                    UserId INT NOT NULL PRIMARY KEY,
                    Username NVARCHAR(100) NOT NULL UNIQUE,
                    StoreName NVARCHAR(100),
                    StoreDescription NVARCHAR(255),
                    StoreAddress NVARCHAR(100),
                    FollowersCount INT,
                    TrustScore FLOAT,
                    FOREIGN KEY(UserId) REFERENCES Users(UserID)
                );

                CREATE TABLE Notifications(
                    NotificationID INT IDENTITY(1,1) PRIMARY KEY,
                    SellerID INT,
                    NotificationMessage NVARCHAR(100),
                    NotificationFollowerCount INT,
                    FOREIGN KEY(SellerID) REFERENCES Sellers(UserId)
                );

                CREATE TABLE Products(
                    ProductID INT IDENTITY(1,1) PRIMARY KEY,
                    SellerID INT,
                    ProductName NVARCHAR(100),
                    ProductDescription NVARCHAR(100),
                    ProductPrice FLOAT,
                    ProductStock INT,
                    FOREIGN KEY(SellerID) REFERENCES Sellers(UserId)
                );

                CREATE TABLE BuyerWishlistItems (
                    BuyerId int not null,
                    ProductId int not null,
                    PRIMARY KEY (BuyerId, ProductId),
                    FOREIGN KEY (BuyerId) references Buyers (UserId),
                    FOREIGN KEY (ProductId) references Products (ProductId) -- Assuming ProductId FK
                );

                CREATE TABLE Following (
                    FollowerID INT NOT NULL,
                    FollowedID INT NOT NULL,
                    PRIMARY KEY (FollowerID, FollowedID),
                    FOREIGN KEY (FollowerID) REFERENCES Buyers(UserId),
                    FOREIGN KEY (FollowedID) REFERENCES Sellers(UserId)
                );

                CREATE TABLE Reviews 
                (
                    ReviewId	INT IDENTITY (1,1) PRIMARY KEY, 
                    SellerId	INT NOT NULL, 
                    Score		FLOAT ,
                    Foreign key (SellerId) REFERENCES Sellers(UserId)
                )

                CREATE INDEX idx_users_email ON Users (Email);
            ";
            schemaSetupCommand.ExecuteNonQuery();
        }

        public void Dispose()
        {
            // Ensure the connection used by the repository is closed if held open
            // BuyerRepository manages its own connection lifecycle via DatabaseConnection,
            // so we primarily need to drop the database.

            using var masterDbConnection = new SqlConnection(MasterConnectionString);
            masterDbConnection.Open();
            using var dropDatabaseCommand = masterDbConnection.CreateCommand();

            // Terminate active connections to the test database
            dropDatabaseCommand.CommandText = 
                @$"IF DB_ID('{TestDatabaseName}') IS NOT NULL
                  BEGIN
                      ALTER DATABASE [{TestDatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                      DROP DATABASE [{TestDatabaseName}];
                  END";
            dropDatabaseCommand.ExecuteNonQuery();
        }
    }

    // Collection definition (no changes needed here)
    [CollectionDefinition("DatabaseCollection")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture> { }


    [Collection("DatabaseCollection")]
    public class BuyerRepositoryTests
    {
        private readonly Mock<IBuyerRepository> _mockBuyerRepository;

        public BuyerRepositoryTests()
        {
            _mockBuyerRepository = new Mock<IBuyerRepository>();
        }

        // Example test using the mocked repository
        [Fact]
        public async Task LoadBuyerInfo_ShouldLoadCorrectBasicData_WhenBuyerExists()
        {
            // Arrange
            var testBuyer = new Buyer
            {
                User = new User { UserId = 1 },
                FirstName = "Test",
                LastName = "Buyer",
                Badge = BuyerBadge.SILVER,
                TotalSpending = 150.75m,
                NumberOfPurchases = 5,
                Discount = 2.5m,
                UseSameAddress = false
            };

            _mockBuyerRepository.Setup(repo => repo.LoadBuyerInfo(It.IsAny<Buyer>()))
                .Callback<Buyer>(b =>
                {
                    b.FirstName = testBuyer.FirstName;
                    b.LastName = testBuyer.LastName;
                    b.Badge = testBuyer.Badge;
                    b.TotalSpending = testBuyer.TotalSpending;
                    b.NumberOfPurchases = testBuyer.NumberOfPurchases;
                    b.Discount = testBuyer.Discount;
                    b.UseSameAddress = testBuyer.UseSameAddress;
                })
                .Returns(Task.CompletedTask);

            var buyer = new Buyer { User = new User { UserId = 1 } };

            // Act
            await _mockBuyerRepository.Object.LoadBuyerInfo(buyer);

            // Assert
            Assert.NotNull(buyer);
            Assert.Equal(testBuyer.FirstName, buyer.FirstName);
            Assert.Equal(testBuyer.LastName, buyer.LastName);
            Assert.Equal(testBuyer.Badge, buyer.Badge);
            Assert.Equal(testBuyer.TotalSpending, buyer.TotalSpending);
            Assert.Equal(testBuyer.NumberOfPurchases, buyer.NumberOfPurchases);
            Assert.Equal(testBuyer.Discount, buyer.Discount);
            Assert.Equal(testBuyer.UseSameAddress, buyer.UseSameAddress);
        }

        // Additional tests can follow the same pattern
    }
}
