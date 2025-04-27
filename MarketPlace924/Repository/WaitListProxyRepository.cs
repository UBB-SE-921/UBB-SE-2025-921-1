// <copyright file="WaitListProxyRepository.cs" company="PlaceholderCompany">
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
    /// Proxy repository class for managing waitlist operations via REST API.
    /// </summary>
    public class WaitListProxyRepository : IWaitListRepository
    {
        private const string ApiBaseRoute = "api/waitlist"; // Match the controller route
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitListProxyRepository"/> class.
        /// </summary>
        /// <param name="baseApiUrl">The base url of the API server.</param>
        public WaitListProxyRepository(string baseApiUrl)
        {
            this.httpClient = new HttpClient();
            this.httpClient.BaseAddress = new Uri(baseApiUrl);
        }

        /// <inheritdoc />
        public async Task AddUserToWaitlist(int userId, int productWaitListId)
        {
            var response = await this.httpClient.PostAsync($"{ApiBaseRoute}/user/{userId}/product/{productWaitListId}", null);
            response.EnsureSuccessStatusCode();
        }

        /// <inheritdoc />
        public async Task<List<UserWaitList>> GetUsersInWaitlist(int waitListProductId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/product/{waitListProductId}/users");
            response.EnsureSuccessStatusCode();
            var users = response.Content.ReadFromJsonAsync<List<UserWaitList>>()
                                .GetAwaiter().GetResult();
            return users ?? new List<UserWaitList>();
        }

        /// <inheritdoc />
        public async Task<List<UserWaitList>> GetUsersInWaitlistOrdered(int productId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/product/{productId}/users/ordered");
            response.EnsureSuccessStatusCode();
            var users = response.Content.ReadFromJsonAsync<List<UserWaitList>>()
                                .GetAwaiter().GetResult();
            return users ?? new List<UserWaitList>();
        }

        /// <inheritdoc />
        public async Task<int> GetUserWaitlistPosition(int userId, int productId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/user/{userId}/product/{productId}/position");
            response.EnsureSuccessStatusCode();
            var position = response.Content.ReadFromJsonAsync<int>()
                                   .GetAwaiter().GetResult();
            return position;
        }

        /// <inheritdoc />
        public async Task<List<UserWaitList>> GetUserWaitlists(int userId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/user/{userId}/waitlists");
            response.EnsureSuccessStatusCode();
            var waitlists = response.Content.ReadFromJsonAsync<List<UserWaitList>>()
                                     .GetAwaiter().GetResult();
            return waitlists ?? new List<UserWaitList>();
        }

        /// <inheritdoc />
        public async Task<int> GetWaitlistSize(int productWaitListId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/product/{productWaitListId}/size");
            response.EnsureSuccessStatusCode();
            var size = response.Content.ReadFromJsonAsync<int>()
                               .GetAwaiter().GetResult();
            return size;
        }

        /// <inheritdoc />
        public async Task<bool> IsUserInWaitlist(int userId, int productId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/user/{userId}/product/{productId}/exists");
            response.EnsureSuccessStatusCode();
            var exists = response.Content.ReadFromJsonAsync<bool>()
                                 .GetAwaiter().GetResult();
            return exists;
        }

        /// <inheritdoc />
        public async Task RemoveUserFromWaitlist(int userId, int productWaitListId)
        {
            var response = await this.httpClient.DeleteAsync($"{ApiBaseRoute}/user/{userId}/product/{productWaitListId}");
            response.EnsureSuccessStatusCode();
        }
    }
}