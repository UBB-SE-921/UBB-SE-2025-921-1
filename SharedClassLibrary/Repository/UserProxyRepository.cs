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
            await this.ThrowOnError(nameof(AddUser), response);
        }

        /// <inheritdoc />
        public async Task<bool> EmailExists(string email)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/email-exists?email={email}");
            await this.ThrowOnError(nameof(EmailExists), response);

            var found = await response.Content.ReadFromJsonAsync<bool>();
            return found;
        }

        /// <inheritdoc />
        public async Task<List<User>> GetAllUsers()
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}");
            await this.ThrowOnError(nameof(GetAllUsers), response);

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
            await this.ThrowOnError(nameof(GetFailedLoginsCountByUserId), response);

            var failedLoginsCount = await response.Content.ReadFromJsonAsync<int>();
            return failedLoginsCount;
        }

        /// <inheritdoc />
        public async Task<int> GetTotalNumberOfUsers()
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/count");
            await this.ThrowOnError(nameof(GetTotalNumberOfUsers), response);

            var userCount = await response.Content.ReadFromJsonAsync<int>();
            return userCount;
        }

        /// <inheritdoc />
        public async Task<User?> GetUserByEmail(string email)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/email/{email}");
            await this.ThrowOnError(nameof(GetUserByEmail), response);

            var user = await response.Content.ReadFromJsonAsync<User>();
            return user;
        }

        /// <inheritdoc />
        public async Task<User?> GetUserByUsername(string username)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/username/{username}");
            await this.ThrowOnError(nameof(GetUserByUsername), response);

            var user = await response.Content.ReadFromJsonAsync<User>();
            return user;
        }

        /// <inheritdoc />
        public async Task LoadUserPhoneNumberAndEmailById(User user)
        {
            int userId = user.UserId;
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/phone-email/{user.UserId}");
            await this.ThrowOnError(nameof(LoadUserPhoneNumberAndEmailById), response);

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
            await this.ThrowOnError(nameof(UpdateUser), response);
        }

        /// <inheritdoc />
        public async Task UpdateUserFailedLoginsCount(User user, int newValueOfFailedLogIns)
        {
            var response = await this.httpClient.PutAsJsonAsync($"{ApiBaseRoute}/update-failed-logins/{newValueOfFailedLogIns}", user);
            await this.ThrowOnError(nameof(UpdateUserFailedLoginsCount), response);
        }

        /// <inheritdoc />
        public async Task UpdateUserPhoneNumber(User user)
        {
            var response = await this.httpClient.PutAsJsonAsync($"{ApiBaseRoute}/update-phone-number", user);
            await this.ThrowOnError(nameof(UpdateUserPhoneNumber), response);
        }

        /// <inheritdoc />
        public async Task<bool> UsernameExists(string username)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/username-exists?username={username}");
            await this.ThrowOnError(nameof(UsernameExists), response);

            var found = await response.Content.ReadFromJsonAsync<bool>();
            return found;
        }

        private async Task ThrowOnError(string methodName, HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = response.ReasonPhrase;
                }
                throw new Exception($"{methodName}: {errorMessage}");
            }
        }
    }
}
