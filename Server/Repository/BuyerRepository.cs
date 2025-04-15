// <copyright file="BuyerRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SharedClassLibrary.IRepository
{
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using MarketPlace924.DBConnection;
    using Microsoft.Data.SqlClient;

    /// <summary>
    /// Repository class for managing buyer-related database operations.
    /// </summary>
    /// <param name="databaseConnection">The database connection instance.</param>
    public class BuyerRepository(DatabaseConnection databaseConnection) : IBuyerRepository
    {
        private readonly DatabaseConnection databaseConnection = databaseConnection;

        /// <inheritdoc/>
        public async Task LoadBuyerInfo(Buyer buyerEntity)
        {
            await this.databaseConnection.OpenConnection();
            var sqlConnection = this.databaseConnection.GetConnection();
            var sqlSelectBuyersCommand = sqlConnection.CreateCommand();
            sqlSelectBuyersCommand.CommandText = @"select 
                FirstName, LastName, Badge, 
                TotalSpending, NumberOfPurchases, Discount, 
                ShippingAddressId, BillingAddressId, UseSameAddress
            from Buyers where UserID = @userID";
            sqlSelectBuyersCommand.Parameters.AddWithValue("@userID", buyerEntity.Id);
            var sqlDataReader = await sqlSelectBuyersCommand.ExecuteReaderAsync();
            if (!await sqlDataReader.ReadAsync())
            {
                await sqlDataReader.CloseAsync();
                return;
            }

            var useSameAddress = sqlDataReader.GetBoolean(sqlDataReader.GetOrdinal("UseSameAddress"));
            var billingAddressId = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("BillingAddressId"));
            var shippingAddressId = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("ShippingAddressId"));

            buyerEntity.FirstName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("FirstName"));
            buyerEntity.LastName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("LastName"));
            buyerEntity.Badge = this.ParseBadge(sqlDataReader.GetString(sqlDataReader.GetOrdinal("Badge")));
            buyerEntity.TotalSpending = sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("TotalSpending"));
            buyerEntity.NumberOfPurchases = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("NumberOfPurchases"));
            buyerEntity.Discount = sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("Discount"));
            buyerEntity.UseSameAddress = useSameAddress;

            await sqlDataReader.CloseAsync();

            var billingAddress = (await this.LoadAddress(billingAddressId, sqlConnection)) !;
            buyerEntity.BillingAddress = billingAddress;

            if (useSameAddress)
            {
                buyerEntity.ShippingAddress = billingAddress;
            }
            else
            {
                buyerEntity.ShippingAddress = (await this.LoadAddress(shippingAddressId, sqlConnection)) !;
            }

            // FollowingUsersIds For My Market
            var sqlSelectFollowingCommand = this.databaseConnection.GetConnection().CreateCommand();
            sqlSelectFollowingCommand.CommandText = "SELECT FollowedID FROM Following WHERE FollowerID = @FollowerID";
            sqlSelectFollowingCommand.Parameters.AddWithValue("@FollowerID", buyerEntity.Id);

            List<int> sellersIDs = new List<int>();
            using (sqlDataReader = sqlSelectFollowingCommand.ExecuteReader())
            {
                while (sqlDataReader.Read())
                {
                    sellersIDs.Add(Convert.ToInt32(sqlDataReader["FollowedID"]));
                }
            }

            buyerEntity.FollowingUsersIds = sellersIDs;

            this.databaseConnection.CloseConnection();
        }

        /// <inheritdoc/>
        public async Task SaveInfo(Buyer buyerEntity)
        {
            await this.databaseConnection.OpenConnection();
            var sqlConnection = this.databaseConnection.GetConnection();
            var sqlCommand = sqlConnection.CreateCommand();

            // Check if buyer exists
            sqlCommand.CommandText = "SELECT COUNT(*) FROM Buyers WHERE UserId = @UserId";
            sqlCommand.Parameters.AddWithValue("@UserId", buyerEntity.Id);

            var sqlResult = await sqlCommand.ExecuteScalarAsync();
            var doesBuyerExist = (sqlResult != null) && (Convert.ToInt32(sqlResult) > 0);

            // Handle addresses
            if (!buyerEntity.UseSameAddress)
            {
                await this.PersistAddress(buyerEntity.BillingAddress, sqlConnection);
                await this.PersistAddress(buyerEntity.ShippingAddress, sqlConnection);
            }
            else
            {
                await this.PersistAddress(buyerEntity.BillingAddress, sqlConnection);
                buyerEntity.ShippingAddress = buyerEntity.BillingAddress;
            }

            // Clear parameters for the next command
            sqlCommand.Parameters.Clear();

            if (doesBuyerExist)
            {
                // Update existing buyer
                sqlCommand.CommandText = @"UPDATE Buyers 
                                SET FirstName = @FirstName,
                                    LastName = @LastName,
                                    BillingAddressId = @BillingAddressId,
                                    ShippingAddressId = @ShippingAddressId,
                                    UseSameAddress = @UseSameAddress,
                                    Badge = @Badge,
                                    TotalSpending = @TotalSpending,
                                    NumberOfPurchases = @NumberOfPurchases,
                                    Discount = @Discount
                                WHERE UserId = @UserId";
            }
            else
            {
                // Insert new buyer
                sqlCommand.CommandText = @"INSERT INTO Buyers (UserId, FirstName, LastName, BillingAddressId, ShippingAddressId, UseSameAddress, Badge,
                                TotalSpending, NumberOfPurchases, Discount)
                                VALUES (@UserId, @FirstName, @LastName, @BillingAddressId, @ShippingAddressId, @UseSameAddress, @Badge, @TotalSpending, @NumberOfPurchases, @Discount)";
            }

            sqlCommand.Parameters.AddWithValue("@UserId", buyerEntity.Id);
            sqlCommand.Parameters.AddWithValue("@FirstName", buyerEntity.FirstName);
            sqlCommand.Parameters.AddWithValue("@LastName", buyerEntity.LastName);
            sqlCommand.Parameters.AddWithValue("@BillingAddressId", buyerEntity.BillingAddress.Id);
            sqlCommand.Parameters.AddWithValue("@ShippingAddressId", buyerEntity.ShippingAddress.Id);
            sqlCommand.Parameters.AddWithValue("@UseSameAddress", buyerEntity.UseSameAddress);
            sqlCommand.Parameters.AddWithValue("@Badge", buyerEntity.Badge.ToString());
            sqlCommand.Parameters.AddWithValue("@TotalSpending", buyerEntity.TotalSpending);
            sqlCommand.Parameters.AddWithValue("@NumberOfPurchases", buyerEntity.NumberOfPurchases);
            sqlCommand.Parameters.AddWithValue("@Discount", buyerEntity.Discount);
            await sqlCommand.ExecuteNonQueryAsync();
        }

        /// <inheritdoc/>
        public async Task<BuyerWishlist> GetWishlist(int userId)
        {
            await this.databaseConnection.OpenConnection();
            var sqlConnection = this.databaseConnection.GetConnection();
            var sqlSelectWishlistCommand = sqlConnection.CreateCommand();
            sqlSelectWishlistCommand.CommandText = @"SELECT ProductId from BuyerWishlistItems WHERE BuyerId = @userId";
            sqlSelectWishlistCommand.Parameters.AddWithValue("@userId", userId);
            var sqlDataReader = await sqlSelectWishlistCommand.ExecuteReaderAsync();
            var buyerWishlist = new BuyerWishlist();
            while (await sqlDataReader.ReadAsync())
            {
                buyerWishlist.Items.Add(new BuyerWishlistItem(sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("ProductId"))));
            }

            await sqlDataReader.CloseAsync();
            return buyerWishlist;
        }

        /// <inheritdoc/>
        public async Task<List<BuyerLinkage>> GetBuyerLinkages(int userId)
        {
            await this.databaseConnection.OpenConnection();
            var sqlConnection = this.databaseConnection.GetConnection();
            var sqlSelectBuyerLinkagesCommand = sqlConnection.CreateCommand();
            sqlSelectBuyerLinkagesCommand.CommandText = @"SELECT RequestingBuyerId, ReceivingBuyerId, IsApproved
                            FROM BuyerLinkage 
                            WHERE RequestingBuyerId = @userId OR  ReceivingBuyerId =@userId";
            sqlSelectBuyerLinkagesCommand.Parameters.AddWithValue("@userId", userId);
            var sqlDataReader = await sqlSelectBuyerLinkagesCommand.ExecuteReaderAsync();
            var buyerLinkages = new List<BuyerLinkage>();
            while (await sqlDataReader.ReadAsync())
            {
                buyerLinkages.Add(ReadBuyerLinkage(sqlDataReader, userId));
            }

            await sqlDataReader.CloseAsync();
            return buyerLinkages;
        }

        /// <inheritdoc/>
        public async Task CreateLinkageRequest(int requestingBuyerId, int receivingBuyerId)
        {
            await this.databaseConnection.OpenConnection();
            var sqlInsertLinkageCommand = this.databaseConnection.GetConnection().CreateCommand();
            sqlInsertLinkageCommand.CommandText = @"INSERT INTO BuyerLinkage(RequestingBuyerId, ReceivingBuyerId, IsApproved)
                            VALUES (@RequestingBuyerId, @ReceivingBuyerId, @IsApproved);";
            sqlInsertLinkageCommand.Parameters.AddWithValue("@RequestingBuyerId", requestingBuyerId);
            sqlInsertLinkageCommand.Parameters.AddWithValue("@ReceivingBuyerId", receivingBuyerId);
            sqlInsertLinkageCommand.Parameters.AddWithValue("@IsApproved", false);

            await sqlInsertLinkageCommand.ExecuteNonQueryAsync();
        }

        /// <inheritdoc/>
        public async Task UpdateLinkageRequest(int requestingBuyerId, int receivingBuyerId)
        {
            await this.databaseConnection.OpenConnection();
            var sqlUpdateLinkageCommand = this.databaseConnection.GetConnection().CreateCommand();
            sqlUpdateLinkageCommand.CommandText = @"UPDATE BuyerLinkage 
                            SET IsApproved=@IsApproved
                            WHERE RequestingBuyerId=@RequestingBuyerId 
                              AND ReceivingBuyerId=@ReceivingBuyerId;";
            sqlUpdateLinkageCommand.Parameters.AddWithValue("@RequestingBuyerId", requestingBuyerId);
            sqlUpdateLinkageCommand.Parameters.AddWithValue("@ReceivingBuyerId", receivingBuyerId);
            sqlUpdateLinkageCommand.Parameters.AddWithValue("@IsApproved", true);

            await sqlUpdateLinkageCommand.ExecuteNonQueryAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteLinkageRequest(int requestingBuyerId, int receivingBuyerId)
        {
            await this.databaseConnection.OpenConnection();
            var sqlDeleteLinkageCommand = this.databaseConnection.GetConnection().CreateCommand();
            sqlDeleteLinkageCommand.CommandText = @"DELETE FROM BuyerLinkage 
                            WHERE RequestingBuyerId=@RequestingBuyerId
                              AND ReceivingBuyerId=@ReceivingBuyerId;";
            sqlDeleteLinkageCommand.Parameters.AddWithValue("@RequestingBuyerId", requestingBuyerId);
            sqlDeleteLinkageCommand.Parameters.AddWithValue("@ReceivingBuyerId", receivingBuyerId);

            return await sqlDeleteLinkageCommand.ExecuteNonQueryAsync() > 0;
        }

        /// <inheritdoc/>
        public async Task<List<Buyer>> FindBuyersWithShippingAddress(Address shippingAddress)
        {
            await this.databaseConnection.OpenConnection();
            var sqlFindBuyersCommand = this.databaseConnection.GetConnection().CreateCommand();
            sqlFindBuyersCommand.CommandText = @"
            SELECT b.UserId FROM Buyers b
            INNER JOIN BuyerAddress BA on BA.Id = b.ShippingAddressId
            WHERE lower(BA.StreetLine) = lower(@StreetLine) 
            AND lower(BA.City) = lower(@City)
            AND lower(BA.Country) = lower(@Country)
            AND lower(BA.PostalCode) = lower(@PostalCode)
            ";
            sqlFindBuyersCommand.Parameters.AddWithValue("@StreetLine", shippingAddress.StreetLine);
            sqlFindBuyersCommand.Parameters.AddWithValue("@City", shippingAddress.City);
            sqlFindBuyersCommand.Parameters.AddWithValue("@Country", shippingAddress.Country);
            sqlFindBuyersCommand.Parameters.AddWithValue("@PostalCode", shippingAddress.PostalCode);
            var sqlDataReader = await sqlFindBuyersCommand.ExecuteReaderAsync();
            var buyerUserIds = new List<int>();
            while (await sqlDataReader.ReadAsync())
            {
                buyerUserIds.Add(sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("UserId")));
            }

            await sqlDataReader.CloseAsync();
            var buyerList = new List<Buyer>();
            foreach (var buyerId in buyerUserIds)
            {
                buyerList.Add(new Buyer
                {
                    User = new User { UserId = buyerId },
                });
            }

            return buyerList;
        }

        /// <inheritdoc/>
        public async Task CreateBuyer(Buyer buyerEntity)
        {
            await this.databaseConnection.OpenConnection();
            var sqlConnection = this.databaseConnection.GetConnection();
            var sqlCreateBuyerCommand = sqlConnection.CreateCommand();

            // Always persist the billing address first
            await this.PersistAddress(buyerEntity.BillingAddress, sqlConnection);

            // If UseSameAddress is true, we don't need to persist a separate shipping address
            // and we'll use the same ID for both
            if (!buyerEntity.UseSameAddress)
            {
                await this.PersistAddress(buyerEntity.ShippingAddress, sqlConnection);
            }
            else
            {
                buyerEntity.ShippingAddress = buyerEntity.BillingAddress;
            }

            sqlCreateBuyerCommand.CommandText =
                @"INSERT INTO Buyers (UserId, FirstName, LastName, BillingAddressId, ShippingAddressId, UseSameAddress, Badge,
                TotalSpending, NumberOfPurchases, Discount)
                            VALUES (@UserID, @FirstName, @LastName,  @BillingAddressId, @ShippingAddressId, @UseSameAddress, @Badge, 0, 0,0) ";

            sqlCreateBuyerCommand.Parameters.AddWithValue("@UserId", buyerEntity.Id);
            sqlCreateBuyerCommand.Parameters.AddWithValue("@FirstName", buyerEntity.FirstName);
            sqlCreateBuyerCommand.Parameters.AddWithValue("@LastName", buyerEntity.LastName);
            sqlCreateBuyerCommand.Parameters.AddWithValue("@BillingAddressId", buyerEntity.BillingAddress.Id);
            sqlCreateBuyerCommand.Parameters.AddWithValue("@UseSameAddress", buyerEntity.UseSameAddress);
            sqlCreateBuyerCommand.Parameters.AddWithValue("@ShippingAddressId", buyerEntity.ShippingAddress.Id);
            sqlCreateBuyerCommand.Parameters.AddWithValue("@Badge", buyerEntity.Badge.ToString());
            await sqlCreateBuyerCommand.ExecuteNonQueryAsync();
        }

        /// <inheritdoc/>
        public async Task<List<int>> GetFollowingUsersIds(int buyerId)
        {
            List<int> followingUsersIDs = new List<int>();

            await this.databaseConnection.OpenConnection();

            var sqlSelectFollowersCommand = this.databaseConnection.GetConnection().CreateCommand();
            sqlSelectFollowersCommand.CommandText = "SELECT FollowedID FROM Following WHERE FollowerID = @FollowerID";
            sqlSelectFollowersCommand.Parameters.AddWithValue("@FollowerID", buyerId);

            using (var sqlDataReader = await sqlSelectFollowersCommand.ExecuteReaderAsync())
            {
                while (await sqlDataReader.ReadAsync())
                {
                    followingUsersIDs.Add(Convert.ToInt32(sqlDataReader["FollowedID"]));
                }
            }

            this.databaseConnection.CloseConnection();

            return followingUsersIDs;
        }

        /// <inheritdoc/>
        public async Task<List<Seller>> GetFollowedSellers(List<int>? followingUsersIds)
        {
            if (followingUsersIds == null || followingUsersIds.Count == 0)
            {
                return new List<Seller>();
            }

            await this.databaseConnection.OpenConnection();

            var sqlFollowedSellersCommand = this.databaseConnection.GetConnection().CreateCommand();

            // Add the followed seller IDs to the parameter (using the list of IDs)
            string formattedSellersIds = string.Join(",", followingUsersIds);
            sqlFollowedSellersCommand.CommandText = $"SELECT u.Email, u.PhoneNumber, s.* " +
                                  $"FROM Users u " +
                                  $"INNER JOIN Sellers s " +
                                  $"ON u.UserId = s.UserId " +
                                  $"WHERE s.UserId IN ({formattedSellersIds})";

            List<Seller> followedSellersList = new List<Seller>();
            using (var sqlDataReader = await sqlFollowedSellersCommand.ExecuteReaderAsync())
            {
                while (await sqlDataReader.ReadAsync())
                {
                    var sellerEmail = sqlDataReader.IsDBNull(0) ? string.Empty : sqlDataReader.GetString(0);
                    var sellerPhoneNumber = sqlDataReader.IsDBNull(1) ? string.Empty : sqlDataReader.GetString(1);
                    var sellerId = sqlDataReader.GetInt32(2);
                    var username = sqlDataReader.IsDBNull(3) ? string.Empty : sqlDataReader.GetString(3);
                    var storeName = sqlDataReader.IsDBNull(4) ? string.Empty : sqlDataReader.GetString(4);
                    var storeDescription = sqlDataReader.IsDBNull(5) ? string.Empty : sqlDataReader.GetString(5);
                    var storeAddress = sqlDataReader.IsDBNull(6) ? string.Empty : sqlDataReader.GetString(6);
                    var followersCount = sqlDataReader.IsDBNull(7) ? 0 : sqlDataReader.GetInt32(7);
                    var trustScore = sqlDataReader.IsDBNull(8) ? 0.0 : sqlDataReader.GetDouble(8);

                    var user = new User(userID: sellerId, username: username, email: sellerEmail, phoneNumber: sellerPhoneNumber);
                    Seller seller = new Seller(user, storeName, storeDescription, storeAddress, followersCount, trustScore);

                    followedSellersList.Add(seller);
                }
            }

            this.databaseConnection.CloseConnection();
            return followedSellersList;
        }

        /// <inheritdoc/>
        public async Task<List<Seller>> GetAllSellers()
        {
            await this.databaseConnection.OpenConnection();

            var sqlGetAllSellersCommand = this.databaseConnection.GetConnection().CreateCommand();

            sqlGetAllSellersCommand.CommandText = $"SELECT u.Email, u.PhoneNumber, s.UserId, s.Username, s.StoreName, s.StoreDescription, s.StoreAddress, s.FollowersCount, s.TrustScore " +
                                  $"FROM Users u " +
                                  $"INNER JOIN Sellers s " +
                                  $"ON u.UserId = s.UserId ";

            List<Seller> allSellersList = new List<Seller>();
            using (var sqlDataReader = await sqlGetAllSellersCommand.ExecuteReaderAsync())
            {
                while (await sqlDataReader.ReadAsync())
                {
                    var sellerEmail = sqlDataReader.IsDBNull(0) ? string.Empty : sqlDataReader.GetString(0);
                    var sellerPhoneNumber = sqlDataReader.IsDBNull(1) ? string.Empty : sqlDataReader.GetString(1);
                    var sellerId = sqlDataReader.GetInt32(2);
                    var username = sqlDataReader.IsDBNull(3) ? string.Empty : sqlDataReader.GetString(3);
                    var storeName = sqlDataReader.IsDBNull(4) ? string.Empty : sqlDataReader.GetString(4);
                    var storeDescription = sqlDataReader.IsDBNull(5) ? string.Empty : sqlDataReader.GetString(5);
                    var storeAddress = sqlDataReader.IsDBNull(6) ? string.Empty : sqlDataReader.GetString(6);
                    var followersCount = sqlDataReader.IsDBNull(7) ? 0 : sqlDataReader.GetInt32(7);
                    var trustScore = sqlDataReader.IsDBNull(8) ? 0.0 : sqlDataReader.GetDouble(8);

                    var user = new User(userID: sellerId, username: username, email: sellerEmail, phoneNumber: sellerPhoneNumber);
                    Seller seller = new Seller(user, storeName, storeDescription, storeAddress, followersCount, trustScore);

                    allSellersList.Add(seller);
                }
            }

            this.databaseConnection.CloseConnection();
            return allSellersList;
        }

        /// <inheritdoc/>
        public async Task<List<Product>> GetProductsFromSeller(int sellerId)
        {
            await this.databaseConnection.OpenConnection();

            var sqlGetProductsCommand = this.databaseConnection.GetConnection().CreateCommand();
            sqlGetProductsCommand.CommandText = "SELECT * FROM Products WHERE SellerID = @SellerID";
            sqlGetProductsCommand.Parameters.AddWithValue("@SellerID", sellerId);

            List<Product> sellerProductList = new List<Product>();

            using (var sqlDataReader = await sqlGetProductsCommand.ExecuteReaderAsync())
            {
                while (await sqlDataReader.ReadAsync())
                {
                    var productId = sqlDataReader.GetInt32(0);
                    var productName = sqlDataReader.GetString(2);
                    var productDescription = sqlDataReader.GetString(3);
                    var productPrice = sqlDataReader.GetDouble(4);

                    sellerProductList.Add(new Product(productId, productName, productDescription, productPrice, 0, sellerId));
                }
            }

            this.databaseConnection.CloseConnection();
            return sellerProductList;
        }

        /// <inheritdoc/>
        public async Task<bool> CheckIfBuyerExists(int buyerId)
        {
            await this.databaseConnection.OpenConnection();
            var sqlCheckBuyerExistsCommand = this.databaseConnection.GetConnection().CreateCommand();
            sqlCheckBuyerExistsCommand.CommandText = "SELECT COUNT(*) FROM Buyers WHERE UserId = @UserId";
            sqlCheckBuyerExistsCommand.Parameters.AddWithValue("@UserId", buyerId);

            var buyerExistsCount = await sqlCheckBuyerExistsCommand.ExecuteScalarAsync();
            this.databaseConnection.CloseConnection();

            // Return true if a record exists (i.e., count > 0)
            return Convert.ToInt32(buyerExistsCount) > 0;
        }

        /// <inheritdoc/>
        public async Task<bool> IsFollowing(int buyerId, int sellerId)
        {
            await this.databaseConnection.OpenConnection();
            var sqlIsFollowingCommand = this.databaseConnection.GetConnection().CreateCommand();
            sqlIsFollowingCommand.CommandText =
                "SELECT COUNT(*) FROM Following WHERE FollowerID = @FollowerID AND FollowedID = @FollowedID";
            sqlIsFollowingCommand.Parameters.AddWithValue("@FollowerID", buyerId);
            sqlIsFollowingCommand.Parameters.AddWithValue("@FollowedID", sellerId);

            var sqlIsFollowingResult = await sqlIsFollowingCommand.ExecuteScalarAsync();
            this.databaseConnection.CloseConnection();

            // Return true if a record exists (i.e., count > 0)
            return Convert.ToInt32(sqlIsFollowingResult) > 0;
        }

        /// <inheritdoc/>
        public async Task FollowSeller(int buyerId, int sellerId)
        {
            await this.databaseConnection.OpenConnection();

            var sqlFollowSellerCommand = this.databaseConnection.GetConnection().CreateCommand();
            sqlFollowSellerCommand.CommandText = "INSERT INTO Following (FollowerID, FollowedID) VALUES (@FollowerID, @FollowedID)";
            sqlFollowSellerCommand.Parameters.AddWithValue("@FollowerID", buyerId);
            sqlFollowSellerCommand.Parameters.AddWithValue("@FollowedID", sellerId);
            await sqlFollowSellerCommand.ExecuteNonQueryAsync();

            sqlFollowSellerCommand = this.databaseConnection.GetConnection().CreateCommand();
            sqlFollowSellerCommand.CommandText = "UPDATE Sellers SET FollowersCount = FollowersCount + 1 WHERE UserId = @UserId";
            sqlFollowSellerCommand.Parameters.AddWithValue("@UserId", sellerId);
            await sqlFollowSellerCommand.ExecuteNonQueryAsync();

            this.databaseConnection.CloseConnection();
        }

        /// <inheritdoc/>
        public async Task UnfollowSeller(int buyerId, int sellerId)
        {
            await this.databaseConnection.OpenConnection();

            var sqlUnfollowSellerCommand = this.databaseConnection.GetConnection().CreateCommand();
            sqlUnfollowSellerCommand.CommandText = "DELETE FROM Following WHERE FollowerID = @FollowerID AND FollowedID = @FollowedID";
            sqlUnfollowSellerCommand.Parameters.AddWithValue("@FollowerID", buyerId);
            sqlUnfollowSellerCommand.Parameters.AddWithValue("@FollowedID", sellerId);
            await sqlUnfollowSellerCommand.ExecuteNonQueryAsync();

            sqlUnfollowSellerCommand = this.databaseConnection.GetConnection().CreateCommand();
            sqlUnfollowSellerCommand.CommandText = "UPDATE Sellers SET FollowersCount = FollowersCount - 1 WHERE UserId = @UserId";
            sqlUnfollowSellerCommand.Parameters.AddWithValue("@UserId", sellerId);
            await sqlUnfollowSellerCommand.ExecuteNonQueryAsync();

            this.databaseConnection.CloseConnection();
        }

        /// <inheritdoc/>
        public async Task<int> GetTotalCount()
        {
            await this.databaseConnection.OpenConnection();
            var sqlGetTotalCountCommand = this.databaseConnection.GetConnection().CreateCommand();

            sqlGetTotalCountCommand.CommandText = "SELECT Count(*) FROM Buyers";

            var totalBuyersCount = (int)await sqlGetTotalCountCommand.ExecuteScalarAsync();

            this.databaseConnection.CloseConnection();
            return totalBuyersCount;
        }

        /// <inheritdoc/>
        public async Task UpdateAfterPurchase(Buyer buyerEntity)
        {
            await this.databaseConnection.OpenConnection();
            var sqlConnection = this.databaseConnection.GetConnection();
            var sqlUpdateBuyerCommand = sqlConnection.CreateCommand();
            sqlUpdateBuyerCommand.CommandText = @"UPDATE Buyers
                        SET 
                            TotalSpending=@TotalSpending,
                            NumberOfPurchases=@NumberOfPurchases,
                            Badge=@Badge,
                            Discount=@Discount
                        WHERE
                            UserId=@UserId;";
            sqlUpdateBuyerCommand.Parameters.AddWithValue("@TotalSpending", buyerEntity.TotalSpending);
            sqlUpdateBuyerCommand.Parameters.AddWithValue("@NumberOfPurchases", buyerEntity.NumberOfPurchases);
            sqlUpdateBuyerCommand.Parameters.AddWithValue("@Badge", buyerEntity.Badge.ToString());
            sqlUpdateBuyerCommand.Parameters.AddWithValue("@Discount", buyerEntity.Discount);
            sqlUpdateBuyerCommand.Parameters.AddWithValue("@UserId", buyerEntity.Id);
            await sqlUpdateBuyerCommand.ExecuteNonQueryAsync();
        }

        /// <inheritdoc/>
        public async Task RemoveWishilistItem(int buyerId, int productId)
        {
            await this.databaseConnection.OpenConnection();
            var sqlConnection = this.databaseConnection.GetConnection();
            var sqlBuyerWishlistCommand = sqlConnection.CreateCommand();
            sqlBuyerWishlistCommand.CommandText = @"DELETE FROM BuyerWishlistItems
                          WHERE
                            BuyerId=@BuyerId and ProductId=@ProductId";
            sqlBuyerWishlistCommand.Parameters.AddWithValue("@BuyerId", buyerId);
            sqlBuyerWishlistCommand.Parameters.AddWithValue("@ProductId", productId);
            await sqlBuyerWishlistCommand.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Loads address information from the database.
        /// </summary>
        /// <param name="addressId">The ID of the address to load.</param>
        /// <param name="sqlConnection">The SQL connection to use.</param>
        /// <returns>A task containing the loaded address or null if not found.</returns>
        public async Task<Address?> LoadAddress(int addressId, SqlConnection sqlConnection)
        {
            var sqlLoadAddressCommand = sqlConnection.CreateCommand();
            sqlLoadAddressCommand.CommandText = @"select StreetLine, City, Country, PostalCode
                        from BuyerAddress where Id = @addressId";
            sqlLoadAddressCommand.Parameters.AddWithValue("@addressId", addressId);

            var sqlDataReader = await sqlLoadAddressCommand.ExecuteReaderAsync();
            if (!await sqlDataReader.ReadAsync())
            {
                await sqlDataReader.CloseAsync();
                return null;
            }

            var loadedAddress = new Address
            {
                Id = addressId,
                StreetLine = sqlDataReader.GetString(sqlDataReader.GetOrdinal("StreetLine")),
                City = sqlDataReader.GetString(sqlDataReader.GetOrdinal("City")),
                Country = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Country")),
                PostalCode = sqlDataReader.GetString(sqlDataReader.GetOrdinal("PostalCode")),
            };
            await sqlDataReader.CloseAsync();
            return loadedAddress;
        }

        /// <summary>
        /// Reads buyer linkage information from a SQL data reader.
        /// </summary>
        /// <param name="sqlDataReader">The SQL data reader containing linkage information.</param>
        /// <param name="userId">The ID of the current user.</param>
        /// <returns>A BuyerLinkage object containing the read information.</returns>
        private static BuyerLinkage ReadBuyerLinkage(SqlDataReader sqlDataReader, int userId)
        {
            var requestingBuyerId = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("RequestingBuyerId"));
            var receivingBuyerId = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("ReceivingBuyerId"));
            var isApproved = sqlDataReader.GetBoolean(sqlDataReader.GetOrdinal("IsApproved"));
            var linkedBuyerId = requestingBuyerId;
            var buyerLinkageStatus = BuyerLinkageStatus.Confirmed;

            if (requestingBuyerId == userId)
            {
                linkedBuyerId = receivingBuyerId;
                if (!isApproved)
                {
                    buyerLinkageStatus = BuyerLinkageStatus.PendingSelf;
                }
            }
            else if (receivingBuyerId == userId)
            {
                linkedBuyerId = requestingBuyerId;
                if (!isApproved)
                {
                    buyerLinkageStatus = BuyerLinkageStatus.PendingOther;
                }
            }

            var buyerLinkage = new BuyerLinkage
            {
                Buyer = new Buyer
                {
                    User = new User { UserId = linkedBuyerId },
                },
                Status = buyerLinkageStatus,
            };
            return buyerLinkage;
        }

        /// <summary>
        /// Persists an address to the database.
        /// </summary>
        /// <param name="address">The address to persist.</param>
        /// <param name="sqlConnection">The SQL connection to use.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task PersistAddress(Address address, SqlConnection sqlConnection)
        {
            if (address.Id == 0)
            {
                await this.InsertAddress(address, sqlConnection);
            }
            else
            {
                await this.UpdateAddress(address, sqlConnection);
            }
        }

        /// <summary>
        /// Updates an existing address in the database.
        /// </summary>
        /// <param name="address">The address to update.</param>
        /// <param name="sqlConnection">The SQL connection to use.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task UpdateAddress(Address address, SqlConnection sqlConnection)
        {
            var sqlUpdateAddressCommand = sqlConnection.CreateCommand();
            sqlUpdateAddressCommand.CommandText = @"UPDATE BuyerAddress
                        SET 
                            StreetLine=@StreetLine,
                            City=@City,
                            Country=@Country,
                            PostalCode=@PostalCode
                        WHERE
                            ID=@ID;";
            sqlUpdateAddressCommand.Parameters.AddWithValue("@StreetLine", address.StreetLine);
            sqlUpdateAddressCommand.Parameters.AddWithValue("@City", address.City);
            sqlUpdateAddressCommand.Parameters.AddWithValue("@Country", address.Country);
            sqlUpdateAddressCommand.Parameters.AddWithValue("@PostalCode", address.PostalCode);
            sqlUpdateAddressCommand.Parameters.AddWithValue("@ID", address.Id);
            await sqlUpdateAddressCommand.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Inserts a new address into the database.
        /// </summary>
        /// <param name="buyerAddress">The address to insert.</param>
        /// <param name="sqlConnection">The SQL connection to use.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task InsertAddress(Address buyerAddress, SqlConnection sqlConnection)
        {
            var sqlInsertAddressCommand = sqlConnection.CreateCommand();
            sqlInsertAddressCommand.CommandText = @"INSERT INTO BuyerAddress(StreetLine,City,Country,PostalCode)
                        Values(@StreetLine, @City, @Country, @PostalCode); SELECT SCOPE_IDENTITY();";
            sqlInsertAddressCommand.Parameters.AddWithValue("@StreetLine", buyerAddress.StreetLine);
            sqlInsertAddressCommand.Parameters.AddWithValue("@City", buyerAddress.City);
            sqlInsertAddressCommand.Parameters.AddWithValue("@Country", buyerAddress.Country);
            sqlInsertAddressCommand.Parameters.AddWithValue("@PostalCode", buyerAddress.PostalCode);
            buyerAddress.Id = Convert.ToInt32(await sqlInsertAddressCommand.ExecuteScalarAsync());
        }

        /// <summary>
        /// Parses a badge string into a BuyerBadge enum value.
        /// </summary>
        /// <param name="badgeString">The badge string to parse.</param>
        /// <returns>The parsed BuyerBadge enum value.</returns>
        private BuyerBadge ParseBadge(string badgeString)
        {
            return (BuyerBadge)Enum.Parse(typeof(BuyerBadge), badgeString, true);
        }
    }
}
