using Database.Models;
using Database.RepositoryImplementations;
using Framework.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using NCrontab;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Framework.Helpers.ExtensionMethods;
using Microsoft.Extensions.DependencyInjection;
using Framework.Helpers;
using Framework.Emails;
using Framework.Interfaces;
using Framework.Emails.EmailTypes;
using Framework.Implementations;
using Framework.Providers;
using Framework.Notifications.NotificationTypes;
using Microsoft.Extensions.Configuration;
using System.Transactions;
using Framework.Models;

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

    public class TherapistSessionPaymentService : IHostedService
    {
        private readonly IMailService _mailService;
        private readonly IConfiguration _configuration;
        private readonly ISystemErrorLogger _systemErrors;
        //private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly INotificationRepository _notificationRepository;
        private UnitOfWork _context;
        private readonly CrontabSchedule _crontabSchedule;
        private DateTime _nextRun;
        private const string SCHEDULE = "0 0 5 * * *"; //run day at 4 am
        //private const string SCHEDULE = "0 * * * * *"; //svaki minut
        private readonly UserManager<CustomClient> _userManager;

        public TherapistSessionPaymentService(IServiceScopeFactory serviceScopeFactory)
        {
            _systemErrors = new SystemErrorLogger();
            _context = new UnitOfWork(new LajsnaProbaContext());

            try
            {
                var scope = serviceScopeFactory.CreateScope();
                _mailService = scope.ServiceProvider.GetRequiredService<IMailService>();
                _configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                _notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                _userManager = scope.ServiceProvider.GetRequiredService<UserManager<CustomClient>>();

                _crontabSchedule = CrontabSchedule.Parse(SCHEDULE, new CrontabSchedule.ParseOptions { IncludingSeconds = true });
                _nextRun = _crontabSchedule.GetNextOccurrence(DateTime.UtcNow);
            }
            catch (Exception e)
            {
                _systemErrors.SaveError(e, "BackgroundTasks", "TherapistSessionPaymentService", "Constructor");

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

                        await PayBookedSessionsToTherapistsAsync();

                        _nextRun = _crontabSchedule.GetNextOccurrence(DateTime.UtcNow);
                    }
                }, cancellationToken);
            }
            catch (Exception e)
            {
                _systemErrors.SaveError(e, "BackgroundTasks", "TherapistSessionPaymentService", "StartAsync");

                HandleErrorAsync(
                    hostedServiceInformationMessage: $"Hosted service failed in " +
                    $"'StartAsync': {e.GetRootException().Message}".TakeMax(450),
                    title: "Error: One or more therapists are left unpaid",
                    body: $"Hosted service 'TherapistSessionPaymentService' failed in 'StartAsync', " +
                    $"at UTC {DateTime.UtcNow}, leaving one or more therapists unpaid. " +
                    $"Investigate the table 'HostedServicesInformation' for more details."
                    ).Wait();
            }

            return Task.CompletedTask;
        }

        private async Task PayBookedSessionsToTherapistsAsync()
        {
            try
            {
                var notPaidYet = _context.BookedSessions.Find(s => s.TherapistIsPaid == false).ToList();

                if (notPaidYet.Any())
                {
                    var now = DateTime.UtcNow;
                    foreach (var sessionToPay in notPaidYet.Where(s => now > s.EndTime).ToList())
                    {
                        await PayTherapistAsync(sessionToPay);
                    }
                }
                else
                {
                    _context.HostedServicesInformation.Insert(new HostedServicesInformation
                    {
                        Id = Helper.GenerateNumbersId(),
                        Information = "No therapist to be paid.",
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
                    "BackgroundTasks", "TherapistSessionPaymentService", "PayBookedSessionsToTherapistsAsync");

                await HandleErrorAsync(
                    hostedServiceInformationMessage: $"Hosted service failed in 'PayBookedSessionsToTherapistsAsync': {e.GetRootException().Message}".TakeMax(450),
                    title: "Error: One or more therapists are left unpaid",
                    body: $"Hosted service 'TherapistSessionPaymentService' failed in 'PayBookedSessionsToTherapistsAsync', at UTC {DateTime.UtcNow}, leaving one or more therapists unpaid. Investigate the table 'HostedServicesInformation' for more details."
                    );
            }
        }

        private async Task PayTherapistAsync(BookedSessions bookedSession)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(bookedSession.TherapistPaymentTransactionId))
                {
                    await _systemErrors.SaveErrorAsync($"Therapist '{bookedSession.TherapistId}' is already paid for booked session '{bookedSession.Id}', " +
                            $"because BookedSession.TherapistPaymentTransactionId is not null and its value is '{bookedSession.TherapistPaymentTransactionId}'," +
                            $"which means the transaction has already happened.", "BackgroundTasks", "TherapistSessionPaymentService", "PayTherapistAsync");

                    bookedSession.TherapistIsPaid = true;
                    _context.BookedSessions.Update(bookedSession);
                    await _context.SaveAsync();
                    
                    return;
                }

                var therapist = _context.Therapists.GetById(bookedSession.TherapistId);
                var therapistUser = await _userManager.FindByTherapistAccountIdAsync(bookedSession.TherapistId);

                if (therapist != null && therapistUser != null)
                {
                    TherapistFee therapistFee = null;
                    double amountToAdd = 0;

                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        try
                        {
                            therapistFee = _configuration.GetSection("TherapistFee").Get<TherapistFee>();

                            // We keep X percent fee (THERAPIST_FEE)
                            amountToAdd = bookedSession.Price * (1 - (therapistFee.PercentageAmount / 100));

                            therapist.Earnings += amountToAdd;

                            bookedSession.TherapistIsPaid = true;
                            bookedSession.TherapistPaymentTransactionId = Helper.GenerateNumbersId();

                            _context.Transactions.Insert(new Transactions
                            {
                                Id = bookedSession.TherapistPaymentTransactionId,
                                SenderId = PaymentTypesProvider.CounterApathyPayment.Id,
                                ReceiverId = therapistUser.Id,
                                DateTime = DateTime.UtcNow,
                                Amount = amountToAdd,
                                CurrencyCode = therapistFee.CurrencyCode
                            });

                            _context.TherapistEarningsLogs.Insert(new TherapistEarningsLogs
                            {
                                Id = Helper.GenerateNumbersId(),
                                TherapistId = bookedSession.TherapistId,
                                Amount = amountToAdd,
                                CurrentAmount = therapist.Earnings,
                                EarningsDateTime = DateTime.UtcNow,
                                PaymentTypeId = PaymentTypesProvider.CounterApathyPayment.Id,
                                PaymentTypeName = PaymentTypesProvider.CounterApathyPayment.Name,
                                TransactionId = bookedSession.TherapistPaymentTransactionId
                            });

                            _context.HostedServicesInformation.Insert(new HostedServicesInformation
                            {
                                Id = Helper.GenerateNumbersId(),
                                Information = $"-> Therapist '{bookedSession.TherapistId}' is paid {therapistFee.CurrencyCode} {amountToAdd}. BookingId: {bookedSession.Id}. SessionId: {bookedSession.SessionId}.".TakeMax(450),
                                InformationType = HostedServicesInformationType.Information.TakeMax(128),
                                ExecutionDateTime = DateTime.UtcNow
                            });

                            _context.Therapists.Update(therapist);
                            _context.BookedSessions.Update(bookedSession);

                            await _context.SaveAsync();

                            scope.Complete();
                        }
                        catch(Exception)
                        {
                            scope.Dispose();
                            throw;
                        }
                    }

                    IDateTimeHelper dateHelper = new DateTimeHelper();

                    await _notificationRepository.SendAsync(new Notifications
                    {
                        Id = Helper.GenerateNumbersId(),
                        SenderUserId = SystemInformation.Name,
                        ReceiverUserId = therapistUser.Id,
                        Title = $"Isplaćeni ste {therapistFee.CurrencyCode} {amountToAdd} za seansu klijentom {bookedSession.ClientFirstName} {bookedSession.ClientLastName}, " +
                        $"datuma {dateHelper.ConvertDateTimeFromUtcToLocalDateString(bookedSession.StartTime)}, " +
                        $"u {dateHelper.ConvertDateTimeFromUtcToLocalTimeString(bookedSession.StartTime)}h.",
                        Body = null,
                        Severity = "success",
                        Read = false,
                        SendingDateTime = DateTime.UtcNow,
                        Icon = "fal fa-money-bill-alt",
                        Important = false
                    });

                    await _mailService.SendEmailAsync(new Email
                    {
                        ToEmail = therapistUser.Email,
                        Subject = $"Sredstva u iznosu od {therapistFee.CurrencyCode} {amountToAdd} su dodata na Vaš nalog",
                        Body = $"Poštovani," +
                        $"\n" +
                        $"Isplaćeni ste {therapistFee.CurrencyCode} {amountToAdd} za seansu klijentom {bookedSession.ClientFirstName} {bookedSession.ClientLastName}, " +
                        $"datuma {dateHelper.ConvertDateTimeFromUtcToLocalDateString(bookedSession.StartTime)}, " +
                        $"u {dateHelper.ConvertDateTimeFromUtcToLocalTimeString(bookedSession.StartTime)}h."
                    });
                }
                else throw new Exception($"Therapist account or it's user account not found. TherapistId '{bookedSession.TherapistId}', UserId '{(therapistUser != null ? therapistUser.Id : "NULL")}'.");
            }
            catch (Exception e)
            {
                _systemErrors.SaveError(e, "BackgroundTasks", "TherapistSessionPaymentService", "PayTherapistAsync");

                await HandleErrorAsync(
                    hostedServiceInformationMessage: $"Hosted service failed in 'PayTherapistAsync': {e.GetRootException().Message}".TakeMax(450),
                    title: "Error: One or more therapists are left unpaid",
                    body: $"Hosted service 'TherapistSessionPaymentService' failed in 'PayTherapistAsync', at UTC {DateTime.UtcNow}, leaving one or more therapists unpaid. Investigate the table 'HostedServicesInformation' for more details."
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
            public static string Information { get { return "TherapistSessionPaymentService | Information"; } }
            public static string Error { get { return "TherapistSessionPaymentService | Error"; } }
        }
    }
}
