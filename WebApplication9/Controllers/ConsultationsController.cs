using Database.Models;
using Framework.Emails;
using Framework.Helpers;
using Framework.Helpers.ExtensionMethods;
using Framework.Implementations;
using Framework.Interfaces;
using Framework.Models;
using Framework.Notifications;
using Framework.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication9.Base;
using WebApplication9.Implementations;
using WebApplication9.Interfaces;
using WebApplication9.ViewModels;

namespace WebApplication9.Controllers
{
    [Authorize(Roles = "Client")]
    public class ConsultationsController : BaseController
    {
        private const int DAYS_BOOKED_IN_ADVANCE = 1;
        private readonly IFileRepository _files;
        private readonly ISessionsFunctionsProvider _sessionsFunctions;
        private readonly ITherapistFunctionsProvider _therapistFunctions;
        private readonly ICustomClientFunctionsProvider _clientFunctions;
        private readonly IConsultationsFunctionsProvider _consultationsFunctions;

        public ConsultationsController(IErrorLogger error,
            IMailService mailService,
            IDateTimeHelper dateHelper,
            IHttpContextAccessor contextAccessor,
            INotificationRepository notificationRepository,
            UserManager<CustomClient> userManager,
            SignInManager<CustomClient> signInManager) : base(error, mailService, dateHelper, contextAccessor, notificationRepository, userManager, signInManager)
        {
            _files = new FileRepository();
            _sessionsFunctions = new SessionsFunctionsProvider();
            _therapistFunctions = new TherapistFunctionsProvider();
            _clientFunctions = new CustomClientFunctionsProvider(contextAccessor);
            _consultationsFunctions = new ConsultationsFunctionsProvider(dateHelper);
        }

