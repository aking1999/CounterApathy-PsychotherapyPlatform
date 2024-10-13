using Database.Models;
using DataTransferObjects.ViewModels.Therapist;
using Framework.Emails;
using Framework.Helpers;
using Framework.Helpers.ExtensionMethods;
using Framework.Implementations;
using Framework.Interfaces;
using Framework.Models;
using Framework.Models.Settings;
using Framework.Notifications;
using Framework.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using WebApplication9.Areas.Therapist.ViewModels;
using WebApplication9.Base;
using WebApplication9.Implementations;
using WebApplication9.Interfaces;

namespace WebApplication9.Areas.Therapist.Controllers
{
    [Area(areaName: "Therapist")]
    [Authorize(Roles = "Therapist")]
    public class ConsultationsController : BaseController
    {
        public const int MIN_DAYS_ADD_CONSULTATION_IN_ADVANCE = 1;
        private readonly int _consultationDurationInMinutes;
        private readonly IWebHostEnvironment _environment;
        private readonly IFileRepository _files;
        private readonly ISessionsFunctionsProvider _sessionsFunctions;
        private readonly ITherapistFunctionsProvider _therapistFunctions;
        private readonly IConsultationsFunctionsProvider _consultationsFunctions;

        public ConsultationsController(IConfiguration configuration,
            IWebHostEnvironment environment,
            IErrorLogger error,
            IMailService mailService,
            IDateTimeHelper dateHelper,
            IHttpContextAccessor contextAccessor,
            INotificationRepository notificationRepository,
            UserManager<CustomClient> userManager,
            SignInManager<CustomClient> signInManager) : base(error, mailService, dateHelper, contextAccessor, notificationRepository, userManager, signInManager)
        {
            _environment = environment;

            _consultationDurationInMinutes = configuration.GetSection("ConsultationSettings").Get<ConsultationSettings>().DurationInMinutes;

            _files = new FileRepository();
            _sessionsFunctions = new SessionsFunctionsProvider();
            _therapistFunctions = new TherapistFunctionsProvider();
            _consultationsFunctions = new ConsultationsFunctionsProvider(dateHelper);
        }

