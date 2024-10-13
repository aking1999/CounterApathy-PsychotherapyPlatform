using Database.Models;
using Database.RepositoryImplementations;
using Framework.Helpers;
using Framework.Helpers.ExtensionMethods;
using Framework.Implementations;
using Framework.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NCrontab;
using Stripe;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundTasks
{
    public class UnbookUnpaidSessionService : IHostedService
    {
        private UnitOfWork _context;
        private readonly ISystemErrorLogger _systemErrors;
        private readonly CrontabSchedule _crontabSchedule;
        private DateTime _nextRun;
        private const string SCHEDULE = "* */10 * * * *"; //run every 10mins
        private const int DELAY_IN_MILLISECONDS_BETWEEN_STRIPE_API_CALLS = 10;

        public UnbookUnpaidSessionService(IServiceScopeFactory serviceScopeFactory)
        {
            _systemErrors = new SystemErrorLogger();
            _context = new UnitOfWork(new LajsnaProbaContext());

            try
            {
                var scope = serviceScopeFactory.CreateScope();
                StripeConfiguration.ApiKey = scope.ServiceProvider.GetRequiredService<IConfiguration>().GetSection("StripeSettings:ApiKey")?.Value;

                _crontabSchedule = CrontabSchedule.Parse(SCHEDULE, new CrontabSchedule.ParseOptions { IncludingSeconds = true });
                _nextRun = _crontabSchedule.GetNextOccurrence(DateTime.UtcNow);
            }
            catch (Exception e)
            {
                _systemErrors.SaveError(e, "BackgroundTasks", "UnbookUnpaidSessionService", "Constructor");

                HandleError($"Hosted service failed in 'Constructor': {e.GetRootException().Message}".TakeMax(450));

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

                        await UnbookUnpaidExpiredSessionsAsync();

                        _nextRun = _crontabSchedule.GetNextOccurrence(DateTime.UtcNow);
                    }
                }, cancellationToken);
            }
            catch (Exception e)
            {
                _systemErrors.SaveError(e, "BackgroundTasks", "UnbookUnpaidSessionService", "StartAsync");

                HandleError($"Hosted service failed in " + $"'StartAsync': {e.GetRootException().Message}".TakeMax(450));
            }

            return Task.CompletedTask;
        }

        private async Task UnbookUnpaidExpiredSessionsAsync()
        {
            try
            {
                var utcNow = DateTime.UtcNow;
                var paymentIntentType = Framework.Providers.StripePaymentIntentSystemEventTypeNameProvider.SessionInstantPayment;
                var expiredPaymentIntents = _context.StripePaymentIntents.Find(intent => intent.Status == 0 && intent.MustSucceedUntil < utcNow && intent.SystemEventTypeName == paymentIntentType).ToList();

                if (expiredPaymentIntents.Any())
                {
                    foreach (var expiredPaymentIntent in expiredPaymentIntents)
                    {
                        try
                        {
                            var service = new PaymentIntentService();
                            var paymentIntentInStripe = await service.GetAsync(expiredPaymentIntent.Id);

                            if (paymentIntentInStripe.Status == "succeeded")
                            {
                                expiredPaymentIntent.StripeEventTypeName = Events.PaymentIntentSucceeded;
                                expiredPaymentIntent.Status = 1;
                                _context.StripePaymentIntents.Update(expiredPaymentIntent);

                                // Maybe it is a good idea here to add a find method that
                                // looks for _context.BookedSession for the paymentIntent, and if it does not exist,
                                // it means the user paid for the sessions because the paymentIntent succeedded but the webhook did not
                                // insert a new BookedSession in the database. See later if it becomes a problem, add a mail sender here to
                                // send email to let the admins know that user did not get his booked session.
                            }
                            else
                            {
                                await Task.Delay(DELAY_IN_MILLISECONDS_BETWEEN_STRIPE_API_CALLS);

                                string sessionId = string.Empty;

                                if (paymentIntentInStripe.Status != "canceled")
                                {
                                    await service.CancelAsync(expiredPaymentIntent.Id);

                                    _context.HostedServicesInformation.Insert(new HostedServicesInformation
                                    {
                                        Id = Helper.GenerateNumbersId(),
                                        Information = $"-> PaymentIntent is Stripe '{paymentIntentInStripe.Id}' is cancelled.".TakeMax(450),
                                        InformationType = HostedServicesInformationType.Information.TakeMax(128),
                                        ExecutionDateTime = DateTime.UtcNow
                                    });
                                }
                                else
                                {
                                    await _systemErrors.SaveErrorAsync("PaymentIntent.Status in Stripe is already cancelled.", "BackgroundTasks", "UnbookUnpaidSessionService", "UnbookUnpaidExpiredSessionsAsync");
                                }

                                if (paymentIntentInStripe.Metadata.TryGetValue("SessionId", out sessionId))
                                {
                                    var sessionToUnbook = _context.Sessions.GetById(sessionId);
                                    if (sessionToUnbook == null)
                                    {
                                        await _systemErrors.SaveErrorAsync("Session not found in database even though there is a session to unbook in database.", "BackgroundTasks", "UnbookUnpaidSessionService", "UnbookUnpaidExpiredSessionsAsync");
                                    }
                                    else
                                    {
                                        sessionToUnbook.Booked = 0;
                                        _context.Sessions.Update(sessionToUnbook);
                                        _context.HostedServicesInformation.Insert(new HostedServicesInformation
                                        {
                                            Id = Helper.GenerateNumbersId(),
                                            Information = $"-> Session '{sessionToUnbook.Id}' is unbooked.".TakeMax(450),
                                            InformationType = HostedServicesInformationType.Information.TakeMax(128),
                                            ExecutionDateTime = DateTime.UtcNow
                                        });
                                    }
                                }
                                else
                                {
                                    await _systemErrors.SaveErrorAsync($"PaymentIntent in Stripe '{paymentIntentInStripe.Id}' does not have 'SessionId' key in it's Metadata property, even though there is a session to unbook in database.", "BackgroundTasks", "UnbookUnpaidSessionService", "UnbookUnpaidExpiredSessionsAsync");
                                }

                                _context.HostedServicesInformation.Insert(new HostedServicesInformation
                                {
                                    Id = Helper.GenerateNumbersId(),
                                    Information = $"-> PaymentIntent '{expiredPaymentIntent.Id}' is deleted from database.".TakeMax(450),
                                    InformationType = HostedServicesInformationType.Information.TakeMax(128),
                                    ExecutionDateTime = DateTime.UtcNow
                                });

                                _context.StripePaymentIntents.Delete(expiredPaymentIntent);

                                sessionId = string.Empty;
                            }

                            await Task.Delay(DELAY_IN_MILLISECONDS_BETWEEN_STRIPE_API_CALLS);
                        }
                        catch(Exception innerException)
                        {
                            await _systemErrors.SaveErrorAsync(innerException,
                                "BackgroundTasks", "UnbookUnpaidSessionService", "UnbookUnpaidExpiredSessionsAsync");
                        }
                    }

                    await _context.SaveAsync();
                }
            }
            catch (Exception e)
            {
                await _systemErrors.SaveErrorAsync(e,
                    "BackgroundTasks", "UnbookUnpaidSessionService", "UnbookUnpaidExpiredSessionsAsync");

                HandleError($"Hosted service failed in 'UnbookUnpaidExpiredSessionsAsync': {e.GetRootException().Message}".TakeMax(450));
            }

            RefreshContext();
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public int UntilNextExecution() => Math.Max(0, (int)_nextRun.Subtract(DateTime.UtcNow).TotalMilliseconds);

        public DateTime GetNextExecution() => _crontabSchedule.GetNextOccurrence(DateTime.UtcNow);

        public DateTime GetNextExecution(DateTime afterDate) => _crontabSchedule.GetNextOccurrence(afterDate);

        private void RefreshContext()
        {
            _context = new UnitOfWork(new LajsnaProbaContext());
        }

        private void HandleError(string hostedServiceInformationMessage)
        {
            _context.HostedServicesInformation.Insert(new HostedServicesInformation
            {
                Id = Helper.GenerateNumbersId(),
                Information = hostedServiceInformationMessage,
                InformationType = HostedServicesInformationType.Error.TakeMax(128),
                ExecutionDateTime = DateTime.UtcNow
            });

            _context.Save();
        }

        private class HostedServicesInformationType
        {
            public static string Information { get { return "UnbookUnpaidSessionService | Information"; } }
            public static string Error { get { return "UnbookUnpaidSessionService | Error"; } }
        }
    }
}
