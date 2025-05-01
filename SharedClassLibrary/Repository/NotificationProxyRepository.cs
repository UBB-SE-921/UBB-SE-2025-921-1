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
        public void AddNotification(Notification notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            Task.Run(() => this.AddNotificationAsync(notification)).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public List<Notification> GetNotificationsForUser(int recipientId)
        {
            return Task.Run(() => this.GetNotificationsForUserAsync(recipientId)).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public void MarkAsRead(int notificationId)
        {
            Task.Run(() => this.MarkAsReadAsync(notificationId)).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Asynchronously adds a new notification.
        /// </summary>
        /// <param name="notification">The notification to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task AddNotificationAsync(Notification notification)
        {
            var response = await this.httpClient.PostAsJsonAsync($"{ApiBaseRoute}", notification);
            await this.ThrowOnError(nameof(AddNotificationAsync), response);
        }

        /// <summary>
        /// Asynchronously retrieves notifications for a user based on recipient ID.
        /// This is the actual async implementation used by the synchronous wrapper.
        /// </summary>
        /// <param name="recipientId">The ID of the recipient.</param>
        /// <returns>A list of notifications for the user.</returns>
        private async Task<List<Notification>> GetNotificationsForUserAsync(int recipientId)
        {
            var response = await this.httpClient.GetAsync($"{ApiBaseRoute}/user/{recipientId}");
            await this.ThrowOnError(nameof(GetNotificationsForUserAsync), response);
            var notifications = await response.Content.ReadFromJsonAsync<List<Notification>>();
            return notifications ?? new List<Notification>();
        }

        /// <summary>
        /// Asynchronously marks a notification as read.
        /// </summary>
        /// <param name="notificationId">The ID of the notification to mark as read.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task MarkAsReadAsync(int notificationId)
        {
            // Using PutAsync for idempotent update.
            var response = await this.httpClient.PutAsync($"{ApiBaseRoute}/{notificationId}/mark-read", null); // No body needed for this PUT.
            await this.ThrowOnError(nameof(MarkAsReadAsync), response);
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
