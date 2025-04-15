// -----------------------------------------------------------------------
// <copyright file="SellerRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SharedClassLibrary.IRepository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Data.SqlClient;
    using Server.DBConnection;
    using SharedClassLibrary.Domain;

    /// <summary>
    /// Repository for managing seller-related data operations.
    /// </summary>
    public class SellerRepository : ISellerRepository
    {
        private DatabaseConnection connection;
        private IUserRepository userRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SellerRepository"/> class.
        /// </summary>
        /// <param name="connection">The database connection to be used.</param>
        /// <param name="userRepository">The user repository to be used.</param>
        public SellerRepository(DatabaseConnection connection, IUserRepository userRepository)
        {
            this.connection = connection;
            this.userRepository = userRepository;
        }

        /// <summary>
        /// Gets the seller information for a given user.
        /// </summary>
        /// <param name="user">The user whose seller information is to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the seller information.</returns>
        public async Task<Seller> GetSellerInfo(User user)
        {
            var seller = new Seller(user);

            await this.connection.OpenConnection();
            var command = this.connection.GetConnection().CreateCommand();
            command.CommandText = @"
                SELECT * FROM Sellers
                WHERE UserId = @UserID";
            command.Parameters.AddWithValue("@UserID", seller.Id);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (!await reader.ReadAsync())
                {
                    this.connection.CloseConnection();
                    return seller;
                }

                seller.StoreName = reader.GetString(reader.GetOrdinal("StoreName"));
                seller.StoreDescription = reader.GetString(reader.GetOrdinal("StoreDescription"));
                seller.StoreAddress = reader.GetString(reader.GetOrdinal("StoreAddress"));
                seller.FollowersCount = reader.GetInt32(reader.GetOrdinal("FollowersCount"));
                seller.TrustScore = reader.GetDouble(reader.GetOrdinal("TrustScore"));
            }

            this.connection.CloseConnection();
            return seller;
        }

        /// <summary>
        /// Gets the notifications for a given seller.
        /// </summary>
        /// <param name="sellerID">The ID of the seller whose notifications are to be retrieved.</param>
        /// <param name="maxNotifications">The maximum number of notifications to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of notifications.</returns>
        public async Task<List<string>> GetNotifications(int sellerID, int maxNotifications)
        {
            await this.connection.OpenConnection();
            var command = this.connection.GetConnection().CreateCommand();
            command.CommandText = "SELECT TOP(@MaxNotifications) NotificationMessage FROM Notifications WHERE SellerID = @SellerID";
            command.Parameters.Add(new SqlParameter("@MaxNotifications", maxNotifications));
            command.Parameters.Add(new SqlParameter("@SellerID", sellerID));
            var notifications = new List<string>();
            using (var reader = await command.ExecuteReaderAsync())
            {
                int notificationMessageColumn = reader.GetOrdinal("NotificationMessage");
                while (await reader.ReadAsync())
                {
                    notifications.Add(reader.GetString(notificationMessageColumn));
                }
            }

            this.connection.CloseConnection();
            return notifications;
        }

        /// <summary>
        /// Updates the seller information.
        /// </summary>
        /// <param name="seller">The seller whose information is to be updated.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task UpdateSeller(Seller seller)
        {
            await this.connection.OpenConnection();
            var command = this.connection.GetConnection().CreateCommand();

            command.CommandText = @"
                        UPDATE Sellers 
                        SET StoreName = @StoreName, StoreDescription = @StoreDescription, StoreAddress = @StoreAddress, FollowersCount = @FollowersCount, TrustScore = @TrustScore 
                        WHERE UserID = @UserID";
            command.Parameters.Add(new SqlParameter("@StoreName", seller.StoreName));
            command.Parameters.Add(new SqlParameter("@StoreDescription", seller.StoreDescription));
            command.Parameters.Add(new SqlParameter("@StoreAddress", seller.StoreAddress));
            command.Parameters.Add(new SqlParameter("@FollowersCount", seller.FollowersCount));
            command.Parameters.Add(new SqlParameter("@TrustScore", seller.TrustScore));
            command.Parameters.Add(new SqlParameter("@UserID", seller.Id));
            await command.ExecuteNonQueryAsync();
            this.connection.CloseConnection();
        }

        /// <summary>
        /// Gets the products for a given seller.
        /// </summary>
        /// <param name="sellerID">The ID of the seller whose products are to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of products.</returns>
        public async Task<List<Product>> GetProducts(int sellerID)
        {
            await this.connection.OpenConnection();
            var command = this.connection.GetConnection().CreateCommand();
            command.CommandText = "SELECT * FROM Products WHERE SellerID = @SellerID";
            command.Parameters.Add(new SqlParameter("@SellerID", sellerID));
            var products = new List<Product>();
            using (var reader = await command.ExecuteReaderAsync())
            {
                int productIdColumn = reader.GetOrdinal("ProductID");
                int productNameColumn = reader.GetOrdinal("ProductName");
                int productDescriptionColumn = reader.GetOrdinal("ProductDescription");
                int productPriceColumn = reader.GetOrdinal("ProductPrice");
                int productStockColumn = reader.GetOrdinal("ProductStock");
                while (await reader.ReadAsync())
                {
                    var productID = reader.GetInt32(productIdColumn);
                    var productName = reader.GetString(productNameColumn);
                    var productDescription = reader.GetString(productDescriptionColumn);
                    var productPrice = reader.GetDouble(productPriceColumn);
                    var productStock = reader.GetInt32(productStockColumn);
                    products.Add(new Product(productID, productName, productDescription, productPrice, productStock, sellerID));
                }
            }

            this.connection.CloseConnection();
            return products;
        }

        /// <summary>
        /// Adds a new seller.
        /// </summary>
        /// <param name="seller">The seller to be added.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task AddSeller(Seller seller)
        {
            await this.connection.OpenConnection();
            var command = this.connection.GetConnection().CreateCommand();
            command.CommandText = @"INSERT INTO Sellers (UserId, Username, StoreName, StoreDescription, StoreAddress, FollowersCount, TrustScore)
                            VALUES 
                                    (@UserId, @Username, @StoreName, @StoreDescription, @StoreAddress, 0, 0)";
            command.Parameters.AddWithValue("@UserId", seller.Id);
            command.Parameters.AddWithValue("@Username", seller.Username);
            command.Parameters.AddWithValue("@StoreName", string.Empty);
            command.Parameters.AddWithValue("@StoreDescription", string.Empty);
            command.Parameters.AddWithValue("@StoreAddress", string.Empty);
            await command.ExecuteNonQueryAsync();
            this.connection.CloseConnection();
        }

        /// <summary>
        /// Gets the reviews for a given seller.
        /// </summary>
        /// <param name="sellerId">The ID of the seller whose reviews are to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of reviews.</returns>
        public async Task<List<Review>> GetReviews(int sellerId)
        {
            var reviews = new List<Review>();
            await this.connection.OpenConnection();
            var command = this.connection.GetConnection().CreateCommand();
            command.CommandText = "SELECT * FROM Reviews WHERE SellerID = @SellerID";
            command.Parameters.Add(new SqlParameter("@SellerID", sellerId));

            using (var reader = await command.ExecuteReaderAsync())
            {
                int reviewIdColumn = reader.GetOrdinal("ReviewId");
                int scoreColumn = reader.GetOrdinal("Score");
                while (await reader.ReadAsync())
                {
                    var reviewId = reader.GetInt32(reviewIdColumn);
                    var score = reader.GetDouble(scoreColumn);
                    var review = new Review(reviewId, sellerId, score);
                    reviews.Add(review);
                }
            }

            this.connection.CloseConnection();
            return reviews;
        }

        /// <summary>
        /// Updates the trust score for a given seller.
        /// </summary>
        /// <param name="sellerId">The ID of the seller whose trust score is to be updated.</param>
        /// <param name="trustScore">The new trust score.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task UpdateTrustScore(int sellerId, double trustScore)
        {
            await this.connection.OpenConnection();
            var command = this.connection.GetConnection().CreateCommand();
            command.CommandText = "UPDATE Sellers Set TrustScore=@TrustScore WHERE UserId=@UserId";
            command.Parameters.Add(new SqlParameter("@TrustScore", trustScore));
            command.Parameters.Add(new SqlParameter("@UserId", sellerId));
            await command.ExecuteNonQueryAsync();
            this.connection.CloseConnection();
        }

        /// <summary>
        /// Gets the last follower count for a given seller.
        /// </summary>
        /// <param name="sellerId">The ID of the seller whose last follower count is to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the last follower count.</returns>
        public async Task<int> GetLastFollowerCount(int sellerId)
        {
            await this.connection.OpenConnection();
            int lastFollowerCount = 0;

            // Get the last follower count from the latest notification
            var command = this.connection.GetConnection().CreateCommand();
            command.CommandText = "SELECT TOP 1 NotificationFollowerCount FROM Notifications WHERE SellerID = @SellerId ORDER BY NotificationID DESC";
            command.Parameters.AddWithValue("@SellerId", sellerId);
            object? result = await command.ExecuteScalarAsync();

            if (result != null)
            {
                lastFollowerCount = Convert.ToInt32(result);
            }

            this.connection.CloseConnection();
            return lastFollowerCount;
        }

        /// <summary>
        /// Adds a new follower notification for a given seller.
        /// </summary>
        /// <param name="sellerId">The ID of the seller.</param>
        /// <param name="currentFollowerCount">The current follower count.</param>
        /// <param name="message">The notification message.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task AddNewFollowerNotification(int sellerId, int currentFollowerCount, string message)
        {
            await this.connection.OpenConnection();

            // Insert the new notification
            var command = this.connection.GetConnection().CreateCommand();
            command.CommandText = "INSERT INTO Notifications (SellerID, NotificationMessage, NotificationFollowerCount) VALUES (@SellerID, @NotificationMessage, @NotificationFollowerCount)";
            command.Parameters.AddWithValue("@SellerId", sellerId);
            command.Parameters.AddWithValue("@NotificationMessage", message);
            command.Parameters.AddWithValue("@NotificationFollowerCount", currentFollowerCount);
            await command.ExecuteNonQueryAsync();
            this.connection.CloseConnection();
        }
    }
}
