using Database.Models;
using Framework.Emails;
using Framework.Emails.EmailTypes;
using Framework.Helpers;
using Framework.Helpers.ExtensionMethods;
using Framework.Interfaces;
using Framework.Models;
using Framework.Notifications;
using Framework.Notifications.NotificationTypes;
using Framework.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Stripe;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using WebApplication9.Base;
using WebApplication9.Implementations;
using WebApplication9.Interfaces;
using WebApplication9.Models;

namespace WebApplication9.Controllers
{
    [Route("webhook")]
    [ApiController]
    public class WebhookController : BaseController
    {
        private readonly StripeSettings _stripeSettings;
        private readonly IStripeFunctionsProvider _stripeFunctions;
        private readonly ISessionsFunctionsProvider _sessionsFunctions;

        public WebhookController(IConfiguration configuration,
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
            _sessionsFunctions = new SessionsFunctionsProvider();
        }

        [HttpPost]
        public async Task<IActionResult> Index()
        {
            try
            {
                //const string endpointSecret = "whsec_3HgC6QduRF0zY3AkrXxB0r2qTOPRCSD7";

                var stripeEvent = EventUtility.ConstructEvent(
                    await new StreamReader(HttpContext.Request.Body).ReadToEndAsync(),
                    Request.Headers["Stripe-Signature"],
                    _stripeSettings.WebhookSecret, 300, false);

                if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    var paymentIntentInDatabase = _context.StripePaymentIntents.GetById(paymentIntent.Id);
                    if (paymentIntentInDatabase == null)
                    {
                        await _mailService.SendEmailAsync(new EmailForRole
                        {
                            Subject = "Webhook Error: PaymentIntent does not exist in database",
                            Body = $"PaymentIntent '{paymentIntent.Id}' does not exist in database. paymentIntentInDatabase = null."
                        }, UserRoles.Admin);

                        await HandleErrorJsonAsync($"PaymentIntent '{paymentIntent.Id}' does not exist in database. paymentIntentInDatabase = null.");

                        return BadRequest();
                    }

                    if (paymentIntentInDatabase.Status != 0 || paymentIntentInDatabase.MustSucceedUntil < DateTime.UtcNow)
                    {
                        await _mailService.SendEmailAsync(new EmailForRole
                        {
                            Subject = "Webhook Error: PaymentIntent is not pending",
                            Body = $"PaymentIntentInDatabase '{paymentIntentInDatabase.Id}' either does not have Status = '0' or " +
                            $"it has expired."
                        }, UserRoles.Admin);

                        await HandleErrorJsonAsync($"PaymentIntentInDatabase '{paymentIntentInDatabase.Id}' either does not have Status = '0' or " +
                            $"it has expired.");

                        return BadRequest();
                    }

                    if (paymentIntentInDatabase.Status == 1 || paymentIntentInDatabase.StripeEventTypeName == Events.PaymentIntentSucceeded)
                    {
                        await _mailService.SendEmailAsync(new EmailForRole
                        {
                            Subject = "Webhook Error: PaymentIntent has already succeedded",
                            Body = $"PaymentIntentInDatabase '{paymentIntentInDatabase.Id}' has Status = '{paymentIntentInDatabase.Status}' and " +
                            $"StripeEventTypeName = '{paymentIntentInDatabase.StripeEventTypeName}' therefore the Webhook Event has not been " +
                            $"proccessed for the second time. Inspect the WebhookController, database and Stripe Dashboard to find the potential solutions."
                        }, UserRoles.Admin);

                        await HandleErrorJsonAsync($"PaymentIntentInDatabase '{paymentIntentInDatabase.Id}' has Status = '{paymentIntentInDatabase.Status}' and " +
                            $"StripeEventTypeName = '{paymentIntentInDatabase.StripeEventTypeName}' therefore the Webhook Event has not been " +
                            $"proccessed for the second time. Inspect the WebhookController, database and Stripe Dashboard to find the potential solutions.");

                        return BadRequest();
                    }

                    if (paymentIntentInDatabase.SystemEventTypeName == StripePaymentIntentSystemEventTypeNameProvider.WebCreditTopUp)
                    {
                        if (!paymentIntent.Metadata.TryGetValue("UserId", out string userIdFromPaymentIntent))
                        {
                            await _mailService.SendEmailAsync(new EmailForRole
                            {
                                Subject = "Webhook Error: PaymentIntent does not contain user ID",
                                Body = $"PaymentIntent '{paymentIntent.Id}' from Stripe does not contain field called 'UserId' in it's Metadata property.",
                            }, UserRoles.Admin);

                            await HandleErrorJsonAsync($"PaymentIntent '{paymentIntent.Id}' from Stripe does not contain field called 'UserId' in it's Metadata property.");

                            return BadRequest();
                        }

                        if (paymentIntentInDatabase.UserIdOrAnonymous != userIdFromPaymentIntent)
                        {
                            await _mailService.SendEmailAsync(new EmailForRole
                            {
                                Subject = "Webhook Error: PaymentIntentses' user IDs do not match",
                                Body = $"PaymentIntentInDatabase '{paymentIntentInDatabase.Id}' has user ID '{paymentIntentInDatabase.UserIdOrAnonymous}' " +
                                $"but PaymentIntent '{paymentIntent.Id}' from Stripe has user ID '{userIdFromPaymentIntent}'."
                            }, UserRoles.Admin);

                            await HandleErrorJsonAsync($"PaymentIntentInDatabase '{paymentIntentInDatabase.Id}' has user ID '{paymentIntentInDatabase.UserIdOrAnonymous}' " +
                                $"but PaymentIntent '{paymentIntent.Id}' from Stripe has user ID '{userIdFromPaymentIntent}'.");

                            return BadRequest();
                        }

                        if (paymentIntentInDatabase.CustomerId != paymentIntent.CustomerId)
                        {
                            await _mailService.SendEmailAsync(new EmailForRole
                            {
                                Subject = "Webhook Error: PaymentIntentses' customer IDs do not match",
                                Body = $"PaymentIntentInDatabase '{paymentIntentInDatabase.Id}' has Customer ID '{paymentIntentInDatabase.CustomerId}' " +
                                $"but PaymentIntent '{paymentIntent.Id}' from Stripe has Customer ID '{paymentIntent.CustomerId}'."
                            }, UserRoles.Admin);

                            await HandleErrorJsonAsync($"PaymentIntentInDatabase '{paymentIntentInDatabase.Id}' has Customer ID '{paymentIntentInDatabase.CustomerId}' " +
                                $"but PaymentIntent '{paymentIntent.Id}' from Stripe has Customer ID '{paymentIntent.CustomerId}'.");

                            return BadRequest();
                        }

                        // Here it is good to ask if user is null because even though we allow the client to
                        // book a session without having an account (user = null), we do not allow the
                        // client to add web credit to his account without having an account
                        var user = await _userManager.FindByIdAsync(paymentIntentInDatabase.UserIdOrAnonymous);
                        if (user == null)
                        {
                            await _mailService.SendEmailAsync(new EmailForRole
                            {
                                Subject = "Webhook Error: User does not exist in database",
                                Body = $"PaymentIntentInDatabase '{paymentIntentInDatabase.Id}' has a user ID '{paymentIntentInDatabase.UserIdOrAnonymous}' " +
                                $"which does not exists among the users."
                            }, UserRoles.Admin);

                            await HandleErrorJsonAsync($"PaymentIntentInDatabase '{paymentIntentInDatabase.Id}' has a user ID '{paymentIntentInDatabase.UserIdOrAnonymous}' " +
                                $"which does not exists among the users.");

                            return BadRequest();
                        }

                        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            try
                            {
                                var paymentIntentAmount = paymentIntent.Amount / 100;

                                using (var scope2 = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
                                {
                                    try
                                    {
                                        var webCreditBeforeAdding = user.WebCredit.HasValue ? user.WebCredit.Value : 0;
                                        user.WebCredit = webCreditBeforeAdding + paymentIntentAmount;
                                        var updated = await _userManager.UpdateAsync(user);

                                        if (!updated.Succeeded) throw new GeneralException(string.Join('|', updated.Errors.Select(e => e.Description)));

                                        scope2.Complete();
                                    }
                                    catch (Exception)
                                    {
                                        scope2.Dispose();
                                        throw;
                                    }
                                }

                                _context.Transactions.Insert(new Transactions
                                {
                                    Id = paymentIntent.Id,
                                    SenderId = PaymentTypesProvider.PaymentCards.Id,
                                    ReceiverId = user.Id,
                                    DateTime = DateTime.UtcNow,
                                    Amount = paymentIntentAmount,
                                    CurrencyCode = paymentIntent.Currency.ToUpper()
                                });

                                var webCreditLogId = Helper.GenerateNumbersId();

                                _context.WebCreditLogs.Insert(new WebCreditLogs
                                {
                                    Id = webCreditLogId,
                                    UserId = user.Id,
                                    Email = user.Email,
                                    Amount = paymentIntentAmount,
                                    ExecutionDateTime = DateTime.UtcNow,
                                    PaymentTypeId = PaymentTypesProvider.PaymentCards.Id,
                                    PaymentTypeName = PaymentTypesProvider.PaymentCards.Name,
                                    CurrentAmount = user.WebCredit.Value,
                                    TransactionId = paymentIntent.Id
                                });

                                paymentIntentInDatabase.WebCreditLogId = webCreditLogId;

                                paymentIntentInDatabase.Status = 1;
                                paymentIntentInDatabase.StripeEventTypeName = Events.PaymentIntentSucceeded;

                                _context.StripePaymentIntents.Update(paymentIntentInDatabase);

                                await _context.SaveAsync();

                                await _notificationRepository.SendAsync(new Notifications
                                {
                                    Id = Helper.GenerateNumbersId(),
                                    SenderUserId = SystemInformation.Name,
                                    ReceiverUserId = user.Id,
                                    Title = $"Na Vaš nalog je dodat Veb kredit u iznosu od {paymentIntent.Currency.ToUpper()} {paymentIntentAmount}.",
                                    Body = null,
                                    Severity = "success",
                                    Read = false,
                                    SendingDateTime = DateTime.UtcNow,
                                    Icon = "fal fa-usd-circle",
                                    Important = true
                                });

                                await _mailService.SendWebCreditAddedEmailAsync(new WebCreditAddedEmail
                                {
                                    ToEmail = user.Email,
                                    FirstName = user.FirstName,
                                    Amount = paymentIntentAmount.ToString(),
                                    CurrentAmount = user.WebCredit.Value.ToString()
                                }, includeTemplateIfExists: true);

                                scope.Complete();

                                return Ok();
                            }
                            catch (Exception)
                            {
                                scope.Dispose();
                                throw;
                            }
                        }
                    }
                    else if (paymentIntentInDatabase.SystemEventTypeName == StripePaymentIntentSystemEventTypeNameProvider.SessionInstantPayment)
                    {
                        var sessionId = string.Empty;
                        var bookingId = string.Empty;

                        if (!paymentIntent.Metadata.TryGetValue("UserId", out string userIdFromPaymentIntent) ||
                            !paymentIntent.Metadata.TryGetValue("SessionId", out sessionId) ||
                            !paymentIntent.Metadata.TryGetValue("BookingId", out bookingId))
                        {
                            await _mailService.SendEmailAsync(new EmailForRole
                            {
                                Subject = "Webhook Error: PaymentIntent does not contain all the required information",
                                Body = $"PaymentIntent '{paymentIntent.Id}' from Stripe does not contain all the required fields - " +
                                $"UserId = '{(!string.IsNullOrWhiteSpace(userIdFromPaymentIntent) ? userIdFromPaymentIntent : "null")}', " +
                                $"SessionId = '{(!string.IsNullOrWhiteSpace(sessionId) ? sessionId : "null")}', " +
                                $"BookingId = '{(!string.IsNullOrWhiteSpace(bookingId) ? bookingId : "null")}' " +
                                $"in it's Metadata property.",
                            }, UserRoles.Admin);

                            await HandleErrorJsonAsync($"PaymentIntent '{paymentIntent.Id}' from Stripe does not contain all the required fields - " +
                                $"UserId = '{(!string.IsNullOrWhiteSpace(userIdFromPaymentIntent) ? userIdFromPaymentIntent : "null")}', " +
                                $"SessionId = '{(!string.IsNullOrWhiteSpace(sessionId) ? sessionId : "null")}', " +
                                $"BookingId = '{(!string.IsNullOrWhiteSpace(bookingId) ? bookingId : "null")}' " +
                                $"in it's Metadata property.");

                            return BadRequest();
                        }

                        if (paymentIntentInDatabase.UserIdOrAnonymous != userIdFromPaymentIntent)
                        {
                            await _mailService.SendEmailAsync(new EmailForRole
                            {
                                Subject = "Webhook Error: PaymentIntentses' user IDs do not match",
                                Body = $"PaymentIntentInDatabase '{paymentIntentInDatabase.Id}' has user ID '{paymentIntentInDatabase.UserIdOrAnonymous}' " +
                                $"but PaymentIntent '{paymentIntent.Id}' from Stripe has user ID '{userIdFromPaymentIntent}'."
                            }, UserRoles.Admin);

                            await HandleErrorJsonAsync($"PaymentIntentInDatabase '{paymentIntentInDatabase.Id}' has user ID '{paymentIntentInDatabase.UserIdOrAnonymous}' " +
                                $"but PaymentIntent '{paymentIntent.Id}' from Stripe has user ID '{userIdFromPaymentIntent}'.");

                            return BadRequest();
                        }

                        if (paymentIntentInDatabase.CustomerId != paymentIntent.CustomerId)
                        {
                            await _mailService.SendEmailAsync(new EmailForRole
                            {
                                Subject = "Webhook Error: PaymentIntentses' customer IDs do not match",
                                Body = $"PaymentIntentInDatabase '{paymentIntentInDatabase.Id}' has Customer ID '{paymentIntentInDatabase.CustomerId}' " +
                                $"but PaymentIntent '{paymentIntent.Id}' from Stripe has Customer ID '{paymentIntent.CustomerId}'."
                            }, UserRoles.Admin);

                            await HandleErrorJsonAsync($"PaymentIntentInDatabase '{paymentIntentInDatabase.Id}' has Customer ID '{paymentIntentInDatabase.CustomerId}' " +
                                $"but PaymentIntent '{paymentIntent.Id}' from Stripe has Customer ID '{paymentIntent.CustomerId}'.");

                            return BadRequest();
                        }

                        if (paymentIntentInDatabase.Email.ToLowerInvariant() != paymentIntent.ReceiptEmail.ToLowerInvariant())
                        {
                            await _mailService.SendEmailAsync(new EmailForRole
                            {
                                Subject = "Webhook Error: PaymentIntentses emails do not match",
                                Body = $"PaymentIntentInDatabase '{paymentIntentInDatabase.Id}' has email '{paymentIntentInDatabase.Email}' " +
                                $"but PaymentIntent '{paymentIntent.Id}' from Stripe has email '{paymentIntent.ReceiptEmail}'."
                            }, UserRoles.Admin);

                            await HandleErrorJsonAsync($"PaymentIntentInDatabase '{paymentIntentInDatabase.Id}' has email '{paymentIntentInDatabase.Email}' " +
                                 $"but PaymentIntent '{paymentIntent.Id}' from Stripe has email '{paymentIntent.ReceiptEmail}'.");

                            return BadRequest();
                        }

                        var sessionToBook = _context.Sessions.GetById(sessionId);

                        if (sessionToBook == default)
                        {
                            await _mailService.SendEmailAsync(new EmailForRole
                            {
                                Subject = "Webhook Error: Session does not exist",
                                Body = $"Session '{sessionId}' does not exist in the database."
                            }, UserRoles.Admin);

                            await HandleErrorJsonAsync($"Session '{sessionId}' does not exist in the database.");

                            return BadRequest();
                        }

                        string therapistUserId = null;
                        string therapistId = null;
                        string therapistFirstName = null;
                        string therapistLastName = null;
                        string therapistEmail = null;
                        string therapistPhoneNumber = null;
                        string therapistStreet = null;
                        string therapistHouseNumber = null;
                        string therapistCity = null;
                        string therapistPostalCode = null;
                        string therapistCountry = null;
                        string clientFirstName = null;
                        string clientLastName = null;
                        string contactMethodId = null;
                        string contactMethodName = null;
                        string contactMethodColor = null;
                        string contactMethodIcon = null;
                        if (!paymentIntent.Metadata.TryGetValue("TherapistUserId", out therapistUserId) ||
                            !paymentIntent.Metadata.TryGetValue("TherapistId", out therapistId) ||
                            !paymentIntent.Metadata.TryGetValue("TherapistFirstName", out therapistFirstName) ||
                            !paymentIntent.Metadata.TryGetValue("TherapistLastName", out therapistLastName) ||
                            !paymentIntent.Metadata.TryGetValue("TherapistEmail", out therapistEmail) ||
                            !paymentIntent.Metadata.TryGetValue("TherapistPhoneNumber", out therapistPhoneNumber) ||
                            !paymentIntent.Metadata.TryGetValue("TherapistStreet", out therapistStreet) ||
                            !paymentIntent.Metadata.TryGetValue("TherapistHouseNumber", out therapistHouseNumber) ||
                            !paymentIntent.Metadata.TryGetValue("TherapistCity", out therapistCity) ||
                            !paymentIntent.Metadata.TryGetValue("TherapistPostalCode", out therapistPostalCode) ||
                            !paymentIntent.Metadata.TryGetValue("TherapistCountry", out therapistCountry) ||
                            !paymentIntent.Metadata.TryGetValue("ClientFirstName", out clientFirstName) ||
                            !paymentIntent.Metadata.TryGetValue("ClientLastName", out clientLastName) ||
                            !paymentIntent.Metadata.TryGetValue("ContactMethodId", out contactMethodId) ||
                            !paymentIntent.Metadata.TryGetValue("ContactMethodName", out contactMethodName) ||
                            !paymentIntent.Metadata.TryGetValue("ContactMethodColor", out contactMethodColor) ||
                            !paymentIntent.Metadata.TryGetValue("ContactMethodIcon", out contactMethodIcon))
                        {
                            var error = $"PaymentIntent '{paymentIntent.Id}' from Stripe does not contain all the required fields - " +
                                $"TherapistUserId = '{(!string.IsNullOrWhiteSpace(therapistUserId) ? therapistUserId : "null")}', " +
                                $"TherapistId = '{(!string.IsNullOrWhiteSpace(therapistId) ? therapistId : "null")}', " +
                                $"TherapistFirstName = '{(!string.IsNullOrWhiteSpace(therapistFirstName) ? therapistFirstName : "null")}', " +
                                $"TherapistLastName = '{(!string.IsNullOrWhiteSpace(therapistLastName) ? therapistLastName : "null")}', " +
                                $"TherapistEmail = '{(!string.IsNullOrWhiteSpace(therapistEmail) ? therapistEmail : "null")}', " +
                                $"TherapistPhoneNumber = '{(!string.IsNullOrWhiteSpace(therapistPhoneNumber) ? therapistPhoneNumber : "null")}', " +
                                $"TherapistStreet = '{(!string.IsNullOrWhiteSpace(therapistStreet) ? therapistStreet : "null")}', " +
                                $"TherapistHouseNumber = '{(!string.IsNullOrWhiteSpace(therapistHouseNumber) ? therapistHouseNumber : "null")}', " +
                                $"TherapistCity = '{(!string.IsNullOrWhiteSpace(therapistCity) ? therapistCity : "null")}', " +
                                $"TherapistPostalCode = '{(!string.IsNullOrWhiteSpace(therapistPostalCode) ? therapistPostalCode : "null")}', " +
                                $"TherapistCountry = '{(!string.IsNullOrWhiteSpace(therapistCountry) ? therapistCountry : "null")}', " +
                                $"ClientFirstName = '{(!string.IsNullOrWhiteSpace(clientFirstName) ? clientFirstName : "null")}', " +
                                $"ClientLastName = '{(!string.IsNullOrWhiteSpace(clientLastName) ? clientLastName : "null")}', " +
                                $"ContactMethodId = '{(!string.IsNullOrWhiteSpace(contactMethodId) ? contactMethodId : "null")}', " +
                                $"ContactMethodName = '{(!string.IsNullOrWhiteSpace(contactMethodName) ? contactMethodName : "null")}', " +
                                $"ContactMethodColor = '{(!string.IsNullOrWhiteSpace(contactMethodColor) ? contactMethodColor : "null")}', " +
                                $"ContactMethodIcon = '{(!string.IsNullOrWhiteSpace(contactMethodIcon) ? contactMethodIcon : "null")}', " +
                                $"in it's Metadata property.";

                            await _mailService.SendEmailAsync(new EmailForRole
                            {
                                Subject = "Webhook Error: PaymentIntent is missing required info",
                                Body = error
                            }, UserRoles.Admin);

                            await HandleErrorJsonAsync(error);

                            return BadRequest();
                        }

                        _context.Transactions.Insert(new Transactions
                        {
                            Id = paymentIntentInDatabase.Id,
                            SenderId = userIdFromPaymentIntent,
                            ReceiverId = PaymentTypesProvider.CounterApathyPayment.Id,
                            DateTime = DateTime.UtcNow,
                            Amount = sessionToBook.Price,
                            CurrencyCode = paymentIntent.Currency.ToUpper()
                        });

                        var bookedSession = new BookedSessions
                        {
                            Id = bookingId,
                            SessionId = sessionId,
                            TherapistId = therapistId,
                            TherapistFirstName = therapistFirstName,
                            TherapistLastName = therapistLastName,
                            TherapistEmail = therapistEmail,
                            TherapistPhoneNumber = therapistPhoneNumber,
                            TherapistStreet = therapistStreet,
                            TherapistHouseNumber = therapistHouseNumber,
                            TherapistCity = therapistCity,
                            TherapistPostalCode = therapistPostalCode,
                            TherapistCountry = therapistCountry,
                            ClientId = userIdFromPaymentIntent,
                            ClientFirstName = clientFirstName,
                            ClientLastName = clientLastName,
                            ClientEmail = paymentIntentInDatabase.Email,
                            ClientPhoneNumber = paymentIntentInDatabase.PhoneNumber,
                            Subject = "Session",
                            Description = null,
                            Price = sessionToBook.Price,
                            Type = sessionToBook.Type,
                            StartTime = sessionToBook.StartDateTime,
                            EndTime = sessionToBook.EndDateTime,
                            BookingDate = DateTime.UtcNow,
                            ContactMethodId = contactMethodId,
                            ContactMethodName = contactMethodName,
                            ContactMethodColor = contactMethodColor,
                            ContactMethodIcon = contactMethodIcon,
                            Status = 0,
                            TherapistIsPaid = false,
                            ClientBookingTransactionId = paymentIntentInDatabase.Id
                        };

                        _context.BookedSessions.Insert(bookedSession);
                        _context.BookedSessionsContactMethods.Insert(new BookedSessionsContactMethods
                        {
                            BookedSessionId = bookingId,
                            ContactMethodId = contactMethodId
                        });

                        sessionToBook.Booked = 1;
                        paymentIntentInDatabase.Status = 1;
                        paymentIntentInDatabase.StripeEventTypeName = Events.PaymentIntentSucceeded;

                        _context.StripePaymentIntents.Update(paymentIntentInDatabase);

                        await _context.SaveAsync();

                        //send email to anonymous/unauthorized/authorized user
                        await _mailService.SendClientBookedSessionEmailAsync(new BookedSessionEmail
                        {
                            BookedSession = bookedSession
                        }, includeTemplateIfExists: true);

                        //send email to therapist
                        await _mailService.SendTherapistSessionBookedEmailAsync(new BookedSessionEmail
                        {
                            BookedSession = bookedSession
                        }, includeTemplateIfExists: true);

                        var sessionStart = _dateHelper.ConvertDateTimeFromUtcToLocal(sessionToBook.StartDateTime);

                        //send notification to therapist
                        await _notificationRepository.SendAsync(new Notifications
                        {
                            Id = Helper.GenerateNumbersId(),
                            SenderUserId = SystemInformation.Name,
                            ReceiverUserId = therapistUserId,
                            Title = $"Klijent {clientFirstName} {clientLastName} je zakazao seansu sa Vama. Kontakt metoda je {contactMethodName}. " +
                            $"Seansa počinje datuma {_dateHelper.DateStringFromDateTime(sessionStart)}, u {_dateHelper.TimeStringFromDateTime(sessionStart)}h. " +
                            "Za više detalja, proverite imejl koji Vam je upravo stigao u inboks ili spam.",
                            Body = null,
                            Severity = "success",
                            Read = false,
                            SendingDateTime = DateTime.UtcNow,
                            Icon = "fal fa-calendar-check",
                            Important = false
                        });

                        if (!userIdFromPaymentIntent.IsAnonymousOrUnauthorized())
                        {
                            // if we dont wanna use UserId, we can say FindByEmail
                            if(await _userManager.FindByIdAsync(userIdFromPaymentIntent) == null)
                            {
                                await _mailService.SendEmailAsync(new EmailForRole
                                {
                                    Subject = "Webhook Error: User not found",
                                    Body = $"User '{userIdFromPaymentIntent}' does not exist in the AspNetUsers table."
                                }, UserRoles.Admin);

                                await HandleErrorJsonAsync($"User '{userIdFromPaymentIntent}' does not exist in the AspNetUsers table.");

                                return BadRequest();
                            }

                            //send notification to user
                            await _notificationRepository.SendAsync(new Notifications
                            {
                                Id = Helper.GenerateNumbersId(),
                                SenderUserId = SystemInformation.Name,
                                ReceiverUserId = userIdFromPaymentIntent,
                                Title = $"Uspešno ste zakazali seansu sa psihoterapeutom {therapistFirstName} {therapistLastName}, kontakt metoda je {contactMethodName}. " +
                                $"Seansa počinje datuma {_dateHelper.DateStringFromDateTime(sessionStart)}, u {_dateHelper.TimeStringFromDateTime(sessionStart)}h. " +
                                "Za više detalja, proverite imejl koji Vam je upravo stigao u inboks ili spam.",
                                Body = null,
                                Severity = "success",
                                Read = false,
                                SendingDateTime = DateTime.UtcNow,
                                Icon = "fal fa-calendar-check",
                                Important = false
                            });
                        }

                        return Ok();
                    }
                    else
                    {
                        await _mailService.SendEmailAsync(new EmailForRole
                        {
                            Subject = "Unrecognized SystemEventTypeName from PaymentIntentInDatabase",
                            Body = $"PaymentIntentInDatabase '{paymentIntentInDatabase.Id}' has SystemEventTypeName '{paymentIntentInDatabase.SystemEventTypeName}'"
                        }, UserRoles.Admin);

                        await HandleErrorJsonAsync($"PaymentIntentInDatabase '{paymentIntentInDatabase.Id}' has SystemEventTypeName '{paymentIntentInDatabase.SystemEventTypeName}'");

                        return BadRequest();
                    }
                }
                else
                {
                    // Here we return Ok() on purpose
                    // We only listen for PaymentIntent.Succeedded event because
                    // we dont have to listen for others - not needed for now.
                    // In case we need to listen for other events, in the above if-statemenet
                    // we will add to listen to more events:
                    // if (stripeEvent.Type == Events.SomeOtherEvent)
                    return Ok();
                }
            }
            catch (Exception e)
            {
                await HandleErrorJsonAsync(e);
                return StatusCode(500);
            }
        }
    }
}
