using Database.Models;
using DataTransferObjects.Models.Client;
//using PayPalHttp;
using DataTransferObjects.ViewModels.Client;
using Framework.Emails;
using Framework.Emails.EmailTypes;
using Framework.Helpers;
using Framework.Helpers.ExtensionMethods;
using Framework.Interfaces;
using Framework.Models;
using Framework.Notifications;
using Framework.Notifications.NotificationTypes;
using Framework.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Transactions;
using WebApplication9.Base;
using WebApplication9.Helpers;

namespace WebApplication9.Controllers
{
    [Authorize(Roles = "Client")]
    public class PayPalController : BaseController
    {
        private static PayPalSettings _payPalSettings;
        private const string EXCHANGE_RATE_API_URI = @"https://kurs.resenje.org/api/v1/currencies/usd/rates/today";

        public PayPalController(IConfiguration configuration,
            IErrorLogger errors,
            IMailService mailService,
            IDateTimeHelper dateHelper,
            IHttpContextAccessor contextAccessor,
            INotificationRepository notificationRepository,
            UserManager<CustomClient> userManager) : base(errors, mailService, dateHelper, contextAccessor, notificationRepository, userManager)
        {
            _payPalSettings = configuration.GetSection("PayPalSettings").Get<PayPalSettings>();
        }

