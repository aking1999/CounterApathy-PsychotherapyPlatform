using Database.Models;
using Framework.Helpers.ExtensionMethods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication9.Areas.Therapist.ViewModels;
using System;
using System.Globalization;
using WebApplication9.Base;
using Framework.Notifications;
using Framework.Emails;
using WebApplication9.Interfaces;
using WebApplication9.Implementations;
using Framework.Interfaces;
using Framework.Models;
using Microsoft.AspNetCore.Http;
using WebApplication9.ViewModels;
using DataTransferObjects.ViewModels.Therapist;
using Framework.Helpers;
using Microsoft.Extensions.Configuration;
using Framework.Providers;
using System.Transactions;
using Framework.Emails.EmailTypes;

namespace WebApplication9.Areas.Therapist.Controllers
{
    [Area(areaName: "Therapist")]
    [Authorize(Roles = "Therapist")]
    public class SessionsController : BaseController
    {
        public const int MIN_HOURS_SESSION_DURATION = 1;
        public const int MIN_DAYS_ADD_SESSION_IN_ADVANCE = 1;
        private readonly IDropdownHelper _dropdown;
        private readonly IWebHostEnvironment _environment;
        private readonly IRatingFunctionsProvider _ratingFunctions;
        private readonly ISessionsFunctionsProvider _sessionsFunctions;
        private readonly ITherapistFunctionsProvider _therapistFunctions;
        private readonly IConsultationsFunctionsProvider _consultationsFunctions;
        private readonly TherapistFee _therapistFee;

        public SessionsController(IConfiguration configuration,
            IErrorLogger error,
            IMailService mailService,
            IDateTimeHelper dateHelper,
            IWebHostEnvironment environment,
            IHttpContextAccessor contextAccessor,
            INotificationRepository notificationRepository,
            UserManager<CustomClient> userManager,
            SignInManager<CustomClient> signInManager) : base(error, mailService, dateHelper, contextAccessor, notificationRepository, userManager, signInManager)
        {
            _environment = environment;
            _dropdown = new DropdownHelper();
            _ratingFunctions = new RatingFunctionsProvider();
            _sessionsFunctions = new SessionsFunctionsProvider();
            _therapistFunctions = new TherapistFunctionsProvider();
            _consultationsFunctions = new ConsultationsFunctionsProvider(dateHelper);
            _therapistFee = configuration.GetSection("TherapistFee").Get<TherapistFee>();
        }

        [HttpGet("/terapeut/seanse/zakazane-seanse/{filter?}/{predicate?}")]
        public async Task<IActionResult> BookedSessions(string filter = null, string predicate = null)
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                {
                    ShowToastOnThisPageIfSet();
                    return View(_sessionsFunctions.GetTherapistBookedSessions((await _userManager.GetUserAsync(User)).TherapistAccountId, filter, predicate));
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

        [HttpGet("/terapeut/seanse")]
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

        [HttpPost("/terapeut/seanse")]
        public async Task<IActionResult> GetSessions()
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                    return Json(new
                    {
                        sessions = _therapistFunctions.GetUpcomingSessionsViewModels((await _userManager.GetUserAsync(User)).TherapistAccountId)
                    });
                else if (completionStatus == TherapistAccountCompletionStatus.Error)
                    await HandleErrorJsonAsync("Unable to load user.");
            }
            catch (Exception e)
            {
                await HandleErrorJsonAsync(e);
            }

            return Json(new
            {
                sessions = new List<SessionViewModel>()
            });
        }

