using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using SharedClassLibrary.Service;
using MarketPlace924.ViewModel;
using Moq;
using Xunit;

namespace XUnitTestProject.ViewModelTests
{
    public class SellerProfileViewModelTests
    {
        private const int maxNotifications = 6;
        private readonly Mock<IUserService> userServiceMock;
        private readonly Mock<ISellerService> sellerServiceMock;
        private readonly User testUser;
        private readonly Seller testSeller;

        public SellerProfileViewModelTests()
        {
            // setup common test data
            this.userServiceMock = new Mock<IUserService>();
            this.sellerServiceMock = new Mock<ISellerService>();

            this.testUser = new User
            {
                UserId = 1,
                Username = "testuser",
                Email = "test@example.com"
            };

            this.testSeller = new Seller(this.testUser)
            {
                User = this.testUser,
                StoreName = "Test Store",
                StoreAddress = "Test Address",
                StoreDescription = "Test Description",
                FollowersCount = 100
            };
        }

        [Fact]
        public void Constructor_WithValidUser_InitializesPropertiesCorrectly()
        {
            // arrange
            this.sellerServiceMock.Setup(x => x.GetSellerByUser(It.Is<User>(u => u.UserId == this.testUser.UserId)))
                .ReturnsAsync(this.testSeller);

            // act
            var viewModel = new SellerProfileViewModel(this.testUser, this.userServiceMock.Object, this.sellerServiceMock.Object);

            // assert
            Assert.NotNull(viewModel.Seller);
            Assert.NotNull(viewModel.FilteredProducts);
            Assert.NotNull(viewModel.Products);
            Assert.NotNull(viewModel.Notifications);
        }

        [Fact]
        public void FilterProducts_WithEmptySearchText_ShowsAllProducts()
        {
            // arrange
            var products = new List<Product>
            {
                new Product(1, "Product1", "desc", 100, 1, this.testSeller.Id),
                new Product(2, "Product2", "desc", 200, 2, this.testSeller.Id),
                new Product(3, "Product3", "desc", 300, 3, this.testSeller.Id)
            };

            this.sellerServiceMock.Setup(x => x.GetSellerByUser(It.Is<User>(u => u.UserId == this.testUser.UserId)))
                .ReturnsAsync(this.testSeller);
            this.sellerServiceMock.Setup(x => x.GetAllProducts(this.testSeller.Id))
                .ReturnsAsync(products);

            var viewModel = new SellerProfileViewModel(this.testUser, this.userServiceMock.Object, this.sellerServiceMock.Object);

            // act
            viewModel.FilterProducts(string.Empty);

            // assert
            Assert.Equal(3, viewModel.FilteredProducts.Count);
        }

        [Fact]
        public void FilterProducts_WithSearchText_FiltersProductsCorrectly()
        {
            // arrange
            var products = new List<Product>
            {
                new Product(1, "Product1", "desc", 100, 1, this.testSeller.Id),
                new Product(2, "Product2", "desc", 200, 2, this.testSeller.Id),
                new Product(3, "Different", "desc", 300, 3, this.testSeller.Id)
            };

            this.sellerServiceMock.Setup(x => x.GetSellerByUser(It.Is<User>(u => u.UserId == this.testUser.UserId)))
                .ReturnsAsync(this.testSeller);
            this.sellerServiceMock.Setup(x => x.GetAllProducts(this.testSeller.Id))
                .ReturnsAsync(products);

            var viewModel = new SellerProfileViewModel(this.testUser, this.userServiceMock.Object, this.sellerServiceMock.Object);

            // act
            viewModel.FilterProducts("Product");

            // assert
            Assert.Equal(2, viewModel.FilteredProducts.Count);
            Assert.All(viewModel.FilteredProducts, p => Assert.Contains("Product", p.Name));
        }

        [Fact]
        public void SortProducts_SortsByPriceAscending()
        {
            // arrange
            var products = new List<Product>
            {
                new Product(1, "Product1", "desc", 300, 1, this.testSeller.Id),
                new Product(2, "Product2", "desc", 100, 2, this.testSeller.Id),
                new Product(3, "Product3", "desc", 200, 3, this.testSeller.Id)
            };

            this.sellerServiceMock.Setup(x => x.GetSellerByUser(It.Is<User>(u => u.UserId == this.testUser.UserId)))
                .ReturnsAsync(this.testSeller);
            this.sellerServiceMock.Setup(x => x.GetAllProducts(this.testSeller.Id))
                .ReturnsAsync(products);

            var viewModel = new SellerProfileViewModel(this.testUser, this.userServiceMock.Object, this.sellerServiceMock.Object);

            // act
            viewModel.SortProducts();

            // assert
            Assert.Equal(100, viewModel.FilteredProducts[0].Price);
            Assert.Equal(200, viewModel.FilteredProducts[1].Price);
            Assert.Equal(300, viewModel.FilteredProducts[2].Price);
        }

