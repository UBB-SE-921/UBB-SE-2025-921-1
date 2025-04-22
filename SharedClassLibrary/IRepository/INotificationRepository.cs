using System.Collections.Generic;
using System.Data;
using SharedClassLibrary.Domain;

namespace SharedClassLibrary.IRepository
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