        [HttpPost("/terapeut/seanse/dodavanje-seanse-preko-polja")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSessionTileClicked([FromForm] AddSessionViewModel addVm)
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                {
                    var user = await _userManager.GetUserAsync(User);

                    if (_context.TherapistSupportTickets.ReadOnlyAny(t => t.TherapistId == user.TherapistAccountId &&
                                                                          t.TopicId == TherapistSupportTicketTopicsProvider.RequestAccountDeletion.Id))
                        return Json(new
                        {
                            success = false,
                            title = "Podneli ste zahtev za gašenje naloga",
                            body = "Nije moguće dodavanje seansi nakon podnošenja zahteva za gašenje naloga.",
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

                    var model = addVm.TileClicked;

                    if (!DateTime.TryParseExact(model.StartEndDate + " " + model.StartTime, "dd/MMM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime sessionStart) ||
                        !DateTime.TryParseExact(model.StartEndDate + " " + model.EndTime, "dd/MMM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime sessionEnd))
                        return Json(new
                        {
                            success = false,
                            title = "Netačan datum",
                            body = "Molimo osvežite stranicu i pokušajte ponovo ili kontaktirajte podršku za terapeute.",
                            severity = "info"
                        });

                    var sessionStartUtc = _dateHelper.ConvertDateTimeFromLocalToUtc(sessionStart);
                    var sessionEndUtc = _dateHelper.ConvertDateTimeFromLocalToUtc(sessionEnd);

                    if (sessionStartUtc.Date < DateTime.UtcNow.Date.AddDays(MIN_DAYS_ADD_SESSION_IN_ADVANCE))
                        return Json(new
                        {
                            success = false,
                            title = $"Datum početka mora biti za bar {MIN_DAYS_ADD_SESSION_IN_ADVANCE * 24} časa od sada",
                            body = $"Izaberite datum koji je za bar {MIN_DAYS_ADD_SESSION_IN_ADVANCE * 24} časa ispred trenutnog datuma.",
                            severity = "info"
                        });

                    if (sessionStartUtc.AddHours(MIN_HOURS_SESSION_DURATION) > sessionEndUtc)
                        return Json(new
                        {
                            success = false,
                            title = $"Seansa mora trajati bar {MIN_HOURS_SESSION_DURATION} sat(a)",
                            body = $"Izaberite vreme početka i završetka seanse tako da seansa traje bar {MIN_HOURS_SESSION_DURATION} sat(a).",
                            severity = "info"
                        });

                    if (_consultationsFunctions.TherapistHasConsultationDuringPeriod(user.TherapistAccountId, sessionStartUtc, sessionEndUtc))
                        return Json(new
                        {
                            success = false,
                            title = "Vreme ove seanse se preklapa sa vremenom dodatih konsultacija",
                            body = "Već ste dodali jedan termin za besplatne konsultacije koji se završava nakon vremena početka ove seanse. " +
                            "Promenite vreme početka ove seanse.",
                            severity = "info"
                        });

                    if (_sessionsFunctions.TherapistHasSessionDuringPeriod(user.TherapistAccountId, sessionStartUtc, sessionEndUtc))
                        return Json(new
                        {
                            success = false,
                            title = "Vremena seansi se preklapaju",
                            body = "Već ste dodali jednu seansu koja se završava nakon vremena početka ove seanse. " +
                            "Promenite vreme početka ove seanse.",
                            severity = "info"
                        });

                    if (!_dropdown.SessionTypeExists(model.Type.Value))
                        return Json(new
                        {
                            success = false,
                            title = "Izabrali ste nepostojeći tip seanse",
                            body = "Seansa može biti samo individualna ili grupa.",
                            severity = "info"
                        });

                    var session = new Sessions
                    {
                        Id = Helper.GenerateNumbersId(),
                        TherapistId = user.TherapistAccountId,
                        Subject = "Session",
                        Price = model.Price.Value,
                        Type = model.Type.Value,
                        StartDateTime = sessionStartUtc,
                        EndDateTime = sessionEndUtc,
                        Color = null,
                        Icon = null,
                        Booked = 0
                    };

                    _context.Sessions.Insert(session);
                    await _context.SaveAsync();

                    var sessionPriceAfterFees = session.Price * (1 - (_therapistFee.PercentageAmount / 100));
                    var feeAmount = session.Price - sessionPriceAfterFees;

                    return Json(new
                    {
                        success = true,
                        title = "Seansa uspešno dodata",
                        review = "Proverite podatke o seansi",
                        date = _dateHelper.DateStringFromDateTime(sessionStart),
                        startTime = _dateHelper.TimeStringFromDateTime(sessionStart),
                        endTime = _dateHelper.TimeStringFromDateTime(sessionEnd),
                        duration = (session.EndDateTime - session.StartDateTime).ToString(),
                        type = model.Type.Value == 0 ? "Individualna seansa" : "Grupna seansa",
                        price = $"{session.Price} {_therapistFee.CurrencyCode} ({sessionPriceAfterFees} {_therapistFee.CurrencyCode} + {feeAmount} {_therapistFee.CurrencyCode} provizija)",
                        severity = "success",
                        session = new SessionViewModel
                        {
                            SessionId = session.Id,
                            StartDateTime = session.StartDateTime,
                            EndDateTime = session.EndDateTime,
                            Type = session.Type,
                            Price = session.Price
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
            catch (Exception e)
            {
                return await HandleErrorJsonAsync(e);
            }
        }

        [HttpPost("/terapeut/seanse/dodavanje-seanse-preko-dugmeta")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSessionButtonClicked([FromForm] AddSessionViewModel addVm)
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                {
                    var user = await _userManager.GetUserAsync(User);

                    if (_context.TherapistSupportTickets.ReadOnlyAny(t => t.TherapistId == user.TherapistAccountId &&
                                                                          t.TopicId == TherapistSupportTicketTopicsProvider.RequestAccountDeletion.Id))
                        return Json(new
                        {
                            success = false,
                            title = "Podneli ste zahtev za gašenje naloga",
                            body = "Nije moguće dodavanje seansi nakon podnošenja zahteva za gašenje naloga.",
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

                    var model = addVm.ButtonClicked;

                    if (!DateTime.TryParseExact(model.StartEndDate + " " + model.StartTime, "dd/MMM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime sessionStart) ||
                        !DateTime.TryParseExact(model.StartEndDate + " " + model.EndTime, "dd/MMM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime sessionEnd))
                        return Json(new
                        {
                            success = false,
                            title = "Netačan datum",
                            body = "Molimo osvežite stranicu i pokušajte ponovo ili kontaktirajte podršku za terapeute.",
                            severity = "info"
                        });

                    var sessionStartUtc = _dateHelper.ConvertDateTimeFromLocalToUtc(sessionStart);
                    var sessionEndUtc = _dateHelper.ConvertDateTimeFromLocalToUtc(sessionEnd);

                    if (sessionStartUtc.Date < DateTime.UtcNow.Date.AddDays(1))
                        return Json(new
                        {
                            success = false,
                            title = $"Datum početka mora biti za bar {MIN_DAYS_ADD_SESSION_IN_ADVANCE * 24} časa od sada",
                            body = $"Izaberite datum koji je za bar {MIN_DAYS_ADD_SESSION_IN_ADVANCE * 24} časa ispred trenutnog datuma.",
                            severity = "info"
                        });

                    if (sessionStartUtc.AddHours(1) > sessionEndUtc)
                        return Json(new
                        {
                            success = false,
                            title = $"Seansa mora trajati bar {MIN_HOURS_SESSION_DURATION} sat(a)",
                            body = $"Izaberite vreme početka i završetka seanse tako da seansa traje bar {MIN_HOURS_SESSION_DURATION} sat(a).",
                            severity = "info"
                        });

                    if (_consultationsFunctions.TherapistHasConsultationDuringPeriod(user.TherapistAccountId, sessionStartUtc, sessionEndUtc))
                        return Json(new
                        {
                            success = false,
                            title = "Vreme ove seanse se preklapa sa vremenom dodatih konsultacija",
                            body = "Već ste dodali jedan termin za besplatne konsultacije koji se završava nakon vremena početka ove seanse. " +
                            "Promenite vreme početka ove seanse.",
                            severity = "info"
                        });

                    if (_sessionsFunctions.TherapistHasSessionDuringPeriod(user.TherapistAccountId, sessionStartUtc, sessionEndUtc))
                        return Json(new
                        {
                            success = false,
                            title = "Vremena seansi se preklapaju",
                            body = "Već ste dodali jednu seansu koja se završava nakon vremena početka ove seanse. " +
                            "Promenite vreme početka ove seanse.",
                            severity = "info"
                        });

                    if (!_dropdown.SessionTypeExists(model.Type.Value))
                        return Json(new
                        {
                            success = false,
                            title = "Izabrali ste nepostojeći tip seanse",
                            body = "Seansa može biti samo individualna ili grupa.",
                            severity = "info"
                        });

                    var session = new Sessions
                    {
                        Id = Helper.GenerateNumbersId(),
                        TherapistId = user.TherapistAccountId,
                        Subject = "Session",
                        Price = model.Price.Value,
                        Type = model.Type.Value,
                        StartDateTime = sessionStartUtc,
                        EndDateTime = sessionEndUtc,
                        Color = null,
                        Icon = null,
                        Booked = 0
                    };

                    _context.Sessions.Insert(session);
                    await _context.SaveAsync();

                    var sessionPriceAfterFees = session.Price * (1 - (_therapistFee.PercentageAmount / 100));
                    var feeAmount = session.Price - sessionPriceAfterFees;

                    return Json(new
                    {
                        success = true,
                        title = "Seansa uspešno dodata",
                        review = "Proverite podatke o seansi",
                        date = _dateHelper.DateStringFromDateTime(sessionStart),
                        startTime = _dateHelper.TimeStringFromDateTime(sessionStart),
                        endTime = _dateHelper.TimeStringFromDateTime(sessionEnd),
                        duration = (session.EndDateTime - session.StartDateTime).ToString(),
                        type = model.Type.Value == 0 ? "Individualna seansa" : "Grupna seansa",
                        price = $"{session.Price} {_therapistFee.CurrencyCode} ({sessionPriceAfterFees} {_therapistFee.CurrencyCode} + {feeAmount} {_therapistFee.CurrencyCode} provizija)",
                        severity = "success",
                        session = new SessionViewModel
                        {
                            SessionId = session.Id,
                            StartDateTime = session.StartDateTime,
                            EndDateTime = session.EndDateTime,
                            Type = session.Type,
                            Price = session.Price
                        }
                    });
                }
                else if (completionStatus == TherapistAccountCompletionStatus.NotSetUpYet)
                {
                    return Json(new
                    {
                        success = false,
                        redirectUrl = Url.Action("AccountSetup", "Account", new { Area = "" })
                    });
                }
                else throw new GeneralException("Unable to load user.", signOutUser: true);
            }
            catch (Exception e)
            {
                return await HandleErrorJsonAsync(e);
            }
        }

        [HttpPost("/terapeut/seanse/brisanje-seanse")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSession(string sessionId)
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                {
                    var user = await _userManager.GetUserAsync(User);

                    var sessionToDelete = _context.Sessions.Find(s => s.Id == sessionId &&
                                                                      s.TherapistId == user.TherapistAccountId)
                                                           .SingleOrDefault();

                    if (sessionToDelete == default)
                        return Json(new
                        {
                            success = false,
                            title = "Seansa nije pronađena",
                            body = "",
                            severity = "info"
                        });

                    if (sessionToDelete.Booked != 0)
                        return Json(new
                        {
                            success = false,
                            title = "Nije dozvoljeno brisanje zakazanih seansi",
                            body = "",
                            severity = "info"
                        });

                    _context.Sessions.Delete(sessionToDelete);
                    await _context.SaveAsync();

                    return Json(new
                    {
                        success = true,
                        title = "Seansa uspešno obrisana",
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
            catch (Exception e)
            {
                return await HandleErrorJsonAsync(e);
            }
        }

        [HttpGet("/terapeut/seanse/zakazane-seanse/detalji/{bookingId}")]
        public async Task<IActionResult> Details(string bookingId)
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                {
                    if (string.IsNullOrWhiteSpace(bookingId)) return RedirectToAction("BookedSessions");

                    ShowToastOnThisPageIfSet();

                    var bookedSessionAddInviteLink = await _ratingFunctions.MapToApprovedDetailsRatingAsync((await _userManager.GetUserAsync(User)).TherapistAccountId, bookingId);

                    if (bookedSessionAddInviteLink != default)
                        return View(bookedSessionAddInviteLink);
                    else
                    {
                        _session.SetToast("Seansa nije pronađena", null, "info");
                        return RedirectToAction("BookedSessions");
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

        [HttpPost("/terapeut/seanse/zakazane-seanse/detalji/dodavanje-linka-za-pristup")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> AddSessionInviteLink([FromForm] BookedSessionAddInviteLinkViewModel details)
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

                    BookedSessions bookedSession;
                    ContactMethods contactMethod;

                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        try
                        {
                            bookedSession = _context.BookedSessions.ReadOnlyFind(s => s.Id == details.BookedSession.BookingId && s.SessionId == details.BookedSession.SessionId && s.TherapistId == user.TherapistAccountId).SingleOrDefault();
                            contactMethod = _context.ContactMethods.ReadOnlyFind(cm => cm.Id == bookedSession.ContactMethodId).SingleOrDefault();
                            if (bookedSession == default || contactMethod == default)
                            {
                                await HandleErrorJsonAsync($"BookedSession '{(bookedSession != null ? bookedSession.Id : "null")}' or ContactMethod '{(contactMethod != null ? contactMethod.Id : "null")}' does not exist.");

                                return Json(new
                                {
                                    success = false,
                                    title = "Došlo je do greške",
                                    body = "Molimo osvežite stranicu i pokušajte ponovo ili kontaktirajte podršku za terapeute.",
                                    severity = "error"
                                });
                            }

                            if (await _context.Sessions.ReadOnlyAnyAsync(s => s.Id == bookedSession.SessionId && s.Booked != 1))
                                return Json(new
                                {
                                    success = false,
                                    title = "Došlo je do greške",
                                    body = "Molimo osvežite stranicu i pokušajte ponovo ili kontaktirajte podršku za terapeute.",
                                    severity = "error"
                                });

                            if (bookedSession.EndTime <= DateTime.UtcNow)
                                return Json(new
                                {
                                    success = false,
                                    title = "Link za pristup ne može biti dodat",
                                    body = "Ova seansa je završena.",
                                    severity = "info"
                                });

                            if (bookedSession.ContactMethodId == ContactMethodsProvider.InPerson.Id)
                                return Json(new
                                {
                                    success = false,
                                    title = "Link za pristup ne može biti dodat",
                                    body = "Link za pristup se može dodati samo seansama koje se održavaju online. Ova seansa se održava lično.",
                                    severity = "info"
                                });

                            var bscm = _context.BookedSessionsContactMethods.Find(i => i.BookedSessionId == bookedSession.Id && i.ContactMethodId == contactMethod.Id).SingleOrDefault();
                            if (bscm != null)
                            {
                                bscm.InviteLink = details.InviteLink.Trim();
                                bscm.LinkAddedDateTime = DateTime.UtcNow;

                                _context.BookedSessionsContactMethods.Update(bscm);
                            }
                            else
                            {
                                _context.BookedSessionsContactMethods.Insert(new BookedSessionsContactMethods
                                {
                                    BookedSessionId = bookedSession.Id,
                                    ContactMethodId = contactMethod.Id,
                                    InviteLink = details.InviteLink.Trim(),
                                    LinkAddedDateTime = DateTime.UtcNow
                                });
                            }

                            await _context.SaveAsync();
                            scope.Complete();
                        }
                        catch (Exception)
                        {
                            scope.Dispose();
                            throw;
                        }
                    }

                    // Send notification to user if not anonymous or unauthorized
                    if (!bookedSession.ClientId.IsAnonymousOrUnauthorized())
                        await _notificationRepository.SendAsync(new Notifications
                        {
                            Id = Helper.GenerateNumbersId(),
                            SenderUserId = user.Id,
                            ReceiverUserId = bookedSession.ClientId,
                            Title = $"Vaša seansa sa psihoterapeutom {bookedSession.TherapistFirstName} {bookedSession.TherapistLastName} " +
                                $"se održava na {bookedSession.ContactMethodName}-u, datuma {_dateHelper.ConvertDateTimeFromUtcToLocalDateString(bookedSession.StartTime)}, " +
                                $"u {_dateHelper.ConvertDateTimeFromUtcToLocalTimeString(bookedSession.StartTime)}h. {bookedSession.ContactMethodName} link za " +
                                $"pristup seansi je poslat na {bookedSession.ClientEmail}. Ako ne vidite imejl u inboksu, proverite spam ili kontaktirajte korisničku podršku.",
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
                        Title = $"{bookedSession.ContactMethodName} link za pristup je uspešno poslat na {bookedSession.ClientEmail}.",
                        Body = null,
                        Severity = "primary",
                        Read = false,
                        SendingDateTime = DateTime.UtcNow,
                        Icon = "fal fa-paper-plane",
                        Important = true
                    });

                    // Send email to user email
                    await _mailService.SendSessionInviteLinkEmailAsync(new SessionInviteLinkEmail
                    {
                        BookingId = bookedSession.Id,
                        Type = bookedSession.Type,
                        ClientId = bookedSession.ClientId,
                        ClientEmail = bookedSession.ClientEmail,
                        ClientFirstName = bookedSession.ClientFirstName,
                        TherapistFirstName = bookedSession.TherapistFirstName,
                        TherapistLastName = bookedSession.TherapistLastName,
                        TherapistEmail = bookedSession.TherapistEmail,
                        TherapistPhoneNumber = bookedSession.TherapistPhoneNumber,
                        StartTime = bookedSession.StartTime,
                        ContactMethodName = bookedSession.ContactMethodName,
                        InviteLink = details.InviteLink
                    }, includeTemplateIfExists: true);

                    return Json(new
                    {
                        success = true,
                        title = $"Link za pristup uspešno poslat",
                        body = $"{bookedSession.ContactMethodName} link za pristup je uspešno poslat na {bookedSession.ClientEmail}.",
                        severity = "success"
                    });
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

        [HttpPost("/terapeut/seanse/zakazane-seanse/isplata-održane-seanse")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetPaid([FromForm] string bookingId)
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                {
                    if (string.IsNullOrWhiteSpace(bookingId))
                        return Json(new
                        {
                            success = false,
                            title = "Došlo je do greške",
                            body = "Molimo osvežite stranicu i pokušajte ponovo ili kontaktirajte podršku za terapeute.",
                            severity = "error"
                        });

                    var therapistUser = await _userManager.GetUserAsync(User);
                    var therapist = _context.Therapists.GetById(therapistUser.TherapistAccountId);

                    var amountBeforeAdding = therapist.Earnings;

                    var sessionToGetPaidFor = (from bookedSession in _context.BookedSessions.Find(s => s.Id == bookingId && s.TherapistId == therapist.Id).ToList()
                                               join session in _context.Sessions.Find(s => s.TherapistId == therapist.Id).ToList()
                                               on bookedSession.SessionId equals session.Id
                                               select new
                                               {
                                                   session,
                                                   bookedSession
                                               }).SingleOrDefault();

                    if (sessionToGetPaidFor == default)
                    {
                        await HandleErrorJsonAsync($"Booked session with ID '{bookingId}' does not exist OR therapist '{therapistUser.TherapistAccountId}' does not have the selected Sessions/BookedSession.");
                        return Json(new
                        {
                            success = false,
                            title = $"Greška prilikom isplate seanse #{bookingId}",
                            body = "Molimo kontaktirajte podršku za terapeute.",
                            severity = "error"
                        });
                    }

                    if (sessionToGetPaidFor.session == null)
                    {
                        await HandleErrorJsonAsync($"No session for bookingId '{bookingId}'.");
                        return Json(new
                        {
                            success = false,
                            title = $"Greška prilikom isplate seanse #{bookingId}",
                            body = "Molimo kontaktirajte podršku za terapeute.",
                            severity = "error"
                        });
                    }

                    if (sessionToGetPaidFor.bookedSession == null)
                    {
                        await HandleErrorJsonAsync($"No booked session for bookingId '{bookingId}'.");
                        return Json(new
                        {
                            success = false,
                            title = $"Greška prilikom isplate seanse #{bookingId}",
                            body = "Molimo kontaktirajte podršku za terapeute.",
                            severity = "error"
                        });
                    }

                    if (sessionToGetPaidFor.session.Booked != 1)
                    {
                        await HandleErrorJsonAsync($"Session '{sessionToGetPaidFor.session.Id}' is not flagged as booked (1). It's flag is {sessionToGetPaidFor.session.Booked}.");
                        return Json(new
                        {
                            success = false,
                            title = $"Greška prilikom isplate seanse #{bookingId}",
                            body = "Molimo kontaktirajte podršku za terapeute.",
                            severity = "error"
                        });
                    }

                    if (string.IsNullOrWhiteSpace(sessionToGetPaidFor.bookedSession.ClientBookingTransactionId))
                    {
                        await HandleErrorJsonAsync($"Booked session with ID '{bookingId}' does not have ClientBookingTransactionId, " +
                            $"which means that the client with ID '{sessionToGetPaidFor.bookedSession.ClientId}' has not booked the session or the transaction is lost.");
                        return Json(new
                        {
                            success = false,
                            title = $"Greška prilikom isplate seanse #{bookingId}",
                            body = "Molimo kontaktirajte podršku za terapeute.",
                            severity = "error"
                        });
                    }

                    var transactionForClientBookedSession = _context.Transactions.GetById(sessionToGetPaidFor.bookedSession.ClientBookingTransactionId);

                    if (transactionForClientBookedSession == null)
                    {
                        await HandleErrorJsonAsync($"Transaction with ID '{sessionToGetPaidFor.bookedSession.ClientBookingTransactionId}' does not " +
                            $"exist in table Transactions, but it exist in table BookedSessions.");
                        return Json(new
                        {
                            success = false,
                            title = $"Greška prilikom isplate seanse #{bookingId}",
                            body = "Molimo kontaktirajte podršku za terapeute.",
                            severity = "error"
                        });
                    }

                    if (sessionToGetPaidFor.bookedSession.TherapistIsPaid.GetValueOrDefault())
                    {
                        await HandleErrorJsonAsync($"Therapist '{therapist.Id}' is already paid for booked session '{sessionToGetPaidFor.bookedSession.Id}'.");
                        return Json(new
                        {
                            success = false,
                            title = $"Već ste isplaćeni za zakazanu seansu #{bookingId}",
                            body = "Za svaku održanu seansu, možete biti isplaćeni samo jednom.",
                            severity = "error"
                        });
                    }

                    if (!string.IsNullOrWhiteSpace(sessionToGetPaidFor.bookedSession.TherapistPaymentTransactionId))
                    {
                        await HandleErrorJsonAsync($"Therapist '{therapist.Id}' is already paid for booked session '{sessionToGetPaidFor.bookedSession.Id}', " +
                            $"because BookedSession.TherapistPaymentTransactionId is not null and its value is '{sessionToGetPaidFor.bookedSession.TherapistPaymentTransactionId}', " +
                            $"which means the transaction has already happened.");
                        return Json(new
                        {
                            success = false,
                            title = $"Već ste isplaćeni za zakazanu seansu #{bookingId}",
                            body = "Za svaku održanu seansu, možete biti isplaćeni samo jednom.",
                            severity = "error"
                        });
                    }

                    if (!sessionToGetPaidFor.bookedSession.ClientId.IsAnonymousOrUnauthorized())
                    {
                        var webCreditLogs = _context.WebCreditLogs
                                            .ReadOnlyFind(log => log.TransactionId == transactionForClientBookedSession.Id);

                        if (webCreditLogs.Count() > 1)
                        {
                            await HandleErrorJsonAsync($"WebCreditLog with transactionId '{transactionForClientBookedSession.Id}' does not " +
                                $"exist OR there are multiple.");
                            return Json(new
                            {
                                success = false,
                                title = $"Greška prilikom isplate seanse #{bookingId}",
                                body = "Molimo kontaktirajte podršku za terapeute.",
                                severity = "error"
                            });
                        }

                        var webCreditLog = webCreditLogs.SingleOrDefault();

                        if (webCreditLog == default)
                        {
                            await HandleErrorJsonAsync($"WebCreditLog with transactionId '{transactionForClientBookedSession.Id}' does not " +
                                $"exist OR WebCreditLog.UserId does not match Transaction.SenderId '{transactionForClientBookedSession.SenderId}'.");
                            return Json(new
                            {
                                success = false,
                                title = $"Greška prilikom isplate seanse #{bookingId}",
                                body = "Molimo kontaktirajte podršku za terapeute.",
                                severity = "error"
                            });
                        }

                        var webCreditLogAmountAbs = Math.Abs(webCreditLog.Amount);
                        var transactionForClientBookedSessionAmountAbs = Math.Abs(transactionForClientBookedSession.Amount);
                        var sessionToGetPaidForSessionPriceAbs = Math.Abs(sessionToGetPaidFor.session.Price);
                        var sessionToGetPaidForBookedSessionPriceAbs = Math.Abs(sessionToGetPaidFor.bookedSession.Price);
                        if (!AllSame(webCreditLogAmountAbs, transactionForClientBookedSessionAmountAbs,
                            sessionToGetPaidForSessionPriceAbs, sessionToGetPaidForBookedSessionPriceAbs))
                        {
                            await HandleErrorJsonAsync($"These 4 amounts are not all equal: WebCreditLog.Amount: {webCreditLogAmountAbs} -- Transaction.Amount: {transactionForClientBookedSessionAmountAbs} " +
                                $"-- Session.Price: {sessionToGetPaidForSessionPriceAbs} -- BookedSession.Price: {sessionToGetPaidForBookedSessionPriceAbs}. WebCreditLogId '{webCreditLog.Id}', " +
                                $"TransactionId '{transactionForClientBookedSession.Id}', SessionId '{sessionToGetPaidFor.session.Id}', BookedSessionId '{sessionToGetPaidFor.bookedSession.Id}'.");
                            return Json(new
                            {
                                success = false,
                                title = $"Greška prilikom isplate seanse #{bookingId}",
                                body = "Molimo kontaktirajte podršku za terapeute.",
                                severity = "error"
                            });
                        }
                    }
                    
                    var transactionForClientBookedSessionAmountAbs2 = Math.Abs(transactionForClientBookedSession.Amount);
                    var sessionToGetPaidForSessionPriceAbs2 = Math.Abs(sessionToGetPaidFor.session.Price);
                    var sessionToGetPaidForBookedSessionPriceAbs2 = Math.Abs(sessionToGetPaidFor.bookedSession.Price);
                    if (!AllSame(transactionForClientBookedSessionAmountAbs2,
                        sessionToGetPaidForSessionPriceAbs2, sessionToGetPaidForBookedSessionPriceAbs2))
                    {
                        await HandleErrorJsonAsync($"These 3 amounts are not all equal: Transaction.Amount: {transactionForClientBookedSessionAmountAbs2} " +
                            $"-- Session.Price: {sessionToGetPaidForSessionPriceAbs2} -- BookedSession.Price: {sessionToGetPaidForBookedSessionPriceAbs2}. " +
                            $"TransactionId '{transactionForClientBookedSession.Id}', SessionId '{sessionToGetPaidFor.session.Id}', BookedSessionId '{sessionToGetPaidFor.bookedSession.Id}'.");
                        return Json(new
                        {
                            success = false,
                            title = $"Greška prilikom isplate seanse #{bookingId}",
                            body = "Molimo kontaktirajte podršku za terapeute.",
                            severity = "error"
                        });
                    }

                    if (sessionToGetPaidFor.session.StartDateTime != sessionToGetPaidFor.bookedSession.StartTime)
                    {
                        await HandleErrorJsonAsync($"Start datetime of session '{sessionToGetPaidFor.session.Id}' is not equal to start datetime of booked session '{sessionToGetPaidFor.bookedSession.Id}'.");
                        return Json(new
                        {
                            success = false,
                            title = $"Greška prilikom isplate seanse #{bookingId}",
                            body = "Molimo kontaktirajte podršku za terapeute.",
                            severity = "error"
                        });
                    }

                    if (sessionToGetPaidFor.session.EndDateTime != sessionToGetPaidFor.bookedSession.EndTime)
                    {
                        await HandleErrorJsonAsync($"End datetime of session '{sessionToGetPaidFor.session.Id}' is not equal to end datetime of booked session '{sessionToGetPaidFor.bookedSession.Id}'.");
                        return Json(new
                        {
                            success = false,
                            title = $"Greška prilikom isplate seanse #{bookingId}",
                            body = "Molimo kontaktirajte podršku za terapeute.",
                            severity = "error"
                        });
                    }

                    if (sessionToGetPaidFor.session.EndDateTime > DateTime.UtcNow)
                    {
                        await HandleErrorJsonAsync($"Session '{sessionToGetPaidFor.session.Id}' has not ended yet.");
                        return Json(new
                        {
                            success = false,
                            title = $"Greška prilikom isplate seanse #{bookingId}",
                            body = "Molimo kontaktirajte podršku za terapeute.",
                            severity = "error"
                        });
                    }

                    if (sessionToGetPaidFor.bookedSession.EndTime > DateTime.UtcNow)
                    {
                        await HandleErrorJsonAsync($"Booked session '{sessionToGetPaidFor.bookedSession.Id}' has not ended yet.");
                        return Json(new
                        {
                            success = false,
                            title = $"Greška prilikom isplate seanse #{bookingId}",
                            body = "Molimo kontaktirajte podršku za terapeute.",
                            severity = "error"
                        });
                    }

                    double amountToAdd = 0;

                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        try
                        {
                            var transactionId = Helper.GenerateNumbersId();

                            amountToAdd = sessionToGetPaidFor.bookedSession.Price * (1 - (_therapistFee.PercentageAmount / 100));

                            sessionToGetPaidFor.bookedSession.TherapistPaymentTransactionId = transactionId;

                            _context.Transactions.Insert(new Transactions
                            {
                                Id = transactionId,
                                SenderId = PaymentTypesProvider.CounterApathyPayment.Id,
                                ReceiverId = therapistUser.Id,
                                DateTime = DateTime.UtcNow,
                                Amount = amountToAdd,
                                CurrencyCode = _therapistFee.CurrencyCode
                            });

                            _context.TherapistEarningsLogs.Insert(new TherapistEarningsLogs
                            {
                                Id = Helper.GenerateNumbersId(),
                                TherapistId = therapist.Id,
                                Amount = amountToAdd,
                                CurrentAmount = therapist.Earnings + amountToAdd,
                                EarningsDateTime = DateTime.UtcNow,
                                PaymentTypeId = PaymentTypesProvider.CounterApathyPayment.Id,
                                PaymentTypeName = PaymentTypesProvider.CounterApathyPayment.Name,
                                TransactionId = transactionId
                            });

                            therapist.Earnings += amountToAdd;
                            sessionToGetPaidFor.bookedSession.TherapistIsPaid = true;

                            _context.Therapists.Update(therapist);
                            _context.BookedSessions.Update(sessionToGetPaidFor.bookedSession);

                            await _context.SaveAsync();

                            scope.Complete();
                        }
                        catch (Exception)
                        {
                            scope.Dispose();
                            throw;
                        }
                    }

                    await _notificationRepository.SendAsync(new Notifications
                    {
                        Id = Helper.GenerateNumbersId(),
                        SenderUserId = SystemInformation.Name,
                        ReceiverUserId = therapistUser.Id,
                        Title = $"Isplaćeni ste {_therapistFee.CurrencyCode} {amountToAdd} za seansu sa klijentom " +
                        $"{sessionToGetPaidFor.bookedSession.ClientFirstName} {sessionToGetPaidFor.bookedSession.ClientLastName}, " +
                        $"datuma {_dateHelper.ConvertDateTimeFromUtcToLocalDateString(sessionToGetPaidFor.bookedSession.StartTime)}, " +
                        $"u {_dateHelper.ConvertDateTimeFromUtcToLocalTimeString(sessionToGetPaidFor.bookedSession.StartTime)}h.",
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
                        Subject = $"Sredstva u iznosu od {_therapistFee.CurrencyCode} {amountToAdd} su dodata na Vaš nalog",
                        Body = $"Poštovani," +
                        $"\n" +
                        $"Isplaćeni ste {_therapistFee.CurrencyCode} {amountToAdd} za seansu sa klijentom " +
                        $"{sessionToGetPaidFor.bookedSession.ClientFirstName} {sessionToGetPaidFor.bookedSession.ClientLastName}, " +
                        $"datuma {_dateHelper.ConvertDateTimeFromUtcToLocalDateString(sessionToGetPaidFor.bookedSession.StartTime)}, " +
                        $"u {_dateHelper.ConvertDateTimeFromUtcToLocalTimeString(sessionToGetPaidFor.bookedSession.StartTime)}h."
                    });

                    var paymentDateTime = _dateHelper.ConvertDateTimeFromUtcToLocal(DateTime.UtcNow);

                    return Json(new
                    {
                        success = true,
                        therapistProfilePhotoPath = User.GetProfilePhotoPathOrDefaultPhotoPath(_environment),
                        therapistFullName = therapistUser.FirstName + " " + therapistUser.LastName,
                        amountBeforeAdding = amountBeforeAdding,
                        amountAdded = amountToAdd,
                        totalAmount = therapist.Earnings,
                        currencyCode = "RSD",
                        paymentDate = _dateHelper.DateStringFromDateTime(paymentDateTime),
                        paymentTime = _dateHelper.TimeStringFromDateTime(paymentDateTime)
                    });
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

        private bool AllSame(params double[] numbers)
        {
            bool first = true;
            int comparand = 0;
            foreach (int i in numbers)
            {
                if (first) comparand = i;
                else if (i != comparand) return false;
                first = false;
            }
            return true;
        }
    }
}