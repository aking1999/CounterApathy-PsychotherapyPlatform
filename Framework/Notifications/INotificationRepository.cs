using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Notifications
{
    public interface INotificationRepository
    {
        List<NotificationsViewModel> Get(string userId);
        void Send(Database.Models.Notifications notification);
        void Read(string notificationId, string userId);
    }
}
