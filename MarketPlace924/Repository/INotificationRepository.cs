using System.Collections.Generic;
using System.Data;
using Marketplace924.Domain;

namespace Marketplace924.Repository
{
    public interface INotificationRepository
    {
        void AddNotification(Notification notification);
        Notification CreateFromDataReader(IDataReader reader);
        void Dispose();
        List<Notification> GetNotificationsForUser(int recipientId);
        void MarkAsRead(int notificationId);
    }
}