        [HttpGet("/terapeut/konsultacije/zakazane-besplatne-konsultacije/{filter?}/{predicate?}")]
        public async Task<IActionResult> BookedConsultations(string filter = null, string predicate = null)
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                {
                    ShowToastOnThisPageIfSet();
                    return View(_consultationsFunctions.GetTherapistBookedConsultations((await _userManager.GetUserAsync(User)).TherapistAccountId, filter, predicate));
                }
                else if (completionStatus == TherapistAccountCompletionStatus.NotSetUpYet)
                    return RedirectToAction("AccountSetup", "Account", new { Area = "Therapist" });
                else throw new GeneralException("Unable to load user.", signOutUser: true);
            }
            catch(Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpGet("/terapeut/izbor-tipa-termina")]
        public async Task<IActionResult> ChooseAppointmentType()
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                {
                    ShowToastOnThisPageIfSet();
                    return View();
                }
                else if (completionStatus == TherapistAccountCompletionStatus.NotSetUpYet)
                    return RedirectToAction("AccountSetup", "Account", new { Area = "Therapist" });
                else throw new GeneralException("Unable to load user.", signOutUser: true);
            }
            catch(Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpGet("/terapeut/besplatne-konsultacije")]
        public async Task<IActionResult> All()
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                {
                    ShowToastOnThisPageIfSet();

                    return View();
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

        [HttpPost("/terapeut/besplatne-konsultacije")]
        public async Task<IActionResult> GetConsultations()
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                    return Json(new
                    {
                        consultations = _therapistFunctions.GetUpcomingConsultationsViewModels((await _userManager.GetUserAsync(User)).TherapistAccountId)
                    });
                else if (completionStatus == TherapistAccountCompletionStatus.Error)
                    await HandleErrorJsonAsync("Unable to load user.");
            }
            catch(Exception e)
            {
                await HandleErrorJsonAsync(e);
            }

            return Json(new
            {
                consultations = new List<ConsultationViewModel>()
            });
        }

        [HttpPost("/terapeut/besplatne-konsultacije/dodavanje-besplatne-konsultacije")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddConsultation([FromForm] AddSessionViewModel addVm)
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if(completionStatus == TherapistAccountCompletionStatus.Completed)
                {
                    var user = await _userManager.GetUserAsync(User);

                    if (_context.TherapistSupportTickets.ReadOnlyAny(t => t.TherapistId == user.TherapistAccountId &&
                                                                          t.TopicId == TherapistSupportTicketTopicsProvider.RequestAccountDeletion.Id))
                        return Json(new
                        {
                            success = false,
                            title = "Podneli ste zahtev za gašenje naloga",
                            body = "Nije moguće dodavanje konsultacija  nakon podnošenja zahteva za gašenje naloga.",
                            severity = "info"
                        });

                    if (!ModelState.IsValid)
                        return Json(new
                        {
                            success = false,
                            title = "Popunite sva obavezna polja",
                            body = "",
                            severity = "info"
                        });

                    var model = addVm.AddConsultation;

                    if (!DateTime.TryParseExact(model.StartEndDate + " " + model.StartTime, "dd/MMM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime consultationStart))
                        return Json(new
                        {
                            success = false,
                            title = "Netačan datum",
                            body = "Molimo osvežite stranicu i pokušajte ponovo ili kontaktirajte podršku za terapeute.",
                            severity = "info"
                        });

                    var consultationEnd = consultationStart.AddMinutes(_consultationDurationInMinutes);

                    var consultationStartUtc = _dateHelper.ConvertDateTimeFromLocalToUtc(consultationStart);
                    var consultationEndUtc = _dateHelper.ConvertDateTimeFromLocalToUtc(consultationEnd);

                    if (consultationStart.Date != consultationEnd.Date)
                        return Json(new
                        {
                            success = false,
                            title = "Konsultacije moraju da počnu i da se završe u istom danu",
                            body = "Promenite vreme početka ovog termina konsultacija.",
                            severity = "info"
                        });

                    if(consultationStartUtc.Date < DateTime.UtcNow.Date.AddDays(MIN_DAYS_ADD_CONSULTATION_IN_ADVANCE))
                        return Json(new
                        {
                            success = false,
                            title = $"Datum početka mora biti za bar {MIN_DAYS_ADD_CONSULTATION_IN_ADVANCE * 24} časa od sada",
                            body = $"Izaberite datum koji je za bar {MIN_DAYS_ADD_CONSULTATION_IN_ADVANCE * 24} časa ispred trenutnog datuma.",
                            severity = "info"
                        });

                    if(_consultationsFunctions.TherapistHasConsultationDuringPeriod(user.TherapistAccountId, consultationStartUtc, consultationEndUtc))
                        return Json(new
                        {
                            success = false,
                            title = "Vremena konsultacija se preklapaju",
                            body = "Već ste dodali jedan termin za besplatne konsultacije koji se završava nakon vremena početka ovog termina. " +
                            "Promenite vreme početka ovog termina konsultacija.",
                            severity = "info"
                        });

                    if (_sessionsFunctions.TherapistHasSessionDuringPeriod(user.TherapistAccountId, consultationStartUtc, consultationEndUtc))
                        return Json(new
                        {
                            success = false,
                            title = "Vreme ovog termina konsultacija se preklapa sa vremenom dodate seanse",
                            body = "Već ste dodali jednu seansu koja se završava nakon vremena početka ovog termina konsultacija. " +
                            "Promenite vreme početka ovog termina konsultacija.",
                            severity = "info"
                        });

                    var consultation = new Consultations
                    {
                        Id = Helper.GenerateNumbersId(),
                        TherapistId = user.TherapistAccountId,
                        StartDateTime = consultationStartUtc,
                        EndDateTime = consultationEndUtc,
                        Booked = 0
                    };

                    _context.Consultations.Insert(consultation);
                    await _context.SaveAsync();

                    return Json(new
                    {
                        success = true,
                        title = "Besplatna konsultacija uspešno dodata",
                        review = "Proverite podatke",
                        date = _dateHelper.DateStringFromDateTime(consultationStart),
                        startTime = _dateHelper.TimeStringFromDateTime(consultationStart),
                        endTime = _dateHelper.TimeStringFromDateTime(consultationEnd),
                        duration = (consultation.EndDateTime - consultation.StartDateTime).ToString(),
                        severity = "success",
                        consultation = new ConsultationViewModel
                        {
                            ConsultationId = consultation.Id,
                            StartDateTime = consultation.StartDateTime,
                            EndDateTime = consultation.EndDateTime
                        }
                    });
                }
                else if (completionStatus == TherapistAccountCompletionStatus.NotSetUpYet)
                    return Json(new
                    {
                        success = false,
                        redirectUrl = Url.Action("AccountSetup", "Account", new { Area = "" })
                    });
                else throw new GeneralException("Unable to load user.", signOutUser: true);
            }
            catch(Exception e)
            {
                return await HandleErrorJsonAsync(e);
            }
        }

        [HttpPost("/terapeut/besplatne-konsultacije/brisanje-besplatne-konsultacije")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConsultation(string consultationId)
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                {
                    var user = await _userManager.GetUserAsync(User);

                    var consultationToDelete = _context.Consultations.Find(c => c.Id == consultationId &&
                                                                                c.TherapistId == user.TherapistAccountId)
                                                                     .SingleOrDefault();

                    if (consultationToDelete == default)
                        return Json(new
                        {
                            success = false,
                            title = "Konsultacija nije pronađena",
                            body = "",
                            severity = "info"
                        });

                    if (consultationToDelete.Booked != 0)
                        return Json(new
                        {
                            success = false,
                            title = "Nije dozvoljeno brisanje zakazanih termina konsultacija",
                            body = "",
                            severity = "info"
                        });

                    _context.Consultations.Delete(consultationToDelete);
                    await _context.SaveAsync();

                    return Json(new
                    {
                        success = true,
                        title = "Termin konsultacije uspešno obrisan",
                        body = "",
                        severity = "success"
                    });
                }
                else if (completionStatus == TherapistAccountCompletionStatus.NotSetUpYet)
                    return Json(new
                    {
                        success = false,
                        location = Url.Action("AccountSetup", "Account", new { Area = "Therapist" })
                    });
                else throw new GeneralException("Unable to load user.", signOutUser: true);
            }
            catch(Exception e)
            {
                return await HandleErrorJsonAsync(e);
            }
        }

        [HttpGet("/terapeut/besplatne-konsultacije/zakazane-besplatne-konsultacije/detalji/{bookingId}")]
        public async Task<IActionResult> Details(string bookingId)
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                {
                    if (string.IsNullOrWhiteSpace(bookingId)) return RedirectToAction("BookedConsultations");

                    ShowToastOnThisPageIfSet();

                    var bookedConsultationInviteLink = await _consultationsFunctions.GetBookedConsultationDetailsForTherapistAsync((await _userManager.GetUserAsync(User)).TherapistAccountId, bookingId);

                    if (bookedConsultationInviteLink != default)
                        return View(bookedConsultationInviteLink);
                    else
                    {
                        _session.SetToast("Konsultacija nije pronađena", null, "info");
                        return RedirectToAction("BookedConsultations");
                    }
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

        [HttpPost("/terapeut/besplatne-konsultacije/zakazane-besplatne-konsultacije/detalji/dodavanje-linka-za-pristup")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> AddConsultationInviteLink([FromForm] BookedConsultationAddInviteLinkViewModel details)
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                {
                    if (!ModelState.IsValid)
                    {
                        return Json(new
                        {
                            success = false,
                            title = "Popunite sva obavezna polja",
                            body = "",
                            severity = "info"
                        });
                    }

                    var user = await _userManager.GetUserAsync(User);

                    BookedConsultations bookedConsultation;
                    ContactMethods contactMethod;

                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        try
                        {
                            bookedConsultation = _context.BookedConsultations.ReadOnlyFind(c => c.Id == details.BookedConsultation.BookingId && c.ConsultationId == details.BookedConsultation.ConsultationId && c.TherapistId == user.TherapistAccountId).SingleOrDefault();
                            contactMethod = _context.ContactMethods.ReadOnlyFind(cm => cm.Id == bookedConsultation.ContactMethodId).SingleOrDefault();
                            if (bookedConsultation == default || contactMethod == default)
                            {
                                await HandleErrorJsonAsync($"BookedConsultation '{(bookedConsultation != null ? bookedConsultation.Id : "'null'")}' or ContactMethod '{(contactMethod != null ? contactMethod.Id : "'null'")}' does not exist.");

                                return Json(new
                                {
                                    success = false,
                                    title = "Došlo je do greške",
                                    body = "Molimo osvežite stranicu i pokušajte ponovo ili kontaktirajte podršku za terapeute.",
                                    severity = "error"
                                });
                            }

                            if (await _context.Sessions.ReadOnlyAnyAsync(s => s.Id == bookedConsultation.ConsultationId && s.Booked != 1))
                                return Json(new
                                {
                                    success = false,
                                    title = "Došlo je do greške",
                                    body = "Molimo osvežite stranicu i pokušajte ponovo ili kontaktirajte podršku za terapeute.",
                                    severity = "error"
                                });

                            if (bookedConsultation.EndDateTime <= DateTime.UtcNow)
                                return Json(new
                                {
                                    success = false,
                                    title = "Link za pristup ne može biti dodat",
                                    body = "Ove konsultacije su završene.",
                                    severity = "info"
                                });

                            if (bookedConsultation.ContactMethodId == ContactMethodsProvider.InPerson.Id)
                                return Json(new
                                {
                                    success = false,
                                    title = "Link za pristup ne može biti dodat",
                                    body = "Link za pristup se može dodati samo konsultacijama koje se održavaju online. Ove konsultacije se održavaju lično.",
                                    severity = "info"
                                });

                            var bccm = _context.BookedConsultationsContactMethods.Find(i => i.BookedConsultationId == bookedConsultation.Id && i.ContactMethodId == contactMethod.Id).SingleOrDefault();
                            if(bccm != null)
                            {
                                bccm.InviteLink = details.InviteLink.Trim();
                                bccm.LinkAddedDateTime = DateTime.UtcNow;

                                _context.BookedConsultationsContactMethods.Update(bccm);
                            }
                            else
                            {
                                _context.BookedConsultationsContactMethods.Insert(new BookedConsultationsContactMethods
                                {
                                    BookedConsultationId = bookedConsultation.Id,
                                    ContactMethodId = contactMethod.Id,
                                    InviteLink = details.InviteLink.Trim(),
                                    LinkAddedDateTime = DateTime.UtcNow
                                });
                            }

                            await _context.SaveAsync();
                            scope.Complete();
                        }
                        catch(Exception)
                        {
                            scope.Dispose();
                            throw;
                        }
                    }

                    // Send notification to user
                    await _notificationRepository.SendAsync(new Notifications
                    {
                        Id = Helper.GenerateNumbersId(),
                        SenderUserId = user.Id,
                        ReceiverUserId = bookedConsultation.ClientId,
                        Title = $"Vaše konsultacije sa psihoterapeutom {bookedConsultation.TherapistFirstName} {bookedConsultation.TherapistLastName} " +
                            $"se održavaju na {bookedConsultation.ContactMethodName}-u, datuma {_dateHelper.ConvertDateTimeFromUtcToLocalDateString(bookedConsultation.StartDateTime)}, " +
                            $"u {_dateHelper.ConvertDateTimeFromUtcToLocalTimeString(bookedConsultation.StartDateTime)}h. {bookedConsultation.ContactMethodName} link za " +
                            $"pristup konsultacijama je poslat na {bookedConsultation.ClientEmail}. Ako ne vidite imejl u inboksu, proverite spam ili kontaktirajte korisničku podršku.",
                        Body = null,
                        Severity = "info",
                        Read = false,
                        SendingDateTime = DateTime.UtcNow,
                        Icon = "far fa-link",
                        Important = true
                    });

                    // Send notification to therapist
                    await _notificationRepository.SendAsync(new Notifications
                    {
                        Id = Helper.GenerateNumbersId(),
                        SenderUserId = user.Id,
                        ReceiverUserId = user.Id,
                        Title = $"{bookedConsultation.ContactMethodName} link za pristup je uspešno poslat na {bookedConsultation.ClientEmail}.",
                        Body = null,
                        Severity = "primary",
                        Read = false,
                        SendingDateTime = DateTime.UtcNow,
                        Icon = "fal fa-paper-plane",
                        Important = true
                    });

                    await _mailService.SendConsultationInviteLinkEmailAsync(new Framework.Emails.EmailTypes.ConsultationInviteLinkEmail
                    {
                        BookingId = bookedConsultation.Id,
                        ClientId = bookedConsultation.ClientId,
                        ClientEmail = bookedConsultation.ClientEmail,
                        ClientFirstName = bookedConsultation.ClientFirstName,
                        TherapistFirstName = bookedConsultation.TherapistFirstName,
                        TherapistLastName = bookedConsultation.TherapistLastName,
                        TherapistEmail = bookedConsultation.TherapistEmail,
                        TherapistPhoneNumber = bookedConsultation.TherapistPhoneNumber,
                        StartDateTime = bookedConsultation.StartDateTime,
                        ContactMethodName = bookedConsultation.ContactMethodName,
                        InviteLink = details.InviteLink
                    }, includeTemplateIfExists: true);

                    return Json(new
                    {
                        success = true,
                        title = $"Link za pristup uspešno poslat",
                        body = $"{bookedConsultation.ContactMethodName} link za pristup je uspešno poslat na {bookedConsultation.ClientEmail}.",
                        severity = "success"
                    });
                }
                else if(completionStatus == TherapistAccountCompletionStatus.NotSetUpYet)
                    return Json(new
                    {
                        success = false,
                        redirectUrl = Url.Action("AccountSetup", "Account", new { Area = "Therapist" })
                    });
                else throw new GeneralException("Unable to load user.", signOutUser: true);
            }
            catch(Exception e)
            {
                return await HandleErrorJsonAsync(e);
            }
        }
    }
}
