using Database.Models;
using Database.RepositoryImplementations;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;

namespace Framework.Notifications
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly UnitOfWork _context;
        private IHubContext<NotificationHub> _hubContext;

        public NotificationRepository(IHubContext<NotificationHub> hubContext)
        {
            _context = new UnitOfWork(new LajsnaProbaContext());
            _hubContext = hubContext;
        }

        public List<NotificationsViewModel> Get(string userId)
        {
            var notifications = _context.Notifications
                                        .Find(u => u.ReceiverUserId == userId && u.Read == false)
                                        .OrderByDescending(n => n.SendingDateTime)
                                        .ToList();

            var notificationsVm = new List<NotificationsViewModel>();

            foreach(var notification in notifications)
            {
                var notificationVm = new NotificationsViewModel();
                notificationVm.Map(notification);
                notificationsVm.Add(notificationVm);
            }

            return notificationsVm;
        }

        public void Read(string notificationId, string userId)
        {
            var notifications = _context.Notifications
                                        .Find(n => n.ReceiverUserId.Equals(userId)
                                        && n.Id == notificationId)
                                        .ToList();
                                        
            var notification = new Database.Models.Notifications();

            if (notifications.Count > 0)
                notification = notifications[0];

            notification.Read = true;
            _context.Notifications.Update(notification);
            _context.SaveAsync().Wait();
        }

        public void Send(Database.Models.Notifications notification)
        {
            _context.Notifications.Insert(notification);
            _context.SaveAsync().Wait();

            _hubContext.Clients.User(notification.ReceiverUserId).SendAsync("displayNotification", "");
        }
    }
}