        [HttpPost("/paypal/izvršavanje-paypal-transakcije")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ExecuteOrder([FromForm] PayPalAddWebCreditViewModel webCreditVm)
        {
            var paypalRequestId = Helper.GenerateNumbersId();
            var paypalRequest = new PayPalPaymentRequests
            {
                Id = paypalRequestId,
                WebCreditLogId = null,
                PayPalPayerId = null,
                PayPalPaymentId = null,
                PayPalTransactionId = null,
                PrimaryCurrencyCode = "USD",
                SecondaryCurrencyCode = "RSD",
                SecondaryCurrencyAmount = webCreditVm.Amount,
                ExchangeRate = await ExchangeRate.GetUsdToRsdRateAsync(),
                FeePercentage = _payPalSettings.FeePercentage,
                SecondaryCurrencyFeeAmount = (webCreditVm.Amount * (1 + (_payPalSettings.FeePercentage / 100))) - webCreditVm.Amount,
                RequestDateTime = DateTime.UtcNow,
                Status = 0,
                PaymentCompleteDateTime = null
            };

            try
            {
                var user = await _userManager.GetUserAsync(User) ?? throw new GeneralException("Unable to load user.", signOutUser: true);

                paypalRequest.UserId = user.Id;
                paypalRequest.PrimaryCurrencyAmount = paypalRequest.SecondaryCurrencyAmount / paypalRequest.ExchangeRate;
                paypalRequest.PrimaryCurrencyFeeAmount = (paypalRequest.PrimaryCurrencyAmount * (1 + (_payPalSettings.FeePercentage / 100))) - paypalRequest.PrimaryCurrencyAmount;

                if (!ModelState.IsValid)
                    return Json(new
                    {
                        success = false,
                        title = "Popunite sva obavezna polja",
                        body = "",
                        severity = "info"
                    });

                var setup = new PayPalPaymentHelper.PayPalSetup
                {
                    PayerApprovedOrderId = Request?.Query["token"]
                };

                if (string.IsNullOrWhiteSpace(Request?.Query["PayerID"]))
                {
                    var userPayPalRequests = _context.PayPalPaymentRequests.Find(req => req.UserId == user.Id).OrderByDescending(req => req.RequestDateTime).ToList();
                    if (userPayPalRequests.Any() && userPayPalRequests[0].Status == 0)
                    {
                        userPayPalRequests[0].Status = -1;
                        await _context.SaveAsync();
                    }

                    //paypalRequest = new PayPalPaymentRequests
                    //{
                    //    SecondaryCurrencyFeeAmount = (webCreditVm.PayPal.Amount * (1 + (_payPalSettings.FeePercentage / 100))) - webCreditVm.PayPal.Amount,
                    //    RequestDateTime = DateTime.UtcNow,
                    //    Status = 0,
                    //    PaymentCompleteDateTime = null
                    //};

                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        try
                        {
                            //setup.RedirectUrl = $"{Request.Scheme}://{Request.Host}//ExecutePayPalReturnOrder";
                            setup.RedirectUrl = Url.Action("ExecutePayPalReturnOrder", "PayPal", null, Url.ActionContext.HttpContext.Request.Scheme);
                            var response = (await PayPalPaymentHelper.CreateOrderAsync(paypalRequest.PrimaryCurrencyAmount + paypalRequest.PrimaryCurrencyFeeAmount, setup));

                            setup.ApproveUrl = response.Result<Order>().Links.Find(link => link.Rel.Trim().ToLower() == "approve").Href;

                            if (!string.IsNullOrWhiteSpace(setup.ApproveUrl)) // && response.StatusCode == HttpStatusCode.Ok
                            {
                                _context.PayPalPaymentRequests.Insert(paypalRequest);

                                await _context.SaveAsync();

                                scope.Complete();

                                return Json(new
                                {
                                    success = true,
                                    approveUrl = setup.ApproveUrl
                                });
                            }
                        }
                        catch (Exception)
                        {
                            scope.Dispose();
                            throw;
                        }
                    }
                }

                throw new Exception("Error while proccessing PayPal payment. Request.Query['PayerID'] is not null or empty.");
            }
            catch (GeneralException ge)
            {
                await _notificationRepository.SendAsync(new NotificationForRole
                {
                    SenderUserId = "System",
                    Title = "Error during PayPal payment: " + ge.GetRootException().Message,
                    Body = null,
                    Severity = "danger",
                    SendingDateTime = DateTime.UtcNow,
                    Icon = "far fa-exclamation-circle",
                    Important = true
                }, UserRoles.Admin);

                await _mailService.SendEmailAsync(new EmailForRole
                {
                    Subject = "Error during PayPal payment",
                    Body = ge.GetRootException().Message
                }, UserRoles.Admin);

                paypalRequest.Status = -1;
                if (_context.PayPalPaymentRequests.GetById(paypalRequestId) == null)
                    _context.PayPalPaymentRequests.Insert(paypalRequest);

                await _context.SaveAsync();

                return await HandleErrorJsonAsync(ge);
            }
            catch (Exception e)
            {
                await _notificationRepository.SendAsync(new NotificationForRole
                {
                    SenderUserId = "System",
                    Title = "Error during PayPal payment: " + e.GetRootException().Message,
                    Body = null,
                    Severity = "danger",
                    SendingDateTime = DateTime.UtcNow,
                    Icon = "far fa-exclamation-circle",
                    Important = true
                }, UserRoles.Admin);

                await _mailService.SendEmailAsync(new EmailForRole
                {
                    Subject = "Error during PayPal payment",
                    Body = e.GetRootException().Message
                }, UserRoles.Admin);

                await HandleErrorJsonAsync(e);

                paypalRequest.Status = -1;
                if (_context.PayPalPaymentRequests.GetById(paypalRequestId) == null)
                    _context.PayPalPaymentRequests.Insert(paypalRequest);

                await _context.SaveAsync();

                return Json(new
                {
                    success = false,
                    redirectUrl = Url.Action("PaymentUnsuccessful", "PayPal", new { Area = "" })
                });
            }
        }

