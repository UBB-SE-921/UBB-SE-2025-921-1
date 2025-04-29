// -----------------------------------------------------------------------
// <copyright file="ShoppingCartRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Server.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Server.DBConnection;
    using SharedClassLibrary.IRepository;
    using SharedClassLibrary.Domain;
    using Microsoft.Data.SqlClient;

    /// <summary>
    /// Repository for managing shopping cart operations in the database.
    /// </summary>
    public class ShoppingCartRepository : IShoppingCartRepository
    {
        private DatabaseConnection databaseConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShoppingCartRepository"/> class.
        /// </summary>
        /// <param name="databaseConnection">The database connection to be used by the repository.</param>
        public ShoppingCartRepository(DatabaseConnection databaseConnection)
        {
            this.databaseConnection = databaseConnection;
        }

        /// <summary>
        /// Adds a product to the user's shopping cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer. Must be a positive integer.</param>
        /// <param name="productId">The ID of the product to add to the cart. Must be a positive integer.</param>
        /// <param name="quantity">The quantity of the product to add. Must be greater than 0.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Thrown when buyerId, productId, or quantity is invalid.</exception>
        public async Task AddProductToCartAsync(int buyerId, int productId, int quantity)
        {
            if (buyerId <= 0)
            {
                throw new ArgumentException("Buyer ID must be a positive integer.", nameof(buyerId));
            }

            if (productId <= 0)
            {
                throw new ArgumentException("Product ID must be a positive integer.", nameof(productId));
            }

            if (quantity <= 0)
            {
                throw new ArgumentException("Quantity must be greater than 0.", nameof(quantity));
            }

            await this.databaseConnection.OpenConnection();

            var sqlConnection = this.databaseConnection.GetConnection();
            var sqlCommand = sqlConnection.CreateCommand();

            // First check if the item already exists in the cart
            sqlCommand.CommandText = @"
                SELECT COUNT(*) FROM BuyerCartItems 
                WHERE BuyerId = @BuyerId AND ProductId = @ProductId";

            sqlCommand.Parameters.AddWithValue("@BuyerId", buyerId);
            sqlCommand.Parameters.AddWithValue("@ProductId", productId);

            int exists = (int)await sqlCommand.ExecuteScalarAsync();

            // Clear parameters for reuse
            sqlCommand.Parameters.Clear();

            if (exists > 0)
            {
                // Update the quantity
                sqlCommand.CommandText = @"
                    UPDATE BuyerCartItems 
                    SET Quantity = Quantity + @Quantity 
                    WHERE BuyerId = @BuyerId AND ProductId = @ProductId";

                sqlCommand.Parameters.AddWithValue("@BuyerId", buyerId);
                sqlCommand.Parameters.AddWithValue("@ProductId", productId);
                sqlCommand.Parameters.AddWithValue("@Quantity", quantity);
            }
            else
            {
                // Insert new item
                sqlCommand.CommandText = @"
                    INSERT INTO BuyerCartItems (BuyerId, ProductId, Quantity) 
                    VALUES (@BuyerId, @ProductId, @Quantity)";

                sqlCommand.Parameters.AddWithValue("@BuyerId", buyerId);
                sqlCommand.Parameters.AddWithValue("@ProductId", productId);
                sqlCommand.Parameters.AddWithValue("@Quantity", quantity);
            }

            await sqlCommand.ExecuteNonQueryAsync();
            this.databaseConnection.CloseConnection();
        }

        /// <summary>
        /// Removes a product from the user's shopping cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer. Must be a positive integer.</param>
        /// <param name="productId">The ID of the product to remove from the cart. Must be a positive integer.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Thrown when buyerId or productId is invalid.</exception>
        public async Task RemoveProductFromCartAsync(int buyerId, int productId)
        {
            if (buyerId <= 0)
            {
                throw new ArgumentException("Buyer ID must be a positive integer.", nameof(buyerId));
            }

            if (productId <= 0)
            {
                throw new ArgumentException("Product ID must be a positive integer.", nameof(productId));
            }

            await this.databaseConnection.OpenConnection();

            var sqlConnection = this.databaseConnection.GetConnection();
            var sqlCommand = sqlConnection.CreateCommand();

            sqlCommand.CommandText = @"
                DELETE FROM BuyerCartItems 
                WHERE BuyerId = @BuyerId AND ProductId = @ProductId";

            sqlCommand.Parameters.AddWithValue("@BuyerId", buyerId);
            sqlCommand.Parameters.AddWithValue("@ProductId", productId);

            await sqlCommand.ExecuteNonQueryAsync();
            this.databaseConnection.CloseConnection();
        }

        /// <summary>
        /// Updates the quantity of a product in the user's shopping cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer. Must be a positive integer.</param>
        /// <param name="productId">The ID of the product to update. Must be a positive integer.</param>
        /// <param name="quantity">The new quantity. Must be greater than 0.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Thrown when buyerId, productId, or quantity is invalid.</exception>
        public async Task UpdateProductQuantityAsync(int buyerId, int productId, int quantity)
        {
            if (buyerId <= 0)
            {
                throw new ArgumentException("Buyer ID must be a positive integer.", nameof(buyerId));
            }

            if (productId <= 0)
            {
                throw new ArgumentException("Product ID must be a positive integer.", nameof(productId));
            }

            if (quantity <= 0)
            {
                throw new ArgumentException("Quantity must be greater than 0.", nameof(quantity));
            }

            await this.databaseConnection.OpenConnection();

            var sqlConnection = this.databaseConnection.GetConnection();
            var sqlCommand = sqlConnection.CreateCommand();

            sqlCommand.CommandText = @"
                UPDATE BuyerCartItems 
                SET Quantity = @Quantity 
                WHERE BuyerId = @BuyerId AND ProductId = @ProductId";

            sqlCommand.Parameters.AddWithValue("@BuyerId", buyerId);
            sqlCommand.Parameters.AddWithValue("@ProductId", productId);
            sqlCommand.Parameters.AddWithValue("@Quantity", quantity);

            await sqlCommand.ExecuteNonQueryAsync();
            this.databaseConnection.CloseConnection();
        }

        /// <summary>
        /// Gets all products in the user's shopping cart with their quantities.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer. Must be a positive integer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a dictionary with product objects as keys and quantities as values.</returns>
        /// <exception cref="ArgumentException">Thrown when buyerId is invalid.</exception>
        public async Task<Dictionary<Product, int>> GetCartItemsAsync(int buyerId)
        {
            if (buyerId <= 0)
            {
                throw new ArgumentException("Buyer ID must be a positive integer.", nameof(buyerId));
            }

            var cartItems = new Dictionary<Product, int>();

            await this.databaseConnection.OpenConnection();
            var sqlCommand = this.databaseConnection.GetConnection().CreateCommand();

            sqlCommand.CommandText = @"
                SELECT p.ProductID, p.ProductName, p.ProductDescription, p.ProductPrice, p.ProductType, p.SellerID, ci.Quantity
                FROM BuyerCartItems ci
                INNER JOIN Products p ON ci.ProductId = p.ProductID
                WHERE ci.BuyerId = @BuyerId";

            sqlCommand.Parameters.AddWithValue("@BuyerId", buyerId);

            using (var reader = await sqlCommand.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var product = new Product(
                        reader.GetInt32(reader.GetOrdinal("ProductID")),
                        reader.GetString(reader.GetOrdinal("ProductName")),
                        reader.GetString(reader.GetOrdinal("ProductDescription")),
                        reader.GetDouble(reader.GetOrdinal("ProductPrice")),
                        0, // Category - not present in your structure, using default
                        reader.GetInt32(reader.GetOrdinal("SellerID"))
                    );

                    int quantity = reader.GetInt32(reader.GetOrdinal("Quantity"));
                    cartItems.Add(product, quantity);
                }
            }

            this.databaseConnection.CloseConnection();
            return cartItems;
        }

        /// <summary>
        /// Clears all items from the user's shopping cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer. Must be a positive integer.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Thrown when buyerId is invalid.</exception>
        public async Task ClearCartAsync(int buyerId)
        {
            if (buyerId <= 0)
            {
                throw new ArgumentException("Buyer ID must be a positive integer.", nameof(buyerId));
            }

            await this.databaseConnection.OpenConnection();

            var sqlConnection = this.databaseConnection.GetConnection();
            var sqlCommand = sqlConnection.CreateCommand();

            sqlCommand.CommandText = "DELETE FROM BuyerCartItems WHERE BuyerId = @BuyerId";
            sqlCommand.Parameters.AddWithValue("@BuyerId", buyerId);

            await sqlCommand.ExecuteNonQueryAsync();
            this.databaseConnection.CloseConnection();
        }

        /// <summary>
        /// Gets the total number of items in the user's shopping cart (sum of all quantities).
        /// </summary>
        /// <param name="buyerId">The ID of the buyer. Must be a positive integer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the total number of items.</returns>
        /// <exception cref="ArgumentException">Thrown when buyerId is invalid.</exception>
        public async Task<int> GetCartItemCountAsync(int buyerId)
        {
            if (buyerId <= 0)
            {
                throw new ArgumentException("Buyer ID must be a positive integer.", nameof(buyerId));
            }

            await this.databaseConnection.OpenConnection();
            var sqlCommand = this.databaseConnection.GetConnection().CreateCommand();

            sqlCommand.CommandText = @"
                SELECT SUM(Quantity)
                FROM BuyerCartItemspa
                WHERE BuyerId = @BuyerId";

            sqlCommand.Parameters.AddWithValue("@BuyerId", buyerId);

            var result = await sqlCommand.ExecuteScalarAsync();
            int totalItems = 0;

            if (result != null && result != DBNull.Value)
            {
                totalItems = Convert.ToInt32(result);
            }

            this.databaseConnection.CloseConnection();
            return totalItems;
        }

        /// <summary>
        /// Checks if a specific product is in the buyer's cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer. Must be a positive integer.</param>
        /// <param name="productId">The ID of the product to check. Must be a positive integer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the product is in the cart, otherwise false.</returns>
        /// <exception cref="ArgumentException">Thrown when buyerId or productId is invalid.</exception>
        public async Task<bool> IsProductInCartAsync(int buyerId, int productId)
        {
            if (buyerId <= 0)
            {
                throw new ArgumentException("Buyer ID must be a positive integer.", nameof(buyerId));
            }

            if (productId <= 0)
            {
                throw new ArgumentException("Product ID must be a positive integer.", nameof(productId));
            }

            await this.databaseConnection.OpenConnection();
            var sqlCommand = this.databaseConnection.GetConnection().CreateCommand();

            sqlCommand.CommandText = @"
                SELECT COUNT(*)
                FROM BuyerCartItems
                WHERE BuyerId = @BuyerId AND ProductId = @ProductId";

            sqlCommand.Parameters.AddWithValue("@BuyerId", buyerId);
            sqlCommand.Parameters.AddWithValue("@ProductId", productId);

            var result = await sqlCommand.ExecuteScalarAsync();
            int count = 0;

            if (result != null && result != DBNull.Value)
            {
                count = Convert.ToInt32(result);
            }

            this.databaseConnection.CloseConnection();
            return count > 0;
        }

        /// <summary>
        /// Gets the quantity of a specific product in the buyer's cart.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer. Must be a positive integer.</param>
        /// <param name="productId">The ID of the product to check. Must be a positive integer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is the quantity of the product in the cart, or 0 if it's not in the cart.</returns>
        /// <exception cref="ArgumentException">Thrown when buyerId or productId is invalid.</exception>
        public async Task<int> GetProductQuantityAsync(int buyerId, int productId)
        {
            if (buyerId <= 0)
            {
                throw new ArgumentException("Buyer ID must be a positive integer.", nameof(buyerId));
            }

            if (productId <= 0)
            {
                throw new ArgumentException("Product ID must be a positive integer.", nameof(productId));
            }

            await this.databaseConnection.OpenConnection();
            var sqlCommand = this.databaseConnection.GetConnection().CreateCommand();

            sqlCommand.CommandText = @"
                SELECT Quantity
                FROM BuyerCartItems
                WHERE BuyerId = @BuyerId AND ProductId = @ProductId";

            sqlCommand.Parameters.AddWithValue("@BuyerId", buyerId);
            sqlCommand.Parameters.AddWithValue("@ProductId", productId);

            var result = await sqlCommand.ExecuteScalarAsync();
            int quantity = 0;

            if (result != null && result != DBNull.Value)
            {
                quantity = Convert.ToInt32(result);
            }

            this.databaseConnection.CloseConnection();
            return quantity;
        }
    }
}
