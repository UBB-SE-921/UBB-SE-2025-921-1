using System;
using System.Collections.Generic;
using System.Linq;
using SharedClassLibrary.Domain;
using MarketPlace924.Repository;
using SharedClassLibrary.Shared;
using SharedClassLibrary.IRepository;
using MarketPlace924.Helper;

namespace MarketPlace924.Service
{
    public class NotificationContentService : INotificationContentService
    {
        private readonly INotificationRepository notificationRepository;

        public NotificationContentService()
        {
            this.notificationRepository = new NotificationProxyRepository(AppConfig.GetBaseApiUrl());
        }

        public string GetUnreadNotificationsCountText(int unreadCount)
        {
            return $"You've got #{unreadCount} unread notifications.";
        }

        public List<Notification> GetNotificationsForUser(int recipientId)
        {
            return notificationRepository.GetNotificationsForUser(recipientId);
        }

        public void MarkAsRead(int notificationId)
        {
            notificationRepository.MarkAsRead(notificationId);
        }

        public void AddNotification(Notification notification)
        {
            notificationRepository.AddNotification(notification);
        }

        public void Dispose()
        {
            notificationRepository.Dispose();
        }
    }
}