using SharedClassLibrary.Domain;
using System;
using System.Collections.Generic;
using Xunit;

namespace XUnitTestProject.DomainTests
{
    /// <summary>
    /// Contains unit tests for the <see cref="SharedClassLibrary.Domain.BuyerWishlist"/> class.
    /// </summary>
    public class BuyerWishlistTests
    {
        /// <summary>
        /// Tests the default initialization of the BuyerWishlist class.
        /// Ensures that the Items list is created and empty upon instantiation.
        /// </summary>
        [Fact]
        public static void BuyerWishlist_ShouldInitialize_ByDefault()
        {
            var wishlist = new BuyerWishlist();

            Assert.NotNull(wishlist.Items);
            Assert.IsType<List<BuyerWishlistItem>>(wishlist.Items);
            Assert.Empty(wishlist.Items);

            Assert.Equal(string.Empty, wishlist.Code);
        }

        /// <summary>
        /// Tests that items can be added to the wishlist's Items collection.
        /// Although the 'Items' property has only a getter, the underlying List object it returns
        /// can be modified. This test verifies that behavior.
        /// </summary>
        [Fact]
        public static void BuyerWishlist_ShouldAllowAddingItems_ToList()
        {
            var wishlist = new BuyerWishlist();
            var item1 = new BuyerWishlistItem(productId: 1);
            var item2 = new BuyerWishlistItem(productId: 2);

            wishlist.Items.Add(item1);
            wishlist.Items.Add(item2);

            Assert.Equal(2, wishlist.Items.Count);
            Assert.Contains(item1, wishlist.Items);
            Assert.Contains(item2, wishlist.Items);

            Assert.Equal(wishlist.Items[0].ProductId, item1.ProductId);
            Assert.Equal(wishlist.Items[1].ProductId, item2.ProductId);
        }
    }
} 