        [Fact]
        public void ValidateFields_WithValidData_ReturnsNoErrors()
        {
            // arrange
            this.sellerServiceMock.Setup(x => x.GetSellerByUser(It.Is<User>(u => u.UserId == this.testUser.UserId)))
                .ReturnsAsync(this.testSeller);
            var viewModel = new SellerProfileViewModel(this.testUser, this.userServiceMock.Object, this.sellerServiceMock.Object);
            viewModel.StoreName = "Valid Store";
            viewModel.Email = "valid@example.com";
            viewModel.PhoneNumber = "1234567890";
            viewModel.Address = "Valid Address";
            viewModel.Description = "Valid Description";

            // act
            var errors = viewModel.ValidateFields();

            // assert
            Assert.Empty(errors);
            Assert.Empty(viewModel.StoreNameError);
            Assert.Empty(viewModel.EmailError);
            Assert.Empty(viewModel.PhoneNumberError);
            Assert.Empty(viewModel.AddressError);
            Assert.Empty(viewModel.DescriptionError);
        }

        [Fact]
        public void ValidateFields_WithInvalidData_ReturnsErrors()
        {
            // arrange
            this.sellerServiceMock.Setup(x => x.GetSellerByUser(It.Is<User>(u => u.UserId == this.testUser.UserId)))
                .ReturnsAsync(this.testSeller);
            var viewModel = new SellerProfileViewModel(this.testUser, this.userServiceMock.Object, this.sellerServiceMock.Object);
            viewModel.StoreName = string.Empty;
            viewModel.Email = "invalid-email";
            viewModel.PhoneNumber = string.Empty;
            viewModel.Address = string.Empty;
            viewModel.Description = string.Empty;

            // act
            var errors = viewModel.ValidateFields();

            // assert
            Assert.Equal(5, errors.Count);
            Assert.Contains("Store name is required", viewModel.StoreNameError);
            Assert.Contains("Valid email is required", viewModel.EmailError);
            Assert.Contains("Phone number is required", viewModel.PhoneNumberError);
            Assert.Contains("Address is required", viewModel.AddressError);
            Assert.Contains("Description is required", viewModel.DescriptionError);
        }

        [Fact]
        public async Task LoadNotifications_WithValidData_UpdatesNotificationsCollection()
        {
            // arrange
            var notifications = new List<string>
            {
                "Notification 1",
                "Notification 2"
            };
            this.sellerServiceMock.Setup(x => x.GetSellerByUser(It.Is<User>(u => u.UserId == this.testUser.UserId)))
                .ReturnsAsync(this.testSeller);
            this.sellerServiceMock.Setup(x => x.GetNotifications(this.testSeller.Id, maxNotifications))
                .ReturnsAsync(notifications);
            var viewModel = new SellerProfileViewModel(this.testUser, this.userServiceMock.Object, this.sellerServiceMock.Object);

            // act
            await viewModel.LoadNotifications();

            // assert
            Assert.Equal(2, viewModel.Notifications.Count);
            Assert.Contains("Notification 1", viewModel.Notifications);
            Assert.Contains("Notification 2", viewModel.Notifications);
        }

        [Fact]
        public void IsExpanderExpanded_WhenExpanded_LoadsNotifications()
        {
            // arrange
            var notifications = new List<string> { "Notification 1" };
            this.sellerServiceMock.Setup(x => x.GetSellerByUser(It.Is<User>(u => u.UserId == this.testUser.UserId)))
                .ReturnsAsync(this.testSeller);
            this.sellerServiceMock.Setup(x => x.GetNotifications(this.testSeller.Id, maxNotifications))
                .ReturnsAsync(notifications);
            var viewModel = new SellerProfileViewModel(this.testUser, this.userServiceMock.Object, this.sellerServiceMock.Object);

            // act
            viewModel.IsExpanderExpanded = true;

            // assert
            Assert.True(viewModel.IsExpanderExpanded);
            this.sellerServiceMock.Verify(x => x.GetNotifications(this.testSeller.Id, maxNotifications), Times.Once);
        }

        [Fact]
        public void IsExpanderExpanded_WhenCollapsed_DoesNotLoadNotifications()
        {
            // arrange
            this.sellerServiceMock.Setup(x => x.GetSellerByUser(It.Is<User>(u => u.UserId == this.testUser.UserId)))
                .ReturnsAsync(this.testSeller);
            var viewModel = new SellerProfileViewModel(this.testUser, this.userServiceMock.Object, this.sellerServiceMock.Object);

            // act
            viewModel.IsExpanderExpanded = false;

            // assert
            Assert.False(viewModel.IsExpanderExpanded);
            this.sellerServiceMock.Verify(x => x.GetNotifications(It.IsAny<int>(), maxNotifications), Times.Never);
        }
    }
}
