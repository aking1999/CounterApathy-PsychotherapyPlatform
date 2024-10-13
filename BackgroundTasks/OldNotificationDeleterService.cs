using Database.Models;
using Database.RepositoryImplementations;
using Framework.Emails;
using Framework.Emails.EmailTypes;
using Framework.Helpers;
using Framework.Helpers.ExtensionMethods;
using Framework.Implementations;
using Framework.Interfaces;
using Framework.Notifications;
using Framework.Notifications.NotificationTypes;
using Framework.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NCrontab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundTasks
{
    public class OldNotificationDeleterService : IHostedService
    {
        private readonly IMailService _mailService;
        private readonly INotificationRepository _notificationRepository;
        private UnitOfWork _context;
        private readonly ISystemErrorLogger _systemErrors;
        private readonly CrontabSchedule _crontabSchedule;
        private DateTime _nextRun;
        private const string SCHEDULE = "0 0 6 * * *"; //run day at 6 am
        //private const string SCHEDULE = "0 * * * * *"; //svaki minut
        private const int DELETE_NOTIFICATIONS_OLDER_THAN_MONTHS = 1;

        public OldNotificationDeleterService(IServiceScopeFactory serviceScopeFactory)
        {
            _systemErrors = new SystemErrorLogger();
            _context = new UnitOfWork(new LajsnaProbaContext());

            try
            {
                var scope = serviceScopeFactory.CreateScope();
                _mailService = scope.ServiceProvider.GetRequiredService<IMailService>();
                _notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

                _crontabSchedule = CrontabSchedule.Parse(SCHEDULE, new CrontabSchedule.ParseOptions { IncludingSeconds = true });
                _nextRun = _crontabSchedule.GetNextOccurrence(DateTime.UtcNow);
            }
            catch(Exception e)
            {
                _systemErrors.SaveError(e, "BackgroundTasks", "OldNotificationDeleterService", "Constructor");

                _context.HostedServicesInformation.Insert(new HostedServicesInformation
                {
                    Id = Helper.GenerateNumbersId(),
                    Information = $"Hosted service failed in 'Constructor': {e.GetRootException().Message}".TakeMax(450),
                    InformationType = HostedServicesInformationType.Error.TakeMax(128),
                    ExecutionDateTime = DateTime.UtcNow
                });

                _context.Save();
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                Task.Run(async () =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        await Task.Delay(UntilNextExecution(), cancellationToken);

                        await DeleteOldNotificationsAsync();

                        _nextRun = _crontabSchedule.GetNextOccurrence(DateTime.UtcNow);
                    }
                }, cancellationToken);
            }
            catch(Exception e)
            {
                _systemErrors.SaveError(e, "BackgroundTasks", "OldNotificationDeleterService", "StartAsync");

                HandleErrorAsync(
                    hostedServiceInformationMessage: $"Hosted service failed in 'StartAsync': {e.GetRootException().Message}".TakeMax(450),
                    title: "Error: OldNotificationDeleterService failed in 'StartAsync'",
                    body: $"Hosted service 'OldNotificationDeleterService' failed in 'StartAsync', at UTC {DateTime.UtcNow}."
                    ).Wait();

                _context.Save();
            }

            return Task.CompletedTask;
        }

        private async Task DeleteOldNotificationsAsync()
        {
            try
            {
                var today = DateTime.UtcNow;
                var deleteNotificationsBeforeDate = today.AddMonths(DELETE_NOTIFICATIONS_OLDER_THAN_MONTHS * (-1));
                var oldNotifications = _context.Notifications.Find(n => n.SendingDateTime <= deleteNotificationsBeforeDate && (!n.Important.HasValue || (n.Important.HasValue && !n.Important.Value))).ToList();

                if (oldNotifications.Any())
                {
                    oldNotifications.ForEach(n => _context.Notifications.Delete(n));

                    _context.HostedServicesInformation.Insert(new HostedServicesInformation
                    {
                        Id = Helper.GenerateNumbersId(),
                        Information = "Notification(s) deleted.",
                        InformationType = HostedServicesInformationType.Information.TakeMax(128),
                        ExecutionDateTime = DateTime.UtcNow
                    });
                }
                else
                {
                    _context.HostedServicesInformation.Insert(new HostedServicesInformation
                    {
                        Id = Helper.GenerateNumbersId(),
                        Information = "No notification to be deleted.",
                        InformationType = HostedServicesInformationType.Information.TakeMax(128),
                        ExecutionDateTime = DateTime.UtcNow
                    });
                }
                                      
            }
            catch(Exception e)
            {
                await _systemErrors.SaveErrorAsync(e,
                    "BackgroundTasks", "OldNotificationDeleterService", "DeleteOldNotificationsAsync");

                HandleErrorAsync(
                    hostedServiceInformationMessage: $"Hosted service failed in 'DeleteOldNotificationsAsync': {e.GetRootException().Message}".TakeMax(450),
                    title: "Error: OldNotificationDeleterService failed to delete notifications",
                    body: $"Hosted service 'OldNotificationDeleterService' failed in 'DeleteOldNotificationsAsync', at UTC {DateTime.UtcNow}."
                    ).Wait();
            }

            await _context.SaveAsync();
            RefreshContext();
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private int UntilNextExecution() => Math.Max(0, (int)_nextRun.Subtract(DateTime.UtcNow).TotalMilliseconds);

        private void RefreshContext()
        {
            _context = new UnitOfWork(new LajsnaProbaContext());
        }

        private async Task HandleErrorAsync(string hostedServiceInformationMessage, string title, string body)
        {
            _context.HostedServicesInformation.Insert(new HostedServicesInformation
            {
                Id = Helper.GenerateNumbersId(),
                Information = hostedServiceInformationMessage,
                InformationType = HostedServicesInformationType.Error.TakeMax(128),
                ExecutionDateTime = DateTime.UtcNow
            });

            _context.Save();

            await _notificationRepository.SendAsync(new NotificationForRole
            {
                SenderUserId = SystemInformation.Name,
                Title = body,
                Body = null,
                Severity = "danger",
                SendingDateTime = DateTime.UtcNow,
                Icon = "far fa-exclamation-circle",
                Important = true
            }, UserRoles.Admin);

            await _mailService.SendEmailAsync(new EmailForRole
            {
                Subject = title,
                Body = body
            }, UserRoles.Admin);

            return;
        }

        private class HostedServicesInformationType
        {
            public static string Information { get { return "OldNotificationDeleterService | Information"; } }
            public static string Error { get { return "OldNotificationDeleterService | Error"; } }
        }
    }
}
