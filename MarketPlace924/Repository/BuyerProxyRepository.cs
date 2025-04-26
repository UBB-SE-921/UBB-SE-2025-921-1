// <copyright file="BuyerProxyRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using System.Web;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// Repository class for managing buyer-related database operations.
    /// </summary>
    /// <param name="databaseConnection">The database connection instance.</param>
    public class BuyerProxyRepository : IBuyerRepository
    {
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuyerProxyRepository"/> class.
        /// </summary>
        /// <param name="baseApiUrl">The base url of the API.</param>
        public BuyerProxyRepository(string baseApiUrl)
        {
            this.httpClient = new HttpClient();
            this.httpClient.BaseAddress = new System.Uri(baseApiUrl);
        }

        /// <inheritdoc />
        public async Task<bool> CheckIfBuyerExists(int buyerId)
        {
            var response = await this.httpClient.GetAsync($"api/buyers/{buyerId}/exists");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<bool>();
        }

        /// <inheritdoc />
        public async Task CreateBuyer(Buyer buyerEntity)
        {
            var response = await this.httpClient.PostAsJsonAsync("api/buyers/create", buyerEntity);
            response.EnsureSuccessStatusCode();
        }

        /// <inheritdoc />
        public async Task CreateLinkageRequest(int requestingBuyerId, int receivingBuyerId)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["requestingBuyerId"] = requestingBuyerId.ToString();
            query["receivingBuyerId"] = receivingBuyerId.ToString();
            string queryString = query.ToString() ?? string.Empty;

            var response = await this.httpClient.PostAsync($"api/buyers/linkages/create?{queryString}", null); // No body needed for this POST
            response.EnsureSuccessStatusCode();
        }

        /// <inheritdoc />
        public async Task<bool> DeleteLinkageRequest(int requestingBuyerId, int receivingBuyerId)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["requestingBuyerId"] = requestingBuyerId.ToString();
            query["receivingBuyerId"] = receivingBuyerId.ToString();
            string queryString = query.ToString() ?? string.Empty;

            var response = await this.httpClient.DeleteAsync($"api/buyers/linkages/delete?{queryString}");

            // Check for 404 specifically if the API returns it when not found
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }

            response.EnsureSuccessStatusCode(); // Throws for other errors
            return true; // Assume success if EnsureSuccessStatusCode doesn't throw
        }

        /// <inheritdoc />
        public async Task<List<Buyer>> FindBuyersWithShippingAddress(Address shippingAddress)
        {
            var response = await this.httpClient.PostAsJsonAsync("api/buyers/find-by-shipping-address", shippingAddress);
            response.EnsureSuccessStatusCode();
            var buyers = await response.Content.ReadFromJsonAsync<List<Buyer>>();
            return buyers ?? new List<Buyer>();
        }

        /// <inheritdoc />
        public async Task FollowSeller(int buyerId, int sellerId)
        {
            var response = await this.httpClient.PostAsync($"api/buyers/{buyerId}/follow/{sellerId}", null); // No body
            response.EnsureSuccessStatusCode();
        }

        /// <inheritdoc />
        public async Task<List<Seller>> GetAllSellers()
        {
            var response = await this.httpClient.GetAsync("api/buyers/sellers/all");
            response.EnsureSuccessStatusCode();
            var sellers = await response.Content.ReadFromJsonAsync<List<Seller>>();
            return sellers ?? new List<Seller>();
        }

        /// <inheritdoc />
        public async Task<List<BuyerLinkage>> GetBuyerLinkages(int buyerId)
        {
            var response = await this.httpClient.GetAsync($"api/buyers/{buyerId}/linkages");
            response.EnsureSuccessStatusCode();
            var linkages = await response.Content.ReadFromJsonAsync<List<BuyerLinkage>>();
            return linkages ?? new List<BuyerLinkage>();
        }

        /// <inheritdoc />
        public async Task<List<Seller>> GetFollowedSellers(List<int>? followingUsersIds)
        {
            if (followingUsersIds == null)
            {
                followingUsersIds = new List<int>();
            }

            var response = await this.httpClient.PostAsJsonAsync($"api/buyers/followed-sellers", followingUsersIds);
            response.EnsureSuccessStatusCode();
            var sellers = await response.Content.ReadFromJsonAsync<List<Seller>>();
            return sellers ?? new List<Seller>();
        }

        /// <inheritdoc />
        public async Task<List<int>> GetFollowingUsersIds(int buyerId)
        {
            var response = await this.httpClient.GetAsync($"api/buyers/{buyerId}/following/ids");
            response.EnsureSuccessStatusCode();
            var ids = await response.Content.ReadFromJsonAsync<List<int>>();
            return ids ?? new List<int>();
        }

        /// <inheritdoc />
        public async Task<List<Product>> GetProductsFromSeller(int sellerId)
        {
            var response = await this.httpClient.GetAsync($"api/buyers/sellers/{sellerId}/products");
            response.EnsureSuccessStatusCode();
            var products = await response.Content.ReadFromJsonAsync<List<Product>>();
            return products ?? new List<Product>();
        }

        /// <inheritdoc />
        public async Task<int> GetTotalCount()
        {
            var response = await this.httpClient.GetAsync("api/buyers/count");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<int>();
        }

        /// <inheritdoc />
        public async Task<BuyerWishlist> GetWishlist(int buyerId)
        {
            var response = await this.httpClient.GetAsync($"api/buyers/{buyerId}/wishlist");
            response.EnsureSuccessStatusCode();
            var wishlist = await response.Content.ReadFromJsonAsync<BuyerWishlist>();
            return wishlist ?? new BuyerWishlist(); // Return empty wishlist if null
        }

        /// <inheritdoc />
        public async Task<bool> IsFollowing(int buyerId, int sellerId)
        {
            var response = await this.httpClient.GetAsync($"api/buyers/{buyerId}/following/check/{sellerId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<bool>();
        }

        /// <inheritdoc />
        public async Task LoadBuyerInfo(Buyer buyerEntity)
        {
            int buyerId = buyerEntity.Id;
            var response = await this.httpClient.GetAsync($"api/buyers/{buyerId}/info");
            response.EnsureSuccessStatusCode();
            var loadedBuyer = await response.Content.ReadFromJsonAsync<Buyer>();
            if (loadedBuyer == null)
            {
                throw new InvalidOperationException($"Failed to load buyer info for ID: {buyerEntity.Id}. API returned null.");
            }

            // Update the passed-in buyerEntity with loaded data
            buyerEntity.FirstName = loadedBuyer.FirstName;
            buyerEntity.LastName = loadedBuyer.LastName;
            buyerEntity.Badge = loadedBuyer.Badge;
            buyerEntity.TotalSpending = loadedBuyer.TotalSpending;
            buyerEntity.NumberOfPurchases = loadedBuyer.NumberOfPurchases;
            buyerEntity.Discount = loadedBuyer.Discount;
            buyerEntity.UseSameAddress = loadedBuyer.UseSameAddress;
            buyerEntity.BillingAddress = loadedBuyer.BillingAddress;
            buyerEntity.ShippingAddress = loadedBuyer.ShippingAddress;
            buyerEntity.FollowingUsersIds = loadedBuyer.FollowingUsersIds;
        }

        /// <inheritdoc />
        public async Task RemoveWishilistItem(int buyerId, int productId)
        {
            var response = await this.httpClient.DeleteAsync($"api/buyers/{buyerId}/wishlist/remove/{productId}");
            response.EnsureSuccessStatusCode();
        }

        /// <inheritdoc />
        public async Task SaveInfo(Buyer buyerEntity)
        {
            var response = await this.httpClient.PostAsJsonAsync("api/buyers/save", buyerEntity);
            response.EnsureSuccessStatusCode();
        }

        /// <inheritdoc />
        public async Task UnfollowSeller(int buyerId, int sellerId)
        {
            var response = await this.httpClient.DeleteAsync($"api/buyers/{buyerId}/unfollow/{sellerId}");
            response.EnsureSuccessStatusCode();
        }

        /// <inheritdoc />
        public async Task UpdateAfterPurchase(Buyer buyerEntity)
        {
            var response = await this.httpClient.PutAsJsonAsync("api/buyers/update-after-purchase", buyerEntity);
            response.EnsureSuccessStatusCode();
        }

        /// <inheritdoc />
        public async Task UpdateLinkageRequest(int requestingBuyerId, int receivingBuyerId)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["requestingBuyerId"] = requestingBuyerId.ToString();
            query["receivingBuyerId"] = receivingBuyerId.ToString();
            string queryString = query.ToString() ?? string.Empty;

            var response = await this.httpClient.PutAsync($"api/buyers/linkages/update?{queryString}", null); // No body needed for this PUT
            response.EnsureSuccessStatusCode();
        }
    }
}
