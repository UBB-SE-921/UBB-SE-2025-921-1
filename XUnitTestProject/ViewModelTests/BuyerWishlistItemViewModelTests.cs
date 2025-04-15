using MarketPlace924.ViewModel;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace XUnitTestProject.ViewModelTests
{
    public class BuyerWishlistItemViewModelTests
    {
        [Fact]
        public void Constructor_PropertiesInitializedWithDefaultValues()
        {
            // Act
            var viewModel = new BuyerWishlistItemViewModel();
            
            // Assert
            Assert.Equal(0, viewModel.ProductId);
            Assert.Equal(string.Empty, viewModel.Title);
            Assert.Equal(0m, viewModel.Price);
            Assert.Equal(string.Empty, viewModel.Description);
            Assert.Equal(string.Empty, viewModel.ImageSource);
            Assert.True(viewModel.OwnItem); // Default is true
        }
        
        [Fact]
        public void ProductId_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var viewModel = new BuyerWishlistItemViewModel();
            
            // Act
            viewModel.ProductId = 42;
            
            // Assert
            Assert.Equal(42, viewModel.ProductId);
        }
        
        [Fact]
        public void Title_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var viewModel = new BuyerWishlistItemViewModel();
            
            // Act
            viewModel.Title = "Test Product";
            
            // Assert
            Assert.Equal("Test Product", viewModel.Title);
        }
        
        [Fact]
        public void Price_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var viewModel = new BuyerWishlistItemViewModel();
            
            // Act
            viewModel.Price = 99.99m;
            
            // Assert
            Assert.Equal(99.99m, viewModel.Price);
        }
        
        [Fact]
        public void Description_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var viewModel = new BuyerWishlistItemViewModel();
            
            // Act
            viewModel.Description = "This is a test description";
            
            // Assert
            Assert.Equal("This is a test description", viewModel.Description);
        }
        
        [Fact]
        public void ImageSource_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var viewModel = new BuyerWishlistItemViewModel();
            
            // Act
            viewModel.ImageSource = "image-path.jpg";
            
            // Assert
            Assert.Equal("image-path.jpg", viewModel.ImageSource);
        }
        
        [Fact]
        public void OwnItem_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var viewModel = new BuyerWishlistItemViewModel();
            
            // Act
            viewModel.OwnItem = false;
            
            // Assert
            Assert.False(viewModel.OwnItem);
        }
        
        [Fact]
        public void RemoveCallback_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var viewModel = new BuyerWishlistItemViewModel();
            var mockCallback = new Mock<IOnBuyerWishlistItemRemoveCallback>();
            
            // Act
            viewModel.RemoveCallback = mockCallback.Object;
            
            // Assert
            Assert.Same(mockCallback.Object, viewModel.RemoveCallback);
        }
        
        [Fact]
        public void Remove_CallsRemoveCallback_WithCorrectProductId()
        {
            // Arrange
            var viewModel = new BuyerWishlistItemViewModel
            {
                ProductId = 123,
                RemoveCallback = new TestRemoveCallback()
            };
            var callback = (TestRemoveCallback)viewModel.RemoveCallback;
            
            // Act
            viewModel.Remove();
            
            // Assert
            Assert.True(callback.WasCalled);
            Assert.Equal(123, callback.LastProductId);
        }
        
        [Fact]
        public void ItemInitialization_WithConstructor_AllPropertiesSetCorrectly()
        {
            // Arrange
            var mockCallback = new Mock<IOnBuyerWishlistItemRemoveCallback>();
            
            // Act
            var viewModel = new BuyerWishlistItemViewModel
            {
                ProductId = 42,
                Title = "Test Product",
                Price = 99.99m,
                Description = "Test Description",
                ImageSource = "test-image.jpg",
                OwnItem = false,
                RemoveCallback = mockCallback.Object
            };
            
            // Assert
            Assert.Equal(42, viewModel.ProductId);
            Assert.Equal("Test Product", viewModel.Title);
            Assert.Equal(99.99m, viewModel.Price);
            Assert.Equal("Test Description", viewModel.Description);
            Assert.Equal("test-image.jpg", viewModel.ImageSource);
            Assert.False(viewModel.OwnItem);
            Assert.Same(mockCallback.Object, viewModel.RemoveCallback);
        }
        
        // Helper class to test the Remove method
        private class TestRemoveCallback : IOnBuyerWishlistItemRemoveCallback
        {
            public bool WasCalled { get; private set; }
            public int LastProductId { get; private set; }
            
            public Task OnBuyerWishlistItemRemove(int productId)
            {
                WasCalled = true;
                LastProductId = productId;
                return Task.CompletedTask;
            }
        }
    }
}