        [Route("/paypal/izvršavanje-paypal-return-transakcije")]
        public async Task<IActionResult> ExecutePayPalReturnOrder()
        {
            var userPayPalRequest = new PayPalPaymentRequests();

            try
            {
                var user = await _userManager.GetUserAsync(User) ?? throw new GeneralException("Unable to load user.", signOutUser: true);
                var userPayPalRequests = _context.PayPalPaymentRequests.Find(req => req.UserId == user.Id).OrderByDescending(req => req.RequestDateTime).ToList();

                if (!userPayPalRequests.Any() || userPayPalRequests[0].Status != 0)
                    return RedirectToAction("Index", "Home");

                userPayPalRequest = userPayPalRequests[0];

                userPayPalRequest.PayPalPayerId = Request?.Query["token"];
                if (!string.IsNullOrWhiteSpace(userPayPalRequest.PayPalPayerId))
                {
                    var setup = new PayPalPaymentHelper.PayPalSetup
                    {
                        PayerApprovedOrderId = userPayPalRequest.PayPalPayerId
                    };

                    var response = await PayPalPaymentHelper.CaptureOrderAsync(setup);
                    var statusCode = response.StatusCode;
                    var order = response.Result<Order>();

                    if (!string.IsNullOrWhiteSpace(order.Status) && order.Status.Trim().ToUpper() == "COMPLETED" &&
                        order.PurchaseUnits != null && order.PurchaseUnits.Any() &&
                        order.PurchaseUnits[0].Payments != null && order.PurchaseUnits[0].Payments.Captures != null &&
                        order.PurchaseUnits[0].Payments.Captures.Any())
                    {
                        var paypalPaymentType = _context.PaymentTypes.GetById(PaymentTypesProvider.PayPal.Id);
                        if (paypalPaymentType == null)
                        {
                            paypalPaymentType = PaymentTypesProvider.PayPal;

                            await HandleErrorAsync($"PayPal payment type with PaymentTypeId '{PaymentTypesProvider.PayPal.Id}' does not exist in the database.");
                        }

                        userPayPalRequest.PayPalPaymentId = order.Id;
                        userPayPalRequest.PayPalTransactionId = order.PurchaseUnits[0].Payments.Captures[0].Id;
                        userPayPalRequest.PaymentCompleteDateTime = DateTime.UtcNow;
                        await _context.SaveAsync();

                        var webCredit = user.WebCredit.HasValue ? user.WebCredit.Value : 0;
                        user.WebCredit = webCredit + userPayPalRequest.SecondaryCurrencyAmount;

                        var updated = await _userManager.UpdateAsync(user);
                        if (!updated.Succeeded)
                        {
                            await HandleErrorAsync(string.Join('|', updated.Errors.Select(e => e.Description)));
                            var aspUser = _context.AspNetUsers.GetById(user.Id) ?? throw new GeneralException($"CRITICAL: Unable to load user. User has paid for {userPayPalRequest.SecondaryCurrencyAmount} Web Credit, but has not received the credit yet due to an error.", signOutUser: true);
                            aspUser.WebCredit = user.WebCredit.Value;

                            if ((await _context.SaveAsync()) < 1)
                                throw new Exception($"CRITICAL: Unable save changes to the database. User has paid for {userPayPalRequest.SecondaryCurrencyAmount} Web Credit, but has not received the credit yet due to an error.");
                        }

                        var webCreditLogId = Helper.GenerateNumbersId();
                        var transactionId = Helper.GenerateNumbersId();

                        _context.Transactions.Insert(new Transactions
                        {
                            Id = transactionId,
                            SenderId = PaymentTypesProvider.CounterApathyPayment.Id,
                            ReceiverId = user.Id,
                            DateTime = DateTime.UtcNow,
                            Amount = userPayPalRequest.SecondaryCurrencyAmount,
                            CurrencyCode = userPayPalRequest.SecondaryCurrencyCode
                        });

                        _context.WebCreditLogs.Insert(new WebCreditLogs
                        {
                            Id = webCreditLogId,
                            UserId = user.Id,
                            Email = user.Email,
                            Amount = userPayPalRequest.SecondaryCurrencyAmount,
                            CurrentAmount = user.WebCredit.Value,
                            ExecutionDateTime = DateTime.UtcNow,
                            PaymentTypeId = paypalPaymentType.Id,
                            PaymentTypeName = paypalPaymentType.Name,
                            TransactionId = transactionId
                        });

                        userPayPalRequest.WebCreditLogId = webCreditLogId;
                        userPayPalRequest.Status = 1;

                        await _context.SaveAsync();

                        await _notificationRepository.SendAsync(new Notifications
                        {
                            Id = Helper.GenerateNumbersId(),
                            SenderUserId = "System",
                            ReceiverUserId = user.Id,
                            Title = $"Na Vaš nalog je dodat Veb kredit u iznosu od {userPayPalRequest.SecondaryCurrencyCode} {userPayPalRequest.SecondaryCurrencyAmount}.",
                            Body = null,
                            Severity = "primary",
                            Read = false,
                            SendingDateTime = DateTime.UtcNow,
                            Icon = "fab fa-paypal",
                            Important = true
                        });

                        //await _mailService.SendPayPalPaymentSuccessfulEmailAsync(new PayPalPaymentSuccessfulEmail
                        //{
                        //    PaymentRequestId = userPayPalRequest.Id,
                        //    PaymentId = userPayPalRequest.PayPalPaymentId,
                        //    TransactionId = userPayPalRequest.PayPalTransactionId,
                        //    PrimaryCurrencyTotalPaid = (userPayPalRequest.PrimaryCurrencyAmount + userPayPalRequest.PrimaryCurrencyFeeAmount).ToString("#.##"),
                        //    PrimaryCurrencyCode = userPayPalRequest.PrimaryCurrencyCode,
                        //    SecondaryCurrencyCode = userPayPalRequest.SecondaryCurrencyCode,
                        //    SecondaryCurrencyAmount = userPayPalRequest.SecondaryCurrencyAmount.ToString("#.##"),
                        //    PrimaryCurrencyFeeAmount = userPayPalRequest.PrimaryCurrencyFeeAmount.ToString("#.##"),
                        //    ExchangeRate = userPayPalRequest.ExchangeRate.ToString("#.##"),
                        //    FeePercentage = userPayPalRequest.FeePercentage.ToString("#.##"),
                        //    TotalWebCreditInAccount = user.WebCredit.Value.ToString("#.##"),
                        //    PaymentCompleteDateTime = _dateHelper.ConvertDateTimeFromUtcToLocalString(userPayPalRequest.PaymentCompleteDateTime ?? default)
                        //}, includeTemplateIfExists: true);

                        await _mailService.SendWebCreditAddedEmailAsync(new WebCreditAddedEmail
                        {
                            ToEmail = user.Email,
                            FirstName = user.FirstName,
                            Amount = userPayPalRequest.SecondaryCurrencyAmount.ToString("#.##"),
                            CurrentAmount = user.WebCredit.Value.ToString("#.##")
                        }, includeTemplateIfExists: true);

                        return RedirectToAction("PaymentSuccessful", "PayPal");
                    }
                    else return RedirectToAction("PaymentUnsuccessful", "PayPal");
                }

                throw new Exception("Query string is missing PayPal PayerID.");
            }
            catch (GeneralException ge)
            {
                await _notificationRepository.SendAsync(new NotificationForRole
                {
                    SenderUserId = "System",
                    Title = "Error during PayPal payment: " + ge.GetRootException().Message,
                    Body = null,
                    Severity = "danger",
                    SendingDateTime = DateTime.UtcNow,
                    Icon = "far fa-exclamation-circle",
                    Important = true
                }, UserRoles.Admin);

                await _mailService.SendEmailAsync(new EmailForRole
                {
                    Subject = "Error during PayPal payment",
                    Body = ge.GetRootException().Message
                }, UserRoles.Admin);

                userPayPalRequest.Status = -1;
                await _context.SaveAsync();
                return await HandleErrorAsync(ge);
            }
            catch (Exception e)
            {
                await _notificationRepository.SendAsync(new NotificationForRole
                {
                    SenderUserId = "System",
                    Title = "Error during PayPal payment: " + e.GetRootException().Message,
                    Body = null,
                    Severity = "danger",
                    SendingDateTime = DateTime.UtcNow,
                    Icon = "far fa-exclamation-circle",
                    Important = true
                }, UserRoles.Admin);

                await _mailService.SendEmailAsync(new EmailForRole
                {
                    Subject = "Error during PayPal payment",
                    Body = e.GetRootException().Message
                }, UserRoles.Admin);

                await HandleErrorAsync(e);
                userPayPalRequest.Status = -1;
                await _context.SaveAsync();
                return RedirectToAction("PaymentUnsuccessful", "PayPal", new { Area = "" });
            }
        }

