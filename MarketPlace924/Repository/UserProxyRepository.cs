// -----------------------------------------------------------------------
// <copyright file="UserProxyRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace SharedClassLibrary.IRepository
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using SharedClassLibrary.Domain;

    /// <summary>
    /// Provides methods for interacting with the Users table in the database.
    /// </summary>
    public class UserProxyRepository : IUserRepository
    {
        private readonly HttpClient httpClient;

        public UserProxyRepository(string baseApiUrl)
        {
            this.httpClient = new HttpClient();
            this.httpClient.BaseAddress = new System.Uri(baseApiUrl);
        }

        public async Task AddUser(User user)
        {
            var response = await this.httpClient.PostAsJsonAsync($"/users", user);
            response.EnsureSuccessStatusCode(); // Throw an exception for non-success status codes
        }

        public async Task<bool> EmailExists(string email)
        {
            var response = await this.httpClient.GetAsync($"users/email-exists?email={email}");
            response.EnsureSuccessStatusCode(); // Throw an exception for non-success status codes

            var found = await response.Content.ReadFromJsonAsync<bool>();
            return found;
        }

        public async Task<List<User>> GetAllUsers()
        {
            var response = await this.httpClient.GetAsync($"users");
            response.EnsureSuccessStatusCode(); // Throw an exception for non-success status codes

            var users = await response.Content.ReadFromJsonAsync<List<User>>();
            if (users == null)
            {
                users = new List<User>();
            }

            return users;
        }

        public async Task<int> GetFailedLoginsCountByUserId(int userId)
        {
            var response = await this.httpClient.GetAsync($"users/failed-logins-count/{userId}");
            response.EnsureSuccessStatusCode(); // Throw an exception for non-success status codes

            var failedLoginsCount = await response.Content.ReadFromJsonAsync<int>();
            return failedLoginsCount;
        }

        public async Task<int> GetTotalNumberOfUsers()
        {
            var response = await this.httpClient.GetAsync($"users/count");
            response.EnsureSuccessStatusCode(); // Throw an exception for non-success status codes

            var userCount = await response.Content.ReadFromJsonAsync<int>();
            return userCount;
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            var response = await this.httpClient.GetAsync($"users/email/{email}");
            response.EnsureSuccessStatusCode(); // Throw an exception for non-success status codes

            var user = await response.Content.ReadFromJsonAsync<User>();
            return user;
        }

        public async Task<User?> GetUserByUsername(string username)
        {
            var response = await this.httpClient.GetAsync($"users/username/{username}");
            response.EnsureSuccessStatusCode(); // Throw an exception for non-success status codes

            var user = await response.Content.ReadFromJsonAsync<User>();
            return user;
        }

        public async Task LoadUserPhoneNumberAndEmailById(User user)
        {
            int userId = user.UserId;
            var response = await this.httpClient.GetAsync($"users/phone-email/{user.UserId}");
            response.EnsureSuccessStatusCode(); // Throw an exception for non-success status codes

            var newUser = await response.Content.ReadFromJsonAsync<User>();
            user.PhoneNumber = newUser.PhoneNumber;
            user.Email = newUser.Email;
        }

        public async Task UpdateUser(User user)
        {
            var response = await this.httpClient.PutAsJsonAsync($"/users", user);
            response.EnsureSuccessStatusCode(); // Throw an exception for non-success status codes
        }

        public async Task UpdateUserFailedLoginsCount(User user, int newValueOfFailedLogIns)
        {
            var response = await this.httpClient.PutAsJsonAsync($"/users/update-failed-logins/{newValueOfFailedLogIns}", user);
            response.EnsureSuccessStatusCode(); // Throw an exception for non-success status codes
        }

        public async Task UpdateUserPhoneNumber(User user)
        {
            var response = await this.httpClient.PutAsJsonAsync($"/users/update-phone-number", user);
            response.EnsureSuccessStatusCode(); // Throw an exception for non-success status codes
        }

        public async Task<bool> UsernameExists(string username)
        {
            var response = await this.httpClient.GetAsync($"users/username-exists?username={username}");
            response.EnsureSuccessStatusCode(); // Throw an exception for non-success status codes

            var found = await response.Content.ReadFromJsonAsync<bool>();
            return found;
        }
    }
}
