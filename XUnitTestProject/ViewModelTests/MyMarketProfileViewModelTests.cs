namespace XUnitTestProject.ViewModelTests
{
    using Xunit;
    using MarketPlace924.ViewModel;
    using MarketPlace924.Domain;
    using MarketPlace924.Service;
    using Moq;
    using System.Collections.ObjectModel;

    public class MyMarketProfileViewModelTests
    {
        private readonly Buyer testBuyer;
        private readonly Seller testSeller;
        private readonly List<Product> products;

        public MyMarketProfileViewModelTests()
        {
            testBuyer = new Buyer
            {
                User = new User { UserId = 1 }
            };
            testSeller = new Seller(new User { UserId = 10 }, "Test Store", "", "", 0, 0);
            products = new List<Product> {
                new Product(1, "Lenovo X16", "Gaming Laptop", 1400, 3, 10),
                new Product(2, "Logitech m310", "Mouse", 240, 1, 10)
            };
        }

        [Fact]
        public void Constructor_LoadsInitialData()
        {
            // Arrange
            var mockBuyerService = new Mock<IBuyerService>();
            mockBuyerService.Setup(s => s.GetProductsForViewProfile(testSeller.Id))
                .ReturnsAsync(new List<Product>());
            mockBuyerService.Setup(s => s.IsFollowing(testBuyer.Id, testSeller.Id))
                .ReturnsAsync(false);

            // Act
            var viewModel = new MyMarketProfileViewModel(mockBuyerService.Object, testBuyer, testSeller);

            // Assert
            mockBuyerService.Verify(s => s.GetProductsForViewProfile(testSeller.Id), Times.Once);
            mockBuyerService.Verify(s => s.IsFollowing(testBuyer.Id, testSeller.Id), Times.Once);
        }

        [Fact]
        public void IsFollowing_SetToTrue_UpdatesPropertiesAndNotifies()
        {
            // Arrange
            var mockBuyerService = new Mock<IBuyerService>();
            var viewModel = new MyMarketProfileViewModel(mockBuyerService.Object, testBuyer, testSeller);
            var propertyChangedCounter = 0;
            string? changedPropertyName = null;
            viewModel.PropertyChanged += (sender, e) =>
            {
                propertyChangedCounter++;
                changedPropertyName = e.PropertyName;
            };

            // Act
            viewModel.IsFollowing = true;

            // Assert
            Assert.True(viewModel.IsFollowing);
            Assert.Equal("Unfollow", viewModel.FollowButtonText);
            Assert.Equal("Red", viewModel.FollowButtonColor);
            Assert.Equal(3, propertyChangedCounter); // IsFollowing, FollowButtonText, FollowButtonColor
            Assert.Equal("FollowButtonColor", changedPropertyName); // Last property changed
        }

        [Fact]
        public void IsFollowing_SetToFalse_UpdatesPropertiesAndNotifies()
        {
            // Arrange
            var mockBuyerService = new Mock<IBuyerService>();
            var viewModel = new MyMarketProfileViewModel(mockBuyerService.Object, testBuyer, testSeller);
            var propertyChangedCounter = 0;
            string? changedPropertyName = null;
            viewModel.PropertyChanged += (sender, e) =>
            {
                propertyChangedCounter++;
                changedPropertyName = e.PropertyName;
            };

            // Act
            viewModel.IsFollowing = false;

            // Assert
            Assert.False(viewModel.IsFollowing);
            Assert.Equal("Follow", viewModel.FollowButtonText);
            Assert.Equal("White", viewModel.FollowButtonColor);
            Assert.Equal(3, propertyChangedCounter); // IsFollowing, FollowButtonText, FollowButtonColor
            Assert.Equal("FollowButtonColor", changedPropertyName); // Last property changed
        }

        [Fact]
        public void ToggleFollowCommand_IfFollowing_Unfollows()
        {
            // Arrange
            var mockBuyerService = new Mock<IBuyerService>();
            mockBuyerService.Setup(s => s.CheckIfBuyerExists(testBuyer.Id)).ReturnsAsync(true);
            var viewModel = new MyMarketProfileViewModel(mockBuyerService.Object, testBuyer, testSeller);
            viewModel.IsFollowing = true;

            // Act
            viewModel.FollowCommand.Execute(null);

            // Assert
            mockBuyerService.Verify(s => s.FollowSeller(testBuyer.Id, testSeller.Id), Times.Never);
            mockBuyerService.Verify(s => s.UnfollowSeller(testBuyer.Id, testSeller.Id), Times.Once);
            Assert.False(viewModel.IsFollowing);
        }

        [Fact]
        public void ToggleFollowCommand_IfNotFollowing_Follows()
        {
            // Arrange
            var mockBuyerService = new Mock<IBuyerService>();
            mockBuyerService.Setup(s => s.CheckIfBuyerExists(testBuyer.Id)).ReturnsAsync(true);
            var viewModel = new MyMarketProfileViewModel(mockBuyerService.Object, testBuyer, testSeller);
            viewModel.IsFollowing = false;

            // Act
            viewModel.FollowCommand.Execute(null);

            // Assert
            mockBuyerService.Verify(s => s.FollowSeller(testBuyer.Id, testSeller.Id), Times.Once);
            mockBuyerService.Verify(s => s.UnfollowSeller(testBuyer.Id, testSeller.Id), Times.Never);
            Assert.True(viewModel.IsFollowing);
        }

        [Fact]
        public void FilterProducts_IfSearchTextIsEmpty_ReturnsAllProducts()
        {
            // Arrange
            var mockBuyerService = new Mock<IBuyerService>();
            mockBuyerService.Setup(s => s.GetProductsForViewProfile(testSeller.Id)).ReturnsAsync(products);
            var viewModel = new MyMarketProfileViewModel(mockBuyerService.Object, testBuyer, testSeller);

            // Act
            viewModel.FilterProducts("");

            // Assert
            Assert.Equal(2, viewModel.SellerProducts.Count);
        }

        [Fact]
        public void FilterProducts_IfSearchTextNotFound_ReturnsNoProducts()
        {
            // Arrange
            var mockBuyerService = new Mock<IBuyerService>();
            mockBuyerService.Setup(s => s.GetProductsForViewProfile(testSeller.Id)).ReturnsAsync(products);
            var viewModel = new MyMarketProfileViewModel(mockBuyerService.Object, testBuyer, testSeller);

            // Act
            viewModel.FilterProducts("nothing");

            // Assert
            Assert.Empty(viewModel.SellerProducts);
        }

        [Fact]
        public void FilterProducts_IfSearchTextFound_ReturnsTheCorrectProduct()
        {
            // Arrange
            var mockBuyerService = new Mock<IBuyerService>();
            mockBuyerService.Setup(s => s.GetProductsForViewProfile(testSeller.Id)).ReturnsAsync(products);
            var viewModel = new MyMarketProfileViewModel(mockBuyerService.Object, testBuyer, testSeller);

            // Act
            viewModel.FilterProducts("Lenovo");

            // Assert
            Assert.Single(viewModel.SellerProducts);
            Assert.Equal("Lenovo X16", viewModel.SellerProducts[0].Name);
        }
        [Fact]
        public void Notifications_WhenChanged_UpdatesPropertiesAndNotifies()
        {
            // Arrange
            var mockBuyerService = new Mock<IBuyerService>();
            var viewModel = new MyMarketProfileViewModel(mockBuyerService.Object, testBuyer, testSeller);
            var propertyChangedCounter = 0;
            string? changedPropertyName = null;
            viewModel.PropertyChanged += (sender, e) =>
            {
                propertyChangedCounter++;
                changedPropertyName = e.PropertyName;
            };

            // Act
            viewModel.Notifications = new ObservableCollection<string> { "notification" };

            // Assert
            Assert.Single(viewModel.Notifications);
            Assert.Equal("notification", viewModel.Notifications[0]);
            Assert.Equal(1, propertyChangedCounter);
            Assert.Equal("Notifications", changedPropertyName); // Last property changed
        }
    }
}