        [HttpGet("/paypal/plaćanje-uspešno")]
        public async Task<IActionResult> PaymentSuccessful()
        {
            var userPayPalRequest = new PayPalPaymentRequests();

            try
            {
                var user = await _userManager.GetUserAsync(User) ?? throw new GeneralException("Unable to load user.", signOutUser: true);
                var userPayPalRequests = _context.PayPalPaymentRequests.Find(req => req.UserId == user.Id).OrderByDescending(req => req.RequestDateTime).ToList();

                if (!userPayPalRequests.Any()) return RedirectToAction("Index", "Home", new { Area = "" });

                userPayPalRequest = userPayPalRequests[0];

                if (userPayPalRequest.Status != 1)
                {
                    userPayPalRequest.Status = -1;
                    await _context.SaveAsync();
                    return RedirectToAction("PaymentUnsuccessful");
                }

                var payPalPaymentType = (from log in _context.WebCreditLogs.ReadOnlyFind(l => l.Id == userPayPalRequest.WebCreditLogId).ToList()
                                         join pt in _context.PaymentTypes.ReadOnlyGetAll()
                                         on log.PaymentTypeId equals pt.Id
                                         select pt).SingleOrDefault();

                if (payPalPaymentType == default)
                {
                    payPalPaymentType = PaymentTypesProvider.PayPal;

                    await HandleErrorAsync($"WebCreditLog with ID '{userPayPalRequest.WebCreditLogId}' does not exist for PayPalPaymentRequest with ID '{userPayPalRequest.Id}'.");
                }

                var paymentSuccessful = new PayPalPaymentSuccessfulViewModel
                {
                    PaymentType = payPalPaymentType.Name,
                    PaymentTypeLogo = payPalPaymentType.Logo
                };
                paymentSuccessful.Map(userPayPalRequest);

                return View(paymentSuccessful);
            }
            catch (Exception e)
            {
                await _notificationRepository.SendAsync(new NotificationForRole
                {
                    SenderUserId = "System",
                    Title = "Error during PayPal payment: " + e.GetRootException().Message,
                    Body = null,
                    Severity = "danger",
                    SendingDateTime = DateTime.UtcNow,
                    Icon = "far fa-exclamation-circle",
                    Important = true
                }, UserRoles.Admin);

                await _mailService.SendEmailAsync(new EmailForRole
                {
                    Subject = "Error during PayPal payment",
                    Body = e.GetRootException().Message
                }, UserRoles.Admin);

                return await HandleErrorAsync(e);
            }
        }

