using Database.Models;
using DataTransferObjects.ViewModels.Client;
using Framework.Emails;
using Framework.Implementations;
using Framework.Interfaces;
using Framework.Models;
using Framework.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OtpNet;
using Stripe;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using WebApplication9.Base;
using WebApplication9.Implementations;
using WebApplication9.Interfaces;
using WebApplication9.Models;
using WebApplication9.ViewModels;

namespace WebApplication9.Controllers
{
    public class TransactionsController : BaseController
    {
        private readonly StripeSettings _stripeSettings;
        private readonly IConfiguration _configuration;
        private readonly IStripeFunctionsProvider _stripeFunctions;
        private readonly ICustomClientFunctionsProvider _clientFunctions;

        public TransactionsController(IConfiguration configuration,
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
            _configuration = configuration;
            _stripeFunctions = new StripeFunctionsProvider();
            _clientFunctions = new CustomClientFunctionsProvider(contextAccessor);
        }

        [Authorize(Roles = "Client")]
        [HttpPost("/transakcije/uplati")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Pay([FromForm] CardsAddWebCreditViewModel cardVm)
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
                {
                    if (!ModelState.IsValid)
                        return Json(new
                        {
                            success = false,
                            title = "Popunite sva obavezna polja",
                            body = "",
                            severity = "info"
                        });

                    var user = await _userManager.GetUserAsync(User);
                    PaymentIntent paymentIntent;
                    var paymentIntentClientSecret = string.Empty;

                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        try
                        {
                            var paymentIntentOptions = new PaymentIntentCreateOptions
                            {
                                Amount = Convert.ToInt64(cardVm.Amount * 100),
                                Currency = "rsd",
                                SetupFutureUsage = "off_session",
                                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                                {
                                    Enabled = true
                                },
                                ReceiptEmail = user.Email,
                                Metadata = new System.Collections.Generic.Dictionary<string, string>
                                {
                                    { "UserId", user.Id }
                                },
                                Description = "authorized",
                                StatementDescriptor = _configuration.GetSection("Application:AppName")?.Value
                            };

                            var stripeCustomerId = _stripeFunctions.GetStripeCustomerId(user.Id);

                            if (!string.IsNullOrWhiteSpace(stripeCustomerId))
                            {
                                paymentIntentOptions.Customer = stripeCustomerId;

                                var customerInStripe = await new CustomerService().UpdateAsync(stripeCustomerId, new CustomerUpdateOptions
                                {
                                    Name = $"{user.FirstName} {user.LastName} - {user.Id}",
                                    Phone = user.PhoneNumber,
                                    Metadata = new System.Collections.Generic.Dictionary<string, string>
                                    {
                                        { "UserId", user.Id }
                                    }
                                });

                                _context.StripeCustomers.GetById(stripeCustomerId).PhoneNumber = user.PhoneNumber;
                            }
                            else
                            {
                                var customer = await new CustomerService().CreateAsync(new CustomerCreateOptions
                                {
                                    Name = $"{user.FirstName} {user.LastName} - {user.Id}",
                                    Email = user.Email,
                                    Phone = user.PhoneNumber,
                                    Description = "Generated at Transactions/Pay",
                                    Metadata = new System.Collections.Generic.Dictionary<string, string>
                                    {
                                        { "UserId", user.Id }
                                    }
                                });

                                _context.StripeCustomers.Insert(new StripeCustomers
                                {
                                    Id = customer.Id,
                                    UserIdOrAnonymous = user.Id,
                                    Email = user.Email,
                                    CreatedDateTime = DateTime.UtcNow,
                                    PhoneNumber = user.PhoneNumber
                                });

                                paymentIntentOptions.Customer = customer.Id;
                            }

                            var paymentIntentInDatabase = _stripeFunctions.GetLatestPaymentIntentByUserId(user.Id);
                            var newPaymentIntentInserted = false;

                            if (paymentIntentInDatabase != null &&
                                !string.IsNullOrWhiteSpace(paymentIntentInDatabase.CustomerId) &&
                                DateTime.UtcNow < paymentIntentInDatabase.MustSucceedUntil &&
                                paymentIntentInDatabase.Status == 0)
                            {
                                var paymentIntentInStripe = await new PaymentIntentService().GetAsync(paymentIntentInDatabase.Id);

                                if (paymentIntentInStripe != null &&
                                    paymentIntentInStripe.Status != "succeeded")
                                {
                                    paymentIntent = await new PaymentIntentService().UpdateAsync(paymentIntentInDatabase.Id, new PaymentIntentUpdateOptions
                                    {
                                        Amount = Convert.ToInt64(cardVm.Amount * 100),
                                        Currency = "rsd",
                                        SetupFutureUsage = "off_session",
                                        ReceiptEmail = user.Email,
                                        Metadata = new System.Collections.Generic.Dictionary<string, string>
                                        {
                                            { "UserId", user.Id }
                                        },
                                        StatementDescriptor = _configuration.GetSection("Application:AppName")?.Value
                                    });

                                    paymentIntentInDatabase.PhoneNumber = user.PhoneNumber;
                                    paymentIntentInDatabase.SystemEventTypeName = Framework.Providers.StripePaymentIntentSystemEventTypeNameProvider.WebCreditTopUp;
                                    paymentIntentInDatabase.MustSucceedUntil = DateTime.UtcNow.AddDays(1);
                                }
                                else
                                {
                                    paymentIntent = await new PaymentIntentService().CreateAsync(paymentIntentOptions);

                                    paymentIntentInDatabase = new StripePaymentIntents
                                    {
                                        Id = paymentIntent.Id,
                                        UserIdOrAnonymous = user.Id,
                                        Email = user.Email,
                                        PhoneNumber = user.PhoneNumber,
                                        CustomerId = paymentIntent.CustomerId,
                                        Status = 0,
                                        SystemEventTypeName = Framework.Providers.StripePaymentIntentSystemEventTypeNameProvider.WebCreditTopUp,
                                        MustSucceedUntil = DateTime.UtcNow.AddDays(1),
                                        CreatedDateTime = DateTime.UtcNow
                                    };

                                    _context.StripePaymentIntents.Insert(paymentIntentInDatabase);
                                    newPaymentIntentInserted = true;
                                }
                            }
                            else
                            {
                                paymentIntent = await new PaymentIntentService().CreateAsync(paymentIntentOptions);

                                paymentIntentInDatabase = new StripePaymentIntents
                                {
                                    Id = paymentIntent.Id,
                                    UserIdOrAnonymous = user.Id,
                                    Email = user.Email,
                                    PhoneNumber = user.PhoneNumber,
                                    CustomerId = paymentIntent.CustomerId,
                                    Status = 0,
                                    SystemEventTypeName = Framework.Providers.StripePaymentIntentSystemEventTypeNameProvider.WebCreditTopUp,
                                    MustSucceedUntil = DateTime.UtcNow.AddDays(1),
                                    CreatedDateTime = DateTime.UtcNow
                                };

                                _context.StripePaymentIntents.Insert(paymentIntentInDatabase);
                                newPaymentIntentInserted = true;
                            }

                            if (!newPaymentIntentInserted)
                                _context.StripePaymentIntents.Update(paymentIntentInDatabase);

                            paymentIntentClientSecret = paymentIntent.ClientSecret;

                            await _context.SaveAsync();
                            scope.Complete();
                        }
                        catch (Exception)
                        {
                            scope.Dispose();
                            throw;
                        }
                    }

                    return Json(new
                    {
                        success = true,
                        publicKey = _stripeSettings.PublishableKey,
                        clientSecret = paymentIntentClientSecret,
                        amount = paymentIntent.Amount / 100,
                        currency = paymentIntent.Currency,
                        paymentSuccessfulReturnUrl = Url.Action("PaymentStatus", "Transactions", HttpContext.Request.Scheme),
                        title = "Čestitamo!",
                        body = $"Uspešno ste dodatli RSD {paymentIntent.Amount / 100} Veb kredita na svoj nalog.",
                        severity = "success"
                    });
                }
                else if (emailStatus == EmailConfirmationStatus.NotConfirmed)
                    return Json(new
                    {
                        success = false,
                        redirectUrl = Url.Action("ConfirmEmail", "Authorization", new { Area = "" })
                    });
                else throw new GeneralException("Unable to load user.", signOutUser: true);
            }
            catch (Exception e)
            {
                return await HandleErrorJsonAsync(e);
            }
        }

        [Authorize(Roles = "Client")]
        [HttpGet("/transakcije/status-uplate")]
        public async Task<IActionResult> PaymentStatus()
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
                {
                    var user = await _userManager.GetUserAsync(User);
                    var paymentIntentInDatabase = _stripeFunctions.GetLatestReadOnlyPaymentIntentByUserId(user.Id);
                    var paymentIntent = await new PaymentIntentService().GetAsync(paymentIntentInDatabase.Id);
                    PaymentTypes paymentTypeInDatabase = default;

                    if (paymentIntentInDatabase.Status != 0 && !string.IsNullOrWhiteSpace(paymentIntentInDatabase.WebCreditLogId))
                        paymentTypeInDatabase = (from log in _context.WebCreditLogs.ReadOnlyFind(item => item.Id == paymentIntentInDatabase.WebCreditLogId)
                                                 join paymentType in _context.PaymentTypes.ReadOnlyGetAll()
                                                 on log.PaymentTypeId equals paymentType.Id
                                                 select paymentType).ElementAtOrDefault(0);

                    if (paymentTypeInDatabase == default)
                        paymentTypeInDatabase = new PaymentTypes();

                    return View(new StripePaymentIntentViewModel
                    {
                        PaymentIntentId = paymentIntentInDatabase.Id,
                        Amount = paymentIntent.Amount / 100,
                        Currency = paymentIntent.Currency,
                        Created = _dateHelper.ConvertDateTimeFromUtcToLocalString(paymentIntent.Created),
                        Status = paymentIntentInDatabase.Status
                    });
                }
                else if (emailStatus == EmailConfirmationStatus.NotConfirmed)
                    return RedirectToAction("ConfirmEmail", "Authorization", new { Area = "" });
                else throw new GeneralException("Unable to load user.", signOutUser: true);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }
    }
}
