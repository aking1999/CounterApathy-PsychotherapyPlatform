using Database.Models;
using Database.RepositoryImplementations;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Framework.Interfaces;
using Framework.Helpers.ExtensionMethods;
using Framework.Providers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using Framework.Implementations;

namespace Framework.Notifications
{
    public class NotificationRepository : INotificationRepository
    {
        private const string PROJECT = "Framework";
        private const string CLASS = "NotificationRepository";

        private readonly UnitOfWork _context;
        private readonly IDateTimeHelper _dateHelper;
        private IHubContext<NotificationHub> _hubContext;
        private readonly ISystemErrorLogger _systemErrors;
        private readonly UserManager<CustomClient> _userManager;

        public NotificationRepository(IServiceProvider serviceProvider,
            IDateTimeHelper dateHelper,
            IHubContext<NotificationHub> hubContext)
        {
            _context = new UnitOfWork(new LajsnaProbaContext());
            _dateHelper = dateHelper;
            _hubContext = hubContext;
            _systemErrors = new SystemErrorLogger();
            _userManager = serviceProvider.GetRequiredService<UserManager<CustomClient>>();
        }

        public List<NotificationsViewModel> GetAll(string userId)
        {
            var notifications = _context.Notifications
                            .ReadOnlyFind(u => u.ReceiverUserId == userId)
                            .OrderByDescending(n => n.SendingDateTime)
                            .ToList();

            var notificationsVm = new List<NotificationsViewModel>();

            foreach (var notification in notifications)
            {
                var notificationVm = new NotificationsViewModel(_dateHelper);
                notificationVm.Map(notification);
                notificationsVm.Add(notificationVm);
            }

            return notificationsVm;
        }

        public List<NotificationsViewModel> GetUnread(string userId)
        {
            var notifications = _context.Notifications
                                        .ReadOnlyFind(u => u.ReceiverUserId == userId && u.Read == false)
                                        .OrderByDescending(n => n.SendingDateTime)
                                        .ToList();

            var notificationsVm = new List<NotificationsViewModel>();

            foreach (var notification in notifications)
            {
                var notificationVm = new NotificationsViewModel(_dateHelper);
                notificationVm.Map(notification);
                notificationsVm.Add(notificationVm);
            }

            return notificationsVm;
        }

        public async Task ReadAsync(string notificationId, string userId)
        {
            var notification = _context.Notifications
                                        .Find(n => n.ReceiverUserId.Equals(userId)
                                        && n.Id == notificationId)
                                        .SingleOrDefault();

            if (notification != default)
            {
                notification.Read = true;
                await _context.SaveAsync();
            }

            return;
        }

        public async Task ImportantAsync(string notificationId, string userId)
        {
            var notification = _context.Notifications
                                        .Find(n => n.ReceiverUserId.Equals(userId)
                                        && n.Id == notificationId)
                                        .SingleOrDefault();

            if (notification != default)
            {

                notification.Important = true;
                await _context.SaveAsync();
            }

            return;
        }

        public async Task UnimportantAsync(string notificationId, string userId)
        {
            var notification = _context.Notifications
                                        .Find(n => n.ReceiverUserId.Equals(userId)
                                        && n.Id == notificationId)
                                        .SingleOrDefault();

            if (notification != default)
            {
                notification.Important = false;
                await _context.SaveAsync();
            }

            return;
        }

        public async Task SendAsync(Database.Models.Notifications notification)
        {
            notification.Title = notification.Title.TakeMax(1024);
            notification.Body = notification.Body.TakeMax(384);
            _context.Notifications.Insert(notification);

            if ((await _context.SaveAsync()) > 0)
                await _hubContext.Clients.User(notification.ReceiverUserId).SendAsync("displayNotification", "");
        }

        public async Task SendAsync(NotificationTypes.NotificationForRole notification, string userRole)
        {
            try
            {
                var notificationToInsert = new Database.Models.Notifications
                {
                    SenderUserId = "System",
                    Title = notification.Title.TakeMax(1024),
                    Body = notification.Body.TakeMax(384),
                    Severity = notification.Severity,
                    Read = false,
                    SendingDateTime = notification.SendingDateTime,
                    Icon = notification.Icon,
                    Important = notification.Important
                };

                var usersToNotify = (await _userManager.GetUsersInRoleAsync(userRole)).Select(usr => usr.Id);

                if (!usersToNotify.Any())
                    throw new Exception($"No users in role '{userRole}' to send notification to.");

                foreach (var receiverUserId in usersToNotify)
                {
                    notificationToInsert.Id = Guid.NewGuid().ToString();
                    notificationToInsert.ReceiverUserId = receiverUserId;
                    _context.Notifications.Insert(notificationToInsert);

                    if ((await _context.SaveAsync()) > 0)
                        await _hubContext.Clients.User(receiverUserId).SendAsync("displayNotification", "");
                }
            }
            catch (Exception e)
            {
                await _systemErrors.SaveErrorAsync(e, PROJECT, CLASS, "SendAsync");
            }
        }
    }
}
