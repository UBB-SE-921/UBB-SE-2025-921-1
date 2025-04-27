using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using SharedClassLibrary.Shared;
using SharedClassLibrary.IRepository;

namespace MarketPlace924.Repository
{
    public class NotificationProxyRepository : INotificationRepository
    {
        public void AddNotification(Notification notification)
        {
            throw new NotImplementedException();
        }

        // public Notification CreateFromDataReader(IDataReader reader)
        // {
        //    throw new NotImplementedException();
        // }

        // public void Dispose()
        // {
        //    throw new NotImplementedException();
        // }
        public List<Notification> GetNotificationsForUser(int recipientId)
        {
            throw new NotImplementedException();
        }

        public void MarkAsRead(int notificationId)
        {
            throw new NotImplementedException();
        }
    }
}
