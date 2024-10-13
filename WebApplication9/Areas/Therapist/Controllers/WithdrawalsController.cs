using Database.Models;
using Framework.Helpers.ExtensionMethods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebApplication9.Areas.Therapist.ViewModels;
using WebApplication9.Base;
using Framework.Notifications;
using Framework.Emails;
using Framework.Emails.EmailTypes;
using WebApplication9.Interfaces;
using WebApplication9.Implementations;
using Framework.Interfaces;
using Framework.Implementations;
using Framework.Models;
using Microsoft.AspNetCore.Http;
using Framework.Helpers;
using Framework.Notifications.NotificationTypes;
using Framework.Providers;
using System.Transactions;
using Stripe;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Microsoft.Extensions.Configuration;
using WebApplication9.Models;

namespace WebApplication9.Areas.Therapist.Controllers
{
    [Area(areaName: "Therapist")]
    [Authorize(Roles = "Therapist")]
    public class WithdrawalsController : BaseController
    {
        private const int MIN_DAYS_BETWEEN_WITHDRAWALS = 7;
        private readonly StripeSettings _stripeSettings;
        private readonly IStripeFunctionsProvider _stripeFunctions;
        private readonly ITherapistFunctionsProvider _therapistFunctions;
        private readonly IWithdrawalsFunctionsProvider _withdrawalsFunctions;
        private readonly IExchangeRateFunctionsProvider _exchangeRateFunctions;

        public WithdrawalsController(IConfiguration configuration,
            IErrorLogger error,
            IMailService mailService,
            IDateTimeHelper dateHelper,
            IHttpContextAccessor contextAccessor,
            INotificationRepository notificationRepository,
            UserManager<CustomClient> userManager,
            SignInManager<CustomClient> signInManager) : base(error, mailService, dateHelper, contextAccessor, notificationRepository, userManager, signInManager)
        {
            _stripeSettings = configuration.GetSection("StripeSettings").Get<StripeSettings>();
            StripeConfiguration.ApiKey = _stripeSettings.ApiKey;
            _stripeFunctions = new StripeFunctionsProvider();
            _therapistFunctions = new TherapistFunctionsProvider();
            _withdrawalsFunctions = new WithdrawalsFunctionsProvider();
            _exchangeRateFunctions = new ExchangeRateFunctionsProvider(configuration, error);
        }

