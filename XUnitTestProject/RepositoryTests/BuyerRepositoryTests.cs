using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketPlace924.DBConnection;
using SharedClassLibrary.Domain;
using SharedClassLibrary.IRepository;
using Microsoft.Data.SqlClient;
using Xunit;

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
        private readonly DatabaseFixture _fixture;
        private const int lowIdLimit = 0;
        private static int _uniqueIdCounter = 0;
        private static readonly object _lock = new object();

        public BuyerRepositoryTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        public string GenerateUniqueId()
        {
            lock (_lock)
            {
                _uniqueIdCounter++;
                return $"{DateTime.Now.Ticks}_{_uniqueIdCounter}";
            }
        }

        // Helper to create repository instance with the test database connection string
        private BuyerRepository CreateRepository()
        {
            // Use the constructor of DatabaseConnection that takes a connection string
            var dbConnection = new DatabaseConnection(_fixture.TestConnectionString);
            return new BuyerRepository(dbConnection);
        }

        // Helper method to execute seeding SQL and optionally return the generated ID
        private async Task<int> SeedDataAsync(string sqlQuery, object? parameters = null, bool returnId = false)
        {
            try
            {
                using var connection = new SqlConnection(_fixture.TestConnectionString);
                await connection.OpenAsync();
                
                using var command = connection.CreateCommand();

                if (returnId)
                {
                    // Execute the insert and get the ID in a single command
                    command.CommandText = sqlQuery + "; SELECT CAST(SCOPE_IDENTITY() AS INT)";
                }
                else
                {
                    command.CommandText = sqlQuery;
                }

                if (parameters != null)
                {
                    foreach (var prop in parameters.GetType().GetProperties())
                    {
                        command.Parameters.AddWithValue("@" + prop.Name, prop.GetValue(parameters));
                    }
                }

                Console.WriteLine($"Executing SQL: {command.CommandText}");
                if (parameters != null)
                {
                    Console.WriteLine("Parameters:");
                    foreach (SqlParameter param in command.Parameters)
                    {
                        Console.WriteLine($"  {param.ParameterName}: {param.Value}");
                    }
                }

                if (returnId)
                {
                    // Execute the command and get the ID
                    var queryResult = await command.ExecuteScalarAsync();
                    int invalidId = 0;
                    var resultingId = queryResult == DBNull.Value ? invalidId : Convert.ToInt32(queryResult);
                    
                    Console.WriteLine($"Generated ID: {resultingId}");
                    
                    return resultingId;
                }
                else
                {
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    Console.WriteLine($"Rows affected: {rowsAffected}");
                    return 0;
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error: {ex.Message}");
                Console.WriteLine($"Error Number: {ex.Number}");
                Console.WriteLine($"Line Number: {ex.LineNumber}");
                Console.WriteLine($"Procedure: {ex.Procedure}");
                Console.WriteLine($"Server: {ex.Server}");
                Console.WriteLine($"State: {ex.State}");
                throw;
            }
        }

        // --- Test Methods Start Here ---

        // Class to hold test data for buyer tests
        private class BuyerTestData
        {
            public int UserId { get; set; }
            public int BillingAddressId { get; set; }
            public int ShippingAddressId { get; set; }
            public required string ExpectedFirstName { get; set; }
            public required string ExpectedLastName { get; set; }
            public BuyerBadge ExpectedBadge { get; set; }
            public decimal ExpectedTotalSpending { get; set; }
            public int ExpectedNumberOfPurchases { get; set; }
            public decimal ExpectedDiscount { get; set; }
            public bool ExpectedUseSameAddress { get; set; }
            public required string ExpectedBillingStreetWithId { get; set; }
            public required string ExpectedShippingStreetWithId { get; set; }
        }

        // Helper method to set up test data for buyer with different addresses
        private async Task<BuyerTestData> SetupTestBuyerWithDifferentAddresses()
        {
            const string ExpectedFirstName = "Test";
            const string ExpectedLastName = "Buyer";
            const string ExpectedBillingStreet = "123 Test St";
            const string ExpectedShippingStreet = "456 Main Ave";
            const string ExpectedCity = "Testville";
            const string ExpectedCountry = "Testland";
            const string ExpectedBillingCode = "12345";
            const string ExpectedShippingCode = "67890";
            const BuyerBadge ExpectedBadge = BuyerBadge.SILVER;
            const decimal ExpectedTotalSpending = 150.75m;
            const int ExpectedNumberOfPurchases = 5;
            const decimal ExpectedDiscount = 2.5m;
            const bool ExpectedUseSameAddress = false;
            const int LowIdLimit = 0;
            const string TestPassword = "hashedpass";
            const UserRole TestRole = UserRole.Buyer;

            var uniqueId = GenerateUniqueId();
            var expectedUsername = $"testbuyer_{uniqueId}";
            var expectedEmail = $"buyer_{uniqueId}@test.com";
            var expectedBillingStreetWithId = $"{ExpectedBillingStreet} {uniqueId}";
            var expectedShippingStreetWithId = $"{ExpectedShippingStreet} {uniqueId}";

            // 1. Seed User with unique values
            var userId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = expectedUsername, 
                    Email = expectedEmail, 
                    Password = TestPassword, 
                    Role = TestRole 
                },
                returnId: true);
            Console.WriteLine($"User ID: {userId}");
            Assert.NotEqual(LowIdLimit, userId);

            // 2. Seed Addresses
            var billingAddressId = await SeedDataAsync(
                "INSERT INTO BuyerAddress (StreetLine, City, Country, PostalCode) VALUES (@Street, @City, @Country, @Code)",
                new { 
                    Street = expectedBillingStreetWithId, 
                    City = ExpectedCity, 
                    Country = ExpectedCountry, 
                    Code = ExpectedBillingCode 
                },
                returnId: true);
            Console.WriteLine($"Billing Address ID: {billingAddressId}");
            Assert.True(billingAddressId >= LowIdLimit, $"Billing address ID should be >= {LowIdLimit}");

            var shippingAddressId = await SeedDataAsync(
                "INSERT INTO BuyerAddress (StreetLine, City, Country, PostalCode) VALUES (@Street, @City, @Country, @Code)",
                new { 
                    Street = expectedShippingStreetWithId, 
                    City = ExpectedCity, 
                    Country = ExpectedCountry, 
                    Code = ExpectedShippingCode 
                },
                returnId: true);
            Console.WriteLine($"Shipping Address ID: {shippingAddressId}");
            Assert.True(shippingAddressId >= LowIdLimit, $"Shipping address ID should be >= {LowIdLimit}");

            // 3. Seed Buyer linked to User and Addresses
            await SeedDataAsync(
                "INSERT INTO Buyers (UserId, FirstName, LastName, BillingAddressId, ShippingAddressId, UseSameAddress, Badge, TotalSpending, NumberOfPurchases, Discount) " +
                "VALUES (@UserId, @FName, @LName, @BillingId, @ShippingId, @SameAddr, @Badge, @Spending, @Purchases, @Discount)",
                new
                {
                    UserId = userId,
                    FName = ExpectedFirstName,
                    LName = ExpectedLastName,
                    BillingId = billingAddressId,
                    ShippingId = shippingAddressId,
                    SameAddr = ExpectedUseSameAddress,
                    Badge = ExpectedBadge.ToString(),
                    Spending = ExpectedTotalSpending,
                    Purchases = ExpectedNumberOfPurchases,
                    Discount = ExpectedDiscount
                });

            return new BuyerTestData
            {
                UserId = userId,
                BillingAddressId = billingAddressId,
                ShippingAddressId = shippingAddressId,
                ExpectedFirstName = ExpectedFirstName,
                ExpectedLastName = ExpectedLastName,
                ExpectedBadge = ExpectedBadge,
                ExpectedTotalSpending = ExpectedTotalSpending,
                ExpectedNumberOfPurchases = ExpectedNumberOfPurchases,
                ExpectedDiscount = ExpectedDiscount,
                ExpectedUseSameAddress = ExpectedUseSameAddress,
                ExpectedBillingStreetWithId = expectedBillingStreetWithId,
                ExpectedShippingStreetWithId = expectedShippingStreetWithId
            };
        }

        [Fact]
        public async Task LoadBuyerInfo_ShouldLoadCorrectBasicData_WhenBuyerExists()
        {
            // Arrange
            var testData = await SetupTestBuyerWithDifferentAddresses();
            var repository = CreateRepository();
            var buyer = new Buyer { User = new User { UserId = testData.UserId } };

            // Act
            await repository.LoadBuyerInfo(buyer);

            // Assert
            Assert.NotNull(buyer);
            Assert.Equal(testData.ExpectedFirstName, buyer.FirstName);
            Assert.Equal(testData.ExpectedLastName, buyer.LastName);
            Assert.Equal(testData.ExpectedBadge, buyer.Badge);
            Assert.Equal(testData.ExpectedTotalSpending, buyer.TotalSpending);
            Assert.Equal(testData.ExpectedNumberOfPurchases, buyer.NumberOfPurchases);
            Assert.Equal(testData.ExpectedDiscount, buyer.Discount);
            Assert.Equal(testData.ExpectedUseSameAddress, buyer.UseSameAddress);
        }

        [Fact]
        public async Task LoadBuyerInfo_ShouldLoadCorrectBillingAddress_WhenBuyerExists()
        {
            // Arrange
            var testData = await SetupTestBuyerWithDifferentAddresses();
            var repository = CreateRepository();
            var buyer = new Buyer { User = new User { UserId = testData.UserId } };

            // Act
            await repository.LoadBuyerInfo(buyer);

            // Assert
            Assert.NotNull(buyer.BillingAddress);
            Assert.Equal(testData.BillingAddressId, buyer.BillingAddress.Id);
            Assert.Equal(testData.ExpectedBillingStreetWithId, buyer.BillingAddress.StreetLine);
        }

        [Fact]
        public async Task LoadBuyerInfo_ShouldLoadCorrectShippingAddress_WhenBuyerExists()
        {
            // Arrange
            var testData = await SetupTestBuyerWithDifferentAddresses();
            var repository = CreateRepository();
            var buyer = new Buyer { User = new User { UserId = testData.UserId } };

            // Act
            await repository.LoadBuyerInfo(buyer);

            // Assert
            Assert.NotNull(buyer.ShippingAddress);
            Assert.Equal(testData.ShippingAddressId, buyer.ShippingAddress.Id);
            Assert.Equal(testData.ExpectedShippingStreetWithId, buyer.ShippingAddress.StreetLine);
        }

        [Fact]
        public async Task LoadBuyerInfo_ShouldHaveDifferentAddressObjects_WhenUseSameAddressIsFalse()
        {
            // Arrange
            var testData = await SetupTestBuyerWithDifferentAddresses();
            var repository = CreateRepository();
            var buyer = new Buyer { User = new User { UserId = testData.UserId } };

            // Act
            await repository.LoadBuyerInfo(buyer);

            // Assert
            Assert.NotSame(buyer.BillingAddress, buyer.ShippingAddress);
        }

        [Fact]
        public async Task LoadBuyerInfo_ShouldLoadSameAddress_WhenUseSameAddressIsTrue()
        {
            // Arrange
            const string ExpectedUsername = "sameaddr";
            const string ExpectedEmail = "same@test.com";
            const string ExpectedPassword = "hashedpass";
            const string ExpectedFirstName = "Same";
            const string ExpectedLastName = "Address";
            const string ExpectedStreet = "789 Shared Rd";
            const string ExpectedCity = "Sameville";
            const string ExpectedCountry = "Testland";
            const string ExpectedPostalCode = "54321";
            const BuyerBadge ExpectedBadge = BuyerBadge.BRONZE;
            const decimal ExpectedTotalSpending = 50.0m;
            const int ExpectedNumberOfPurchases = 1;
            const decimal ExpectedDiscount = 0.0m;
            const bool ExpectedUseSameAddress = true;
            const int MinimumValidId = 1;

            var userId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = ExpectedUsername, 
                    Email = ExpectedEmail, 
                    Password = ExpectedPassword, 
                    Role = UserRole.Buyer 
                },
                returnId: true);
            Assert.True(userId >= MinimumValidId, "User ID should be valid");

            var addressId = await SeedDataAsync(
                "INSERT INTO BuyerAddress (StreetLine, City, Country, PostalCode) VALUES (@Street, @City, @Country, @Code)",
                new { 
                    Street = ExpectedStreet, 
                    City = ExpectedCity, 
                    Country = ExpectedCountry, 
                    Code = ExpectedPostalCode 
                },
                returnId: true);
            Assert.True(addressId >= MinimumValidId, "Address ID should be valid");

            await SeedDataAsync(
                "INSERT INTO Buyers (UserId, FirstName, LastName, BillingAddressId, ShippingAddressId, UseSameAddress, Badge, TotalSpending, NumberOfPurchases, Discount) " +
                "VALUES (@UserId, @FName, @LName, @BillingId, @ShippingId, @SameAddr, @Badge, @Spending, @Purchases, @Discount)",
                new
                {
                    UserId = userId,
                    FName = ExpectedFirstName,
                    LName = ExpectedLastName,
                    BillingId = addressId,
                    ShippingId = addressId,
                    SameAddr = ExpectedUseSameAddress,
                    Badge = ExpectedBadge.ToString(),
                    Spending = ExpectedTotalSpending,
                    Purchases = ExpectedNumberOfPurchases,
                    Discount = ExpectedDiscount
                });

            var repository = CreateRepository();
            var buyer = new Buyer { User = new User { UserId = userId } };

            // Act
            await repository.LoadBuyerInfo(buyer);

            // Assert
            Assert.Equal(ExpectedUseSameAddress, buyer.UseSameAddress);

            Assert.NotNull(buyer.BillingAddress);
            Assert.NotNull(buyer.ShippingAddress);
            Assert.Equal(addressId, buyer.BillingAddress.Id);
            Assert.Equal(addressId, buyer.ShippingAddress.Id);
            Assert.Same(buyer.BillingAddress, buyer.ShippingAddress);

            Assert.Equal(ExpectedStreet, buyer.BillingAddress.StreetLine);
        }

        [Fact]
        public async Task SaveInfo_ShouldUpdateBuyerInfo_WhenBuyerExists()
        {
            // Arrange
            const string ExpectedFirstName = "Updated";
            const string ExpectedLastName = "Name";
            const string ExpectedStreet = "456 New St";
            const string ExpectedCity = "New City";
            const string ExpectedCountry = "New Country";
            const string ExpectedPostalCode = "67890";
            const bool ExpectedUseSameAddress = false;
            const string BronzeBadge = "Bronze";
            const decimal InitialSpending = 0.0m;
            const int InitialPurchases = 0;
            const decimal InitialDiscount = 0.0m;

            // 1. Create a buyer first
            var uniqueId = GenerateUniqueId();
            var userId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"testbuyer_{uniqueId}", 
                    Email = $"buyer_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer 
                },
                returnId: true);

            var addressId = await SeedDataAsync(
                "INSERT INTO BuyerAddress (StreetLine, City, Country, PostalCode) VALUES (@Street, @City, @Country, @Code)",
                new { 
                    Street = $"123 Test St {uniqueId}", 
                    City = "Testville", 
                    Country = "Testland", 
                    Code = "12345" 
                },
                returnId: true);

            await SeedDataAsync(
                "INSERT INTO Buyers (UserId, FirstName, LastName, BillingAddressId, ShippingAddressId, UseSameAddress, Badge, TotalSpending, NumberOfPurchases, Discount) " +
                "VALUES (@UserId, @FName, @LName, @BillingId, @ShippingId, @SameAddr, @Badge, @Spending, @Purchases, @Discount)",
                new
                {
                    UserId = userId,
                    FName = "Test",
                    LName = "Buyer",
                    BillingId = addressId,
                    ShippingId = addressId,
                    SameAddr = true,
                    Badge = BronzeBadge,
                    Spending = InitialSpending,
                    Purchases = InitialPurchases,
                    Discount = InitialDiscount
                });

            var repository = CreateRepository();
            var buyer = new Buyer 
            { 
                User = new User { UserId = userId },
                FirstName = ExpectedFirstName,
                LastName = ExpectedLastName,
                UseSameAddress = ExpectedUseSameAddress,
                BillingAddress = new Address
                {
                    Id = addressId,
                    StreetLine = ExpectedStreet,
                    City = ExpectedCity,
                    Country = ExpectedCountry,
                    PostalCode = ExpectedPostalCode
                },
                ShippingAddress = new Address
                {
                    Id = addressId,
                    StreetLine = ExpectedStreet,
                    City = ExpectedCity,
                    Country = ExpectedCountry,
                    PostalCode = ExpectedPostalCode
                }
            };

            // Act
            await repository.SaveInfo(buyer);

            // Assert
            var updatedBuyer = new Buyer { User = new User { UserId = userId } };
            await repository.LoadBuyerInfo(updatedBuyer);

            Assert.Equal(ExpectedFirstName, updatedBuyer.FirstName);
            Assert.Equal(ExpectedLastName, updatedBuyer.LastName);
            Assert.Equal(ExpectedUseSameAddress, updatedBuyer.UseSameAddress);
            Assert.Equal(ExpectedStreet, updatedBuyer.BillingAddress.StreetLine);
            Assert.Equal(ExpectedCity, updatedBuyer.BillingAddress.City);
            Assert.Equal(ExpectedCountry, updatedBuyer.BillingAddress.Country);
            Assert.Equal(ExpectedPostalCode, updatedBuyer.BillingAddress.PostalCode);
        }

        [Fact]
        public async Task GetWishlist_ShouldReturnWishlist_WhenBuyerExists()
        {
            // Arrange
            // 1. Create a buyer
            var uniqueId = GenerateUniqueId();
            var userId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"wishlist_{uniqueId}", 
                    Email = $"wishlist_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer 
                },
                returnId: true);

            var addressId = await SeedDataAsync(
                "INSERT INTO BuyerAddress (StreetLine, City, Country, PostalCode) VALUES (@Street, @City, @Country, @Code)",
                new { 
                    Street = $"123 Test St {uniqueId}", 
                    City = "Testville", 
                    Country = "Testland", 
                    Code = "12345" 
                },
                returnId: true);

            const string BronzeBadge = "Bronze";
            const decimal InitialSpending = 0.0m;
            const int InitialPurchases = 0;
            const decimal InitialDiscount = 0.0m;

            await SeedDataAsync(
                "INSERT INTO Buyers (UserId, FirstName, LastName, BillingAddressId, ShippingAddressId, UseSameAddress, Badge, TotalSpending, NumberOfPurchases, Discount) " +
                "VALUES (@UserId, @FName, @LName, @BillingId, @ShippingId, @SameAddr, @Badge, @Spending, @Purchases, @Discount)",
                new
                {
                    UserId = userId,
                    FName = "Wishlist",
                    LName = "Buyer",
                    BillingId = addressId,
                    ShippingId = addressId,
                    SameAddr = true,
                    Badge = BronzeBadge,
                    Spending = InitialSpending,
                    Purchases = InitialPurchases,
                    Discount = InitialDiscount
                });

            // 2. Create a seller and products
            var sellerId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"seller_{uniqueId}", 
                    Email = $"seller_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Seller 
                },
                returnId: true);

            const string TestStoreName = "Test Store";
            const string TestStoreDescription = "Test Description";
            const string TestStoreAddress = "Test Address";
            const int InitialFollowers = 0;
            const float InitialTrustScore = 0.0f;

            await SeedDataAsync(
                "INSERT INTO Sellers (UserId, Username, StoreName, StoreDescription, StoreAddress, FollowersCount, TrustScore) " +
                "VALUES (@UserId, @Username, @StoreName, @Description, @Address, @Followers, @TrustScore)",
                new
                {
                    UserId = sellerId,
                    Username = $"seller_{uniqueId}",
                    StoreName = TestStoreName,
                    Description = TestStoreDescription,
                    Address = TestStoreAddress,
                    Followers = InitialFollowers,
                    TrustScore = InitialTrustScore
                });

            // Create products and get their IDs
            const string Product1Name = "Product 1";
            const string Product1Description = "Description 1";
            const float Product1Price = 10.0f;
            const int Product1Stock = 100;

            var productId1 = await SeedDataAsync(
                "INSERT INTO Products (SellerID, ProductName, ProductDescription, ProductPrice, ProductStock) " +
                "VALUES (@SellerId, @Name, @Desc, @Price, @Stock); SELECT CAST(SCOPE_IDENTITY() AS INT)",
                new
                {
                    SellerId = sellerId,
                    Name = Product1Name,
                    Desc = Product1Description,
                    Price = Product1Price,
                    Stock = Product1Stock
                },
                returnId: true);

            const string Product2Name = "Product 2";
            const string Product2Description = "Description 2";
            const float Product2Price = 20.0f;
            const int Product2Stock = 50;

            var productId2 = await SeedDataAsync(
                "INSERT INTO Products (SellerID, ProductName, ProductDescription, ProductPrice, ProductStock) " +
                "VALUES (@SellerId, @Name, @Desc, @Price, @Stock); SELECT CAST(SCOPE_IDENTITY() AS INT)",
                new
                {
                    SellerId = sellerId,
                    Name = Product2Name,
                    Desc = Product2Description,
                    Price = Product2Price,
                    Stock = Product2Stock
                },
                returnId: true);

            // 3. Add products to wishlist
            await SeedDataAsync(
                "INSERT INTO BuyerWishlistItems (BuyerId, ProductId) VALUES (@BuyerId, @ProductId)",
                new { BuyerId = userId, ProductId = productId1 });

            await SeedDataAsync(
                "INSERT INTO BuyerWishlistItems (BuyerId, ProductId) VALUES (@BuyerId, @ProductId)",
                new { BuyerId = userId, ProductId = productId2 });

            var repository = CreateRepository();

            // Act
            var wishlist = await repository.GetWishlist(userId);

            // Assert
            const int ExpectedCount = 2;
            Assert.NotNull(wishlist);
            Assert.Equal(ExpectedCount, wishlist.Items.Count);
            Assert.Contains(wishlist.Items, item => item.ProductId == productId1);
            Assert.Contains(wishlist.Items, item => item.ProductId == productId2);
        }

        [Fact]
        public async Task CreateBuyer_ShouldCreateNewBuyer_WhenValidData()
        {
            // Arrange
            const string ExpectedFirstName = "New";
            const string ExpectedLastName = "Buyer";
            const string ExpectedStreet = "123 New St";
            const string ExpectedCity = "New City";
            const string ExpectedCountry = "New Country";
            const string ExpectedPostalCode = "12345";
            const decimal ExpectedTotalSpending = 0.0m;
            const int ExpectedNumberOfPurchases = 0;
            const decimal ExpectedDiscount = 0.0m;
            const BuyerBadge ExpectedBadge = BuyerBadge.BRONZE;
            const bool ExpectedUseSameAddress = true;
            const string TestUsername = "newbuyer";
            const string TestEmail = "new@test.com";
            const string TestPassword = "hashedpass";
            const UserRole TestUserRole = UserRole.Buyer;

            var userId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { Username = TestUsername, Email = TestEmail, Password = TestPassword, Role = (int)TestUserRole },
                returnId: true);

            var repository = CreateRepository();
            var buyer = new Buyer
            {
                User = new User { UserId = userId },
                FirstName = ExpectedFirstName,
                LastName = ExpectedLastName,
                UseSameAddress = ExpectedUseSameAddress,
                Badge = ExpectedBadge,
                TotalSpending = ExpectedTotalSpending,
                NumberOfPurchases = ExpectedNumberOfPurchases,
                Discount = ExpectedDiscount,
                BillingAddress = new Address
                {
                    StreetLine = ExpectedStreet,
                    City = ExpectedCity,
                    Country = ExpectedCountry,
                    PostalCode = ExpectedPostalCode
                },
                ShippingAddress = new Address
                {
                    StreetLine = ExpectedStreet,
                    City = ExpectedCity,
                    Country = ExpectedCountry,
                    PostalCode = ExpectedPostalCode
                }
            };

            // Act
            await repository.CreateBuyer(buyer);

            // Assert
            var loadedBuyer = new Buyer { User = new User { UserId = userId } };
            await repository.LoadBuyerInfo(loadedBuyer);

            Assert.Equal(ExpectedFirstName, loadedBuyer.FirstName);
            Assert.Equal(ExpectedLastName, loadedBuyer.LastName);
            Assert.Equal(ExpectedUseSameAddress, loadedBuyer.UseSameAddress);
            Assert.Equal(ExpectedBadge, loadedBuyer.Badge);
            Assert.Equal(ExpectedTotalSpending, loadedBuyer.TotalSpending);
            Assert.Equal(ExpectedNumberOfPurchases, loadedBuyer.NumberOfPurchases);
            Assert.Equal(ExpectedDiscount, loadedBuyer.Discount);
            Assert.Equal(ExpectedStreet, loadedBuyer.BillingAddress.StreetLine);
            Assert.Equal(ExpectedCity, loadedBuyer.BillingAddress.City);
            Assert.Equal(ExpectedCountry, loadedBuyer.BillingAddress.Country);
            Assert.Equal(ExpectedPostalCode, loadedBuyer.BillingAddress.PostalCode);
            
            // Instead of Assert.Same, verify the addresses have the same values and IDs
            Assert.Equal(loadedBuyer.BillingAddress.Id, loadedBuyer.ShippingAddress.Id);
            Assert.Equal(loadedBuyer.BillingAddress.StreetLine, loadedBuyer.ShippingAddress.StreetLine);
            Assert.Equal(loadedBuyer.BillingAddress.City, loadedBuyer.ShippingAddress.City);
            Assert.Equal(loadedBuyer.BillingAddress.Country, loadedBuyer.ShippingAddress.Country);
            Assert.Equal(loadedBuyer.BillingAddress.PostalCode, loadedBuyer.ShippingAddress.PostalCode);
        }

        [Fact]
        public async Task FollowSeller_ShouldCreateFollowing_WhenValidIds()
        {
            // Arrange
            // 1. Create a buyer
            var uniqueId = GenerateUniqueId();
            var buyerId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"follower_{uniqueId}", 
                    Email = $"follower_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = 2 
                },
                returnId: true);

            var addressId = await SeedDataAsync(
                "INSERT INTO BuyerAddress (StreetLine, City, Country, PostalCode) VALUES (@Street, @City, @Country, @Code)",
                new { 
                    Street = $"123 Test St {uniqueId}", 
                    City = "Testville", 
                    Country = "Testland", 
                    Code = "12345" 
                },
                returnId: true);

            await SeedDataAsync(
                "INSERT INTO Buyers (UserId, FirstName, LastName, BillingAddressId, ShippingAddressId, UseSameAddress, Badge, TotalSpending, NumberOfPurchases, Discount) " +
                "VALUES (@UserId, @FName, @LName, @BillingId, @ShippingId, @SameAddr, @Badge, @Spending, @Purchases, @Discount)",
                new
                {
                    UserId = buyerId,
                    FName = "Follower",
                    LName = "Buyer",
                    BillingId = addressId,
                    ShippingId = addressId,
                    SameAddr = true,
                    Badge = "Bronze",
                    Spending = 0.0m,
                    Purchases = 0,
                    Discount = 0.0m
                });

            // 2. Create a seller
            var sellerId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"followed_{uniqueId}", 
                    Email = $"followed_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = 1 
                },
                returnId: true);

            await SeedDataAsync(
                "INSERT INTO Sellers (UserId, Username, StoreName, StoreDescription, StoreAddress, FollowersCount, TrustScore) " +
                "VALUES (@UserId, @Username, @StoreName, @Description, @Address, @Followers, @TrustScore)",
                new
                {
                    UserId = sellerId,
                    Username = $"followed_{uniqueId}",
                    StoreName = "Followed Store",
                    Description = "Test Description",
                    Address = "Test Address",
                    Followers = 0,
                    TrustScore = 0.0f
                });

            var repository = CreateRepository();

            // Act
            await repository.FollowSeller(buyerId, sellerId);

            // Assert
            var isFollowing = await repository.IsFollowing(buyerId, sellerId);
            Assert.True(isFollowing);

            var followingIds = await repository.GetFollowingUsersIds(buyerId);
            Assert.Contains(sellerId, followingIds);
        }

        [Fact]
        public async Task GetBuyerLinkages_ShouldReturnAllLinkages_WhenBuyerExists()
        {
            // Arrange
            var uniqueId = GenerateUniqueId();
            
            // Create first buyer
            var buyerId1 = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"buyer1_{uniqueId}", 
                    Email = $"buyer1_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer
                },
                returnId: true);

            var addressId1 = await SeedDataAsync(
                "INSERT INTO BuyerAddress (StreetLine, City, Country, PostalCode) VALUES (@Street, @City, @Country, @Code)",
                new { 
                    Street = $"123 Test St {uniqueId}", 
                    City = "Testville", 
                    Country = "Testland", 
                    Code = "12345" 
                },
                returnId: true);

            await SeedDataAsync(
                "INSERT INTO Buyers (UserId, FirstName, LastName, BillingAddressId, ShippingAddressId, UseSameAddress, Badge, TotalSpending, NumberOfPurchases, Discount) " +
                "VALUES (@UserId, @FName, @LName, @BillingId, @ShippingId, @SameAddr, @Badge, @Spending, @Purchases, @Discount)",
                new
                {
                    UserId = buyerId1,
                    FName = "Buyer1",
                    LName = "Test",
                    BillingId = addressId1,
                    ShippingId = addressId1,
                    SameAddr = true,
                    Badge = "Bronze",
                    Spending = 0.0m,
                    Purchases = 0,
                    Discount = 0.0m
                });

            // Create second buyer
            var buyerId2 = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"buyer2_{uniqueId}", 
                    Email = $"buyer2_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer 
                },
                returnId: true);

            var addressId2 = await SeedDataAsync(
                "INSERT INTO BuyerAddress (StreetLine, City, Country, PostalCode) VALUES (@Street, @City, @Country, @Code)",
                new { 
                    Street = $"456 Test St {uniqueId}", 
                    City = "Testville", 
                    Country = "Testland", 
                    Code = "12345" 
                },
                returnId: true);

            await SeedDataAsync(
                "INSERT INTO Buyers (UserId, FirstName, LastName, BillingAddressId, ShippingAddressId, UseSameAddress, Badge, TotalSpending, NumberOfPurchases, Discount) " +
                "VALUES (@UserId, @FName, @LName, @BillingId, @ShippingId, @SameAddr, @Badge, @Spending, @Purchases, @Discount)",
                new
                {
                    UserId = buyerId2,
                    FName = "Buyer2",
                    LName = "Test",
                    BillingId = addressId2,
                    ShippingId = addressId2,
                    SameAddr = true,
                    Badge = "Bronze",
                    Spending = 0.0m,
                    Purchases = 0,
                    Discount = 0.0m
                });

            // Create linkage requests
            await SeedDataAsync(
                "INSERT INTO BuyerLinkage (RequestingBuyerId, ReceivingBuyerId, IsApproved) VALUES (@RequestingId, @ReceivingId, @IsApproved)",
                new { RequestingId = buyerId1, ReceivingId = buyerId2, IsApproved = false });

            await SeedDataAsync(
                "INSERT INTO BuyerLinkage (RequestingBuyerId, ReceivingBuyerId, IsApproved) VALUES (@RequestingId, @ReceivingId, @IsApproved)",
                new { RequestingId = buyerId2, ReceivingId = buyerId1, IsApproved = true });

            var repository = CreateRepository();

            // Act
            var linkages = await repository.GetBuyerLinkages(buyerId1);

            // Assert
            const int ExpectedCount = 2;
            Assert.NotNull(linkages);
            Assert.Equal(ExpectedCount, linkages.Count);
            Assert.Contains(linkages, l => l.Buyer.Id == buyerId2 && l.Status == BuyerLinkageStatus.PendingSelf);
            Assert.Contains(linkages, l => l.Buyer.Id == buyerId2 && l.Status == BuyerLinkageStatus.Confirmed);
        }

        [Fact]
        public async Task CreateLinkageRequest_ShouldCreateNewRequest_WhenValidIds()
        {
            // Arrange
            var uniqueId = GenerateUniqueId();
            
            // Create first buyer
            var buyerId1 = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"buyer1_{uniqueId}", 
                    Email = $"buyer1_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer 
                },
                returnId: true);

            var addressId1 = await SeedDataAsync(
                "INSERT INTO BuyerAddress (StreetLine, City, Country, PostalCode) VALUES (@Street, @City, @Country, @Code)",
                new { 
                    Street = $"123 Test St {uniqueId}", 
                    City = "Testville", 
                    Country = "Testland", 
                    Code = "12345" 
                },
                returnId: true);

            await SeedDataAsync(
                "INSERT INTO Buyers (UserId, FirstName, LastName, BillingAddressId, ShippingAddressId, UseSameAddress, Badge, TotalSpending, NumberOfPurchases, Discount) " +
                "VALUES (@UserId, @FName, @LName, @BillingId, @ShippingId, @SameAddr, @Badge, @Spending, @Purchases, @Discount)",
                new
                {
                    UserId = buyerId1,
                    FName = "Buyer1",
                    LName = "Test",
                    BillingId = addressId1,
                    ShippingId = addressId1,
                    SameAddr = true,
                    Badge = "Bronze",
                    Spending = 0.0m,
                    Purchases = 0,
                    Discount = 0.0m
                });

            // Create second buyer
            var buyerId2 = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"buyer2_{uniqueId}", 
                    Email = $"buyer2_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer 
                },
                returnId: true);

            var addressId2 = await SeedDataAsync(
                "INSERT INTO BuyerAddress (StreetLine, City, Country, PostalCode) VALUES (@Street, @City, @Country, @Code)",
                new { 
                    Street = $"456 Test St {uniqueId}", 
                    City = "Testville", 
                    Country = "Testland", 
                    Code = "12345" 
                },
                returnId: true);

            await SeedDataAsync(
                "INSERT INTO Buyers (UserId, FirstName, LastName, BillingAddressId, ShippingAddressId, UseSameAddress, Badge, TotalSpending, NumberOfPurchases, Discount) " +
                "VALUES (@UserId, @FName, @LName, @BillingId, @ShippingId, @SameAddr, @Badge, @Spending, @Purchases, @Discount)",
                new
                {
                    UserId = buyerId2,
                    FName = "Buyer2",
                    LName = "Test",
                    BillingId = addressId2,
                    ShippingId = addressId2,
                    SameAddr = true,
                    Badge = "Bronze",
                    Spending = 0.0m,
                    Purchases = 0,
                    Discount = 0.0m
                });

            var repository = CreateRepository();

            // Act
            await repository.CreateLinkageRequest(buyerId1, buyerId2);

            // Assert
            var linkages = await repository.GetBuyerLinkages(buyerId1);
            Assert.NotNull(linkages);
            Assert.Contains(linkages, l => l.Buyer.Id == buyerId2 && l.Status == BuyerLinkageStatus.PendingSelf);

            // Also verify from the receiving buyer's perspective
            var receivingLinkages = await repository.GetBuyerLinkages(buyerId2);
            Assert.NotNull(receivingLinkages);
            Assert.Contains(receivingLinkages, l => l.Buyer.Id == buyerId1 && l.Status == BuyerLinkageStatus.PendingOther);
        }

        [Fact]
        public async Task UpdateLinkageRequest_ShouldUpdateStatus_WhenValidIds()
        {
            // Arrange
            var uniqueId = GenerateUniqueId();
            
            // Create first buyer
            var buyerId1 = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"buyer1_{uniqueId}", 
                    Email = $"buyer1_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer 
                },
                returnId: true);

            var addressId1 = await SeedDataAsync(
                "INSERT INTO BuyerAddress (StreetLine, City, Country, PostalCode) VALUES (@Street, @City, @Country, @Code)",
                new { 
                    Street = $"123 Test St {uniqueId}", 
                    City = "Testville", 
                    Country = "Testland", 
                    Code = "12345" 
                },
                returnId: true);

            await SeedDataAsync(
                "INSERT INTO Buyers (UserId, FirstName, LastName, BillingAddressId, ShippingAddressId, UseSameAddress, Badge, TotalSpending, NumberOfPurchases, Discount) " +
                "VALUES (@UserId, @FName, @LName, @BillingId, @ShippingId, @SameAddr, @Badge, @Spending, @Purchases, @Discount)",
                new
                {
                    UserId = buyerId1,
                    FName = "Buyer1",
                    LName = "Test",
                    BillingId = addressId1,
                    ShippingId = addressId1,
                    SameAddr = true,
                    Badge = "Bronze",
                    Spending = 0.0m,
                    Purchases = 0,
                    Discount = 0.0m
                });

            // Create second buyer
            var buyerId2 = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"buyer2_{uniqueId}", 
                    Email = $"buyer2_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer 
                },
                returnId: true);

            var addressId2 = await SeedDataAsync(
                "INSERT INTO BuyerAddress (StreetLine, City, Country, PostalCode) VALUES (@Street, @City, @Country, @Code)",
                new { 
                    Street = $"456 Test St {uniqueId}", 
                    City = "Testville", 
                    Country = "Testland", 
                    Code = "12345" 
                },
                returnId: true);

            await SeedDataAsync(
                "INSERT INTO Buyers (UserId, FirstName, LastName, BillingAddressId, ShippingAddressId, UseSameAddress, Badge, TotalSpending, NumberOfPurchases, Discount) " +
                "VALUES (@UserId, @FName, @LName, @BillingId, @ShippingId, @SameAddr, @Badge, @Spending, @Purchases, @Discount)",
                new
                {
                    UserId = buyerId2,
                    FName = "Buyer2",
                    LName = "Test",
                    BillingId = addressId2,
                    ShippingId = addressId2,
                    SameAddr = true,
                    Badge = "Bronze",
                    Spending = 0.0m,
                    Purchases = 0,
                    Discount = 0.0m
                });

            // Create initial linkage request
            await SeedDataAsync(
                "INSERT INTO BuyerLinkage (RequestingBuyerId, ReceivingBuyerId, IsApproved) VALUES (@RequestingId, @ReceivingId, @IsApproved)",
                new { RequestingId = buyerId1, ReceivingId = buyerId2, IsApproved = false });

            var repository = CreateRepository();

            // Act
            await repository.UpdateLinkageRequest(buyerId1, buyerId2);

            // Assert
            var linkages = await repository.GetBuyerLinkages(buyerId1);
            Assert.NotNull(linkages);
            Assert.Contains(linkages, l => l.Buyer.Id == buyerId2 && l.Status == BuyerLinkageStatus.Confirmed);
        }

        [Fact]
        public async Task DeleteLinkageRequest_ShouldRemoveRequest_WhenValidIds()
        {
            // Arrange
            var uniqueId = GenerateUniqueId();
            
            // Create first buyer
            var buyerId1 = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"buyer1_{uniqueId}", 
                    Email = $"buyer1_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer 
                },
                returnId: true);

            var addressId1 = await SeedDataAsync(
                "INSERT INTO BuyerAddress (StreetLine, City, Country, PostalCode) VALUES (@Street, @City, @Country, @Code)",
                new { 
                    Street = $"123 Test St {uniqueId}", 
                    City = "Testville", 
                    Country = "Testland", 
                    Code = "12345" 
                },
                returnId: true);

            await SeedDataAsync(
                "INSERT INTO Buyers (UserId, FirstName, LastName, BillingAddressId, ShippingAddressId, UseSameAddress, Badge, TotalSpending, NumberOfPurchases, Discount) " +
                "VALUES (@UserId, @FName, @LName, @BillingId, @ShippingId, @SameAddr, @Badge, @Spending, @Purchases, @Discount)",
                new
                {
                    UserId = buyerId1,
                    FName = "Buyer1",
                    LName = "Test",
                    BillingId = addressId1,
                    ShippingId = addressId1,
                    SameAddr = true,
                    Badge = "Bronze",
                    Spending = 0.0m,
                    Purchases = 0,
                    Discount = 0.0m
                });

            // Create second buyer
            var buyerId2 = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"buyer2_{uniqueId}", 
                    Email = $"buyer2_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer 
                },
                returnId: true);

            var addressId2 = await SeedDataAsync(
                "INSERT INTO BuyerAddress (StreetLine, City, Country, PostalCode) VALUES (@Street, @City, @Country, @Code)",
                new { 
                    Street = $"456 Test St {uniqueId}", 
                    City = "Testville", 
                    Country = "Testland", 
                    Code = "12345" 
                },
                returnId: true);

            await SeedDataAsync(
                "INSERT INTO Buyers (UserId, FirstName, LastName, BillingAddressId, ShippingAddressId, UseSameAddress, Badge, TotalSpending, NumberOfPurchases, Discount) " +
                "VALUES (@UserId, @FName, @LName, @BillingId, @ShippingId, @SameAddr, @Badge, @Spending, @Purchases, @Discount)",
                new
                {
                    UserId = buyerId2,
                    FName = "Buyer2",
                    LName = "Test",
                    BillingId = addressId2,
                    ShippingId = addressId2,
                    SameAddr = true,
                    Badge = "Bronze",
                    Spending = 0.0m,
                    Purchases = 0,
                    Discount = 0.0m
                });

            // Create initial linkage request
            await SeedDataAsync(
                "INSERT INTO BuyerLinkage (RequestingBuyerId, ReceivingBuyerId, IsApproved) VALUES (@RequestingId, @ReceivingId, @IsApproved)",
                new { RequestingId = buyerId1, ReceivingId = buyerId2, IsApproved = false });

            var repository = CreateRepository();

            // Act
            var result = await repository.DeleteLinkageRequest(buyerId1, buyerId2);

            // Assert
            Assert.True(result);
            var linkages = await repository.GetBuyerLinkages(buyerId1);
            Assert.NotNull(linkages);
            Assert.DoesNotContain(linkages, l => l.Buyer.Id == buyerId2);
        }

        [Fact]
        public async Task GetFollowedSellers_ShouldReturnFollowedSellers_WhenBuyerExists()
        {
            // Arrange
            var uniqueId = GenerateUniqueId();
            var userId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"buyer_{uniqueId}", 
                    Email = $"buyer_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer 
                },
                returnId: true);

            var addressId = await SeedDataAsync(
                "INSERT INTO BuyerAddress (StreetLine, City, Country, PostalCode) VALUES (@Street, @City, @Country, @Code)",
                new { 
                    Street = $"123 Test St {uniqueId}", 
                    City = "Testville", 
                    Country = "Testland", 
                    Code = "12345" 
                },
                returnId: true);

            await SeedDataAsync(
                "INSERT INTO Buyers (UserId, FirstName, LastName, BillingAddressId, ShippingAddressId, UseSameAddress, Badge, TotalSpending, NumberOfPurchases, Discount) " +
                "VALUES (@UserId, @FName, @LName, @BillingId, @ShippingId, @SameAddr, @Badge, @Spending, @Purchases, @Discount)",
                new
                {
                    UserId = userId,
                    FName = "Test",
                    LName = "Buyer",
                    BillingId = addressId,
                    ShippingId = addressId,
                    SameAddr = true,
                    Badge = "Bronze",
                    Spending = 0.0m,
                    Purchases = 0,
                    Discount = 0.0m
                });

            var sellerId1 = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"seller1_{uniqueId}", 
                    Email = $"seller1_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Seller 
                },
                returnId: true);

            var sellerId2 = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"seller2_{uniqueId}", 
                    Email = $"seller2_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Seller 
                },
                returnId: true);

            await SeedDataAsync(
                "INSERT INTO Sellers (UserId, Username, StoreName, StoreDescription, StoreAddress, FollowersCount, TrustScore) " +
                "VALUES (@UserId, @Username, @StoreName, @Description, @Address, @Followers, @TrustScore)",
                new
                {
                    UserId = sellerId1,
                    Username = $"seller1_{uniqueId}",
                    StoreName = "Test Store 1",
                    Description = "Test Description 1",
                    Address = "Test Address 1",
                    Followers = 0,
                    TrustScore = 0.0f
                });

            await SeedDataAsync(
                "INSERT INTO Sellers (UserId, Username, StoreName, StoreDescription, StoreAddress, FollowersCount, TrustScore) " +
                "VALUES (@UserId, @Username, @StoreName, @Description, @Address, @Followers, @TrustScore)",
                new
                {
                    UserId = sellerId2,
                    Username = $"seller2_{uniqueId}",
                    StoreName = "Test Store 2",
                    Description = "Test Description 2",
                    Address = "Test Address 2",
                    Followers = 0,
                    TrustScore = 0.0f
                });

            await SeedDataAsync(
                "INSERT INTO Following (FollowerID, FollowedID) VALUES (@FollowerId, @FollowedId)",
                new { FollowerId = userId, FollowedId = sellerId1 });

            await SeedDataAsync(
                "INSERT INTO Following (FollowerID, FollowedID) VALUES (@FollowerId, @FollowedId)",
                new { FollowerId = userId, FollowedId = sellerId2 });

            var repository = CreateRepository();
            var followingIds = await repository.GetFollowingUsersIds(userId);

            // Act
            var followedSellers = await repository.GetFollowedSellers(followingIds);

            // Assert
            const int ExpectedCount = 2;
            Assert.Equal(ExpectedCount, followedSellers.Count);
            Assert.Contains(followedSellers, s => s.Id == sellerId1);
            Assert.Contains(followedSellers, s => s.Id == sellerId2);
        }

        [Fact]
        public async Task GetAllSellers_ShouldReturnAllSellers()
        {
            // Arrange
            var uniqueId = GenerateUniqueId();
            
            // Create sellers
            var sellerId1 = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"seller1_{uniqueId}", 
                    Email = $"seller1_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Seller
                },
                returnId: true);

            await SeedDataAsync(
                "INSERT INTO Sellers (UserId, Username, StoreName, StoreDescription, StoreAddress, FollowersCount, TrustScore) " +
                "VALUES (@UserId, @Username, @StoreName, @Description, @Address, @Followers, @TrustScore)",
                new
                {
                    UserId = sellerId1,
                    Username = $"seller1_{uniqueId}",
                    StoreName = "Store 1",
                    Description = "Description 1",
                    Address = "Address 1",
                    Followers = 0,
                    TrustScore = 0.0f
                });

            var sellerId2 = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"seller2_{uniqueId}", 
                    Email = $"seller2_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Seller 
                },
                returnId: true);

            await SeedDataAsync(
                "INSERT INTO Sellers (UserId, Username, StoreName, StoreDescription, StoreAddress, FollowersCount, TrustScore) " +
                "VALUES (@UserId, @Username, @StoreName, @Description, @Address, @Followers, @TrustScore)",
                new
                {
                    UserId = sellerId2,
                    Username = $"seller2_{uniqueId}",
                    StoreName = "Store 2",
                    Description = "Description 2",
                    Address = "Address 2",
                    Followers = 0,
                    TrustScore = 0.0f
                });

            var repository = CreateRepository();

            // Act
            var allSellers = await repository.GetAllSellers();

            // Assert
            const int ExpectedCount = 2;
            Assert.NotNull(allSellers);
            Assert.Equal(ExpectedCount, allSellers.Count);
            Assert.Contains(allSellers, s => s.Id == sellerId1);
            Assert.Contains(allSellers, s => s.Id == sellerId2);
        }

        [Fact]
        public async Task GetProductsFromSeller_ShouldReturnSellerProducts_WhenSellerExists()
        {
            // Arrange
            var uniqueId = GenerateUniqueId();
            
            // Create seller
            var sellerId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"seller_{uniqueId}", 
                    Email = $"seller_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Seller
                },
                returnId: true);

            await SeedDataAsync(
                "INSERT INTO Sellers (UserId, Username, StoreName, StoreDescription, StoreAddress, FollowersCount, TrustScore) " +
                "VALUES (@UserId, @Username, @StoreName, @Description, @Address, @Followers, @TrustScore)",
                new
                {
                    UserId = sellerId,
                    Username = $"seller_{uniqueId}",
                    StoreName = "Test Store",
                    Description = "Test Description",
                    Address = "Test Address",
                    Followers = 0,
                    TrustScore = 0.0f
                });

            // Create products
            var productId1 = await SeedDataAsync(
                "INSERT INTO Products (SellerID, ProductName, ProductDescription, ProductPrice, ProductStock) " +
                "VALUES (@SellerId, @Name, @Desc, @Price, @Stock); SELECT CAST(SCOPE_IDENTITY() AS INT)",
                new
                {
                    SellerId = sellerId,
                    Name = "Product 1",
                    Desc = "Description 1",
                    Price = 10.0f,
                    Stock = 100
                },
                returnId: true);

            var productId2 = await SeedDataAsync(
                "INSERT INTO Products (SellerID, ProductName, ProductDescription, ProductPrice, ProductStock) " +
                "VALUES (@SellerId, @Name, @Desc, @Price, @Stock); SELECT CAST(SCOPE_IDENTITY() AS INT)",
                new
                {
                    SellerId = sellerId,
                    Name = "Product 2",
                    Desc = "Description 2",
                    Price = 20.0f,
                    Stock = 50
                },
                returnId: true);

            var repository = CreateRepository();

            // Act
            var products = await repository.GetProductsFromSeller(sellerId);

            // Assert
            int expectedCount = 2;
            Assert.NotNull(products);
            Assert.Equal(expectedCount, products.Count);
            Assert.Contains(products, p => p.ProductId == productId1);
            Assert.Contains(products, p => p.ProductId == productId2);
        }

        [Fact]
        public async Task CheckIfBuyerExists_ShouldReturnTrue_WhenBuyerExists()
        {
            // Arrange
            var uniqueId = GenerateUniqueId();
            
            // Create buyer
            var buyerId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"buyer_{uniqueId}", 
                    Email = $"buyer_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer
                },
                returnId: true);

            var addressId = await SeedDataAsync(
                "INSERT INTO BuyerAddress (StreetLine, City, Country, PostalCode) VALUES (@Street, @City, @Country, @Code)",
                new { 
                    Street = $"123 Test St {uniqueId}", 
                    City = "Testville", 
                    Country = "Testland", 
                    Code = "12345" 
                },
                returnId: true);

            await SeedDataAsync(
                "INSERT INTO Buyers (UserId, FirstName, LastName, BillingAddressId, ShippingAddressId, UseSameAddress, Badge, TotalSpending, NumberOfPurchases, Discount) " +
                "VALUES (@UserId, @FName, @LName, @BillingId, @ShippingId, @SameAddr, @Badge, @Spending, @Purchases, @Discount)",
                new
                {
                    UserId = buyerId,
                    FName = "Buyer",
                    LName = "Test",
                    BillingId = addressId,
                    ShippingId = addressId,
                    SameAddr = true,
                    Badge = "Bronze",
                    Spending = 0.0m,
                    Purchases = 0,
                    Discount = 0.0m
                });

            var repository = CreateRepository();

            // Act
            var exists = await repository.CheckIfBuyerExists(buyerId);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task UnfollowSeller_ShouldRemoveFollowing_WhenValidIds()
        {
            // Arrange
            var uniqueId = GenerateUniqueId();
            var userId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"buyer_{uniqueId}", 
                    Email = $"buyer_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer
                },
                returnId: true);

            var addressId = await SeedDataAsync(
                "INSERT INTO BuyerAddress (StreetLine, City, Country, PostalCode) VALUES (@Street, @City, @Country, @Code)",
                new { 
                    Street = $"123 Test St {uniqueId}", 
                    City = "Testville", 
                    Country = "Testland", 
                    Code = "12345" 
                },
                returnId: true);

            await SeedDataAsync(
                "INSERT INTO Buyers (UserId, FirstName, LastName, BillingAddressId, ShippingAddressId, UseSameAddress, Badge, TotalSpending, NumberOfPurchases, Discount) " +
                "VALUES (@UserId, @FName, @LName, @BillingId, @ShippingId, @SameAddr, @Badge, @Spending, @Purchases, @Discount)",
                new
                {
                    UserId = userId,
                    FName = "Test",
                    LName = "Buyer",
                    BillingId = addressId,
                    ShippingId = addressId,
                    SameAddr = true,
                    Badge = "Bronze",
                    Spending = 0.0m,
                    Purchases = 0,
                    Discount = 0.0m
                });

            var sellerId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"seller_{uniqueId}", 
                    Email = $"seller_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Seller
                },
                returnId: true);

            await SeedDataAsync(
                "INSERT INTO Sellers (UserId, Username, StoreName, StoreDescription, StoreAddress, FollowersCount, TrustScore) " +
                "VALUES (@UserId, @Username, @StoreName, @Description, @Address, @Followers, @TrustScore)",
                new
                {
                    UserId = sellerId,
                    Username = $"seller_{uniqueId}",
                    StoreName = "Test Store",
                    Description = "Test Description",
                    Address = "Test Address",
                    Followers = 0,
                    TrustScore = 0.0f
                });

            await SeedDataAsync(
                "INSERT INTO Following (FollowerID, FollowedID) VALUES (@FollowerId, @FollowedId)",
                new { FollowerId = userId, FollowedId = sellerId });

            var repository = CreateRepository();

            // Act
            await repository.UnfollowSeller(userId, sellerId);

            // Assert
            var isFollowing = await repository.IsFollowing(userId, sellerId);
            Assert.False(isFollowing);
        }

        [Fact]
        public async Task GetTotalCount_ShouldReturnCorrectCount()
        {
            // Arrange
            var uniqueId = GenerateUniqueId();
            
            // Clean up existing data in correct order to respect foreign key constraints
            await SeedDataAsync("DELETE FROM BuyerWishlistItems", null);
            await SeedDataAsync("DELETE FROM Following", null);
            await SeedDataAsync("DELETE FROM BuyerLinkage", null);
            await SeedDataAsync("DELETE FROM Buyers", null);
            await SeedDataAsync("DELETE FROM BuyerAddress", null);
            await SeedDataAsync($"DELETE FROM Users WHERE Role = {(int)UserRole.Buyer}", null);
            
            // Create first buyer
            var buyerId1 = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"buyer1_{uniqueId}", 
                    Email = $"buyer1_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer
                },
                returnId: true);

            var addressId1 = await SeedDataAsync(
                "INSERT INTO BuyerAddress (StreetLine, City, Country, PostalCode) VALUES (@Street, @City, @Country, @Code)",
                new { 
                    Street = $"123 Test St {uniqueId}", 
                    City = "Testville", 
                    Country = "Testland", 
                    Code = "12345" 
                },
                returnId: true);

            await SeedDataAsync(
                "INSERT INTO Buyers (UserId, FirstName, LastName, BillingAddressId, ShippingAddressId, UseSameAddress, Badge, TotalSpending, NumberOfPurchases, Discount) " +
                "VALUES (@UserId, @FName, @LName, @BillingId, @ShippingId, @SameAddr, @Badge, @Spending, @Purchases, @Discount)",
                new
                {
                    UserId = buyerId1,
                    FName = "Buyer1",
                    LName = "Test",
                    BillingId = addressId1,
                    ShippingId = addressId1,
                    SameAddr = true,
                    Badge = "Bronze",
                    Spending = 0.0m,
                    Purchases = 0,
                    Discount = 0.0m
                });

            // Create second buyer
            var buyerId2 = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"buyer2_{uniqueId}", 
                    Email = $"buyer2_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = 2 
                },
                returnId: true);

            var addressId2 = await SeedDataAsync(
                "INSERT INTO BuyerAddress (StreetLine, City, Country, PostalCode) VALUES (@Street, @City, @Country, @Code)",
                new { 
                    Street = $"456 Test St {uniqueId}", 
                    City = "Testville", 
                    Country = "Testland", 
                    Code = "12345" 
                },
                returnId: true);

            await SeedDataAsync(
                "INSERT INTO Buyers (UserId, FirstName, LastName, BillingAddressId, ShippingAddressId, UseSameAddress, Badge, TotalSpending, NumberOfPurchases, Discount) " +
                "VALUES (@UserId, @FName, @LName, @BillingId, @ShippingId, @SameAddr, @Badge, @Spending, @Purchases, @Discount)",
                new
                {
                    UserId = buyerId2,
                    FName = "Buyer2",
                    LName = "Test",
                    BillingId = addressId2,
                    ShippingId = addressId2,
                    SameAddr = true,
                    Badge = "Bronze",
                    Spending = 0.0m,
                    Purchases = 0,
                    Discount = 0.0m
                });

            var repository = CreateRepository();

            // Act
            var count = await repository.GetTotalCount();

            // Assert
            int expectedCount = 2;
            Assert.Equal(expectedCount, count);
        }

        [Fact]
        public async Task UpdateAfterPurchase_ShouldUpdateBuyerStats_WhenValidData()
        {
            // Arrange
            const decimal EXPECTED_TOTAL_SPENDING = 100.0m;
            const int EXPECTED_NUMBER_OF_PURCHASES = 1;
            const decimal EXPECTED_DEFAULT_SPENDING = 0.0m;
            const int EXPECTED_DEFAULT_PURCHASES = 0;
            const decimal EXPECTED_DEFAULT_DISCOUNT = 0.0m;
            const BuyerBadge EXPECTED_DEFAULT_BADGE = BuyerBadge.BRONZE;

            var uniqueId = GenerateUniqueId();
            var userId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"buyer_{uniqueId}", 
                    Email = $"buyer_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer 
                },
                returnId: true);

            var addressId = await SeedDataAsync(
                "INSERT INTO BuyerAddress (StreetLine, City, Country, PostalCode) VALUES (@Street, @City, @Country, @Code)",
                new { 
                    Street = $"123 Test St {uniqueId}", 
                    City = "Testville", 
                    Country = "Testland", 
                    Code = "12345" 
                },
                returnId: true);

            await SeedDataAsync(
                "INSERT INTO Buyers (UserId, FirstName, LastName, BillingAddressId, ShippingAddressId, UseSameAddress, Badge, TotalSpending, NumberOfPurchases, Discount) " +
                "VALUES (@UserId, @FName, @LName, @BillingId, @ShippingId, @SameAddr, @Badge, @Spending, @Purchases, @Discount)",
                new
                {
                    UserId = userId,
                    FName = "Test",
                    LName = "Buyer",
                    BillingId = addressId,
                    ShippingId = addressId,
                    SameAddr = true,
                    Badge = EXPECTED_DEFAULT_BADGE.ToString(),
                    Spending = EXPECTED_DEFAULT_SPENDING,
                    Purchases = EXPECTED_DEFAULT_PURCHASES,
                    Discount = EXPECTED_DEFAULT_DISCOUNT
                });

            var repository = CreateRepository();
            var buyer = new Buyer { User = new User { UserId = userId } };
            buyer.TotalSpending = EXPECTED_TOTAL_SPENDING;
            buyer.NumberOfPurchases = EXPECTED_NUMBER_OF_PURCHASES;

            // Act
            await repository.UpdateAfterPurchase(buyer);

            // Assert
            var updatedBuyer = new Buyer { User = new User { UserId = userId } };
            await repository.LoadBuyerInfo(updatedBuyer);
            Assert.Equal(EXPECTED_TOTAL_SPENDING, updatedBuyer.TotalSpending);
            Assert.Equal(EXPECTED_NUMBER_OF_PURCHASES, updatedBuyer.NumberOfPurchases);
        }

        [Fact]
        public async Task RemoveWishlistItem_ShouldRemoveItem_WhenValidIds()
        {
            // Arrange
            var uniqueId = GenerateUniqueId();
            
            // Create buyer
            var buyerId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"buyer_{uniqueId}", 
                    Email = $"buyer_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer 
                },
                returnId: true);

            var addressId = await SeedDataAsync(
                "INSERT INTO BuyerAddress (StreetLine, City, Country, PostalCode) VALUES (@Street, @City, @Country, @Code)",
                new { 
                    Street = $"123 Test St {uniqueId}", 
                    City = "Testville", 
                    Country = "Testland", 
                    Code = "12345" 
                },
                returnId: true);

            await SeedDataAsync(
                "INSERT INTO Buyers (UserId, FirstName, LastName, BillingAddressId, ShippingAddressId, UseSameAddress, Badge, TotalSpending, NumberOfPurchases, Discount) " +
                "VALUES (@UserId, @FName, @LName, @BillingId, @ShippingId, @SameAddr, @Badge, @Spending, @Purchases, @Discount)",
                new
                {
                    UserId = buyerId,
                    FName = "Buyer",
                    LName = "Test",
                    BillingId = addressId,
                    ShippingId = addressId,
                    SameAddr = true,
                    Badge = "Bronze",
                    Spending = 0.0m,
                    Purchases = 0,
                    Discount = 0.0m
                });

            // Create seller
            var sellerId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"seller_{uniqueId}", 
                    Email = $"seller_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Seller 
                },
                returnId: true);

            await SeedDataAsync(
                "INSERT INTO Sellers (UserId, Username, StoreName, StoreDescription, StoreAddress, FollowersCount, TrustScore) " +
                "VALUES (@UserId, @Username, @StoreName, @StoreDescription, @StoreAddress, @FollowersCount, @TrustScore)",
                new
                {
                    UserId = sellerId,
                    Username = $"seller_{uniqueId}",
                    StoreName = "Test Store",
                    StoreDescription = "Test Description",
                    StoreAddress = "Test Address",
                    FollowersCount = 0,
                    TrustScore = 0.0
                });

            // Create product
            var productId = await SeedDataAsync(
                "INSERT INTO Products (SellerId, ProductName, ProductDescription, ProductPrice, ProductStock) " +
                "VALUES (@SellerId, @ProductName, @ProductDescription, @ProductPrice, @ProductStock)",
                new
                {
                    SellerId = sellerId,
                    ProductName = "Test Product",
                    ProductDescription = "Test Description",
                    ProductPrice = 10.0,
                    ProductStock = 100
                },
                returnId: true);

            // Add product to wishlist
            await SeedDataAsync(
                "INSERT INTO BuyerWishlistItems (BuyerId, ProductId) VALUES (@BuyerId, @ProductId)",
                new { BuyerId = buyerId, ProductId = productId });

            var repository = CreateRepository();

            // Act
            await repository.RemoveWishilistItem(buyerId, productId);

            // Assert
            var wishlist = await repository.GetWishlist(buyerId);
            Assert.NotNull(wishlist);
            Assert.DoesNotContain(wishlist.Items, item => item.ProductId == productId);
        }

        [Fact]
        public async Task ReadBuyerLinkage_ShouldReturnLinkage_WhenValidIds()
        {
            // Arrange
            var uniqueId = GenerateUniqueId();
            
            // Create first buyer
            var buyerId1 = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"buyer1_{uniqueId}", 
                    Email = $"buyer1_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer
                },
                returnId: true);

            var addressId1 = await SeedDataAsync(
                "INSERT INTO BuyerAddress (StreetLine, City, Country, PostalCode) VALUES (@Street, @City, @Country, @Code)",
                new { 
                    Street = $"123 Test St {uniqueId}", 
                    City = "Testville", 
                    Country = "Testland", 
                    Code = "12345" 
                },
                returnId: true);

            await SeedDataAsync(
                "INSERT INTO Buyers (UserId, FirstName, LastName, BillingAddressId, ShippingAddressId, UseSameAddress, Badge, TotalSpending, NumberOfPurchases, Discount) " +
                "VALUES (@UserId, @FName, @LName, @BillingId, @ShippingId, @SameAddr, @Badge, @Spending, @Purchases, @Discount)",
                new
                {
                    UserId = buyerId1,
                    FName = "Buyer1",
                    LName = "Test",
                    BillingId = addressId1,
                    ShippingId = addressId1,
                    SameAddr = true,
                    Badge = "Bronze",
                    Spending = 0.0m,
                    Purchases = 0,
                    Discount = 0.0m
                });

            // Create second buyer
            var buyerId2 = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"buyer2_{uniqueId}", 
                    Email = $"buyer2_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer
                },
                returnId: true);

            var addressId2 = await SeedDataAsync(
                "INSERT INTO BuyerAddress (StreetLine, City, Country, PostalCode) VALUES (@Street, @City, @Country, @Code)",
                new { 
                    Street = $"456 Test St {uniqueId}", 
                    City = "Testville", 
                    Country = "Testland", 
                    Code = "12345" 
                },
                returnId: true);

            await SeedDataAsync(
                "INSERT INTO Buyers (UserId, FirstName, LastName, BillingAddressId, ShippingAddressId, UseSameAddress, Badge, TotalSpending, NumberOfPurchases, Discount) " +
                "VALUES (@UserId, @FName, @LName, @BillingId, @ShippingId, @SameAddr, @Badge, @Spending, @Purchases, @Discount)",
                new
                {
                    UserId = buyerId2,
                    FName = "Buyer2",
                    LName = "Test",
                    BillingId = addressId2,
                    ShippingId = addressId2,
                    SameAddr = true,
                    Badge = "Bronze",
                    Spending = 0.0m,
                    Purchases = 0,
                    Discount = 0.0m
                });

            var repository = CreateRepository();

            // Act
            await repository.CreateLinkageRequest(buyerId1, buyerId2);
            var linkages = await repository.GetBuyerLinkages(buyerId1);

            // Assert
            Assert.NotNull(linkages);
            Assert.Contains(linkages, l => l.Buyer.Id == buyerId2 && l.Status == BuyerLinkageStatus.PendingSelf);
        }

        [Fact]
        public async Task FindBuyersWithShippingAddress_ShouldReturnBuyers_WhenAddressMatches()
        {
            // Arrange
            var uniqueId = GenerateUniqueId();
            
            // Create first buyer with matching address
            var buyerId1 = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"buyer1_{uniqueId}", 
                    Email = $"buyer1_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer 
                },
                returnId: true);

            var addressId1 = await SeedDataAsync(
                "INSERT INTO BuyerAddress (StreetLine, City, Country, PostalCode) VALUES (@Street, @City, @Country, @Code)",
                new { 
                    Street = $"123 Test St {uniqueId}", 
                    City = "Testville", 
                    Country = "Testland", 
                    Code = "12345" 
                },
                returnId: true);

            await SeedDataAsync(
                "INSERT INTO Buyers (UserId, FirstName, LastName, BillingAddressId, ShippingAddressId, UseSameAddress, Badge, TotalSpending, NumberOfPurchases, Discount) " +
                "VALUES (@UserId, @FName, @LName, @BillingId, @ShippingId, @SameAddr, @Badge, @Spending, @Purchases, @Discount)",
                new
                {
                    UserId = buyerId1,
                    FName = "Buyer1",
                    LName = "Test",
                    BillingId = addressId1,
                    ShippingId = addressId1,
                    SameAddr = true,
                    Badge = "Bronze",
                    Spending = 0.0m,
                    Purchases = 0,
                    Discount = 0.0m
                });

            // Create second buyer with different address
            var buyerId2 = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"buyer2_{uniqueId}", 
                    Email = $"buyer2_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer 
                },
                returnId: true);

            var addressId2 = await SeedDataAsync(
                "INSERT INTO BuyerAddress (StreetLine, City, Country, PostalCode) VALUES (@Street, @City, @Country, @Code)",
                new { 
                    Street = $"456 Different St {uniqueId}", 
                    City = "Otherville", 
                    Country = "Otherland", 
                    Code = "67890" 
                },
                returnId: true);

            await SeedDataAsync(
                "INSERT INTO Buyers (UserId, FirstName, LastName, BillingAddressId, ShippingAddressId, UseSameAddress, Badge, TotalSpending, NumberOfPurchases, Discount) " +
                "VALUES (@UserId, @FName, @LName, @BillingId, @ShippingId, @SameAddr, @Badge, @Spending, @Purchases, @Discount)",
                new
                {
                    UserId = buyerId2,
                    FName = "Buyer2",
                    LName = "Test",
                    BillingId = addressId2,
                    ShippingId = addressId2,
                    SameAddr = true,
                    Badge = "Bronze",
                    Spending = 0.0m,
                    Purchases = 0,
                    Discount = 0.0m
                });

            var repository = CreateRepository();
            var searchAddress = new Address
            {
                StreetLine = $"123 Test St {uniqueId}",
                City = "Testville",
                Country = "Testland",
                PostalCode = "12345"
            };

            // Act
            var buyers = await repository.FindBuyersWithShippingAddress(searchAddress);

            // Assert
            Assert.NotNull(buyers);
            Assert.Single(buyers);
            Assert.Equal(buyerId1, buyers.First().Id);
        }

        [Fact]
        public async Task FindBuyersWithShippingAddress_ShouldReturnEmptyList_WhenNoMatches()
        {
            // Arrange
            var uniqueId = GenerateUniqueId();
            
            // Create a buyer with a specific address
            var buyerId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"buyer_{uniqueId}", 
                    Email = $"buyer_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer 
                },
                returnId: true);

            var addressId = await SeedDataAsync(
                "INSERT INTO BuyerAddress (StreetLine, City, Country, PostalCode) VALUES (@Street, @City, @Country, @Code)",
                new { 
                    Street = $"123 Test St {uniqueId}", 
                    City = "Testville", 
                    Country = "Testland", 
                    Code = "12345" 
                },
                returnId: true);

            await SeedDataAsync(
                "INSERT INTO Buyers (UserId, FirstName, LastName, BillingAddressId, ShippingAddressId, UseSameAddress, Badge, TotalSpending, NumberOfPurchases, Discount) " +
                "VALUES (@UserId, @FName, @LName, @BillingId, @ShippingId, @SameAddr, @Badge, @Spending, @Purchases, @Discount)",
                new
                {
                    UserId = buyerId,
                    FName = "Buyer",
                    LName = "Test",
                    BillingId = addressId,
                    ShippingId = addressId,
                    SameAddr = true,
                    Badge = "Bronze",
                    Spending = 0.0m,
                    Purchases = 0,
                    Discount = 0.0m
                });

            var repository = CreateRepository();
            var searchAddress = new Address
            {
                StreetLine = "Non-existent Street",
                City = "Non-existent City",
                Country = "Non-existent Country",
                PostalCode = "00000"
            };

            // Act
            var buyers = await repository.FindBuyersWithShippingAddress(searchAddress);

            // Assert
            Assert.NotNull(buyers);
            Assert.Empty(buyers);
        }

        [Fact]
        public async Task FindBuyersWithShippingAddress_ShouldBeCaseInsensitive()
        {
            // Arrange
            var uniqueId = GenerateUniqueId();
            
            // Create a buyer with a specific address
            var buyerId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"buyer_{uniqueId}", 
                    Email = $"buyer_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer 
                },
                returnId: true);

            var addressId = await SeedDataAsync(
                "INSERT INTO BuyerAddress (StreetLine, City, Country, PostalCode) VALUES (@Street, @City, @Country, @Code)",
                new { 
                    Street = $"123 Test St {uniqueId}", 
                    City = "Testville", 
                    Country = "Testland", 
                    Code = "12345" 
                },
                returnId: true);

            await SeedDataAsync(
                "INSERT INTO Buyers (UserId, FirstName, LastName, BillingAddressId, ShippingAddressId, UseSameAddress, Badge, TotalSpending, NumberOfPurchases, Discount) " +
                "VALUES (@UserId, @FName, @LName, @BillingId, @ShippingId, @SameAddr, @Badge, @Spending, @Purchases, @Discount)",
                new
                {
                    UserId = buyerId,
                    FName = "Buyer",
                    LName = "Test",
                    BillingId = addressId,
                    ShippingId = addressId,
                    SameAddr = true,
                    Badge = "Bronze",
                    Spending = 0.0m,
                    Purchases = 0,
                    Discount = 0.0m
                });

            var repository = CreateRepository();
            var searchAddress = new Address
            {
                StreetLine = $"123 TEST ST {uniqueId}",
                City = "TESTVILLE",
                Country = "TESTLAND",
                PostalCode = "12345"
            };

            // Act
            var buyers = await repository.FindBuyersWithShippingAddress(searchAddress);

            // Assert
            Assert.NotNull(buyers);
            Assert.Single(buyers);
            Assert.Equal(buyerId, buyers.First().Id);
        }

        [Fact]
        public async Task LoadBuyerInfo_ShouldHandleNullReader_WhenBuyerNotFound()
        {
            // Arrange
            const string EXPECTED_EMPTY_STRING = "";
            const BuyerBadge EXPECTED_DEFAULT_BADGE = BuyerBadge.BRONZE;
            const decimal EXPECTED_DEFAULT_SPENDING = 0.0m;
            const int EXPECTED_DEFAULT_PURCHASES = 0;
            const decimal EXPECTED_DEFAULT_DISCOUNT = 0.0m;
            const int NON_EXISTENT_BUYER_ID = -1;

            var repository = CreateRepository();
            var buyer = new Buyer { User = new User { UserId = NON_EXISTENT_BUYER_ID } }; // Non-existent buyer ID

            // Act
            await repository.LoadBuyerInfo(buyer);

            // Assert
            Assert.Equal(EXPECTED_EMPTY_STRING, buyer.FirstName);
            Assert.Equal(EXPECTED_EMPTY_STRING, buyer.LastName);
            Assert.Equal(EXPECTED_DEFAULT_BADGE, buyer.Badge);
            Assert.Equal(EXPECTED_DEFAULT_SPENDING, buyer.TotalSpending);
            Assert.Equal(EXPECTED_DEFAULT_PURCHASES, buyer.NumberOfPurchases);
            Assert.Equal(EXPECTED_DEFAULT_DISCOUNT, buyer.Discount);
        }

        [Fact]
        public async Task GetFollowedSellers_ShouldReturnEmptyList_WhenNullFollowingIds()
        {
            // Arrange
            var repository = CreateRepository();

            // Act
            var result = await repository.GetFollowedSellers(null);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetFollowedSellers_ShouldReturnEmptyList_WhenEmptyFollowingIds()
        {
            // Arrange
            var repository = CreateRepository();

            // Act
            var result = await repository.GetFollowedSellers(new List<int>());

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task LoadBuyerInfo_ShouldHandleMissingAddress_WhenAddressNotFound()
        {
            // Arrange
            var repository = CreateRepository();
            var uniqueId = GenerateUniqueId();
            
            // Create user
            var userId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"testuser_{uniqueId}", 
                    Email = $"test_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer 
                },
                returnId: true);

            // First create a valid address
            var addressId = await SeedDataAsync(
                "INSERT INTO BuyerAddress (StreetLine, City, Country, PostalCode) VALUES (@Street, @City, @Country, @Code)",
                new { 
                    Street = $"123 Test St {uniqueId}", 
                    City = "Testville", 
                    Country = "Testland", 
                    Code = "12345" 
                },
                returnId: true);

            // Create buyer with the valid address
            await SeedDataAsync(
                "INSERT INTO Buyers (UserId, FirstName, LastName, BillingAddressId, ShippingAddressId, UseSameAddress, Badge, TotalSpending, NumberOfPurchases, Discount) " +
                "VALUES (@UserId, @FName, @LName, @BillingId, @ShippingId, @SameAddr, @Badge, @Spending, @Purchases, @Discount)",
                new
                {
                    UserId = userId,
                    FName = "Test",
                    LName = "User",
                    BillingId = addressId,
                    ShippingId = addressId,
                    SameAddr = true,
                    Badge = "Bronze",
                    Spending = 0.0m,
                    Purchases = 0,
                    Discount = 0.0m
                });

            // Create a new address to update the buyer with
            var newAddressId = await SeedDataAsync(
                "INSERT INTO BuyerAddress (StreetLine, City, Country, PostalCode) VALUES (@Street, @City, @Country, @Code)",
                new { 
                    Street = $"456 New St {uniqueId}", 
                    City = "Newville", 
                    Country = "Newland", 
                    Code = "67890" 
                },
                returnId: true);

            // Update buyer to use the new address
            await SeedDataAsync(
                "UPDATE Buyers SET BillingAddressId = @NewAddressId, ShippingAddressId = @NewAddressId WHERE UserId = @UserId",
                new { UserId = userId, NewAddressId = newAddressId });

            // Now we can delete the old address
            await SeedDataAsync(
                "DELETE FROM BuyerAddress WHERE Id = @Id",
                new { Id = addressId });

            var buyer = new Buyer { User = new User { UserId = userId } };

            // Act
            await repository.LoadBuyerInfo(buyer);

            // Assert
            Assert.NotNull(buyer.BillingAddress);
            Assert.NotNull(buyer.ShippingAddress);
            Assert.Equal(newAddressId, buyer.BillingAddress.Id);
            Assert.Equal(newAddressId, buyer.ShippingAddress.Id);
        }

        [Fact]
        public async Task LoadBuyerInfo_ShouldHandleCaseInsensitiveBadge_WhenLoadingBuyer()
        {
            // Arrange
            var repository = CreateRepository();
            var userId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { Username = "testuser", Email = "test@test.com", Password = "hashedpass", Role = 2 },
                returnId: true);

            var addressId = await SeedDataAsync(
                "INSERT INTO BuyerAddress (StreetLine, City, Country, PostalCode) VALUES (@Street, @City, @Country, @Code)",
                new { Street = "123 Test St", City = "Testville", Country = "Testland", Code = "12345" },
                returnId: true);

            await SeedDataAsync(
                "INSERT INTO Buyers (UserId, FirstName, LastName, BillingAddressId, ShippingAddressId, UseSameAddress, Badge, TotalSpending, NumberOfPurchases, Discount) " +
                "VALUES (@UserId, @FName, @LName, @BillingId, @ShippingId, @SameAddr, @Badge, @Spending, @Purchases, @Discount)",
                new
                {
                    UserId = userId,
                    FName = "Test",
                    LName = "User",
                    BillingId = addressId,
                    ShippingId = addressId,
                    SameAddr = true,
                    Badge = "silver", // Lowercase badge
                    Spending = 0.0m,
                    Purchases = 0,
                    Discount = 0.0m
                });

            var buyer = new Buyer { User = new User { UserId = userId } };

            // Act
            await repository.LoadBuyerInfo(buyer);

            // Assert
            Assert.Equal(BuyerBadge.SILVER, buyer.Badge);
        }

        [Fact]
        public async Task LoadBuyerInfo_ShouldSetEmptyName_WhenNoRowsReturned()
        {
            // Arrange
            var repository = CreateRepository();
            var uniqueId = GenerateUniqueId();
            
            // Create user but don't create a buyer record
            var userId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"testuser_{uniqueId}", 
                    Email = $"test_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer 
                },
                returnId: true);

            var buyer = new Buyer { User = new User { UserId = userId } };

            // Act
            await repository.LoadBuyerInfo(buyer);

            // Assert
            Assert.Equal(string.Empty, buyer.FirstName);
            Assert.Equal(string.Empty, buyer.LastName);
        }

        [Fact]
        public async Task LoadBuyerInfo_ShouldSetDefaultBadgeAndStats_WhenNoRowsReturned()
        {
            // Arrange
            var repository = CreateRepository();
            var uniqueId = GenerateUniqueId();
            
            // Create user but don't create a buyer record
            var userId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"testuser_{uniqueId}", 
                    Email = $"test_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer 
                },
                returnId: true);

            var buyer = new Buyer { User = new User { UserId = userId } };

            // Act
            await repository.LoadBuyerInfo(buyer);

            // Assert
            int expectedTotalSpending = 0;
            int expectedNumberOfPurchases = 0;
            int expectedDiscount = 0;
            Assert.Equal(BuyerBadge.BRONZE, buyer.Badge);
            Assert.Equal(expectedTotalSpending, buyer.TotalSpending);
            Assert.Equal(expectedNumberOfPurchases, buyer.NumberOfPurchases);
            Assert.Equal(expectedDiscount, buyer.Discount);
        }

        [Fact]
        public async Task LoadBuyerInfo_ShouldSetEmptyBillingAddress_WhenNoRowsReturned()
        {
            // Arrange
            var repository = CreateRepository();
            var uniqueId = GenerateUniqueId();
            
            // Create user but don't create a buyer record
            var userId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"testuser_{uniqueId}", 
                    Email = $"test_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer 
                },
                returnId: true);

            var buyer = new Buyer { User = new User { UserId = userId } };

            // Act
            await repository.LoadBuyerInfo(buyer);

            // Assert
            int expectedBillingAddressId = 0;
            Assert.NotNull(buyer.BillingAddress);
            Assert.Equal(string.Empty, buyer.BillingAddress.StreetLine);
            Assert.Equal(string.Empty, buyer.BillingAddress.City);
            Assert.Equal(string.Empty, buyer.BillingAddress.Country);
            Assert.Equal(string.Empty, buyer.BillingAddress.PostalCode);
            Assert.Equal(expectedBillingAddressId, buyer.BillingAddress.Id);
        }

        [Fact]
        public async Task LoadBuyerInfo_ShouldSetEmptyShippingAddress_WhenNoRowsReturned()
        {
            // Arrange
            var repository = CreateRepository();
            var uniqueId = GenerateUniqueId();
            
            // Create user but don't create a buyer record
            var userId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"testuser_{uniqueId}", 
                    Email = $"test_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer 
                },
                returnId: true);

            var buyer = new Buyer { User = new User { UserId = userId } };

            // Act
            await repository.LoadBuyerInfo(buyer);

            // Assert
            int expectedShippingAddressId = 0;
            Assert.NotNull(buyer.ShippingAddress);
            Assert.Equal(string.Empty, buyer.ShippingAddress.StreetLine);
            Assert.Equal(string.Empty, buyer.ShippingAddress.City);
            Assert.Equal(string.Empty, buyer.ShippingAddress.Country);
            Assert.Equal(string.Empty, buyer.ShippingAddress.PostalCode);
            Assert.Equal(expectedShippingAddressId, buyer.ShippingAddress.Id);
        }

        [Fact]
        public async Task LoadBuyerInfo_ShouldSetEmptyNameFields_WhenQueryReturnsEmpty()
        {
            // Arrange
            var repository = CreateRepository();
            var uniqueId = GenerateUniqueId();
            
            // Create user but don't create a buyer record
            var userId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"testuser_{uniqueId}", 
                    Email = $"test_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer
                },
                returnId: true);

            // Create a buyer object with the user ID but no corresponding record in the Buyers table
            var buyer = new Buyer { User = new User { UserId = userId } };

            // Act
            await repository.LoadBuyerInfo(buyer);

            // Assert
            Assert.Equal(string.Empty, buyer.FirstName);
            Assert.Equal(string.Empty, buyer.LastName);
        }

        [Fact]
        public async Task LoadBuyerInfo_ShouldSetDefaultBuyerProperties_WhenQueryReturnsEmpty()
        {
            // Arrange
            var repository = CreateRepository();
            var uniqueId = GenerateUniqueId();
            
            // Create user but don't create a buyer record
            var userId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"testuser_{uniqueId}", 
                    Email = $"test_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer
                },
                returnId: true);

            // Create a buyer object with the user ID but no corresponding record in the Buyers table
            var buyer = new Buyer { User = new User { UserId = userId } };

            // Act
            await repository.LoadBuyerInfo(buyer);

            // Assert
            int expectedTotalSpending = 0;
            int expectedNumberOfPurchases = 0;
            int expectedDiscount = 0;
            
            Assert.Equal(BuyerBadge.BRONZE, buyer.Badge);
            Assert.Equal(expectedTotalSpending, buyer.TotalSpending);
            Assert.Equal(expectedNumberOfPurchases, buyer.NumberOfPurchases);
            Assert.Equal(expectedDiscount, buyer.Discount);
        }

        [Fact]
        public async Task LoadBuyerInfo_ShouldSetEmptyBillingAddressProperties_WhenQueryReturnsEmpty()
        {
            // Arrange
            var repository = CreateRepository();
            var uniqueId = GenerateUniqueId();
            
            // Create user but don't create a buyer record
            var userId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"testuser_{uniqueId}", 
                    Email = $"test_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer
                },
                returnId: true);

            // Create a buyer object with the user ID but no corresponding record in the Buyers table
            var buyer = new Buyer { User = new User { UserId = userId } };

            // Act
            await repository.LoadBuyerInfo(buyer);

            // Assert
            int expectedBillingAddressId = 0;
            Assert.NotNull(buyer.BillingAddress);
            Assert.Equal(string.Empty, buyer.BillingAddress.StreetLine);
            Assert.Equal(string.Empty, buyer.BillingAddress.City);
            Assert.Equal(string.Empty, buyer.BillingAddress.Country);
            Assert.Equal(string.Empty, buyer.BillingAddress.PostalCode);
            Assert.Equal(expectedBillingAddressId, buyer.BillingAddress.Id);
        }

        [Fact]
        public async Task LoadBuyerInfo_ShouldSetEmptyShippingAddressProperties_WhenQueryReturnsEmpty()
        {
            // Arrange
            var repository = CreateRepository();
            var uniqueId = GenerateUniqueId();
            
            // Create user but don't create a buyer record
            var userId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"testuser_{uniqueId}", 
                    Email = $"test_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer
                },
                returnId: true);

            // Create a buyer object with the user ID but no corresponding record in the Buyers table
            var buyer = new Buyer { User = new User { UserId = userId } };

            // Act
            await repository.LoadBuyerInfo(buyer);

            // Assert
            int expectedShippingAddressId = 0;
            Assert.NotNull(buyer.ShippingAddress);
            Assert.Equal(string.Empty, buyer.ShippingAddress.StreetLine);
            Assert.Equal(string.Empty, buyer.ShippingAddress.City);
            Assert.Equal(string.Empty, buyer.ShippingAddress.Country);
            Assert.Equal(string.Empty, buyer.ShippingAddress.PostalCode);
            Assert.Equal(expectedShippingAddressId, buyer.ShippingAddress.Id);
        }

        [Fact]
        public async Task LoadAddress_ShouldReturnNull_WhenNoRowsReturned()
        {
            // Arrange
            var repository = CreateRepository();
            var uniqueId = GenerateUniqueId();
            
            // Create a non-existent address ID
            var nonExistentAddressId = 999999;

            // Create and open connection
            using var connection = new SqlConnection(_fixture.TestConnectionString);
            await connection.OpenAsync();

            // Act
            var address = await repository.LoadAddress(nonExistentAddressId, connection);

            // Assert
            Assert.Null(address);
        }

        [Fact]
        public async Task GetFollowingUsersIds_ShouldReturnMultipleSellers_WhenBuyerFollowsMultipleSellers()
        {
            // Arrange
            var repository = CreateRepository();
            var uniqueId = GenerateUniqueId();
            
            // Create buyer
            var buyerId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"buyer_{uniqueId}", 
                    Email = $"buyer_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer 
                },
                returnId: true);

            var addressId = await SeedDataAsync(
                "INSERT INTO BuyerAddress (StreetLine, City, Country, PostalCode) VALUES (@Street, @City, @Country, @Code)",
                new { 
                    Street = $"123 Test St {uniqueId}", 
                    City = "Testville", 
                    Country = "Testland", 
                    Code = "12345" 
                },
                returnId: true);

            await SeedDataAsync(
                "INSERT INTO Buyers (UserId, FirstName, LastName, BillingAddressId, ShippingAddressId, UseSameAddress, Badge, TotalSpending, NumberOfPurchases, Discount) " +
                "VALUES (@UserId, @FName, @LName, @BillingId, @ShippingId, @SameAddr, @Badge, @Spending, @Purchases, @Discount)",
                new
                {
                    UserId = buyerId,
                    FName = "Test",
                    LName = "Buyer",
                    BillingId = addressId,
                    ShippingId = addressId,
                    SameAddr = true,
                    Badge = "Bronze",
                    Spending = 0.0m,
                    Purchases = 0,
                    Discount = 0.0m
                });

            // Create multiple sellers
            var sellerId1 = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"seller1_{uniqueId}", 
                    Email = $"seller1_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Seller 
                },
                returnId: true);

            var sellerId2 = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"seller2_{uniqueId}", 
                    Email = $"seller2_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Seller 
                },
                returnId: true);

            // Create seller records
            await SeedDataAsync(
                "INSERT INTO Sellers (UserId, Username, StoreName, StoreDescription, StoreAddress, FollowersCount, TrustScore) " +
                "VALUES (@UserId, @Username, @StoreName, @Description, @Address, @Followers, @TrustScore)",
                new
                {
                    UserId = sellerId1,
                    Username = $"seller1_{uniqueId}",
                    StoreName = "Store 1",
                    Description = "Description 1",
                    Address = "Address 1",
                    Followers = 0,
                    TrustScore = 0.0f
                });

            await SeedDataAsync(
                "INSERT INTO Sellers (UserId, Username, StoreName, StoreDescription, StoreAddress, FollowersCount, TrustScore) " +
                "VALUES (@UserId, @Username, @StoreName, @Description, @Address, @Followers, @TrustScore)",
                new
                {
                    UserId = sellerId2,
                    Username = $"seller2_{uniqueId}",
                    StoreName = "Store 2",
                    Description = "Description 2",
                    Address = "Address 2",
                    Followers = 0,
                    TrustScore = 0.0f
                });

            // Create following relationships
            await SeedDataAsync(
                "INSERT INTO Following (FollowerID, FollowedID) VALUES (@FollowerId, @FollowedId)",
                new { FollowerId = buyerId, FollowedId = sellerId1 });

            await SeedDataAsync(
                "INSERT INTO Following (FollowerID, FollowedID) VALUES (@FollowerId, @FollowedId)",
                new { FollowerId = buyerId, FollowedId = sellerId2 });

            // Act
            var followingIds = await repository.GetFollowingUsersIds(buyerId);

            // Assert
            int expectedCount = 2;
            Assert.NotNull(followingIds);
            Assert.Equal(expectedCount, followingIds.Count);
            Assert.Contains(sellerId1, followingIds);
            Assert.Contains(sellerId2, followingIds);
        }

        [Fact]
        public async Task SaveInfo_ShouldPersistDifferentAddresses_WhenUseSameAddressIsFalse()
        {
            // Arrange
            var repository = CreateRepository();
            var uniqueId = GenerateUniqueId();
            
            // Create user
            var userId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"testuser_{uniqueId}", 
                    Email = $"test_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer
                },
                returnId: true);

            // Create buyer with different addresses
            var buyer = new Buyer
            {
                User = new User { UserId = userId },
                FirstName = "Test",
                LastName = "User",
                UseSameAddress = false,
                BillingAddress = new Address
                {
                    StreetLine = $"123 Main St {uniqueId}",
                    City = "Test City",
                    Country = "Test Country",
                    PostalCode = "12345"
                },
                ShippingAddress = new Address
                {
                    StreetLine = $"456 Shipping St {uniqueId}",
                    City = "Shipping City",
                    Country = "Shipping Country",
                    PostalCode = "67890"
                },
                Badge = BuyerBadge.BRONZE,
                TotalSpending = 0.0m,
                NumberOfPurchases = 0,
                Discount = 0.0m
            };

            // Act
            await repository.SaveInfo(buyer);

            // Assert
            var loadedBuyer = new Buyer { User = new User { UserId = userId } };
            await repository.LoadBuyerInfo(loadedBuyer);

            // Verify addresses are different
            string expectedBillingAddress = $"123 Main St {uniqueId}";
            string expectedShippingAddress = $"456 Shipping St {uniqueId}";
            Assert.NotNull(loadedBuyer.BillingAddress);
            Assert.NotNull(loadedBuyer.ShippingAddress);
            Assert.Equal(expectedBillingAddress, loadedBuyer.BillingAddress.StreetLine);
            Assert.Equal(expectedShippingAddress, loadedBuyer.ShippingAddress.StreetLine);
            Assert.False(loadedBuyer.UseSameAddress);
            Assert.NotEqual(loadedBuyer.BillingAddress.Id, loadedBuyer.ShippingAddress.Id);
        }

        [Fact]
        public async Task SaveInfo_ShouldHandleAddressesAndFollowing_WhenBuyerExists()
        {
            // Arrange
            var repository = CreateRepository();
            var uniqueId = GenerateUniqueId();
            
            // Create user
            var userId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"testuser_{uniqueId}", 
                    Email = $"test_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer
                },
                returnId: true);

            // Create seller user
            var sellerUserId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"seller_{uniqueId}", 
                    Email = $"seller_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Seller 
                },
                returnId: true);

            // Create seller record
            await SeedDataAsync(
                "INSERT INTO Sellers (UserId, Username, StoreName, StoreDescription, StoreAddress, FollowersCount, TrustScore) " +
                "VALUES (@UserId, @Username, @StoreName, @StoreDescription, @StoreAddress, @FollowersCount, @TrustScore)",
                new
                {
                    UserId = sellerUserId,
                    Username = "Seller",
                    StoreName = "Test Store",
                    StoreDescription = "Test Description",
                    StoreAddress = "Test Address",
                    FollowersCount = 0,
                    TrustScore = 0.0
                });

            // Create buyer with different addresses
            var buyer = new Buyer
            {
                User = new User { UserId = userId },
                FirstName = "Test",
                LastName = "User",
                UseSameAddress = false,
                BillingAddress = new Address
                {
                    StreetLine = $"123 Main St {uniqueId}",
                    City = "Test City",
                    Country = "Test Country",
                    PostalCode = "12345"
                },
                ShippingAddress = new Address
                {
                    StreetLine = $"456 Shipping St {uniqueId}",
                    City = "Shipping City",
                    Country = "Shipping Country",
                    PostalCode = "67890"
                },
                Badge = BuyerBadge.BRONZE,
                TotalSpending = 0.0m,
                NumberOfPurchases = 0,
                Discount = 0.0m
            };

            // Act
            await repository.SaveInfo(buyer);

            // Add seller to following
            await SeedDataAsync(
                "INSERT INTO Following (FollowerID, FollowedID) VALUES (@FollowerID, @FollowedID)",
                new { FollowerID = userId, FollowedID = sellerUserId });

            // Assert
            var loadedBuyer = new Buyer { User = new User { UserId = userId } };
            await repository.LoadBuyerInfo(loadedBuyer);

            // Verify addresses
            string expectedBillingAddress = $"123 Main St {uniqueId}";
            string expectedShippingAddress = $"456 Shipping St {uniqueId}";
            Assert.NotNull(loadedBuyer.BillingAddress);
            Assert.NotNull(loadedBuyer.ShippingAddress);
            Assert.Equal(expectedBillingAddress, loadedBuyer.BillingAddress.StreetLine);
            Assert.Equal(expectedShippingAddress, loadedBuyer.ShippingAddress.StreetLine);
            Assert.False(loadedBuyer.UseSameAddress);
            Assert.NotEqual(loadedBuyer.BillingAddress.Id, loadedBuyer.ShippingAddress.Id);

            // Verify following
            Assert.Contains(sellerUserId, loadedBuyer.FollowingUsersIds);
        }

        [Fact]
        public async Task SaveInfo_ShouldPersistSameAddress_WhenUseSameAddressIsTrue()
        {
            // Arrange
            var repository = CreateRepository();
            var uniqueId = GenerateUniqueId();

            // Create user
            var userId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"testuser_{uniqueId}", 
                    Email = $"test_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer
                },
                returnId: true);

            // Create buyer with same address
            var buyer = new Buyer
            {
                User = new User { UserId = userId },
                FirstName = "Test",
                LastName = "User",
                UseSameAddress = true,
                BillingAddress = new Address
                {
                    StreetLine = $"123 Main St {uniqueId}",
                    City = "Test City",
                    Country = "Test Country",
                    PostalCode = "12345"
                },
                Badge = BuyerBadge.BRONZE,
                TotalSpending = 0.0m,
                NumberOfPurchases = 0,
                Discount = 0.0m
            };

            // Act
            await repository.SaveInfo(buyer);

            // Assert
            var loadedBuyer = new Buyer { User = new User { UserId = userId } };
            await repository.LoadBuyerInfo(loadedBuyer);

            // Verify addresses are the same
            string expectedBillingAddress = $"123 Main St {uniqueId}";
            string expectedShippingAddress = $"123 Main St {uniqueId}";
            Assert.NotNull(loadedBuyer.BillingAddress);
            Assert.NotNull(loadedBuyer.ShippingAddress);
            Assert.Equal(expectedBillingAddress, loadedBuyer.BillingAddress.StreetLine);
            Assert.Equal(expectedShippingAddress, loadedBuyer.ShippingAddress.StreetLine);
            Assert.True(loadedBuyer.UseSameAddress);
            Assert.Same(loadedBuyer.BillingAddress, loadedBuyer.ShippingAddress);
        }

        [Fact]
        public async Task CreateBuyer_ShouldPersistDifferentAddresses_WhenUseSameAddressIsFalse()
        {
            // Arrange
            var repository = CreateRepository();
            var uniqueId = GenerateUniqueId();

            // Create user
            var userId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"newbuyer_diff_{uniqueId}", 
                    Email = $"newdiff_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer 
                },
                returnId: true);

            var buyer = new Buyer
            {
                User = new User { UserId = userId },
                FirstName = "New",
                LastName = "BuyerDiff",
                UseSameAddress = false, // Key condition for this test
                BillingAddress = new Address
                {
                    StreetLine = $"100 Billing St {uniqueId}",
                    City = "Billing City",
                    Country = "Billing Country",
                    PostalCode = "11111"
                },
                ShippingAddress = new Address // Different shipping address
                {
                    StreetLine = $"200 Shipping St {uniqueId}",
                    City = "Shipping City",
                    Country = "Shipping Country",
                    PostalCode = "22222"
                },
                // Initial stats are set by CreateBuyer, no need to set them here
            };

            // Act
            await repository.CreateBuyer(buyer);

            // Load the data back to verify in each test
            var loadedBuyer = new Buyer { User = new User { UserId = userId } };
            await repository.LoadBuyerInfo(loadedBuyer);
            
            // Assert basic properties
            VerifyBuyerBasicProperties(loadedBuyer, uniqueId);
            
            // Assert addresses
            VerifyBuyerAddresses(loadedBuyer, uniqueId);
            
            // Assert buyer stats
            VerifyBuyerStats(loadedBuyer);
        }
        
        [Fact]
        public async Task CreateBuyer_ShouldSetCorrectBasicProperties_WhenUseSameAddressIsFalse()
        {
            // Arrange
            var repository = CreateRepository();
            var uniqueId = GenerateUniqueId();

            // Create user
            var userId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"newbuyer_diff_{uniqueId}", 
                    Email = $"newdiff_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer 
                },
                returnId: true);

            var buyer = new Buyer
            {
                User = new User { UserId = userId },
                FirstName = "New",
                LastName = "BuyerDiff",
                UseSameAddress = false,
                BillingAddress = new Address
                {
                    StreetLine = $"100 Billing St {uniqueId}",
                    City = "Billing City",
                    Country = "Billing Country",
                    PostalCode = "11111"
                },
                ShippingAddress = new Address
                {
                    StreetLine = $"200 Shipping St {uniqueId}",
                    City = "Shipping City",
                    Country = "Shipping Country",
                    PostalCode = "22222"
                }
            };

            // Act
            await repository.CreateBuyer(buyer);

            // Assert
            var loadedBuyer = new Buyer { User = new User { UserId = userId } };
            await repository.LoadBuyerInfo(loadedBuyer);

            string expectedFirstName = "New";
            string expectedLastName = "BuyerDiff";

            Assert.Equal(expectedFirstName, loadedBuyer.FirstName);
            Assert.Equal(expectedLastName, loadedBuyer.LastName);
            Assert.False(loadedBuyer.UseSameAddress);
        }
        
        [Fact]
        public async Task CreateBuyer_ShouldPersistBillingAddress_WhenUseSameAddressIsFalse()
        {
            // Arrange
            var repository = CreateRepository();
            var uniqueId = GenerateUniqueId();

            // Create user
            var userId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"newbuyer_diff_{uniqueId}", 
                    Email = $"newdiff_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer 
                },
                returnId: true);

            var buyer = new Buyer
            {
                User = new User { UserId = userId },
                FirstName = "New",
                LastName = "BuyerDiff",
                UseSameAddress = false,
                BillingAddress = new Address
                {
                    StreetLine = $"100 Billing St {uniqueId}",
                    City = "Billing City",
                    Country = "Billing Country",
                    PostalCode = "11111"
                },
                ShippingAddress = new Address
                {
                    StreetLine = $"200 Shipping St {uniqueId}",
                    City = "Shipping City",
                    Country = "Shipping Country",
                    PostalCode = "22222"
                }
            };

            // Act
            await repository.CreateBuyer(buyer);

            // Assert
            var loadedBuyer = new Buyer { User = new User { UserId = userId } };
            await repository.LoadBuyerInfo(loadedBuyer);

            string expectedBillingAddressStreetLine = $"100 Billing St {uniqueId}";
            string expectedBillingAddressCity = "Billing City";
            int expectedBillingAddressId = 0;
            
            Assert.NotNull(loadedBuyer.BillingAddress);
            Assert.Equal(expectedBillingAddressStreetLine, loadedBuyer.BillingAddress.StreetLine);
            Assert.Equal(expectedBillingAddressCity, loadedBuyer.BillingAddress.City);
            Assert.NotEqual(expectedBillingAddressId, loadedBuyer.BillingAddress.Id);
        }
        
        [Fact]
        public async Task CreateBuyer_ShouldPersistShippingAddress_WhenUseSameAddressIsFalse()
        {
            // Arrange
            var repository = CreateRepository();
            var uniqueId = GenerateUniqueId();

            // Create user
            var userId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"newbuyer_diff_{uniqueId}", 
                    Email = $"newdiff_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer 
                },
                returnId: true);

            var buyer = new Buyer
            {
                User = new User { UserId = userId },
                FirstName = "New",
                LastName = "BuyerDiff",
                UseSameAddress = false,
                BillingAddress = new Address
                {
                    StreetLine = $"100 Billing St {uniqueId}",
                    City = "Billing City",
                    Country = "Billing Country",
                    PostalCode = "11111"
                },
                ShippingAddress = new Address
                {
                    StreetLine = $"200 Shipping St {uniqueId}",
                    City = "Shipping City",
                    Country = "Shipping Country",
                    PostalCode = "22222"
                }
            };

            // Act
            await repository.CreateBuyer(buyer);

            // Assert
            var loadedBuyer = new Buyer { User = new User { UserId = userId } };
            await repository.LoadBuyerInfo(loadedBuyer);

            string expectedShippingAddressStreetLine = $"200 Shipping St {uniqueId}";
            string expectedShippingAddressCity = "Shipping City";
            int expectedShippingAddressId = 0;
            
            Assert.NotNull(loadedBuyer.ShippingAddress);
            Assert.Equal(expectedShippingAddressStreetLine, loadedBuyer.ShippingAddress.StreetLine);
            Assert.Equal(expectedShippingAddressCity, loadedBuyer.ShippingAddress.City);
            Assert.NotEqual(expectedShippingAddressId, loadedBuyer.ShippingAddress.Id);
        }
        
        [Fact]
        public async Task CreateBuyer_ShouldPersistDifferentAddressIds_WhenUseSameAddressIsFalse()
        {
            // Arrange
            var repository = CreateRepository();
            var uniqueId = GenerateUniqueId();

            // Create user
            var userId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"newbuyer_diff_{uniqueId}", 
                    Email = $"newdiff_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer 
                },
                returnId: true);

            var buyer = new Buyer
            {
                User = new User { UserId = userId },
                FirstName = "New",
                LastName = "BuyerDiff",
                UseSameAddress = false,
                BillingAddress = new Address
                {
                    StreetLine = $"100 Billing St {uniqueId}",
                    City = "Billing City",
                    Country = "Billing Country",
                    PostalCode = "11111"
                },
                ShippingAddress = new Address
                {
                    StreetLine = $"200 Shipping St {uniqueId}",
                    City = "Shipping City",
                    Country = "Shipping Country",
                    PostalCode = "22222"
                }
            };

            // Act
            await repository.CreateBuyer(buyer);

            // Assert
            var loadedBuyer = new Buyer { User = new User { UserId = userId } };
            await repository.LoadBuyerInfo(loadedBuyer);
            
            Assert.NotEqual(loadedBuyer.BillingAddress.Id, loadedBuyer.ShippingAddress.Id);
        }
        
        [Fact]
        public async Task CreateBuyer_ShouldSetDefaultStats_WhenCreatingNewBuyer()
        {
            // Arrange
            var repository = CreateRepository();
            var uniqueId = GenerateUniqueId();

            // Create user
            var userId = await SeedDataAsync(
                "INSERT INTO Users (Username, Email, Password, Role) VALUES (@Username, @Email, @Password, @Role)",
                new { 
                    Username = $"newbuyer_diff_{uniqueId}", 
                    Email = $"newdiff_{uniqueId}@test.com", 
                    Password = "hashedpass", 
                    Role = UserRole.Buyer 
                },
                returnId: true);

            var buyer = new Buyer
            {
                User = new User { UserId = userId },
                FirstName = "New",
                LastName = "BuyerDiff",
                UseSameAddress = false,
                BillingAddress = new Address
                {
                    StreetLine = $"100 Billing St {uniqueId}",
                    City = "Billing City",
                    Country = "Billing Country",
                    PostalCode = "11111"
                },
                ShippingAddress = new Address
                {
                    StreetLine = $"200 Shipping St {uniqueId}",
                    City = "Shipping City",
                    Country = "Shipping Country",
                    PostalCode = "22222"
                }
            };

            // Act
            await repository.CreateBuyer(buyer);

            // Assert
            var loadedBuyer = new Buyer { User = new User { UserId = userId } };
            await repository.LoadBuyerInfo(loadedBuyer);

            decimal expectedTotalSpending = 0.0m;
            int expectedNumberOfPurchases = 0;
            decimal expectedDiscount = 0.0m;
            
            Assert.Equal(BuyerBadge.BRONZE, loadedBuyer.Badge);
            Assert.Equal(expectedTotalSpending, loadedBuyer.TotalSpending);
            Assert.Equal(expectedNumberOfPurchases, loadedBuyer.NumberOfPurchases);
            Assert.Equal(expectedDiscount, loadedBuyer.Discount);
        }
        
        private void VerifyBuyerBasicProperties(Buyer loadedBuyer, string uniqueId)
        {
            string expectedFirstName = "New";
            string expectedLastName = "BuyerDiff";

            Assert.Equal(expectedFirstName, loadedBuyer.FirstName);
            Assert.Equal(expectedLastName, loadedBuyer.LastName);
            Assert.False(loadedBuyer.UseSameAddress);
        }
        
        private void VerifyBuyerAddresses(Buyer loadedBuyer, string uniqueId)
        {
            string expectedBillingAddressStreetLine = $"100 Billing St {uniqueId}";
            string expectedBillingAddressCity = "Billing City";
            int expectedBillingAddressId = 0;
            
            string expectedShippingAddressStreetLine = $"200 Shipping St {uniqueId}";
            string expectedShippingAddressCity = "Shipping City";
            int expectedShippingAddressId = 0;
            
            Assert.NotNull(loadedBuyer.BillingAddress);
            Assert.Equal(expectedBillingAddressStreetLine, loadedBuyer.BillingAddress.StreetLine);
            Assert.Equal(expectedBillingAddressCity, loadedBuyer.BillingAddress.City);
            Assert.NotEqual(expectedBillingAddressId, loadedBuyer.BillingAddress.Id);
            
            Assert.NotNull(loadedBuyer.ShippingAddress);
            Assert.Equal(expectedShippingAddressStreetLine, loadedBuyer.ShippingAddress.StreetLine);
            Assert.Equal(expectedShippingAddressCity, loadedBuyer.ShippingAddress.City);
            Assert.NotEqual(expectedShippingAddressId, loadedBuyer.ShippingAddress.Id);
            
            Assert.NotEqual(loadedBuyer.BillingAddress.Id, loadedBuyer.ShippingAddress.Id);
        }
        
        private void VerifyBuyerStats(Buyer loadedBuyer)
        {
            decimal expectedTotalSpending = 0.0m;
            int expectedNumberOfPurchases = 0;
            decimal expectedDiscount = 0.0m;
            
            Assert.Equal(BuyerBadge.BRONZE, loadedBuyer.Badge);
            Assert.Equal(expectedTotalSpending, loadedBuyer.TotalSpending);
            Assert.Equal(expectedNumberOfPurchases, loadedBuyer.NumberOfPurchases);
            Assert.Equal(expectedDiscount, loadedBuyer.Discount);
        }
    }
}
