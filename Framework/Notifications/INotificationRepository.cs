using Framework.Providers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Notifications
{
    public interface INotificationRepository
    {
        List<NotificationsViewModel> GetAll(string userId);
        List<NotificationsViewModel> GetUnread(string userId);
        //List<NotificationsViewModel> GetImportant(string userId);
        //List<NotificationsViewModel> GetDeleted(string userId);
        Task SendAsync(Database.Models.Notifications notification);
        Task SendAsync(NotificationTypes.NotificationForRole notification, string userRole);
        Task ReadAsync(string notificationId, string userId);
        Task ImportantAsync(string notificationId, string userId);
        Task UnimportantAsync(string notificationId, string userId);
    }
}