        [HttpGet("/terapeut/isplate/podešavanje-isplate")]
        public async Task<IActionResult> PaymentGatewaySetupPage()
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                {
                    var stripeAccountCompletionStatus = await _stripeFunctions.TherapistHasCompletedStripeAccountSetupAsync();
                    if (stripeAccountCompletionStatus == TherapistStripeAccountCompletionStatus.Completed)
                    {
                        ShowToastOnThisPageIfSet();
                        return View(true);
                    }
                    else if (stripeAccountCompletionStatus == TherapistStripeAccountCompletionStatus.NotSetUpYet)
                    {
                        ShowToastOnThisPageIfSet();
                        return View(false);
                    }
                    else throw new GeneralException("Unable to check if therapist has setup stripe account.");
                }
                else if (completionStatus == TherapistAccountCompletionStatus.NotSetUpYet)
                    return RedirectToAction("AccountSetup", "Account", new { Area = "Therapist" });
                else throw new GeneralException("Unable to load user.", signOutUser: true);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpPost("/terapeut/isplate/podešavanje-isplate")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> PaymentGatewaySetup()
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                {
                    var stripeAccountCompletionStatus = await _stripeFunctions.TherapistHasCompletedStripeAccountSetupAsync();
                    if (stripeAccountCompletionStatus == TherapistStripeAccountCompletionStatus.NotSetUpYet)
                    {
                        var therapistUser = await _userManager.GetUserAsync(User);
                        var therapist = _context.Therapists.GetById(therapistUser.TherapistAccountId);

                        var options = new AccountCreateOptions
                        {
                            Type = "express",
                            Country = "RS",
                            Email = therapistUser.Email,
                            BusinessType = "individual",
                            BusinessProfile = new AccountBusinessProfileOptions
                            {
                                Name = $"Psihoterapeut {therapistUser.FirstName} {therapistUser.LastName}",
                                //Url = WebUtility.UrlEncode(new Uri(appDomain, Url.PsychotherapistPublicProfileUrl(therapist.Id)).ToString()),
                                ProductDescription = $"Psihoterapeut {therapistUser.FirstName} {therapistUser.LastName}. {(therapist.About.EndsWith('.') ? therapist.About : therapist.About + ".")}",
                                SupportUrl = WebUtility.UrlEncode(new Uri(new Uri(HttpContext.RequestServices.GetRequiredService<IConfiguration>().GetSection("Application:AppDomain")?.Value), Url.Action("CustomerSupport", "Home", new { Area = "" })).ToString()),
                                SupportEmail = "podrska@counterapathy.com"
                            },
                            Individual = new AccountIndividualOptions
                            {
                                FirstName = therapistUser.FirstName,
                                LastName = therapistUser.LastName,
                                Email = therapistUser.Email,
                                Address = new AddressOptions
                                {
                                    Line1 = therapist.Street + " " + therapist.HouseNumber,
                                    City = therapist.City,
                                    PostalCode = therapist.PostalCode,
                                    Country = "RS"
                                }
                            },
                            Capabilities = new AccountCapabilitiesOptions
                            {
                                Transfers = new AccountCapabilitiesTransfersOptions
                                {
                                    Requested = true
                                }
                            },
                            TosAcceptance = new AccountTosAcceptanceOptions
                            {
                                ServiceAgreement = "recipient"
                            },
                            Metadata = new System.Collections.Generic.Dictionary<string, string>
                            {
                                { "TherapistId", therapist.Id }
                            }
                        };

                        var account = await new AccountService().CreateAsync(options);

                        _context.StripeAccounts.Insert(new StripeAccount
                        {
                            Id = account.Id,
                            TherapistId = therapist.Id,
                            CreatedDateTime = DateTime.UtcNow
                        });

                        await _context.SaveAsync();

                        return Json(new
                        {
                            success = true,
                            location = (await new AccountLinkService().CreateAsync(new AccountLinkCreateOptions
                            {
                                Account = account.Id,
                                RefreshUrl = Url.Action("PaymentGatewaySetupPage", "Withdrawals", new { Area = "Therapist" }, HttpContext.Request.Scheme),
                                ReturnUrl = Url.Action("PaymentGatewaySetupPage", "Withdrawals", new { Area = "Therapist" }, HttpContext.Request.Scheme),
                                Type = "account_onboarding"
                            })).Url
                        });
                    }
                    else if (stripeAccountCompletionStatus == TherapistStripeAccountCompletionStatus.Completed)
                    {
                        return Json(new
                        {
                            success = false,
                            title = "Bankovni račun je već povezan",
                            body = "",
                            severity = "info"
                        });
                    }
                    else throw new GeneralException("Unable to check if therapist has setup stripe account.");
                }
                else if (completionStatus == TherapistAccountCompletionStatus.NotSetUpYet)
                    return Json(new
                    {
                        success = false,
                        location = Url.Action("AccountSetup", "Account", new { Area = "Therapist" })
                    });
                else throw new GeneralException("Unable to load user.", signOutUser: true);
            }
            catch (Exception e)
            {
                return await HandleErrorJsonAsync(e);
            }
        }

        [HttpGet("/terapeut/isplate/stripe-nalog")]
        public async Task<IActionResult> StripeAccount()
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                {
                    var stripeAccountCompletionStatus = await _stripeFunctions.TherapistHasCompletedStripeAccountSetupAsync();
                    if (stripeAccountCompletionStatus == TherapistStripeAccountCompletionStatus.Completed)
                    {
                        ShowToastOnThisPageIfSet();
                        return Redirect((await new LoginLinkService().CreateAsync(_stripeFunctions.GetTherapistStripeAccountId((await _userManager.GetUserAsync(User)).TherapistAccountId))).Url);
                    }
                    else if (stripeAccountCompletionStatus == TherapistStripeAccountCompletionStatus.NotSetUpYet)
                    {
                        _session.SetToast("Morate prvo povezati Stripe nalog sa bankovnim računom", "Kliknite na dugme ispod da biste napravili Stripe nalog i povezali ga sa bankovnim računom.", "info");
                        return RedirectToAction("PaymentGatewaySetupPage", "Withdrawals", new { Area = "Therapist" });
                    }
                    else throw new GeneralException("Unable to check if therapist has setup stripe account.");
                }
                else if (completionStatus == TherapistAccountCompletionStatus.NotSetUpYet)
                    return RedirectToAction("AccountSetup", "Account", new { Area = "Therapist" });
                else throw new GeneralException("Unable to load user.", signOutUser: true);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpGet("/terapeut/isplate/{filter?}/{predicate?}")]
        public async Task<IActionResult> All(string filter = null, string predicate = null)
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                {
                    var stripeAccountCompletionStatus = await _stripeFunctions.TherapistHasCompletedStripeAccountSetupAsync();
                    if (stripeAccountCompletionStatus == TherapistStripeAccountCompletionStatus.Completed)
                    {
                        ShowToastOnThisPageIfSet();
                        return View(_withdrawalsFunctions.GetTherapistWithdrawals((await _userManager.GetUserAsync(User)).TherapistAccountId, filter, predicate));
                    }
                    else if (stripeAccountCompletionStatus == TherapistStripeAccountCompletionStatus.NotSetUpYet)
                    {
                        _session.SetToast("Morate prvo povezati Stripe nalog sa bankovnim računom", "Kliknite na dugme ispod da biste napravili Stripe nalog i povezali ga sa bankovnim računom.", "info");
                        return RedirectToAction("PaymentGatewaySetupPage", "Withdrawals", new { Area = "Therapist" });
                    }
                    else throw new GeneralException("Unable to check if therapist has setup stripe account.");
                }
                else if (completionStatus == TherapistAccountCompletionStatus.NotSetUpYet)
                    return RedirectToAction("AccountSetup", "Account", new { Area = "Therapist" });
                else throw new GeneralException("Unable to load user", signOutUser: true);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpGet("/terapeut/isplate/isplata-sredstava")]
        public async Task<IActionResult> Withdraw()
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                {
                    ShowToastOnThisPageIfSet();

                    var user = await _userManager.GetUserAsync(User);
                    var therapist = _context.Therapists.GetById(user.TherapistAccountId);

                    if (therapist.Earnings < 3500)
                    {
                        _session.SetToast(
                            "Nemate dovoljno sredstava za isplatu",
                            "Minimalna suma za isplatu je 3500 RSD i isplata se može zahtevati na svakih nedelju dana.",
                            "info");
                        return RedirectToAction("Profile", "Account", new { Area = "Therapist" });
                    }

                    var availabilityMessage = _withdrawalsFunctions.RequiredDaysPassedBetweenWithdrawals(therapist.Id, MIN_DAYS_BETWEEN_WITHDRAWALS);

                    if (!string.IsNullOrWhiteSpace(availabilityMessage))
                    {
                        _session.SetToast("Isplata sredstava se može zahtevati na svakih nedelju dana", availabilityMessage, "info");
                        return RedirectToAction("Profile", "Account", new { Area = "Therapist" });
                    }

                    var totalPercentage = 1 + _stripeSettings.StripeConnectTransferFeePercentage / 100 + _stripeSettings.StripeConnectCurrencyConversionFeePercentage / 100;
                    var totalRsd = (decimal)(therapist.Earnings * 100) * totalPercentage;

                    var options = new TransferCreateOptions
                    {
                        Amount = Convert.ToInt64(await _exchangeRateFunctions.ConvertRsdToUsdAsync(totalRsd) + _stripeSettings.StripeConnectFixedFeeAmountInCents),
                        Currency = "usd",
                        Destination = _stripeFunctions.GetTherapistStripeAccountId(therapist.Id)
                    };

                    var withdrawVm = new WithdrawViewModel
                    {
                        HasSetupStripeAccount = (await _stripeFunctions.TherapistHasCompletedStripeAccountSetupAsync(therapist)) == TherapistStripeAccountCompletionStatus.Completed,
                        SubtotalAmount = therapist.Earnings.ToString(),
                        StripeConnectTransferFeePercentage = _stripeSettings.StripeConnectTransferFeePercentage.ToString(),
                        StripeConnectTransferFeePercentageAmount = Math.Round((decimal)therapist.Earnings * (1 + _stripeSettings.StripeConnectTransferFeePercentage / 100) - (decimal)therapist.Earnings, 2).ToString("#.##"),
                        TotalAmount = Math.Round((decimal)therapist.Earnings * (1 + _stripeSettings.StripeConnectTransferFeePercentage / 100), 2).ToString("#.##")
                    };

                    return View(withdrawVm);
                }
                else if (completionStatus == TherapistAccountCompletionStatus.NotSetUpYet)
                    return RedirectToAction("AccountSetup", "Account", new { Area = "Therapist" });
                else throw new GeneralException("Unable to load user.", signOutUser: true);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpPost("/terapeut/isplate/isplata-sredstava")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WithdrawEarnings()
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                {
                    var stripeAccountCompletionStatus = await _stripeFunctions.TherapistHasCompletedStripeAccountSetupAsync();
                    if (stripeAccountCompletionStatus == TherapistStripeAccountCompletionStatus.Completed)
                    {
                        var user = await _userManager.GetUserAsync(User);
                        var therapist = _context.Therapists.GetById(user.TherapistAccountId);

                        if (therapist.Earnings < 3500)
                        {
                            _session.SetToast(
                            "Nemate dovoljno sredstava za isplatu",
                            "Minimalna suma za isplatu je 3500 RSD i isplata se može zahtevati na svakih nedelju dana.",
                            "info");
                            return Json(new
                            {
                                success = false,
                                redirectUrl = Url.Action("Profile", "Account", new { Area = "Therapist" })
                            });
                        }

                        var availabilityMessage = _withdrawalsFunctions.RequiredDaysPassedBetweenWithdrawals(therapist.Id, MIN_DAYS_BETWEEN_WITHDRAWALS);
                        if (!string.IsNullOrWhiteSpace(availabilityMessage))
                        {
                            _session.SetToast("Isplata se može zahtevati na svakih nedelju dana", availabilityMessage, "info");
                            return Json(new
                            {
                                success = false,
                                redirectUrl = Url.Action("Profile", "Account", new { Area = "Therapist" })
                            });
                        }

                        Withdrawals withdraw = null;
                        decimal totalRsd = 0;

                        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            try
                            {
                                var earningsLogs = _context.TherapistEarningsLogs.ReadOnlyFind(log => log.TherapistId == therapist.Id);
                                var totalSumOfAllTherapistEarningsLogs = earningsLogs.Sum(log => log.Amount);
                                var currentEarningsLog = earningsLogs.Aggregate((log1, log2) => log1.EarningsDateTime > log2.EarningsDateTime ? log1 : log2);

                                if (therapist.Earnings != totalSumOfAllTherapistEarningsLogs ||
                                    totalSumOfAllTherapistEarningsLogs != currentEarningsLog.CurrentAmount ||
                                    therapist.Earnings != currentEarningsLog.CurrentAmount)
                                {
                                    await HandleErrorJsonAsync($"These 4 amounts are not all equal: Therapist.Earnings: {therapist.Earnings} -- " +
                                        $"TherapistEarningsLog.CurrentAmount: {currentEarningsLog.CurrentAmount} --  TotalSumOfAllTherapistEarningsLogs: {totalSumOfAllTherapistEarningsLogs} -- " +
                                        $"TherapistId: {therapist.Id} -- TherapistEarningsLogId: {currentEarningsLog.Id}.");
                                    return Json(new
                                    {
                                        success = false,
                                        title = $"Greška prilikom slanja sredstava na Stripe nalog",
                                        body = "Molimo kontaktirajte podršku za terapeute.",
                                        severity = "error"
                                    });
                                }

                                var totalPercentageWithoutCurrencyConversionFeePercentage = 1 + _stripeSettings.StripeConnectTransferFeePercentage / 100;
                                var totalPercentage = totalPercentageWithoutCurrencyConversionFeePercentage + _stripeSettings.StripeConnectCurrencyConversionFeePercentage / 100;
                                var stripeCentsFixedFee = await _exchangeRateFunctions.ConvertUsdToRsdAsync(_stripeSettings.StripeConnectFixedFeeAmountInCents / 100);

                                totalRsd = (decimal)(therapist.Earnings * 100) * totalPercentage + stripeCentsFixedFee;

                                var options = new TransferCreateOptions
                                {
                                    Amount = Convert.ToInt64(await _exchangeRateFunctions.ConvertRsdToUsdAsync(Math.Round(totalRsd, 2))),
                                    Currency = "usd",
                                    Destination = _stripeFunctions.GetTherapistStripeAccountId(therapist.Id)
                                };

                                totalRsd = Math.Round((decimal)therapist.Earnings * totalPercentageWithoutCurrencyConversionFeePercentage, 2);

                                var transactionId = (await new TransferService().CreateAsync(options)).Id;

                                _context.Transactions.Insert(new Transactions
                                {
                                    Id = transactionId,
                                    SenderId = user.Id,
                                    ReceiverId = PaymentTypesProvider.BankTransfer.Id,
                                    DateTime = DateTime.UtcNow,
                                    Amount = therapist.Earnings,
                                    CurrencyCode = "RSD"
                                });

                                withdraw = new Withdrawals
                                {
                                    Id = Helper.GenerateNumbersId(),
                                    TherapistId = therapist.Id,
                                    Email = user.Email,
                                    PhoneNumber = user.PhoneNumber,
                                    FirstName = user.FirstName,
                                    LastName = user.LastName,
                                    Street = therapist.Street,
                                    HouseNumber = therapist.HouseNumber,
                                    City = therapist.City,
                                    PostalCode = therapist.PostalCode,
                                    Country = therapist.Country,
                                    Status = 1,
                                    Amount = therapist.Earnings,
                                    RequestDateTime = DateTime.UtcNow,
                                    AcceptDateTime = DateTime.UtcNow,
                                    BankAccountNumber = "RSxxxxxxxxxxxxxxxxxxxx",
                                    PaymentTypeId = PaymentTypesProvider.BankTransfer.Id,
                                    PaymentTypeName = PaymentTypesProvider.BankTransfer.Name,
                                    TransactionId = transactionId
                                };

                                _context.Withdrawals.Insert(withdraw);

                                therapist.Earnings = 0;

                                _context.TherapistEarningsLogs.Insert(new TherapistEarningsLogs
                                {
                                    Id = Helper.GenerateNumbersId(),
                                    TherapistId = therapist.Id,
                                    Amount = withdraw.Amount * -1,
                                    CurrentAmount = therapist.Earnings,
                                    EarningsDateTime = DateTime.UtcNow,
                                    PaymentTypeId = withdraw.PaymentTypeId,
                                    PaymentTypeName = withdraw.PaymentTypeName,
                                    TransactionId = transactionId
                                });

                                await _context.SaveAsync();

                                scope.Complete();
                            }
                            catch (Exception)
                            {
                                scope.Dispose();
                                throw;
                            }
                        }

                        //sends notification to therapist
                        await _notificationRepository.SendAsync(new Notifications
                        {
                            Id = Helper.GenerateNumbersId(),
                            SenderUserId = "System",
                            ReceiverUserId = user.Id,
                            Title = $"Sredstva u iznosu od RSD {totalRsd} su uspešno poslata na Vaš Stripe nalog, " +
                            $"odakle se automatski prosleđuju na Vaš devizni bankovni račun i ležu u roku od 1-14 radnih dana.",
                            Body = null,
                            Severity = "secondary",
                            Read = false,
                            SendingDateTime = DateTime.UtcNow,
                            Icon = "fal fa-university",
                            Important = true
                        });

                        //sends notification to all admins 
                        await _notificationRepository.SendAsync(new NotificationForRole
                        {
                            SenderUserId = "System",
                            Title = $"Therapist {withdraw.FirstName} {withdraw.LastName} withdrew RSD {totalRsd} to stripe account. " +
                            $"WithdrawalId: '{withdraw.Id}'.",
                            Body = null,
                            Severity = "secondary",
                            SendingDateTime = DateTime.UtcNow,
                            Icon = "fal fa-university",
                            Important = true
                        }, UserRoles.Admin);

                        await _mailService.SendWithdrawalRequestAcceptedEmailAsync(new WithdrawalRequestAcceptedEmail
                        {
                            ToEmail = user.Email,
                            FirstName = withdraw.FirstName,
                            Amount = totalRsd.ToString()
                        }, includeTemplateIfExists: true);

                        return Json(new
                        {
                            success = true,
                            title = $"Sredstva uspešno isplaćena",
                            body = $"Sredstva u iznosu od RSD {totalRsd} su uspešno poslata na Vaš Stripe nalog, " +
                            $"odakle se automatski prosleđuju na Vaš devizni bankovni račun i ležu u roku od 1-14 radnih dana.",
                            severity = "success",
                            redirectUrl = Url.Action("All", "Withdrawals", new { Area = "Therapist" })
                        });
                    }
                    else if (stripeAccountCompletionStatus == TherapistStripeAccountCompletionStatus.NotSetUpYet)
                    {
                        _session.SetToast("Morate prvo povezati Stripe nalog sa bankovnim računom", "Kliknite na dugme ispod da biste napravili Stripe nalog i povezali ga sa bankovnim računom.", "info");
                        return Json(new
                        {
                            success = false,
                            redirectUrl = Url.Action("PaymentGatewaySetupPage", "Withdrawals", new { Area = "Therapist" })
                        });
                    }
                    else throw new GeneralException("Unable to check if therapist has setup stripe account.");
                }
                else if (completionStatus == TherapistAccountCompletionStatus.NotSetUpYet)
                    return Json(new
                    {
                        success = false,
                        redirectUrl = Url.Action("AccountSetup", "Account", new { Area = "Therapist" })
                    });
                else throw new GeneralException("Unable to load user.", signOutUser: true);
            }
            catch (Exception e)
            {
                return await HandleErrorJsonAsync(e);
            }
        }

        //[HttpGet("/aleksa")]
        //public async Task<IActionResult> aleksa()
        //{
        //    //var options = new ChargeCreateOptions
        //    //{
        //    //    Amount = 10000000,
        //    //    Currency = "rsd",
        //    //    Source = "tok_bypassPending",
        //    //    Description = "My First Test Charge (created for API docs at https://www.stripe.com/docs/api)",
        //    //};
        //    //var service = new ChargeService();
        //    //service.Create(options);

        //    //var options = new TopupCreateOptions
        //    //{
        //    //    Amount = 1000000,
        //    //    Currency = "usd",
        //    //    Description = "Top-up for week of May 31",
        //    //    StatementDescriptor = "Weekly top-up",
        //    //};
        //    //var service = new TopupService();
        //    //service.Create(options);

        //    //var options = new PaymentIntentCreateOptions
        //    //{
        //    //    Amount = 2000,
        //    //    Currency = "usd",
        //    //    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
        //    //    {
        //    //        Enabled = true,
        //    //    },
        //    //};
        //    //var service = new PaymentIntentService();
        //    //service.Create(options);
        //    var totalSumOfAllTherapistEarningsLogs = _context.TherapistEarningsLogs
        //                                                                         .ReadOnlyFind(log => log.TherapistId == "760546892339")
        //                                                                         .Select(log => log.Amount).Sum();
        //    return Content($"sum: {totalSumOfAllTherapistEarningsLogs}");


        //}
    }
}