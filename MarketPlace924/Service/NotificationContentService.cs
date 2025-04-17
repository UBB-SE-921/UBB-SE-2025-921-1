using System;
using System.Collections.Generic;
using System.Linq;
using Marketplace924.Domain;
using Marketplace924.Repository;
using Marketplace924.Shared;

namespace Marketplace924.Service
{
    public class NotificationContentService : INotificationContentService
    {
        private readonly INotificationRepository notificationRepository;

        public NotificationContentService()
        {
            this.notificationRepository = new NotificationRepository(Configuration.CONNECTION_STRING);
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