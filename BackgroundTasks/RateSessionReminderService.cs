using Database.Models;
using Database.RepositoryImplementations;
using Framework.Emails;
using Framework.Emails.EmailTypes;
using Framework.Helpers;
using Framework.Helpers.ExtensionMethods;
using Framework.Implementations;
using Framework.Interfaces;
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
    public class RateSessionReminderService : IHostedService
    {
        private readonly IMailService _mailService;
        private readonly ISystemErrorLogger _systemErrors;
        private UnitOfWork _context;
        private readonly CrontabSchedule _crontabSchedule;
        private DateTime _nextRun;
        private const string SCHEDULE = "0 0 10 * * *";

        public RateSessionReminderService(IServiceScopeFactory serviceScopeFactory)
        {
            _systemErrors = new SystemErrorLogger();
            _context = new UnitOfWork(new LajsnaProbaContext());

            try
            {
                _mailService = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IMailService>();

                _crontabSchedule = CrontabSchedule.Parse(SCHEDULE, new CrontabSchedule.ParseOptions { IncludingSeconds = true });
                _nextRun = _crontabSchedule.GetNextOccurrence(DateTime.UtcNow);
            }
            catch(Exception e)
            {
                _systemErrors.SaveError(e, "BackgroundTasks", "RateSessionReminderService", "Constructor");

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

                        await RemindClientsOfUnratedSessionsAsync();

                        _nextRun = _crontabSchedule.GetNextOccurrence(DateTime.UtcNow);
                    }
                }, cancellationToken);
            }
            catch(Exception e)
            {
                _systemErrors.SaveError(e, "BackgroundTasks", "RateSessionReminderService", "StartAsync");

                HandleErrorAsync(
                    hostedServiceInformationMessage: $"Hosted service failed in " +
                    $"'StartAsync': {e.GetRootException().Message}".TakeMax(450),
                    title: "Error: One or more therapists are left unpaid",
                    body: $"Hosted service 'RateSessionReminderService' failed in 'StartAsync', " +
                    $"at UTC {DateTime.UtcNow}, leaving one or more clients unreminded. " +
                    $"Investigate the table 'HostedServicesInformation' for more details."
                    ).Wait();
            }

            return Task.CompletedTask;
        }

        private async Task RemindClientsOfUnratedSessionsAsync()
        {
            try
            {
                var yesterday = DateTime.UtcNow.AddDays(-1);
                var yesterdaysSessions = _context.BookedSessions.ReadOnlyFind(s => s.EndTime.Date == yesterday.Date).ToList();

                if (yesterdaysSessions.Any())
                {
                    var yesterdaysUnratedSessions = new List<BookedSessions>();

                    foreach(var yesterdaysSession in yesterdaysSessions)
                    {
                        if (!_context.PendingRatings.ReadOnlyAny(pr => pr.BookedSessionId == yesterdaysSession.Id) &&
                            !_context.Ratings.ReadOnlyAny(r => r.BookedSessionId == yesterdaysSession.Id))
                            yesterdaysUnratedSessions.Add(yesterdaysSession);
                    }

                    foreach (var sessionToRemind in yesterdaysUnratedSessions.ToLookup(s => s.ClientEmail, s => s))
                    {
                        var bookedSessionsToRemindAbout = new List<BookedSessions>();

                        foreach (var session in sessionToRemind)
                        {
                            bookedSessionsToRemindAbout.Add(session);
                        }

                        //sessionToRemind.Key ovo je imejl
                        //bookedSessionEmailsToRemindAbout ovo su id-evi booked seansi
                        //u mailservice prosledim celu ovu bookedSessionEmailsToRemindAbout
                        //i tamo stavim 3 if-else
                        //prvi if ako je == anonymous || == unauthorized neka generise imejl sa anoniman u URL
                        //ako je == authenticated neka generise onaj URL gde treba da se loguje da bi ocenio
                        // za == authenticated ne mora za svaki pojedinacni url jer se sva ocenjivanja rade ne jednoj stranici
                        //znaci pitam ako za dictionary kljuc "authenticated" ima vrednosti u listi stringova, generisati samo onaj jedan url ka stranici na kojo se sve ocenjivanje za authenticated klijente 
                        //ako je nesto trece nek zapise gresku
                        await _mailService.SendUnratedSessionsReminderEmailAsync(email: new UnratedSessionReminderEmail
                        {
                            ToEmail = sessionToRemind.Key,
                            BookedSessionsToRemindAbout = bookedSessionsToRemindAbout
                        }, includeTemplateIfExists: true);
                    }
                }
                else
                {
                    _context.HostedServicesInformation.Insert(new HostedServicesInformation
                    {
                        Id = Helper.GenerateNumbersId(),
                        Information = "No unrated session to remind clients about.",
                        InformationType = HostedServicesInformationType.Information.TakeMax(128),
                        ExecutionDateTime = DateTime.UtcNow
                    });
                }

                await _context.SaveAsync();
                RefreshContext();
            }
            catch(Exception e)
            {
                await _systemErrors.SaveErrorAsync(e,
                    "BackgroundTasks",
                    "RateSessionReminderService",
                    "RemindClientsOfUnratedSessionsAsync");

                await HandleErrorAsync(
                    hostedServiceInformationMessage: $"Hosted service failed in 'RemindClientsOfUnratedSessionsAsync': {e.GetRootException().Message}".TakeMax(450),
                    title: "Error: One or more clients are left unreminded",
                    body: $"Hosted service 'RateSessionReminderService' failed in 'RemindClientsOfUnratedSessionsAsync', at UTC {DateTime.UtcNow}, leaving one or more clients unreminded. Investigate the table 'HostedServicesInformation' for more details."
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

            await _mailService.SendEmailAsync(new EmailForRole
            {
                Subject = title,
                Body = body
            }, UserRoles.Admin);

            return;
        }

        private class HostedServicesInformationType
        {
            public static string Information { get { return "RateSessionReminderService | Information"; } }
            public static string Error { get { return "RateSessionReminderService | Error"; } }
        }
    }
}
