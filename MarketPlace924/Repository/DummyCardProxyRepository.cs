// <copyright file="DummyCardProxyRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.Repository
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// Proxy repository class for managing dummy card operations via REST API.
    /// </summary>
    public class DummyCardProxyRepository : IDummyCardRepository
    {
        private const string ApiBaseRoute = "api/dummycards";
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="DummyCardProxyRepository"/> class.
        /// </summary>
        /// <param name="baseApiUrl">The base url of the API.</param>
        public DummyCardProxyRepository(string baseApiUrl)
        {
            this.httpClient = new HttpClient();
            this.httpClient.BaseAddress = new Uri(baseApiUrl);
        }

        /// <inheritdoc />
        public async Task DeleteCardAsync(string cardNumber)
        {
            var response = await this.httpClient.DeleteAsync($"{ApiBaseRoute}/{cardNumber}");
            response.EnsureSuccessStatusCode();
        }

        /// <inheritdoc />
        public async Task<float> GetCardBalanceAsync(string cardNumber)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/{cardNumber}/balance");
            response.EnsureSuccessStatusCode();
            var balance = await response.Content.ReadFromJsonAsync<float>();
            return balance;
        }

        /// <inheritdoc />
        public async Task UpdateCardBalanceAsync(string cardNumber, float balance)
        {
            var response = await this.httpClient.PutAsJsonAsync($"{ApiBaseRoute}/{cardNumber}/balance", balance);
            response.EnsureSuccessStatusCode();
        }
    }
}
