// <copyright file="DummyProductProxyRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.Repository
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using SharedClassLibrary.DataTransferObjects;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// Proxy repository class for managing dummy product operations via REST API.
    /// </summary>
    public class DummyProductProxyRepository : IDummyProductRepository
    {
        private const string ApiBaseRoute = "api/dummyproducts";
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="DummyProductProxyRepository"/> class.
        /// </summary>
        /// <param name="baseApiUrl">The base url of the API.</param>
        public DummyProductProxyRepository(string baseApiUrl)
        {
            this.httpClient = new HttpClient();
            this.httpClient.BaseAddress = new System.Uri(baseApiUrl);
        }

        /// <inheritdoc />
        public async Task AddDummyProductAsync(string name, float price, int sellerId, string productType, DateTime startDate, DateTime endDate)
        {
            var requestDto = new DummyProductRequest
            {
                Name = name,
                Price = price,
                SellerID = sellerId,
                ProductType = productType,
                StartDate = startDate,
                EndDate = endDate,
            };

            var response = await this.httpClient.PostAsJsonAsync($"{ApiBaseRoute}", requestDto);
            response.EnsureSuccessStatusCode();
        }

        /// <inheritdoc />
        public async Task DeleteDummyProduct(int id)
        {
            var response = await this.httpClient.DeleteAsync($"{ApiBaseRoute}/{id}");
            response.EnsureSuccessStatusCode();
        }

        /// <inheritdoc />
        public async Task<DummyProduct> GetDummyProductByIdAsync(int productId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{productId}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            var product = await response.Content.ReadFromJsonAsync<DummyProduct>();
            return product;
        }

        /// <inheritdoc />
        public async Task<string> GetSellerNameAsync(int? sellerId)
        {
            if (!sellerId.HasValue)
            {
                return null;
            }

            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/seller/{sellerId.Value}/name");
            response.EnsureSuccessStatusCode();
            var sellerName = await response.Content.ReadAsStringAsync();
            return sellerName;
        }

        /// <inheritdoc />
        public async Task UpdateDummyProductAsync(int id, string name, float price, int sellerId, string productType, DateTime startDate, DateTime endDate)
        {
            var requestDto = new DummyProductRequest
            {
                Name = name,
                Price = price,
                SellerID = sellerId,
                ProductType = productType,
                StartDate = startDate,
                EndDate = endDate,
            };

            var response = await this.httpClient.PutAsJsonAsync($"{ApiBaseRoute}/{id}", requestDto);
            response.EnsureSuccessStatusCode();
        }
    }
}