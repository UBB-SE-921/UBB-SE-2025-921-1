// -----------------------------------------------------------------------
// <copyright file="UserProxyRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SharedClassLibrary.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// A repository implementation that acts as a proxy for user-related operations.
    /// </summary>
    public class UserProxyRepository : IUserRepository
    {
        private const string ApiBaseRoute = "api/users";
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProxyRepository"/> class.
        /// </summary>
        /// <param name="baseApiUrl">The base url of the API.</param>
        public UserProxyRepository(string baseApiUrl)
        {
            this.httpClient = new HttpClient();
            this.httpClient.BaseAddress = new System.Uri(baseApiUrl);
        }

        /// <inheritdoc />
        public async Task AddUser(User user)
        {
            var response = await this.httpClient.PostAsJsonAsync($"{ApiBaseRoute}", user);
            response.EnsureSuccessStatusCode(); // Throw an exception for non-success status codes
        }

        /// <inheritdoc />
        public async Task<bool> EmailExists(string email)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/email-exists?email={email}");
            response.EnsureSuccessStatusCode(); // Throw an exception for non-success status codes

            var found = await response.Content.ReadFromJsonAsync<bool>();
            return found;
        }

        /// <inheritdoc />
        public async Task<List<User>> GetAllUsers()
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}");
            response.EnsureSuccessStatusCode(); // Throw an exception for non-success status codes

            var users = await response.Content.ReadFromJsonAsync<List<User>>();
            if (users == null)
            {
                users = new List<User>();
            }

            return users;
        }

        /// <inheritdoc />
        public async Task<int> GetFailedLoginsCountByUserId(int userId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/failed-logins-count/{userId}");
            response.EnsureSuccessStatusCode(); // Throw an exception for non-success status codes

            var failedLoginsCount = await response.Content.ReadFromJsonAsync<int>();
            return failedLoginsCount;
        }

        /// <inheritdoc />
        public async Task<int> GetTotalNumberOfUsers()
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/count");
            response.EnsureSuccessStatusCode(); // Throw an exception for non-success status codes

            var userCount = await response.Content.ReadFromJsonAsync<int>();
            return userCount;
        }

        /// <inheritdoc />
        public async Task<User?> GetUserByEmail(string email)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/email/{email}");
            response.EnsureSuccessStatusCode(); // Throw an exception for non-success status codes

            var user = await response.Content.ReadFromJsonAsync<User>();
            return user;
        }

        /// <inheritdoc />
        public async Task<User?> GetUserByUsername(string username)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/username/{username}");
            response.EnsureSuccessStatusCode(); // Throw an exception for non-success status codes

            var user = await response.Content.ReadFromJsonAsync<User>();
            return user;
        }

        /// <inheritdoc />
        public async Task LoadUserPhoneNumberAndEmailById(User user)
        {
            int userId = user.UserId;
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/phone-email/{user.UserId}");
            response.EnsureSuccessStatusCode(); // Throw an exception for non-success status codes

            var newUser = await response.Content.ReadFromJsonAsync<User>();
            if (newUser == null)
            {
                throw new InvalidOperationException($"Failed to load user data for UserId: {userId}. The API returned no data.");
            }

            user.PhoneNumber = newUser.PhoneNumber;
            user.Email = newUser.Email;
        }

        /// <inheritdoc />
        public async Task UpdateUser(User user)
        {
            var response = await this.httpClient.PutAsJsonAsync($"{ApiBaseRoute}", user);
            response.EnsureSuccessStatusCode(); // Throw an exception for non-success status codes
        }

        /// <inheritdoc />
        public async Task UpdateUserFailedLoginsCount(User user, int newValueOfFailedLogIns)
        {
            var response = await this.httpClient.PutAsJsonAsync($"{ApiBaseRoute}/update-failed-logins/{newValueOfFailedLogIns}", user);
            response.EnsureSuccessStatusCode(); // Throw an exception for non-success status codes
        }

        /// <inheritdoc />
        public async Task UpdateUserPhoneNumber(User user)
        {
            var response = await this.httpClient.PutAsJsonAsync($"{ApiBaseRoute}/update-phone-number", user);
            response.EnsureSuccessStatusCode(); // Throw an exception for non-success status codes
        }

        /// <inheritdoc />
        public async Task<bool> UsernameExists(string username)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/username-exists?username={username}");
            response.EnsureSuccessStatusCode(); // Throw an exception for non-success status codes

            var found = await response.Content.ReadFromJsonAsync<bool>();
            return found;
        }
    }
}
