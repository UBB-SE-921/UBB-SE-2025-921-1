using SharedClassLibrary.Domain;
using Xunit;

namespace XUnitTestProject.DomainTests
{
    /// <summary>
    /// Contains unit tests for the <see cref="SharedClassLibrary.Domain.BuyerWishlistItem"/> class.
    /// </summary>
    public class BuyerWishlistItemTests
    {
        /// <summary>
        /// Tests the initialization of the BuyerWishlistItem class.
        /// Ensures that the ProductId property is correctly set via the constructor.
        /// </summary>
        [Fact]
        public static void BuyerWishlistItem_ShouldInitialize_WithProductId()
        {
            int expectedProductId = 123;

            var wishlistItem = new BuyerWishlistItem(productId: expectedProductId);

            Assert.Equal(expectedProductId, wishlistItem.ProductId);
        }
    }
} 