        [AllowAnonymous]
        [HttpGet("/besplatne-psihoterapijske-konsultacije")]
        public async Task<IActionResult> Index()
        {
            try
            {
                ShowToastOnThisPageIfSet();
                return View(_therapistFunctions.GetTherapistsWithUpcomingConsultations());
            }
            catch(Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpGet("/zakazane-besplatne-konsultacije/detalji/{bookingId}")]
        public async Task<IActionResult> ConsultationDetails(string bookingId)
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
                {
                    if (string.IsNullOrWhiteSpace(bookingId)) return RedirectToAction("Index", "Consultations");

                    ShowToastOnThisPageIfSet();

                    var bookedConsultationInviteLinkVm = await _consultationsFunctions.GetBookedConsultationDetailsForClientAsync((await _userManager.GetUserAsync(User)).Id, bookingId);

                    if (bookedConsultationInviteLinkVm != default)
                        return View(bookedConsultationInviteLinkVm);
                    else
                    {
                        _session.SetToast("Konsultacije nisu pronađene", null, "info");
                        return RedirectToAction("Index", "Consultations");
                    }
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

        [AllowAnonymous]
        [HttpPost("/preuzmi-besplatne-psihoterapijske-konsultacije")]
        public async Task<JsonResult> GetConsultations(string therapistId)
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
                {
                    if (string.IsNullOrWhiteSpace(therapistId))
                        return Json(new
                        {
                            success = false,
                            title = "Došlo je do greške",
                            body = "Molimo osvežite stranicu i pokušajte ponovo ili kontaktirajte korisničku podršku.",
                            severity = "error"
                        });

                    var partialViewNameBasedOnVisitor = "_BookConsultationModalForAnonymousVisitor";
                    if (User.Identity.IsAuthenticated)
                    {
                        if (User.IsInRole(UserRoles.Client)) partialViewNameBasedOnVisitor = "_BookConsultationModalForClientVisitor";
                        else partialViewNameBasedOnVisitor = "_BookConsultationModalForAuthenticatedNonClientVisitor";
                    }

                    return Json(new
                    {
                        success = true,
                        partialView = await RenderPartialViewToStringAsync(partialViewNameBasedOnVisitor, _consultationsFunctions.GetTherapistConsultationsGroupedByDate(therapistId))
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

        [HttpPost("/zakaži-besplatne-psihoterapijske-konsultacije")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> BookConsultation(string therapistId, string consultationId)
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
                {
                    if (string.IsNullOrWhiteSpace(therapistId) || string.IsNullOrWhiteSpace(consultationId))
                        return Json(new
                        {
                            success = false,
                            title = "Došlo je do greške",
                            body = "Molimo osvežite stranicu i pokušajte ponovo ili kontaktirajte korisničku podršku.",
                            severity = "error"
                        });

                    var user = await _userManager.GetUserAsync(User);

                    var therapistCC = await _userManager.FindByTherapistAccountIdAsync(therapistId);
                    var therapist = _therapistFunctions.GetTherapistWithSetUpAccount(therapistCC.TherapistAccountId);

                    // Check if therapist with Id exists and has setup account
                    if (therapistCC == null || therapist == null)
                    {
                        _session.SetToast("Psihoterapeut nije pronađen", "Molimo izaberite drugog terapeuta i pokušajte ponovo.", "info");
                        return Json(new
                        {
                            success = false,
                            redirectUrl = Url.Action("All", "Therapists", new { Area = "" })
                        });
                    }

                    // Check if user has applied for getting therapist account
                    if (_userManager.HasPendingApplicationForTherapistAccount(user.Id))
                        return Json(new
                        {
                            success = false,
                            title = "Ne možete zakazati besplatne konsultacije dok ste u toku procesa apliciranja za nalog terapeuta",
                            body = "Ulogujte se drugim nalogom i pokušajte ponovo.",
                            severity = "info"
                        });

                    // Check if user has already booked a consultation with specific psychotherapist
                    if (_consultationsFunctions.UserAlreadyBookedConsultationWithTherapist(user.Id, therapistId))
                    {
                        if (await _therapistFunctions.HasAnyUpcomingSessionAsync(therapistId))
                            return Json(new
                            {
                                success = false,
                                consultationAlreadyBooked = true,
                                therapistProfileUrl = Url.PsychotherapistPublicProfileUrlWithName(therapistId, therapistCC.FirstName, therapistCC.LastName),
                                title = $"Već ste zakazali besplatne konsultacije sa terapeutom {therapistCC.FirstName} {therapistCC.LastName}",
                                body = $"Za dalje termine sa psihoterapeutom {therapistCC.FirstName} {therapistCC.LastName}, možete zakazati seansu.",
                                severity = "info"
                            });
                        else return Json(new
                        {
                            success = false,
                            title = "Možete zakazati samo jedan termin besplatnih konsultcija sa svakim terapeutom",
                            body = "Izaberite drugog terapeuta i pokušajte ponovo.",
                            severity = "info"
                        });
                    }
                        

                    // Check if session exists
                    // Check if session belongs to therapist with whom we are booking
                    var consultationToBook = _context.Consultations.Find(s => s.Id == consultationId &&
                                                                    s.TherapistId == therapist.Id &&
                                                                    s.Booked == 0).SingleOrDefault();

                    if (consultationToBook == default || _context.BookedConsultations.ReadOnlyAny(s => s.ConsultationId == consultationToBook.Id))
                        return Json(new
                        {
                            success = false,
                            title = "Ovaj termin besplatnih konsultacija je već zakazan",
                            body = "Molimo izaberite drugi termin za zakazivanje.",
                            severity = "info"
                        });

                    // Check if consultation is overlapping with session
                    if (_consultationsFunctions.UserHasConsultationToAttendDuringPeriod(user.Id, consultationToBook.StartDateTime, consultationToBook.EndDateTime))
                        return Json(new
                        {
                            success = false,
                            title = "Termini se preklapaju",
                            body = "Već imate zakazan termin besplatnih konsultacija u ovom vremenskom rasponu. " +
                            "Molimo izaberite termin sa drugim vremenom početka.",
                            severity = "info"
                        });

                    // Check if sessions are overlapping
                    if (_sessionsFunctions.UserHasSessionToAttendDuringPeriod(user.Id, consultationToBook.StartDateTime, consultationToBook.EndDateTime))
                        return Json(new
                        {
                            success = false,
                            title = "Termini se preklapaju",
                            body = "Već imate zakazanu seansu u ovom vremenskom rasponu. " +
                            "Molimo izaberite termin sa drugim vremenom početka.",
                            severity = "info"
                        });

                    // Check if consultation is being booked less than 1 day in advance
                    if (consultationToBook.StartDateTime < DateTime.UtcNow.AddDays(DAYS_BOOKED_IN_ADVANCE))
                        return Json(new
                        {
                            success = false,
                            title = $"Konsultacije moraju biti zakazane bar {24 * DAYS_BOOKED_IN_ADVANCE} časa pre vremena početka",
                            body = "Molimo izaberite termin koji najranije počinje za 24 časa.",
                            severity = "info"
                        });

                    var bookingId = Helper.GenerateNumbersId();

                    var googleMeet = ContactMethodsProvider.GoogleMeet;

                    var bookedConsultation = new BookedConsultations
                    {
                        Id = bookingId,
                        ConsultationId = consultationId,
                        TherapistId = therapist.Id,
                        TherapistFirstName = therapistCC.FirstName,
                        TherapistLastName = therapistCC.LastName,
                        TherapistEmail = therapistCC.Email,
                        TherapistPhoneNumber = therapistCC.PhoneNumber,
                        ClientId = user.Id,
                        ClientFirstName = user.FirstName,
                        ClientLastName = user.LastName,
                        ClientEmail = user.Email,
                        ClientPhoneNumber = user.PhoneNumber,
                        StartDateTime = consultationToBook.StartDateTime,
                        EndDateTime = consultationToBook.EndDateTime,
                        BookingDate = DateTime.UtcNow,
                        ContactMethodId = googleMeet.Id,
                        ContactMethodName = googleMeet.Name,
                        ContactMethodColor = googleMeet.Color,
                        ContactMethodIcon = googleMeet.Icon
                    };

                    _context.BookedConsultations.Insert(bookedConsultation);

                    _context.BookedConsultationsContactMethods.Insert(new BookedConsultationsContactMethods
                    {
                        BookedConsultationId = bookingId,
                        ContactMethodId = googleMeet.Id
                    });

                    consultationToBook.Booked = 1;
                    _context.Consultations.Update(consultationToBook);
                    await _context.SaveAsync();

                    // Send email to user
                    await _mailService.SendClientBookedConsultationEmailAsync(new Framework.Emails.EmailTypes.BookedConsultationEmail
                    {
                        BookedConsultation = bookedConsultation
                    }, includeTemplateIfExists: true);

                    // Send email to therapist
                    await _mailService.SendTherapistConsultationBookedEmailAsync(new Framework.Emails.EmailTypes.BookedConsultationEmail
                    {
                        BookedConsultation = bookedConsultation
                    }, includeTemplateIfExists: true);

                    var consultationStart = _dateHelper.ConvertDateTimeFromUtcToLocal(consultationToBook.StartDateTime);

                    // Send notification to user
                    await _notificationRepository.SendAsync(new Notifications
                    {
                        Id = Helper.GenerateNumbersId(),
                        SenderUserId = SystemInformation.Name,
                        ReceiverUserId = user.Id,
                        Title = $"Uspešno ste zakazali besplatne konsultacije sa psihoterapeutom {therapistCC.FirstName} {therapistCC.LastName}, kontakt metoda je {googleMeet.Name}. " +
                        $"Konsultacije počinju datuma {_dateHelper.DateStringFromDateTime(consultationStart)}, u {_dateHelper.TimeStringFromDateTime(consultationStart)}h. " +
                        "Za više detalja, proverite imejl koji Vam je upravo stigao u inboks ili spam.",
                        Body = null,
                        Severity = "primary",
                        Read = false,
                        SendingDateTime = DateTime.UtcNow,
                        Icon = "fal fa-comments-alt",
                        Important = false
                    });

                    // Send notification to therapist
                    await _notificationRepository.SendAsync(new Notifications
                    {
                        Id = Helper.GenerateNumbersId(),
                        SenderUserId = SystemInformation.Name,
                        ReceiverUserId = therapistCC.Id,
                        Title = $"Klijent {user.FirstName} {user.LastName} je zakazao besplatne konsultacije sa Vama. Kontakt metoda je {googleMeet.Name}. " +
                        $"Konsultacije počinju datuma {_dateHelper.DateStringFromDateTime(consultationStart)}, u {_dateHelper.TimeStringFromDateTime(consultationStart)}h. " +
                        "Za više detalja, proverite imejl koji Vam je upravo stigao u inboks ili spam.",
                        Body = null,
                        Severity = "primary",
                        Read = false,
                        SendingDateTime = DateTime.UtcNow,
                        Icon = "fal fa-comments-alt",
                        Important = false
                    });

                    return Json(new
                    {
                        success = true,
                        title = $"Uspešno ste zakazali besplatne konsultacije za datum {_dateHelper.DateTimeStringFromDateTime(consultationStart)}h",
                        body = $"Uputstvo za pristup konsultacijama je poslato na {user.Email}.",
                        severity = "success"
                    });
                }
                else if (emailStatus == EmailConfirmationStatus.NotConfirmed)
                {
                    return Json(new { });
                }
                else throw new GeneralException("Unable to load user.");
            }
            catch(Exception e)
            {
                return await HandleErrorJsonAsync(e);
            }
        }

        [AllowAnonymous]
        [HttpPost("/zatraži-besplatne-psihoterapijske-konsultacije")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> BookConsultationRequested()
        {
            if (!User.Identity.IsAuthenticated)
                _session.SetString("BookConsultationRequested", "true");

            return Json(new
            {
                success = true
            });
        }
    }
}
