using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketPlace924.ViewModel;
using Xunit;

namespace XUnitTestProject.ViewModelTests
{
    public class BuyerWishlistItemDetailsProviderViewModelTests
    {
        private readonly BuyerWishlistItemDetailsProvider provider;

        public BuyerWishlistItemDetailsProviderViewModelTests()
        {
            // Create a new instance of the provider for each test
            provider = new BuyerWishlistItemDetailsProvider();
        }

        [Fact]
        public void Constructor_InitializesCorrectly()
        {
            // Arrange & Act
            var newProvider = new BuyerWishlistItemDetailsProvider();
            
            // Assert - The provider should be initialized with a dictionary of mock product details
            Assert.NotNull(newProvider);
        }

        [Theory]
        [InlineData(1, "Stainless Steel Water Bottle", 150)]
        [InlineData(2, "Gaming Laptop", 120000)]
        [InlineData(3, "Wireless Noise-Canceling Headphones", 8000)]
        public void LoadWishlistItemDetails_KnownProductId_LoadsCorrectDetails(int productId, string expectedTitle, decimal expectedPrice)
        {
            // Arrange
            var inputItem = new BuyerWishlistItemViewModel
            {
                ProductId = productId,
                Title = string.Empty,
                Description = string.Empty,
                Price = 0,
                ImageSource = string.Empty,
                OwnItem = true,
                RemoveCallback = null
            };
            
            // Act
            var result = provider.LoadWishlistItemDetails(inputItem);
            
            // Assert
            Assert.Same(inputItem, result); // Should modify and return the same object
            Assert.Equal(productId, result.ProductId);
            Assert.Equal(expectedTitle, result.Title);
            Assert.Equal(expectedPrice, result.Price);
            Assert.NotEmpty(result.Description);
            Assert.NotEmpty(result.ImageSource);
            Assert.True(result.OwnItem); // Should preserve original value
            Assert.Null(result.RemoveCallback); // Should preserve original value
        }

        [Fact]
        public void LoadWishlistItemDetails_AllMockItemIds_ReturnsValidDetailsForEach()
        {
            // This test verifies that all mock IDs in the provider return valid details
            
            // We need access to the private mockProductDetails field to know all available product IDs
            // Instead of using reflection to access it, we'll use the known range of IDs from the implementation
            
            // Arrange & Act & Assert
            // Test all product IDs from 1 to 18 (based on the mock data)
            for (int productId = 1; productId <= 18; productId++)
            {
                var inputItem = new BuyerWishlistItemViewModel { ProductId = productId };
                var result = provider.LoadWishlistItemDetails(inputItem);
                
                Assert.Same(inputItem, result);
                Assert.Equal(productId, result.ProductId);
                
                // If this is a known product ID, properties should be populated
                if (productId >= 1 && productId <= 18)
                {
                    Assert.NotEmpty(result.Title);
                    Assert.NotEmpty(result.Description);
                    Assert.NotEqual(0, result.Price);
                    Assert.NotEmpty(result.ImageSource);
                }
            }
        }

        [Fact]
        public void LoadWishlistItemDetails_PreservesCallbackAndOwnItem()
        {
            // Arrange
            var mockCallback = new MockRemoveCallback();
            var inputItem = new BuyerWishlistItemViewModel
            {
                ProductId = 1,
                OwnItem = false, // Specifically test with false
                RemoveCallback = mockCallback
            };
            
            // Act
            var result = provider.LoadWishlistItemDetails(inputItem);
            
            // Assert
            Assert.Same(inputItem, result);
            Assert.False(result.OwnItem); // Should preserve the false value
            Assert.Same(mockCallback, result.RemoveCallback); // Should preserve the callback
        }

        // Helper class for mocking the IOnBuyerWishlistItemRemoveCallback
        private class MockRemoveCallback : IOnBuyerWishlistItemRemoveCallback
        {
            public Task OnBuyerWishlistItemRemove(int productId)
            {
                return Task.CompletedTask;
            }
        }
    }
}
