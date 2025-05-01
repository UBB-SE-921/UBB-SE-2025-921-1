// <copyright file="NotificationProxyRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SharedClassLibrary.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Text;
    using System.Threading.Tasks;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// Proxy repository class for managing notification operations via REST API.
    /// </summary>
    public class NotificationProxyRepository : INotificationRepository
    {
        private const string ApiBaseRoute = "api/notifications";
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationProxyRepository"/> class.
        /// </summary>
        /// <param name="baseApiUrl">The base url of the API.</param>
        public NotificationProxyRepository(string baseApiUrl)
        {
            this.httpClient = new HttpClient();
            this.httpClient.BaseAddress = new Uri(baseApiUrl);
        }

        /// <inheritdoc />
        public async void AddNotification(Notification notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            var response = await this.httpClient.PostAsJsonAsync($"{ApiBaseRoute}", notification);
            await this.ThrowOnError(nameof(AddNotification), response);
        }

        /// <inheritdoc />
        public async Task<List<Notification>> GetNotificationsForUser(int recipientId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/user/{recipientId}");
            await this.ThrowOnError(nameof(GetNotificationsForUser), response);
            var notifications = await response.Content.ReadFromJsonAsync<List<Notification>>();
            return notifications ?? new List<Notification>();
        }

        /// <inheritdoc />
        public async void MarkAsRead(int notificationId)
        {
            var response = await this.httpClient.PutAsync($"{ApiBaseRoute}/{notificationId}/mark-read", null); // No body needed for this PUT.
            await this.ThrowOnError(nameof(MarkAsRead), response);
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
