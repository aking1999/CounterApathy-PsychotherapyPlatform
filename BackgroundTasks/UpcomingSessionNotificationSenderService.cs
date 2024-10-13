using Database.Models;
using Database.RepositoryImplementations;
using Framework.Interfaces;
using Framework.Notifications;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using NCrontab;
using System;
using Framework.Helpers.ExtensionMethods;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Framework.Emails;
using Framework.Emails.EmailTypes;
using Framework.Providers;
using Framework.Notifications.NotificationTypes;
using Framework.Helpers;
using Framework.Implementations;

namespace BackgroundTasks
{
    public class UpcomingSessionNotificationSenderService : IHostedService
    {
        private readonly IMailService _mailService;
        private readonly IDateTimeHelper _dateHelper;
        private readonly ISystemErrorLogger _systemErrors;
        private readonly INotificationRepository _notificationRepository;
        private UnitOfWork _context;
        private readonly CrontabSchedule _crontabSchedule;
        private DateTime _nextRun;
        private const string SCHEDULE = "* 00 04,12 * * *";
        //private const string SCHEDULE = "0 * * * * *";
        private readonly UserManager<CustomClient> _userManager;

        public UpcomingSessionNotificationSenderService(IServiceScopeFactory serviceScopeFactory)
        {
            _context = new UnitOfWork(new LajsnaProbaContext());

            try
            {
                var scope = serviceScopeFactory.CreateScope();
                _mailService = scope.ServiceProvider.GetRequiredService<IMailService>();
                _dateHelper = scope.ServiceProvider.GetRequiredService<IDateTimeHelper>();
                _systemErrors = new SystemErrorLogger();
                _notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                _userManager = scope.ServiceProvider.GetRequiredService<UserManager<CustomClient>>();

                _crontabSchedule = CrontabSchedule.Parse(SCHEDULE, new CrontabSchedule.ParseOptions { IncludingSeconds = true });
                _nextRun = _crontabSchedule.GetNextOccurrence(DateTime.UtcNow);
            }
            catch (Exception e)
            {
                _systemErrors.SaveError(e, "BackgroundTasks", "UpcomingSessionNotificationSenderService", "Constructor");

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

                        await NotifyParticipantsOfUpcomingSessionsAsync();

                        _nextRun = _crontabSchedule.GetNextOccurrence(DateTime.UtcNow);
                    }
                }, cancellationToken);
            }
            catch (Exception e)
            {
                _systemErrors.SaveError(e, "BackgroundTasks", "UpcomingSessionNotificationSenderService", "StartAsync");

                HandleErrorAsync(
                    hostedServiceInformationMessage: $"Hosted service failed in " +
                    $"'StartAsync': {e.GetRootException().Message}".TakeMax(450),
                    title: "Error: One or more session participants are left unnotified",
                    body: "Hosted service 'UpcomingSessionNotificationSenderService' failed in 'StartAsync', " +
                    "leaving one or more session participants unnotified. " +
                    "Investigate the table 'HostedServicesInformation' for more details."
                    ).Wait();
            }

            return Task.CompletedTask;
        }

        private async Task NotifyParticipantsOfUpcomingSessionsAsync()
        {
            try
            {
                var today = DateTime.UtcNow;
                var todaysSessions = _context.BookedSessions.ReadOnlyFind(s => s.StartTime.Date == today.Date).ToList();

                if (todaysSessions.Any())
                {
                    foreach (var sessionToNotify in todaysSessions.Where(s => s.StartTime.TimeOfDay >= today.TimeOfDay).ToList())
                    {
                        if (sessionToNotify.ClientId != "anonymous" && sessionToNotify.ClientId != "unauthorized")
                            await NotifyClientAsync(sessionToNotify);

                        await NotifyTherapistAsync(sessionToNotify);

                        _context.HostedServicesInformation.Insert(new HostedServicesInformation
                        {
                            Id = Helper.GenerateNumbersId(),
                            Information = $"-> Therapist '{sessionToNotify.TherapistId}' and client '{sessionToNotify.ClientId}' are notified about upcoming session '{sessionToNotify.SessionId}'. BookingId: {sessionToNotify.Id}.".TakeMax(450),
                            InformationType = HostedServicesInformationType.Information.TakeMax(128),
                            ExecutionDateTime = DateTime.UtcNow
                        });
                    }
                }
                else
                {
                    _context.HostedServicesInformation.Insert(new HostedServicesInformation
                    {
                        Id = Helper.GenerateNumbersId(),
                        Information = "No booked session to notify about.",
                        InformationType = HostedServicesInformationType.Information.TakeMax(128),
                        ExecutionDateTime = DateTime.UtcNow
                    });
                }

                await _context.SaveAsync();
                RefreshContext();
            }
            catch (Exception e)
            {
                await _systemErrors.SaveErrorAsync(e,
                    "BackgroundTasks",
                    "UpcomingSessionNotificationSenderService",
                    "NotifyParticipantsOfUpcomingSessionsAsync");

                await HandleErrorAsync(
                    hostedServiceInformationMessage: $"Hosted service failed in " +
                    $"'NotifyParticipantsOfUpcomingSessionsAsync': {e.GetRootException().Message}".TakeMax(450),
                    title: "Error: One or more session participants are left unnotified",
                    body: "Hosted service 'UpcomingSessionNotificationSenderService' failed in 'NotifyParticipantsOfUpcomingSessionsAsync', " +
                    "leaving one or more session participants unnotified. " +
                    "Investigate the table 'HostedServicesInformation' for more details."
                    );
            }
        }

        private async Task NotifyClientAsync(BookedSessions bookedSession)
        {
            try
            {
                var localizedDateTime = _dateHelper.ConvertDateTimeFromUtcToLocal(bookedSession.StartTime);

                await _notificationRepository.SendAsync(new Notifications
                {
                    Id = Helper.GenerateNumbersId(),
                    SenderUserId = "System",
                    ReceiverUserId = bookedSession.ClientId,
                    Title = $"Imate zakazanu seansu sa psihoterapeutom {bookedSession.TherapistFirstName} {bookedSession.TherapistLastName} koja " +
                    $"počinje datuma {_dateHelper.DateStringFromDateTime(localizedDateTime)}, u {_dateHelper.TimeStringFromDateTime(localizedDateTime)}h.",
                    Body = null,
                    Severity = "info",
                    Read = false,
                    SendingDateTime = DateTime.UtcNow,
                    Icon = "fal fa-comment-alt-smile",
                    Important = false
                });
            }
            catch (Exception e)
            {
                _systemErrors.SaveError(e,
                    "BackgroundTasks",
                    "UpcomingSessionNotificationSenderService",
                    "NotifyClientAsync");

                await HandleErrorAsync(
                    hostedServiceInformationMessage: $"Hosted service failed in 'NotifyClientAsync': {e.GetRootException().Message}".TakeMax(450),
                    title: "Error: The client is left unnotified",
                    body: $"Hosted service 'UpcomingSessionNotificationSenderService' failed in " +
                    $"'NotifyClientAsync', at UTC {DateTime.UtcNow}, leaving the client unnotified. " +
                    $"Investigate the table 'HostedServicesInformation' for more details."
                    );
            }

            return;
        }

        private async Task NotifyTherapistAsync(BookedSessions bookedSession)
        {
            try
            {
                var localizedDateTime = _dateHelper.ConvertDateTimeFromUtcToLocal(bookedSession.StartTime);
                var therapistUser = await _userManager.FindByTherapistAccountIdAsync(bookedSession.TherapistId);

                if (therapistUser != null)
                    await _notificationRepository.SendAsync(new Notifications
                    {
                        Id = Helper.GenerateNumbersId(),
                        SenderUserId = "System",
                        ReceiverUserId = therapistUser.Id,
                        Title = $"Imate seansu sa klijentom {bookedSession.ClientFirstName} {bookedSession.ClientLastName} koja " +
                        $"počinje datuma {_dateHelper.DateStringFromDateTime(localizedDateTime)}, u {_dateHelper.TimeStringFromDateTime(localizedDateTime)}h.",
                        Body = null,
                        Severity = "info",
                        Read = false,
                        SendingDateTime = DateTime.UtcNow,
                        Icon = "fal fa-comment-alt-smile",
                        Important = false
                    });
                else throw new Exception($"Therapist user account not found. TherapistId '{bookedSession.TherapistId}', UserId '{(therapistUser != null ? therapistUser.Id : "NULL")}'.");
            }
            catch (Exception e)
            {
                _systemErrors.SaveError(e, "BackgroundTasks", "UpcomingSessionNotificationSenderService", "NotifyTherapistAsync");

                await HandleErrorAsync(
                    hostedServiceInformationMessage: $"Hosted service failed in 'NotifyTherapistAsync': {e.GetRootException().Message}".TakeMax(450),
                    title: "Error: The therapist is left unnotified",
                    body: $"Hosted service 'UpcomingSessionNotificationSenderService' failed in 'NotifyTherapistAsync', " +
                    $"at UTC {DateTime.UtcNow}, leaving the therapist unnotified. " +
                    $"Investigate the table 'HostedServicesInformation' for more details."
                    );
            }
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
            public static string Information { get { return "UpcomingSessionNotificationSenderService | Information"; } }
            public static string Error { get { return "UpcomingSessionNotificationSenderService | Error"; } }
        }
    }
}
