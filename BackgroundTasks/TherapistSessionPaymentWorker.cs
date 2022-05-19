using Database.Models;
using Database.RepositoryImplementations;
using Framework.Notifications;
using Microsoft.Extensions.Hosting;
using NCrontab;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundTasks
{
    // Crontab expression format:
    //
    // * * * * * *
    // - - - - - -
    // | | | | | |
    // | | | | | +----- day of week (0 - 6) (Sunday=0)
    // | | | | +------- month (1 - 12)
    // | | | +--------- day of month (1 - 31)
    // | | +----------- hour (0 - 23)
    // | +------------- min (0 - 59)
    // +--------------- sec (0 - 59)

    // Star (*) in the value field above means all legal values as in 
    // braces for that column. The value column can have a * or a list 
    // of elements separated by commas. An element is either a number in 
    // the ranges shown above or two numbers in the range separated by a 
    // hyphen (meaning an inclusive range). 
    //
    // Source: http://www.adminschoice.com/docs/crontab.htm
    //
    // Odavde sam uzeo dokumentaciju formata: https://searchcode.com/codesearch/view/7185858/

    public class TherapistSessionPaymentWorker : IHostedService
    {
        private UnitOfWork _context;
        private readonly CrontabSchedule _crontabSchedule;
        private DateTime _nextRun;
        private const string Schedule = "0 0 4 * * *"; //run day at 4 am
        //private const string Schedule = "0 * * * * *"; //svaki minut
        private const double THERAPIST_FEE = 200;
        private INotificationRepository _notificationRepository;

        public TherapistSessionPaymentWorker(INotificationRepository notificationRepository)
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

                        await payBookedSessionsToTherapists();

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

        private async Task payBookedSessionsToTherapists()
        {
            try
            {
                var notPaidYet = _context.BookedSessions.Find(s => s.TherapistIsPaid == false).ToList();

                if (notPaidYet.Any())
                {
                    var finishedSessions = notPaidYet.Where(s => DateTime.UtcNow > s.SessionDate.Date.Add(s.EndTime.TimeOfDay)).ToList();

                    foreach (var sessionToPay in finishedSessions)
                    {
                        payTherapist(sessionToPay);

                        var therapist = _context.Therapists.GetById(sessionToPay.TherapistId);

                        if (therapist != null)
                        {
                            var therapistUser = _context.AspNetUsers.Find(u => u.TherapistAccountId == therapist.Id).ToList();

                            if (therapistUser.Count > 0)
                                _notificationRepository.Send(new Notifications
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    SenderUserId = "System",
                                    ReceiverUserId = therapistUser[0].Id,
                                    Title = $"You have been paid {sessionToPay.Price - THERAPIST_FEE} RSD for your session with client {sessionToPay.ClientFirstName} {sessionToPay.ClientLastName}",
                                    Body = "-",
                                    Severity = "success",
                                    Read = false,
                                    SendingDateTime = DateTime.UtcNow,
                                    Icon = "far fa-money-bill-alt"
                                });
                        }

                        _context.HostedServicesInformation.Insert(new HostedServicesInformation
                        {
                            Id = Guid.NewGuid().ToString(),
                            Information = $"-> Therapist [{sessionToPay.TherapistId}] is paid {sessionToPay.Price - THERAPIST_FEE}. Booking ID: {sessionToPay.Id}. Session ID: {sessionToPay.SessionId}.",
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
                        Information = "No therapist to be paid.",
                        InformationType = HostedServicesInformationType.UserInformation,
                        ExecutionDateTime = DateTime.UtcNow
                    });
                }

                await _context.SaveAsync();
                refreshContext();

                //Console.WriteLine($"Running a Background Task [TherapistSessionPaymentWorker] at {DateTime.UtcNow}.");
            }
            catch (Exception e)
            {
                _context.HostedServicesInformation.Insert(new HostedServicesInformation
                {
                    Id = Guid.NewGuid().ToString(),
                    Information = "Background Task failed in payBookedSessionsToTherapists: " + e.Message,
                    InformationType = HostedServicesInformationType.ErrorSystemInformation,
                    ExecutionDateTime = DateTime.UtcNow
                });

                _context.Save();
            }
        }

        private void payTherapist(BookedSessions bookedSession)
        {
            try
            {
                var therapist = _context.Therapists.GetById(bookedSession.TherapistId);

                if (therapist != null)
                {
                    double amountToAdd = 0;

                    // zadrzavamo 200din za sebe
                    if (bookedSession.Price - THERAPIST_FEE > 0)
                        amountToAdd = bookedSession.Price - THERAPIST_FEE;

                    therapist.Earnings += amountToAdd;

                    markPaid(bookedSession);
                }
            }
            catch (Exception e)
            {
                _context.HostedServicesInformation.Insert(new HostedServicesInformation
                {
                    Id = Guid.NewGuid().ToString(),
                    Information = "Background Task failed in payTherapist: " + e.Message,
                    InformationType = HostedServicesInformationType.ErrorSystemInformation,
                    ExecutionDateTime = DateTime.UtcNow
                });

                _context.Save();
            }
        }

        private void markPaid(BookedSessions bookedSession)
        {
            if (bookedSession != null)
            {
                bookedSession.TherapistIsPaid = true;
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
            public static string UserInformation { get { return "TherapistSessionPaymentWorker | user-information"; } }
            public static string SystemInformation { get { return "TherapistSessionPaymentWorker | system-information"; } }
            public static string ErrorUserInformation { get { return "TherapistSessionPaymentWorker | error-user-information"; } }
            public static string ErrorSystemInformation { get { return "TherapistSessionPaymentWorker | error-system-information"; } }
        }
    }
}
