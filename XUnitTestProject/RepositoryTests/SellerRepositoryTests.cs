namespace XUnitTestProject.RepositoryTests
{
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;
    using Microsoft.Data.SqlClient;
    using Xunit;
    using Moq;
    using Azure.Identity;
    using Windows.UI.Notifications;
    using System.Formats.Asn1;
    using static System.Formats.Asn1.AsnWriter;
    using System.Collections.Generic;

    [Collection("DatabaseCollection")]
    public class SellerRepositoryTests : IDisposable
    {
        private const int maxNotifications = 1000;
        private readonly DatabaseFixture _fixture;
        private readonly ISellerRepository sellerRepository;

        public SellerRepositoryTests(DatabaseFixture fixture)
        {
        }

        public void Dispose()
        {
            using var connection = new SqlConnection(_fixture.TestConnectionString);
            connection.Open();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                delete from Reviews;
                delete from Following;
                delete from BuyerWishlistItems;
                delete from Products;
                delete from Notifications;
                delete from Sellers;
                delete from BuyerLinkage;
                delete from Buyers;
                delete from BuyerAddress;
                delete from Users;
            ";
            command.ExecuteNonQuery();
        }

        private async Task<int> CreateUserAsync()
        {
            using var connection = new SqlConnection(_fixture.TestConnectionString);
            await connection.OpenAsync();
            using var command = connection.CreateCommand();

            command.CommandText = @"
            INSERT INTO Users (Username, Email, PhoneNumber, Password, Role, FailedLogins, BannedUntil, IsBanned)
            VALUES ('testuser', 'testuser@example.com', '+40723456789', '', 3, 0, NULL, 0);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
            var userId = await command.ExecuteScalarAsync();
            return (int)userId;
        }

        private async Task AddReview(int sellerId, double score)
        {
            using var connection = new SqlConnection(_fixture.TestConnectionString);
            await connection.OpenAsync();
            using var command = connection.CreateCommand();
            command.CommandText = @"INSERT INTO Reviews (SellerId, Score) VALUES (@SellerId, @Score);";
            command.Parameters.Add(new SqlParameter("@SellerId", sellerId));
            command.Parameters.Add(new SqlParameter("@Score", score));
            await command.ExecuteNonQueryAsync();
        }

        private async Task AddProduct(int sellerId, string name, string description, double price, int stock)
        {
            using var connection = new SqlConnection(_fixture.TestConnectionString);
            await connection.OpenAsync();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Products (SellerID, ProductName, ProductDescription, ProductPrice, ProductStock)
                VALUES (@SellerId, @ProductName, @ProductDescription, @ProductPrice, @ProductStock);";
            command.Parameters.Add(new SqlParameter("@SellerId", sellerId));
            command.Parameters.Add(new SqlParameter("@ProductName", name));
            command.Parameters.Add(new SqlParameter("@ProductDescription", description));
            command.Parameters.Add(new SqlParameter("@ProductPrice", price));
            command.Parameters.Add(new SqlParameter("@ProductStock", stock));
            await command.ExecuteNonQueryAsync();
        }

        [Fact]
        public async Task GetSellerInfo_WithExistingSeller_ReturnsExpectedSeller()
        {
            int userId = await CreateUserAsync();
            var testUser = new User { UserId = userId, Username = "testuser" };
            var expectedSeller = new Seller(testUser);
            await sellerRepository.AddSeller(expectedSeller);

            var actualSeller = await sellerRepository.GetSellerInfo(testUser);

            Assert.Equal(expectedSeller.Username, actualSeller.Username);
        }

        [Fact]
        public async Task GetSellerInfo_WithNonExistingSeller_ReturnsEmptySeller()
        {
            var testUser = new User { UserId = 0, Username = "testuser" };
            var actualSeller = await sellerRepository.GetSellerInfo(testUser);

            Assert.Equal(String.Empty, actualSeller.StoreName);
        }

        [Fact]
        public async Task UpdateSeller_WithStoreName_UpdatesStoreName()
        {
            int userId = await CreateUserAsync();
            var testUser = new User { UserId = userId, Username = "testuser" };
            var seller = new Seller(testUser);
            await sellerRepository.AddSeller(seller);
            seller.StoreName = "teststore";

            // before update
            var originalSeller = await sellerRepository.GetSellerInfo(testUser);
            Assert.NotEqual(seller.StoreName, originalSeller.StoreName);

            await sellerRepository.UpdateSeller(seller);

            // after update
            var updatedSeller = await sellerRepository.GetSellerInfo(testUser);
            Assert.Equal(seller.StoreName, updatedSeller.StoreName);
        }

        [Fact]
        public async Task UpdateTrustScore_WithNewTrustScore_UpdatesTrustScore()
        {
            int userId = await CreateUserAsync();
            var testUser = new User { UserId = userId, Username = "testuser" };
            var seller = new Seller(testUser);
            await sellerRepository.AddSeller(seller);
            double trustScore = 4;

            // before update
            var originalSeller = await sellerRepository.GetSellerInfo(testUser);
            Assert.NotEqual(trustScore, originalSeller.TrustScore);

            await sellerRepository.UpdateTrustScore(userId, trustScore);

            // after update
            var updatedSeller = await sellerRepository.GetSellerInfo(testUser);
            Assert.Equal(trustScore, updatedSeller.TrustScore);
        }

        [Fact]
        public async Task GetNotifications_WithNoNotifications_ReturnsEmptyList()
        {
            int userId = await CreateUserAsync();
            var testUser = new User { UserId = userId, Username = "testuser" };
            var seller = new Seller(testUser);
            await sellerRepository.AddSeller(seller);

            var notifications = await sellerRepository.GetNotifications(userId, maxNotifications);
            Assert.Empty(notifications);
        }

        [Fact]
        public async Task GetNotifications_WithExistingNotifications_ReturnsExpectedNotifications()
        {
            int userId = await CreateUserAsync();
            var testUser = new User { UserId = userId, Username = "testuser" };
            var seller = new Seller(testUser);
            await sellerRepository.AddSeller(seller);
            List<string> expectedNotifications = new List<string>() { "notification1", "notification2" };
            await sellerRepository.AddNewFollowerNotification(userId, 1, expectedNotifications[0]);
            await sellerRepository.AddNewFollowerNotification(userId, 1, expectedNotifications[1]);

            var actualNotifications = await sellerRepository.GetNotifications(userId, maxNotifications);

            Assert.Equal(expectedNotifications, actualNotifications);
        }

        [Fact]
        public async Task GetLastFollowerCount_WithMultipleNotifications_ReturnsLastFollowerCount()
        {
            int userId = await CreateUserAsync();
            var testUser = new User { UserId = userId, Username = "testuser" };
            var seller = new Seller(testUser);
            await sellerRepository.AddSeller(seller);
            string testNotification = "test notification";
            await sellerRepository.AddNewFollowerNotification(userId, 10, testNotification);
            await sellerRepository.AddNewFollowerNotification(userId, 15, testNotification);

            var lastFollowerCount = await sellerRepository.GetLastFollowerCount(userId);

            Assert.Equal(15, lastFollowerCount);
        }

        [Fact]
        public async Task GetReviews_WithExistingReviews_ReturnsExpectedReview()
        {
            int userId = await CreateUserAsync();
            var testUser = new User { UserId = userId, Username = "testuser" };
            var seller = new Seller(testUser);
            await sellerRepository.AddSeller(seller);
            double score = 4.0;
            await AddReview(userId, score);

            var reviews = await sellerRepository.GetReviews(userId);
            Assert.Single(reviews);
            Assert.Equal(userId, reviews[0].SellerId);
            Assert.Equal(score, reviews[0].Score);
        }

        [Fact]
        public async Task GetProducts_WithExistingProduct_ReturnsExpectedData()
        {
            int userId = await CreateUserAsync();
            string name = "testname";
            string description = "test description";
            double price = 100.0;
            int stock = 5;
            var testUser = new User { UserId = userId, Username = "testuser" };
            var seller = new Seller(testUser);
            await sellerRepository.AddSeller(seller);
            await AddProduct(userId, name, description, price, stock);

            var products = await sellerRepository.GetProducts(userId);

            Assert.Single(products);
            Assert.Equal(name, products[0].Name);
            Assert.Equal(description, products[0].Description);
            Assert.Equal(price, products[0].Price);
            Assert.Equal(stock, products[0].Stock);
        }
    }
}
