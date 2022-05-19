using Database.Models;
using Database.RepositoryImplementations;
using Framework.Notifications;
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
    public class UpcomingSessionNotificationSenderWorker : IHostedService
    {
        private UnitOfWork _context;
        private readonly CrontabSchedule _crontabSchedule;
        private DateTime _nextRun;
        private const string Schedule = "* 00 06,13 * * *";
        //private const string Schedule = "0 * * * * *";
        private INotificationRepository _notificationRepository;

        public UpcomingSessionNotificationSenderWorker(INotificationRepository notificationRepository)
        {
            _context = new UnitOfWork(new LajsnaProbaContext());
            _notificationRepository = notificationRepository;

            try
            {
                _crontabSchedule = CrontabSchedule.Parse(Schedule, new CrontabSchedule.ParseOptions { IncludingSeconds = true });
                _nextRun = _crontabSchedule.GetNextOccurrence(DateTime.UtcNow);
            }
            catch (Exception e)
            {
                _context.HostedServicesInformation.Insert(new HostedServicesInformation
                {
                    Id = Guid.NewGuid().ToString(),
                    Information = "Background Task failed in Constructor: " + e.Message,
                    InformationType = HostedServicesInformationType.ErrorSystemInformation,
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
                        await Task.Delay(untilNextExecution(), cancellationToken); // ceka do sledeceg izvrsavanja

                        await notifyParticipantsOfUpcomingSessions();

                        _nextRun = _crontabSchedule.GetNextOccurrence(DateTime.UtcNow);
                    }
                }, cancellationToken);
            }
            catch (Exception e)
            {
                _context.HostedServicesInformation.Insert(new HostedServicesInformation
                {
                    Id = Guid.NewGuid().ToString(),
                    Information = "Background Task failed in StartAsync: " + e.Message,
                    InformationType = HostedServicesInformationType.ErrorSystemInformation,
                    ExecutionDateTime = DateTime.UtcNow
                });

                _context.Save();
            }

            return Task.CompletedTask;
        }

        private async Task notifyParticipantsOfUpcomingSessions()
        {
            try
            {
                var todaysSessions = _context.BookedSessions.Find(s => s.SessionDate.Date == DateTime.UtcNow.Date).ToList();

                if (todaysSessions.Any())
                {
                    var aboutToStart = todaysSessions.Where(s => s.StartTime.TimeOfDay >= DateTime.UtcNow.TimeOfDay).ToList();

                    foreach (var sessionToNotify in aboutToStart)
                    {
                        notifyClient(sessionToNotify);
                        notifyTherapist(sessionToNotify);

                        _context.HostedServicesInformation.Insert(new HostedServicesInformation
                        {
                            Id = Guid.NewGuid().ToString(),
                            Information = $"-> Therapist [{sessionToNotify.TherapistId}] and Client [{sessionToNotify.ClientId}] are notified about upcoming Session [{sessionToNotify.SessionId}]. Booking ID: {sessionToNotify.Id}.",
                            InformationType = HostedServicesInformationType.UserInformation,
                            ExecutionDateTime = DateTime.UtcNow
                        });
                    }
                }
                else
                {
                    _context.HostedServicesInformation.Insert(new HostedServicesInformation
                    {
                        Id = Guid.NewGuid().ToString(),
                        Information = "No Booked Session to notify about.",
                        InformationType = HostedServicesInformationType.UserInformation,
                        ExecutionDateTime = DateTime.UtcNow
                    });
                }

                await _context.SaveAsync();
                refreshContext();

                //Console.WriteLine($"Running a Background Task [UpcomingSessionNotificationSenderWorker] at {DateTime.UtcNow}.");
            }
            catch (Exception e)
            {
                _context.HostedServicesInformation.Insert(new HostedServicesInformation
                {
                    Id = Guid.NewGuid().ToString(),
                    Information = "Background Task failed in notifyParticipantsOfUpcomingSessions: " + e.Message,
                    InformationType = HostedServicesInformationType.ErrorSystemInformation,
                    ExecutionDateTime = DateTime.UtcNow
                });

                _context.Save();
            }
        }

        private void notifyClient(BookedSessions bookedSession)
        {
            try
            {
                var bookedSessionDateTime = bookedSession.SessionDate.Date + bookedSession.StartTime.TimeOfDay;
                var localizedDateTime = TimeZoneInfo.ConvertTimeFromUtc(bookedSessionDateTime, TimeZoneInfo.Local);

                _notificationRepository.Send(new Notifications
                {
                    Id = Guid.NewGuid().ToString(),
                    SenderUserId = "System",
                    ReceiverUserId = bookedSession.ClientId,
                    Title = $"You have an upcoming session with therapist {bookedSession.TherapistFirstName} {bookedSession.TherapistLastName} that starts on {localizedDateTime.Date.ToString("dd/MMM/yyyy")}, at {localizedDateTime.TimeOfDay}",
                    Body = "-",
                    Severity = "primary",
                    Read = false,
                    SendingDateTime = DateTime.UtcNow,
                    Icon = "far fa-comment-alt-smile"
                });
            }
            catch (Exception e)
            {
                _context.HostedServicesInformation.Insert(new HostedServicesInformation
                {
                    Id = Guid.NewGuid().ToString(),
                    Information = "Background Task failed in notifyClient: " + e.Message,
                    InformationType = HostedServicesInformationType.ErrorSystemInformation,
                    ExecutionDateTime = DateTime.UtcNow
                });

                _context.Save();
            }
        }

        private void notifyTherapist(BookedSessions bookedSession)
        {
            try
            {
                var bookedSessionDateTime = bookedSession.SessionDate.Date + bookedSession.StartTime.TimeOfDay;
                var localizedDateTime = TimeZoneInfo.ConvertTimeFromUtc(bookedSessionDateTime, TimeZoneInfo.Local);

                var therapistUser = _context.AspNetUsers.Find(u => u.TherapistAccountId == bookedSession.TherapistId).ToList();

                if (therapistUser.Count > 0)
                    _notificationRepository.Send(new Notifications
                    {
                        Id = Guid.NewGuid().ToString(),
                        SenderUserId = "System",
                        ReceiverUserId = therapistUser[0].Id,
                        Title = $"You have an upcoming session with client {bookedSession.ClientFirstName} {bookedSession.ClientLastName} that starts on {localizedDateTime.Date.ToString("dd/MMM/yyyy")}, at {localizedDateTime.TimeOfDay}",
                        Body = "-",
                        Severity = "primary",
                        Read = false,
                        SendingDateTime = DateTime.UtcNow,
                        Icon = "far fa-exclamation-circle"
                    });
            }
            catch (Exception e)
            {
                _context.HostedServicesInformation.Insert(new HostedServicesInformation
                {
                    Id = Guid.NewGuid().ToString(),
                    Information = "Background Task failed in notifyTherapist: " + e.Message,
                    InformationType = HostedServicesInformationType.ErrorSystemInformation,
                    ExecutionDateTime = DateTime.UtcNow
                });

                _context.Save();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private int untilNextExecution() => Math.Max(0, (int)_nextRun.Subtract(DateTime.UtcNow).TotalMilliseconds);

        private void refreshContext()
        {
            _context = new UnitOfWork(new LajsnaProbaContext());
        }

        private class HostedServicesInformationType
        {
            public static string UserInformation { get { return "UpcomingSessionNotificationSenderWorker | user-information"; } }
            public static string SystemInformation { get { return "UpcomingSessionNotificationSenderWorker | system-information"; } }
            public static string ErrorUserInformation { get { return "UpcomingSessionNotificationSenderWorker | error-user-information"; } }
            public static string ErrorSystemInformation { get { return "UpcomingSessionNotificationSenderWorker | error-system-information"; } }
        }
    }
}
