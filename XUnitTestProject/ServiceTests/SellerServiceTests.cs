using MarketPlace924.Domain;
using MarketPlace924.Repository;
using MarketPlace924.Service;
using Moq;

namespace XUnitTestProject.ServiceTests
{
    public class SellerServiceTests
    {
        private const int maxNotifications = 1000;
        private readonly Mock<ISellerRepository> sellerRepositoryMock;
        private readonly SellerService sellerService;

        public SellerServiceTests()
        {
            sellerRepositoryMock = new Mock<ISellerRepository>();
            sellerService = new SellerService(sellerRepositoryMock.Object);
        }

        [Fact]
        public async Task GetSellerByUser_WithExistingSeller_ReturnsExpectedSeller()
        {
            string userName = "test user";
            string email = "test@example.com";
            string storeName = "test store";
            var user = new User(1, userName, email);
            var expectedSeller = new Seller(user, storeName);

            sellerRepositoryMock
                .Setup(repo => repo.GetSellerInfo(user))
                .ReturnsAsync(expectedSeller);

            var actualSeller = await sellerService.GetSellerByUser(user);

            Assert.Equal(storeName, actualSeller.StoreName);
        }

        [Fact]
        public async Task GetAllProducts_WithExistingProductList_ReturnsProductList()
        {
            int sellerId = 1;
            var expectedProducts = new List<Product>
            {
                new Product(1, "Product A", "Description A", 10.99, 5, sellerId),
                new Product(2, "Product B", "Description B", 19.99, 3, sellerId)
            };

            sellerRepositoryMock
                .Setup(repo => repo.GetProducts(sellerId))
                .ReturnsAsync(expectedProducts);

            var actualProducts = await sellerService.GetAllProducts(sellerId);
            Assert.Equal(expectedProducts.Count, actualProducts.Count);
        }

        [Fact]
        public async Task UpdateSeller_WithValidSeller_CallsRepositoryUpdateSellerOnce()
        {
            var user = new User(1, "seller1", "seller1@example.com");
            var seller = new Seller(user)
            {
                StoreName = "Updated Store",
                StoreDescription = "Updated Description",
                StoreAddress = "Updated Address",
                FollowersCount = 42,
                TrustScore = 4.8
            };

            await sellerService.UpdateSeller(seller);

            sellerRepositoryMock.Verify(repo => repo.UpdateSeller(seller), Times.Once);
        }

        [Fact]
        public async Task CreateSeller_WithValidSeller_CallsRepositoryAddSellerOnce()
        {
            var user = new User(1, "newSeller", "seller@example.com");
            var seller = new Seller(user)
            {
                StoreName = "New Store",
                StoreDescription = "Fresh products",
                StoreAddress = "Market Street 123",
                FollowersCount = 0,
                TrustScore = 0.0
            };

            await sellerService.CreateSeller(seller);

            sellerRepositoryMock.Verify(repo => repo.AddSeller(seller), Times.Once);
        }

        [Fact]
        public async Task CalculateAverageReviewScore_WithExistingReviews_UpdatesTrustScoreAndReturnsAverage()
        {
            int sellerId = 1;
            var reviews = new List<Review>
            {
                new Review(1, sellerId, 4),
                new Review(2, sellerId, 5),
                new Review(3, sellerId, 3)
            };

            sellerRepositoryMock
                .Setup(repo => repo.GetReviews(sellerId))
                .ReturnsAsync(reviews);

            var actualAverage = await sellerService.CalculateAverageReviewScore(sellerId);

            double expectedAverage = (4 + 5 + 3) / 3.0;
            Assert.Equal(expectedAverage, actualAverage, precision: 2);
            sellerRepositoryMock.Verify(repo => repo.UpdateTrustScore(sellerId, expectedAverage), Times.Once);
        }

        [Fact]
        public async Task GetNotifications_WithExistingNotifications_ReturnsExpectedNotifications()
        {
            int sellerId = 1;
            List<string> expectedNotifications = new List<string> { "notification1", "notification2" };
            sellerRepositoryMock.Setup(r => r.GetNotifications(sellerId, maxNotifications)).ReturnsAsync(expectedNotifications);

            var actualNotifications = await sellerService.GetNotifications(sellerId, maxNotifications);

            Assert.Equal(expectedNotifications, actualNotifications);
        }

        [Theory]
        [InlineData(40, 38, "You have lost 2 followers!")]
        [InlineData(40, 39, "You have lost 1 follower!")]
        [InlineData(40, 41, "You have gained 1 new follower!")]
        [InlineData(40, 42, "You have gained 2 new followers!")]
        [InlineData(40, 50, "Amazing! You've reached 50 followers!")]
        [InlineData(40, 100, "Congratulations! You've reached 100 followers!")]
        [InlineData(40, 110, "Incredible! You've reached 110 followers!")]
        public async Task GenerateFollowersChangedNotification_WithFollowersGained_GeneratesFollowersGainedMessage(int oldFollowers, int newFollowers, string notificationMessage)
        {
            int sellerId = 5;
            sellerRepositoryMock.Setup(r => r.GetLastFollowerCount(sellerId)).ReturnsAsync(oldFollowers);

            await sellerService.GenerateFollowersChangedNotification(sellerId, newFollowers);

            sellerRepositoryMock.Verify(r => r.AddNewFollowerNotification(
                sellerId, newFollowers, notificationMessage), Times.Once);

        }

        [Fact]
        public async Task GenerateFollowersChangedNotification_WithNoFollowerGained_DoesNotGenerateNotification()
        {
            int sellerId = 6;
            int count = 42;

            sellerRepositoryMock.Setup(r => r.GetLastFollowerCount(sellerId)).ReturnsAsync(count);

            await sellerService.GenerateFollowersChangedNotification(sellerId, count);

            sellerRepositoryMock.Verify(r => r.AddNewFollowerNotification(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }
    }
}
