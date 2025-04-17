using System;
using System.Collections.Generic;
using Marketplace924.Domain;

namespace Marketplace924.Service
{
    public interface INotificationContentService : IDisposable
    {
        string GetUnreadNotificationsCountText(int unreadCount);
        List<Notification> GetNotificationsForUser(int recipientId);
        void MarkAsRead(int notificationId);
        void AddNotification(Notification notification);
    }
}