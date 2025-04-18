// -----------------------------------------------------------------------
// <copyright file="IShoppingCartRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace MarketPlace924.Repository
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using MarketPlace924.Domain;

    /// <summary>
    /// Interface for repository operations related to shopping cart functionality.
    /// </summary>
    public interface IShoppingCartRepository
    {
        /// <summary>
        /// Adds a product to the user's shopping cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <param name="productId">The ID of the product to add to the cart.</param>
        /// <param name="quantity">The quantity of the product to add.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task AddProductToCartAsync(int buyerId, int productId, int quantity);

        /// <summary>
        /// Removes a product from the user's shopping cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <param name="productId">The ID of the product to remove from the cart.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task RemoveProductFromCartAsync(int buyerId, int productId);

        /// <summary>
        /// Updates the quantity of a product in the user's shopping cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <param name="productId">The ID of the product to update.</param>
        /// <param name="quantity">The new quantity.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task UpdateProductQuantityAsync(int buyerId, int productId, int quantity);

        /// <summary>
        /// Gets all products in the user's shopping cart with their quantities.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a dictionary with product objects as keys and quantities as values.</returns>
        Task<Dictionary<Product, int>> GetCartItemsAsync(int buyerId);

        /// <summary>
        /// Clears all items from the user's shopping cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task ClearCartAsync(int buyerId);

        /// <summary>
        /// Gets the total number of items in the user's shopping cart (sum of all quantities).
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the total number of items.</returns>
        Task<int> GetCartItemCountAsync(int buyerId);

        /// <summary>
        /// Checks if a specific product is in the buyer's cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <param name="productId">The ID of the product to check.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the product is in the cart, otherwise false.</returns>
        Task<bool> IsProductInCartAsync(int buyerId, int productId);

        /// <summary>
        /// Gets the quantity of a specific product in the buyer's cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <param name="productId">The ID of the product to check.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is the quantity of the product in the cart, or 0 if it's not in the cart.</returns>
        Task<int> GetProductQuantityAsync(int buyerId, int productId);
    }
}