        [HttpGet("/paypal/plaćanje-neuspešno")]
        public async Task<IActionResult> PaymentUnsuccessful()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User) ?? throw new GeneralException("Unable to load user.", signOutUser: true);
                var userPayPalRequests = _context.PayPalPaymentRequests.ReadOnlyFind(req => req.UserId == user.Id).OrderByDescending(req => req.RequestDateTime).ToList();

                if (!userPayPalRequests.Any() || userPayPalRequests[0].Status != -1)
                    return RedirectToAction("Index", "Home", new { Area = "" });

                return View();
            }
            catch (Exception e)
            {
                await _notificationRepository.SendAsync(new NotificationForRole
                {
                    SenderUserId = "System",
                    Title = "Error during PayPal payment: " + e.GetRootException().Message,
                    Body = null,
                    Severity = "danger",
                    SendingDateTime = DateTime.UtcNow,
                    Icon = "far fa-exclamation-circle",
                    Important = true
                }, UserRoles.Admin);

                await _mailService.SendEmailAsync(new EmailForRole
                {
                    Subject = "Error during PayPal payment",
                    Body = e.GetRootException().Message
                }, UserRoles.Admin);

                return await HandleErrorAsync(e);
            }
        }

        [HttpGet("/paypal/kurs-dinar-evro")]
        public async Task<JsonResult> GetUsdToRsdExchangeRate()
        {
            try
            {
                return Json(new
                {
                    exchangeRate = await ExchangeRate.GetUsdToRsdRateAsync()
                });
            }
            catch (Exception e)
            {
                await HandleErrorJsonAsync(e);
                return Json(new
                {
                    exchangeRate = 109
                });
            }
        }

        private class PayPalPaymentHelper
        {
            public static PayPalHttpClient CreateClient(PayPalSetup setup)
            {
                return setup.Environment == PayPalSetup.PayPalEnvironmentTypes.Live ?
                    new PayPalHttpClient(new LiveEnvironment(_payPalSettings.ClientID, _payPalSettings.ClientSecret)) :
                    new PayPalHttpClient(new SandboxEnvironment(_payPalSettings.ClientID, _payPalSettings.ClientSecret));
            }

            public static async Task<PayPalHttp.HttpResponse> CreateOrderAsync(double amountToAdd, PayPalSetup setup)
            {
                var order = new OrderRequest
                {
                    CheckoutPaymentIntent = "CAPTURE",
                    PurchaseUnits = new List<PurchaseUnitRequest>()
                    {
                        new PurchaseUnitRequest
                        {
                            Items = new List<Item>()
                            {
                                //ovo je zapravo ono sto naplacujem, znaci bice Session
                                new Item
                                {
                                    Quantity = "1",
                                    Name = "Web Credit",
                                    Description = "",
                                    Sku = "sku",
                                    Tax = new Money { CurrencyCode = "USD", Value = "0" },
                                    UnitAmount = new Money { CurrencyCode = "USD", Value = amountToAdd.ToString("#.##") }
                                }
                            },
                            AmountWithBreakdown = new AmountWithBreakdown
                            {
                                CurrencyCode = "USD",
                                Value = amountToAdd.ToString("#.##"), //suma koji korisnik placa, znaci ono sto unese u inputu (ovoliko web credit mu se dodaje na nalog)
                                AmountBreakdown = new AmountBreakdown
                                {
                                    TaxTotal = new Money
                                    {
                                        CurrencyCode = "USD",
                                        Value = "0"
                                    },
                                    Shipping = new Money
                                    {
                                        CurrencyCode = "USD",
                                        Value = "0"
                                    },
                                    ItemTotal = new Money
                                    {
                                        CurrencyCode = "USD",
                                        Value = amountToAdd.ToString("#.##")
                                    }
                                }
                            }
                        }
                    },
                    ApplicationContext = new ApplicationContext
                    {
                        ReturnUrl = setup.RedirectUrl,
                        CancelUrl = $"https://counterapathy.com/nalog/veb-kredit?Cancel=true",
                        BrandName = "CounterApathy"
                    }
                };

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                
                var request = new OrdersCreateRequest();
                request.Prefer("return=representation");
                //request.Prefer("SOLUTIONTYPE=Sole");
                request.RequestBody(order);

                return await CreateClient(setup).Execute(request);
            }

            public static async Task<PayPalHttp.HttpResponse> CaptureOrderAsync(PayPalSetup setup)
            {
                var request = new OrdersCaptureRequest(setup.PayerApprovedOrderId);
                request.RequestBody(new OrderActionRequest());
                return await CreateClient(setup).Execute(request);
            }

            public class PayPalSetup
            {
                public enum PayPalEnvironmentTypes
                {
                    Live,
                    Sandbox
                }

                public PayPalEnvironmentTypes Environment { get; set; }
                public string ClientId { get; set; }
                public string Secret { get; set; }
                public string RedirectUrl { get; set; }
                public string ApproveUrl { get; set; }
                public string PayerApprovedOrderId { get; set; }

                public PayPalSetup()
                {
                    if (_payPalSettings.Environment.ToLower() == "live") Environment = PayPalEnvironmentTypes.Live;
                    if (_payPalSettings.Environment.ToLower() == "sandbox") Environment = PayPalEnvironmentTypes.Sandbox;
                    else throw new Exception("Unknown PayPal environment.");
                }
            }
        }

        public class PayPalSettings
        {
            public string Environment { get; set; }
            public string ClientID { get; set; }
            public string ClientSecret { get; set; }
            public double FeePercentage { get; set; }
        }

        public class ExchangeRate
        {
            public static async Task<double> GetUsdToRsdRateAsync()
            {
                CurrencyExchangeRate exchangeRateObj = JsonSerializer.Deserialize<CurrencyExchangeRate>((await new HttpClient().GetStringAsync(EXCHANGE_RATE_API_URI)).ToString());
                return exchangeRateObj.code.ToUpper() == "USD" && exchangeRateObj.cash_sell != default ? exchangeRateObj.cash_sell : 109;
            }
        }
    }
}
