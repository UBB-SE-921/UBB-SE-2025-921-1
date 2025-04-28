// <copyright file="OrderProxyRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using SharedClassLibrary.DataTransferObjects;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// Proxy repository class for managing order operations via REST API.
    /// </summary>
    public class OrderProxyRepository : IOrderRepository
    {
        private const string ApiBaseRoute = "api/orders";
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderProxyRepository"/> class.
        /// </summary>
        /// <param name="baseApiUrl">The base url of the API server.</param>
        public OrderProxyRepository(string baseApiUrl)
        {
            this.httpClient = new HttpClient();
            this.httpClient.BaseAddress = new System.Uri(baseApiUrl);
        }

        /// <inheritdoc />
        public async Task AddOrderAsync(int productId, int buyerId, string productType, string paymentMethod, int orderSummaryId, DateTime orderDate)
        {
            var orderData = new AddOrderRequestDto // Assuming a DTO exists
            {
                ProductId = productId,
                BuyerId = buyerId,
                ProductType = productType,
                PaymentMethod = paymentMethod,
                OrderSummaryId = orderSummaryId,
                OrderDate = orderDate,
            };

            var response = await this.httpClient.PostAsJsonAsync(ApiBaseRoute, orderData);
            response.EnsureSuccessStatusCode(); // Throws exception if not successful
        }

        /// <inheritdoc />
        public async Task DeleteOrderAsync(int orderId)
        {
            var response = await this.httpClient.DeleteAsync($"{ApiBaseRoute}/{orderId}");
            response.EnsureSuccessStatusCode();
        }

        /// <inheritdoc />
        public async Task<List<Order>> GetBorrowedOrderHistoryAsync(int buyerId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/buyer/{buyerId}/history/borrowed");
            response.EnsureSuccessStatusCode();
            var orders = await response.Content.ReadFromJsonAsync<List<Order>>();
            return orders ?? new List<Order>();
        }

        /// <inheritdoc />
        public async Task<List<Order>> GetNewOrUsedOrderHistoryAsync(int buyerId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/buyer/{buyerId}/history/new-used");
            response.EnsureSuccessStatusCode();
            var orders = await response.Content.ReadFromJsonAsync<List<Order>>();
            return orders ?? new List<Order>();
        }

        /// <inheritdoc />
        public async Task<List<Order>> GetOrdersByNameAsync(int buyerId, string text)
        {
            // Encode the text parameter for the query string
            var encodedText = System.Net.WebUtility.UrlEncode(text);
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/buyer/{buyerId}/search?text={encodedText}");
            response.EnsureSuccessStatusCode();
            var orders = await response.Content.ReadFromJsonAsync<List<Order>>();
            return orders ?? new List<Order>();
        }

        /// <inheritdoc />
        public async Task<List<Order>> GetOrdersFrom2024Async(int buyerId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/buyer/{buyerId}/history/year/2024");
            response.EnsureSuccessStatusCode();
            var orders = await response.Content.ReadFromJsonAsync<List<Order>>();
            return orders ?? new List<Order>();
        }

        /// <inheritdoc />
        public async Task<List<Order>> GetOrdersFrom2025Async(int buyerId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/buyer/{buyerId}/history/year/2025");
            response.EnsureSuccessStatusCode();
            var orders = await response.Content.ReadFromJsonAsync<List<Order>>();
            return orders ?? new List<Order>();
        }

        /// <inheritdoc />
        public async Task<List<Order>> GetOrdersFromLastSixMonthsAsync(int buyerId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/buyer/{buyerId}/history/months/6");
            response.EnsureSuccessStatusCode();
            var orders = await response.Content.ReadFromJsonAsync<List<Order>>();
            return orders ?? new List<Order>();
        }

        /// <inheritdoc />
        public async Task<List<Order>> GetOrdersFromLastThreeMonthsAsync(int buyerId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/buyer/{buyerId}/history/months/3");
            response.EnsureSuccessStatusCode();
            var orders = await response.Content.ReadFromJsonAsync<List<Order>>();
            return orders ?? new List<Order>();
        }

        /// <inheritdoc />
        public async Task<List<Order>> GetOrdersFromOrderHistoryAsync(int orderHistoryId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/history-group/{orderHistoryId}");
            response.EnsureSuccessStatusCode();
            var orders = await response.Content.ReadFromJsonAsync<List<Order>>();
            return orders ?? new List<Order>();
        }

        /// <inheritdoc />
        public async Task<OrderSummary> GetOrderSummaryAsync(int orderSummaryId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/summary/{orderSummaryId}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException($"OrderSummary with ID {orderSummaryId} not found via API.");
            }

            response.EnsureSuccessStatusCode();
            var summary = await response.Content.ReadFromJsonAsync<OrderSummary>();
            return summary ?? throw new InvalidOperationException("Failed to deserialize OrderSummary.");
        }

        /// <inheritdoc />
        public async Task<List<OrderDisplayInfo>> GetOrdersWithProductInfoAsync(int userId, string? searchText = null, string? timePeriod = null)
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(searchText))
            {
                queryParams.Add($"searchText={System.Net.WebUtility.UrlEncode(searchText)}");
            }

            if (!string.IsNullOrEmpty(timePeriod))
            {
                queryParams.Add($"timePeriod={System.Net.WebUtility.UrlEncode(timePeriod)}");
            }

            string queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;

            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/buyer/{userId}/display{queryString}");
            response.EnsureSuccessStatusCode();
            var orderInfos = await response.Content.ReadFromJsonAsync<List<OrderDisplayInfo>>();
            return orderInfos ?? new List<OrderDisplayInfo>();
        }

        /// <inheritdoc />
        public async Task<Dictionary<int, string>> GetProductCategoryTypesAsync(int userId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/buyer/{userId}/category-types");
            response.EnsureSuccessStatusCode();
            var categories = await response.Content.ReadFromJsonAsync<Dictionary<int, string>>();
            return categories ?? new Dictionary<int, string>();
        }

        /// <inheritdoc />
        public async Task UpdateOrderAsync(int orderId, string productType, string paymentMethod, DateTime orderDate)
        {
            var orderData = new UpdateOrderRequestDto
            {
                ProductType = productType,
                PaymentMethod = paymentMethod,
                OrderDate = orderDate,
            };

            var response = await this.httpClient.PutAsJsonAsync($"{ApiBaseRoute}/{orderId}", orderData);
            response.EnsureSuccessStatusCode();
        }
    }
}