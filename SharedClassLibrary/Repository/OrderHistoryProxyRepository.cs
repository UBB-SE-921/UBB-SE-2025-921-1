// <copyright file="OrderHistoryProxyRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// Proxy repository class for managing order history operations via REST API.
    /// </summary>
    public class OrderHistoryProxyRepository : IOrderHistoryRepository
    {
        private const string ApiBaseRoute = "api/orderhistory";
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderHistoryProxyRepository"/> class.
        /// </summary>
        /// <param name="baseApiUrl">The base url of the API.</param>
        public OrderHistoryProxyRepository(string baseApiUrl)
        {
            this.httpClient = new HttpClient();
            this.httpClient.BaseAddress = new System.Uri(baseApiUrl);
        }

        /// <inheritdoc />
        public async Task<List<Product>> GetProductsFromOrderHistoryAsync(int orderHistoryId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{orderHistoryId}/dummy-products");
            response.EnsureSuccessStatusCode();

            var products = await response.Content.ReadFromJsonAsync<List<Product>>();
            return products ?? new List<Product>();
        }
    }
}