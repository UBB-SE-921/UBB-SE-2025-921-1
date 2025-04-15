namespace XUnitTestProject.ViewModelTests
{
    using Xunit;
    using MarketPlace924.ViewModel;
    using SharedClassLibrary.Domain;
    using MarketPlace924.Service;
    using Moq;
    using System.Collections.ObjectModel;
    using Windows.Foundation.Metadata;
    using Microsoft.UI.Xaml;

    public class MyMarketViewModelTests
    {
        private readonly List<Product> products;
        private readonly Product product;
        private readonly User testUser;
        private readonly List<Seller> sellers;

        public MyMarketViewModelTests()
        {
            this.testUser = new User
            {
                UserId = 1,
                Username = "Testuser",
            };
            product = new Product(1, "Lenovo X16", "Gaming Laptop", 1400, 3, 1);
            products = new List<Product> {
                new Product(1, "Lenovo X16", "Gaming Laptop", 1400, 3, 10),
                new Product(2, "Logitech m310", "Mouse", 240, 1, 10)
            };
            sellers = new List<Seller>
            {
                new Seller(new User { UserId = 1 })
                {
                    StoreName = "Store1"
                },
                new Seller(new User { UserId = 2 })
                {
                    StoreName = "Store2"
                }
            };
        }

        [Fact]
        public void Constructor_WhenCalled_FollowingListNotVisible()
        {
            // Arrange
            var mockBuyerService = new Mock<IBuyerService>();

            // Act
            var viewModel = new MyMarketViewModel(mockBuyerService.Object, testUser);

            // Assert
            Assert.False(viewModel.IsFollowingListVisible);
        }

        [Fact]
        public void MyMarketProducts_WhenChanged_UpdatesPropertiesAndNotifies()
        {
            // Arrange
            var mockBuyerService = new Mock<IBuyerService>();
            var viewModel = new MyMarketViewModel(mockBuyerService.Object, testUser);
            var propertyChangedCounter = 0;
            string? changedPropertyName = null;
            viewModel.PropertyChanged += (sender, e) =>
            {
                propertyChangedCounter++;
                changedPropertyName = e.PropertyName;
            };

            // Act
            viewModel.MyMarketProducts = new ObservableCollection<Product> {
                new Product(1, "Testproduct", "Test description", 1400, 3, 1)
            };

            // Assert
            Assert.Single(viewModel.MyMarketProducts);
            Assert.Equal("Testproduct", viewModel.MyMarketProducts[0].Name);
            Assert.Equal(1, propertyChangedCounter);
            Assert.Equal("MyMarketProducts", changedPropertyName); // Last property changed
        }

        [Fact]
        public void MyMarketFollowing_WhenChanged_UpdatesPropertiesAndNotifies()
        {
            // Arrange
            var mockBuyerService = new Mock<IBuyerService>();
            var viewModel = new MyMarketViewModel(mockBuyerService.Object, testUser);
            var propertyChangedCounter = 0;
            string? changedPropertyName = null;
            viewModel.PropertyChanged += (sender, e) =>
            {
                propertyChangedCounter++;
                changedPropertyName = e.PropertyName;
            };

            // Act
            viewModel.MyMarketFollowing = new ObservableCollection<Seller> {
                new Seller(testUser)
            };

            // Assert
            Assert.Single(viewModel.MyMarketFollowing);
            Assert.Equal("Testuser", viewModel.MyMarketFollowing[0].Username);
            Assert.Equal(1, propertyChangedCounter);
            Assert.Equal("MyMarketFollowing", changedPropertyName); // Last property changed
        }

        [Fact]
        public void AllSellersList_WhenChanged_UpdatesPropertiesAndNotifies()
        {
            // Arrange
            var mockBuyerService = new Mock<IBuyerService>();
            var viewModel = new MyMarketViewModel(mockBuyerService.Object, testUser);
            var propertyChangedCounter = 0;
            string? changedPropertyName = null;
            viewModel.PropertyChanged += (sender, e) =>
            {
                propertyChangedCounter++;
                changedPropertyName = e.PropertyName;
            };

            // Act
            viewModel.AllSellersList = new ObservableCollection<Seller> {
                new Seller(testUser)
            };

            // Assert
            Assert.Single(viewModel.AllSellersList);
            Assert.Equal("Testuser", viewModel.AllSellersList[0].Username);
            Assert.Equal(1, propertyChangedCounter);
            Assert.Equal("AllSellersList", changedPropertyName); // Last property changed
        }

        [Fact]
        public void IsFollowingListVisible_ChangedToDifferentValue_UpdatesPropertiesAndNotifies()
        {
            // Arrange
            var mockBuyerService = new Mock<IBuyerService>();
            var viewModel = new MyMarketViewModel(mockBuyerService.Object, testUser);
            var propertyChangedCounter = 0;
            string? changedPropertyName = null;
            viewModel.PropertyChanged += (sender, e) =>
            {
                propertyChangedCounter++;
                changedPropertyName = e.PropertyName;
            };

            // Act
            viewModel.IsFollowingListVisible = true;

            // Assert
            Assert.True(viewModel.IsFollowingListVisible);
            Assert.Equal(3, propertyChangedCounter);
        }

        [Fact]
        public void IsFollowingListVisible_ChangedToSameValue_DoesntNotify()
        {
            // Arrange
            var mockBuyerService = new Mock<IBuyerService>();
            var viewModel = new MyMarketViewModel(mockBuyerService.Object, testUser);
            var propertyChangedCounter = 0;
            string? changedPropertyName = null;
            viewModel.PropertyChanged += (sender, e) =>
            {
                propertyChangedCounter++;
                changedPropertyName = e.PropertyName;
            };

            // Act
            viewModel.IsFollowingListVisible = false;

            // Assert
            Assert.False(viewModel.IsFollowingListVisible);
            Assert.Equal(0, propertyChangedCounter);
        }

        [Fact]
        public void FollowingListVisibility_IfFollowingListVisible_IsVisible()
        {
            // Arrange
            var mockBuyerService = new Mock<IBuyerService>();
            var viewModel = new MyMarketViewModel(mockBuyerService.Object, testUser);

            // Act
            viewModel.IsFollowingListVisible = true;

            // Assert
            Assert.Equal(Visibility.Visible, viewModel.FollowingListVisibility);
        }

        [Fact]
        public void FollowingListVisibility_IfFollowingListNotVisible_IsCollapsed()
        {
            // Arrange
            var mockBuyerService = new Mock<IBuyerService>();
            var viewModel = new MyMarketViewModel(mockBuyerService.Object, testUser);

            // Act
            viewModel.IsFollowingListVisible = false;

            // Assert
            Assert.Equal(Visibility.Collapsed, viewModel.FollowingListVisibility);
        }

        [Fact]
        public void ShowFollowingVisibility_IfFollowingListVisible_IsCollapsed()
        {
            // Arrange
            var mockBuyerService = new Mock<IBuyerService>();
            var viewModel = new MyMarketViewModel(mockBuyerService.Object, testUser);

            // Act
            viewModel.IsFollowingListVisible = true;

            // Assert
            Assert.Equal(Visibility.Collapsed, viewModel.ShowFollowingVisibility);
        }

        [Fact]
        public void ShowFollowingVisibility_IfFollowingListNotVisible_IsVisible()
        {
            // Arrange
            var mockBuyerService = new Mock<IBuyerService>();
            var viewModel = new MyMarketViewModel(mockBuyerService.Object, testUser);

            // Act
            viewModel.IsFollowingListVisible = false;

            // Assert
            Assert.Equal(Visibility.Visible, viewModel.ShowFollowingVisibility);
        }

        [Fact]
        public void FilterProducts_IfSearchTextIsEmpty_ReturnsAllProducts()
        {
            // Arrange
            int buyerId = 10;
            var buyer = new Buyer
            {
                User = testUser,
                FollowingUsersIds = new List<int> { buyerId }
            };
            var mockBuyerService = new Mock<IBuyerService>();
            mockBuyerService.Setup(s => s.GetBuyerByUser(testUser)).ReturnsAsync(buyer);
            mockBuyerService.Setup(s => s.GetProductsFromFollowedSellers(buyer.FollowingUsersIds)).ReturnsAsync(products);
            var viewModel = new MyMarketViewModel(mockBuyerService.Object, testUser);

            // Act
            viewModel.FilterProducts("");

            // Assert
            mockBuyerService.Verify(m => m.GetProductsFromFollowedSellers(buyer.FollowingUsersIds), Times.Once());
            Assert.Equal(2, viewModel.MyMarketProducts.Count);
        }

        [Fact]
        public void FilterProducts_IfSearchTextNotFound_ReturnsNoProducts()
        {
            // Arrange
            int buyerId = 10;
            var buyer = new Buyer
            {
                User = testUser,
                FollowingUsersIds = new List<int> { buyerId }
            };
            var mockBuyerService = new Mock<IBuyerService>();
            mockBuyerService.Setup(s => s.GetBuyerByUser(testUser)).ReturnsAsync(buyer);
            mockBuyerService.Setup(s => s.GetProductsFromFollowedSellers(buyer.FollowingUsersIds)).ReturnsAsync(products);
            var viewModel = new MyMarketViewModel(mockBuyerService.Object, testUser);

            // Act
            viewModel.FilterProducts("nothing");

            // Assert
            mockBuyerService.Verify(m => m.GetProductsFromFollowedSellers(buyer.FollowingUsersIds), Times.Once());
            Assert.Empty(viewModel.MyMarketProducts);
        }

        [Fact]
        public void FilterProducts_IfSearchTextFound_ReturnsTheCorrectProduct()
        {
            // Arrange
            int buyerId = 10;
            var buyer = new Buyer
            {
                User = testUser,
                FollowingUsersIds = new List<int> { buyerId }
            };
            var mockBuyerService = new Mock<IBuyerService>();
            mockBuyerService.Setup(s => s.GetBuyerByUser(testUser)).ReturnsAsync(buyer);
            mockBuyerService.Setup(s => s.GetProductsFromFollowedSellers(buyer.FollowingUsersIds)).ReturnsAsync(products);
            var viewModel = new MyMarketViewModel(mockBuyerService.Object, testUser);

            // Act
            viewModel.FilterProducts("Lenovo");

            // Assert
            mockBuyerService.Verify(m => m.GetProductsFromFollowedSellers(buyer.FollowingUsersIds), Times.Once());
            Assert.Single(viewModel.MyMarketProducts);
            Assert.Equal("Lenovo X16", viewModel.MyMarketProducts[0].Name);
        }

        [Fact]
        public void FilterFollowing_IfSearchTextIsEmpty_ReturnsAllFollowedSellers()
        {
            // Arrange
            int buyerId = 10;
            var buyer = new Buyer
            {
                User = testUser,
                FollowingUsersIds = new List<int> { buyerId }
            };
            var mockBuyerService = new Mock<IBuyerService>();
            mockBuyerService.Setup(s => s.GetBuyerByUser(testUser)).ReturnsAsync(buyer);
            mockBuyerService.Setup(s => s.GetFollowedSellers(buyer.FollowingUsersIds)).ReturnsAsync(sellers);
            var viewModel = new MyMarketViewModel(mockBuyerService.Object, testUser);

            // Act
            viewModel.FilterFollowing("");

            // Assert
            mockBuyerService.Verify(m => m.GetFollowedSellers(buyer.FollowingUsersIds), Times.Once());
            Assert.Equal(2, viewModel.MyMarketFollowing.Count);
        }

        [Fact]
        public void FilterFollowing_IfSearchTextNotFound_ReturnsNoFollowedSellers()
        {
            // Arrange
            int buyerId = 10;
            var buyer = new Buyer
            {
                User = testUser,
                FollowingUsersIds = new List<int> { buyerId }
            };
            var mockBuyerService = new Mock<IBuyerService>();
            mockBuyerService.Setup(s => s.GetBuyerByUser(testUser)).ReturnsAsync(buyer);
            mockBuyerService.Setup(s => s.GetFollowedSellers(buyer.FollowingUsersIds)).ReturnsAsync(sellers);
            var viewModel = new MyMarketViewModel(mockBuyerService.Object, testUser);

            // Act
            viewModel.FilterFollowing("nothing");

            // Assert
            mockBuyerService.Verify(m => m.GetFollowedSellers(buyer.FollowingUsersIds), Times.Once());
            Assert.Empty(viewModel.MyMarketFollowing);
        }

        [Fact]
        public void FilterFollowing_IfSearchTextFound_ReturnsFoundSeller()
        {
            // Arrange
            int buyerId = 10;
            var buyer = new Buyer
            {
                User = testUser,
                FollowingUsersIds = new List<int> { buyerId }
            };
            var mockBuyerService = new Mock<IBuyerService>();
            mockBuyerService.Setup(s => s.GetBuyerByUser(testUser)).ReturnsAsync(buyer);
            mockBuyerService.Setup(s => s.GetFollowedSellers(buyer.FollowingUsersIds)).ReturnsAsync(sellers);
            var viewModel = new MyMarketViewModel(mockBuyerService.Object, testUser);

            // Act
            viewModel.FilterFollowing("Store2");

            // Assert
            mockBuyerService.Verify(m => m.GetFollowedSellers(buyer.FollowingUsersIds), Times.Once());
            Assert.Equal("Store2", viewModel.MyMarketFollowing[0].StoreName);
        }

        [Fact]
        public void FilterAllSellers_IfSearchTextFound_ReturnsFoundSeller()
        {
            // Arrange
            int buyerId = 10;
            var buyer = new Buyer
            {
                User = testUser,
                FollowingUsersIds = new List<int> { buyerId }
            };
            var mockBuyerService = new Mock<IBuyerService>();
            mockBuyerService.Setup(s => s.GetBuyerByUser(testUser)).ReturnsAsync(buyer);
            mockBuyerService.Setup(s => s.GetAllSellers()).ReturnsAsync(sellers);
            var viewModel = new MyMarketViewModel(mockBuyerService.Object, testUser);

            // Act
            viewModel.FilterAllSellers("Store2");

            // Assert
            mockBuyerService.Verify(m => m.GetAllSellers(), Times.Once());
            Assert.Equal("Store2", viewModel.AllSellersList[0].StoreName);
        }

        [Fact]
        public void FilterAllSellers_IfSearchTextIsEmpty_ReturnsAllSellers()
        {
            // Arrange
            int buyerId = 10;
            var buyer = new Buyer
            {
                User = testUser,
                FollowingUsersIds = new List<int> { buyerId }
            };
            var mockBuyerService = new Mock<IBuyerService>();
            mockBuyerService.Setup(s => s.GetBuyerByUser(testUser)).ReturnsAsync(buyer);
            mockBuyerService.Setup(s => s.GetAllSellers()).ReturnsAsync(sellers);
            var viewModel = new MyMarketViewModel(mockBuyerService.Object, testUser);

            // Act
            viewModel.FilterAllSellers("");

            // Assert
            mockBuyerService.Verify(m => m.GetAllSellers(), Times.Once());
            Assert.Equal(2, viewModel.AllSellersList.Count);
        }

        [Fact]
        public void FilterAllSellers_IfSearchTextNotFound_ReturnsNoSellers()
        {
            // Arrange
            int buyerId = 10;
            var buyer = new Buyer
            {
                User = testUser,
                FollowingUsersIds = new List<int> { buyerId }
            };
            var mockBuyerService = new Mock<IBuyerService>();
            mockBuyerService.Setup(s => s.GetBuyerByUser(testUser)).ReturnsAsync(buyer);
            mockBuyerService.Setup(s => s.GetAllSellers()).ReturnsAsync(sellers);
            var viewModel = new MyMarketViewModel(mockBuyerService.Object, testUser);

            // Act
            viewModel.FilterAllSellers("nothing");

            // Assert
            mockBuyerService.Verify(m => m.GetAllSellers(), Times.Once());
            Assert.Empty(viewModel.AllSellersList);
        }

        [Fact]
        public void LoadMyMarketData_WhenCalled_ReturnsAllProducts()
        {
            // Arrange
            int buyerId = 10;
            var buyer = new Buyer
            {
                User = testUser,
                FollowingUsersIds = new List<int> { buyerId }
            };
            var mockBuyerService = new Mock<IBuyerService>();
            mockBuyerService.Setup(s => s.GetBuyerByUser(testUser)).ReturnsAsync(buyer);
            mockBuyerService.Setup(s => s.GetProductsFromFollowedSellers(buyer.FollowingUsersIds)).ReturnsAsync(products);
            var viewModel = new MyMarketViewModel(mockBuyerService.Object, testUser);

            // Act
            _ = viewModel.LoadMyMarketData();

            // Assert
            mockBuyerService.Verify(m => m.GetProductsFromFollowedSellers(buyer.FollowingUsersIds), Times.Exactly(2));
            Assert.Equal(2, viewModel.MyMarketProducts.Count);
        }

        [Fact]
        public void LoadFollowing_WhenCalled_SetsFollowedSellersCount()
        {
            // Arrange
            int buyerId = 10;
            var buyer = new Buyer
            {
                User = testUser,
                FollowingUsersIds = new List<int> { buyerId }
            };
            var mockBuyerService = new Mock<IBuyerService>();
            mockBuyerService.Setup(s => s.GetBuyerByUser(testUser)).ReturnsAsync(buyer);
            mockBuyerService.Setup(s => s.GetFollowedSellers(buyer.FollowingUsersIds)).ReturnsAsync(sellers);
            var viewModel = new MyMarketViewModel(mockBuyerService.Object, testUser);

            // Act
            _ = viewModel.LoadFollowing();

            // Assert
            mockBuyerService.Verify(m => m.GetFollowedSellers(buyer.FollowingUsersIds), Times.Exactly(2));
            Assert.Equal(2, viewModel.FollowedSellersCount);
        }

        [Fact]
        public void LoadAllSellers_WhenCalled_ReturnsAllSellers()
        {
            // Arrange
            int buyerId = 10;
            var buyer = new Buyer
            {
                User = testUser,
                FollowingUsersIds = new List<int> { buyerId }
            };
            var mockBuyerService = new Mock<IBuyerService>();
            mockBuyerService.Setup(s => s.GetBuyerByUser(testUser)).ReturnsAsync(buyer);
            mockBuyerService.Setup(s => s.GetAllSellers()).ReturnsAsync(sellers);
            var viewModel = new MyMarketViewModel(mockBuyerService.Object, testUser);

            // Act
            _ = viewModel.LoadAllSellers();

            // Assert
            mockBuyerService.Verify(m => m.GetAllSellers(), Times.Exactly(2));
            Assert.Equal(2, viewModel.AllSellersList.Count);
        }

        [Fact]
        public void RefreshData_WhenCalled_ReturnsBuyer()
        {
            // Arrange
            int buyerId = 10;
            var buyer = new Buyer
            {
                User = testUser,
                FollowingUsersIds = new List<int> { buyerId }
            };
            var mockBuyerService = new Mock<IBuyerService>();
            mockBuyerService.Setup(s => s.GetBuyerByUser(testUser)).ReturnsAsync(buyer);
            var viewModel = new MyMarketViewModel(mockBuyerService.Object, testUser);

            // Act
            _ = viewModel.RefreshData();

            // Assert
            mockBuyerService.Verify(m => m.GetBuyerByUser(testUser), Times.Exactly(2));
        }

        [Fact]
        public void RefreshData_OnException_CatchesException()
        {
            // Arrange
            var expectedException = new InvalidOperationException("Failed to retrieve buyer.");
            int buyerId = 10;
            var buyer = new Buyer
            {
                User = testUser,
                FollowingUsersIds = new List<int> { buyerId }
            };
            var mockBuyerService = new Mock<IBuyerService>();
            mockBuyerService.Setup(s => s.GetBuyerByUser(testUser)).ThrowsAsync(expectedException);
            var viewModel = new MyMarketViewModel(mockBuyerService.Object, testUser);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await viewModel.RefreshData();
            });

            mockBuyerService.Verify(m => m.GetBuyerByUser(testUser), Times.Exactly(2));
        }
    }
}
