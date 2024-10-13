using Database.Models;
using Framework.Helpers.ExtensionMethods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebApplication9.Base;
using Framework.Notifications;
using Framework.Emails;
using WebApplication9.Interfaces;
using WebApplication9.Implementations;
using Framework.Interfaces;
using Framework.Models;
using Microsoft.AspNetCore.Http;
using Framework.Implementations;
using DataTransferObjects.ViewModels.Client;
using Framework.Helpers;
using Framework.Providers;

namespace WebApplication9.Controllers
{
    [Authorize(Roles = "Client")]
    public class SessionsController : BaseController
    {
        private readonly IRatingFunctionsProvider _ratingFunctions;
        private readonly ISessionsFunctionsProvider _sessionsFunctions;
        private readonly ICustomClientFunctionsProvider _clientFunctions;

        public SessionsController(IErrorLogger error,
            IMailService mailService,
            IDateTimeHelper dateHelper,
            IHttpContextAccessor contextAccessor,
            INotificationRepository notificationRepository,
            UserManager<CustomClient> userManager,
            SignInManager<CustomClient> signInManager) : base(error, mailService, dateHelper, contextAccessor, notificationRepository, userManager, signInManager)
        {
            _ratingFunctions = new RatingFunctionsProvider();
            _sessionsFunctions = new SessionsFunctionsProvider();
            _clientFunctions = new CustomClientFunctionsProvider(contextAccessor);
        }

        [HttpGet("/zakazane-seanse/{filter?}/{predicate?}")]
        public async Task<IActionResult> BookedSessions(string filter = null, string predicate = null)
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
                {
                    ShowToastOnThisPageIfSet();
                    return View(await _sessionsFunctions.GetClientBookedSessionsAsync(_userManager.GetUserId(User), filter, predicate));
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

        [HttpGet("/zakazane-seanse/detalji/{bookingId}")]
        public async Task<IActionResult> SessionDetails(string bookingId)
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
                {
                    if (string.IsNullOrWhiteSpace(bookingId)) return RedirectToAction("BookedSessions");

                    ShowToastOnThisPageIfSet();

                    var bookedSessionAddRating = await _ratingFunctions.MapToApprovedOrPendingDetailsRatingAsync((await _userManager.GetUserAsync(User)).Id, bookingId);

                    if (bookedSessionAddRating != default)
                        return View(bookedSessionAddRating);
                    else
                    {
                        _session.SetToast("Seansa nije pronađena", null, "info");
                        return RedirectToAction("BookedSessions");
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
        [HttpGet("/anonimno/zakazane-seanse/detalji/{email}/{bookingId}")]
        public async Task<IActionResult> SessionDetailsUnauthorized(string email, string bookingId)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                    throw new GeneralException("Authorized user attempted session details Url meant for unauthorized users.", signOutUser: true);

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(bookingId)) return RedirectToAction("Index", "Home");

                ShowToastOnThisPageIfSet();

                var bookedSessionAddRating = await _ratingFunctions.MapToApprovedOrPendingDetailsRatingForAnonymousAsync(email, bookingId);

                if (bookedSessionAddRating != default)
                    return View(bookedSessionAddRating);
                else
                {
                    _session.SetToast("Seansa nije pronađena", null, "info");
                    return RedirectToAction("Index", "Home");
                }
            }
            catch(GeneralException ge)
            {
                await HandleErrorAsync(ge);
                return RedirectToAction("SessionDetailsUnauthorized", new { email = email, bookingId = bookingId });
            }
            catch(Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpGet("/zakazane-seanse/neocenjene-seanse")]
        public async Task<IActionResult> UnratedSessions()
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if(emailStatus == EmailConfirmationStatus.Confirmed)
                {
                    ShowToastOnThisPageIfSet();
                    return View(await _sessionsFunctions.GetUnreviewedBookedSessionAddRatingViewModelsAsync((await _userManager.GetUserAsync(User)).Id));
                }
                else if(emailStatus == EmailConfirmationStatus.NotConfirmed)
                    return RedirectToAction("ConfirmEmail", "Authorization", new { Area = "" });
                else throw new GeneralException("Unable to load user.", signOutUser: true);
            }
            catch(Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [AllowAnonymous]
        [HttpGet("/anonimno/zakazane-seanse/neocenjene-seanse/{email}/{bookingId}")]
        public async Task<IActionResult> UnratedSessionsUnauthorized(string email, string bookingId)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                    throw new GeneralException("Authorized user attempted unrated session details Url meant for unauthorized users.", signOutUser: true);

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(bookingId)) return RedirectToAction("Index", "Home");

                ShowToastOnThisPageIfSet();
                return View(await _sessionsFunctions.GetUnreviewedBookedSessionAddRatingViewModelForAnonymousAsync(email, bookingId));
            }
            catch(GeneralException ge)
            {
                await HandleErrorAsync(ge);
                return RedirectToAction("UnratedSessionsUnauthorized", new { email = email, bookingId = bookingId });
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpPost("/zakazane-seanse/ocenjivanje-seanse")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RateSession([FromForm] BookedSessionAddRatingViewModel bookedSessionVm)
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
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

                    var userId = _userManager.GetUserId(User);

                    if (_sessionsFunctions.SessionAlreadyRated(bookedSessionVm.BookingId) ||
                        _sessionsFunctions.SessionRatingPending(bookedSessionVm.BookingId))
                        return Json(new
                        {
                            success = false,
                            title = "Ova seansa je već ocenjena",
                            body = "",
                            severity = "info"
                        });

                    var sessionToRate = (await _sessionsFunctions.GetClientBookedSessionsAsync(userId, null, null))
                                                          .SingleOrDefault(s => s.BookingId == bookedSessionVm.BookingId);

                    if (sessionToRate == default)
                    {
                        _session.SetToast("Seansa nije pronađena", "", "error");
                        return Json(new
                        {
                            success = false,
                            redirectUrl = Url.Action("BookedSessions", "Sessions", new { Area = "" })
                        });
                    }

                    if (await _context.Sessions.ReadOnlyAnyAsync(s => s.Id == sessionToRate.SessionId && s.Booked != 1))
                        return Json(new
                        {
                            success = false,
                            title = "Došlo je do greške",
                            body = "Molimo osvežite stranicu i pokušajte ponovo ili kontaktirajte korisničku podršku.",
                            severity = "error"
                        });

                    if (DateTime.UtcNow < sessionToRate.EndTime)
                        return Json(new
                        {
                            success = false,
                            title = "Seansa još nije završena",
                            body = "Sačekajte da se seansa završi, onda ocenite.",
                            severity = "info"
                        });

                    _context.PendingRatings.Insert(new PendingRatings
                    {
                        Id = Helper.GenerateNumbersId(),
                        BookedSessionId = bookedSessionVm.BookingId,
                        SessionId = bookedSessionVm.SessionId,
                        TherapistId = bookedSessionVm.TherapistId,
                        ClientId = userId,
                        Rating = bookedSessionVm.Rating.StarsRating,
                        Comment = bookedSessionVm.Rating.Comment,
                        RatingDate = DateTime.UtcNow
                    });

                    await _context.SaveAsync();

                    return Json(new
                    {
                        success = true,
                        title = "Svaka čast!",
                        body = "Uspešno ste oceniti seansu.",
                        severity = "success",
                        starsValue = bookedSessionVm.Rating.StarsRating
                        //redirectUrl = Url.Action("BookedSessions", "Sessions", new { Area = "" })
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

        [AllowAnonymous]
        [HttpPost("/anonimno/zakazane-seanse/ocenjivanje-seanse")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RateSessionUnauthorized([FromForm] BookedSessionAddRatingViewModel bookedSessionVm)
        {
            try
            {
                // This must be first, because of GeneralException catch block down there
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

                if (User.Identity.IsAuthenticated)
                    throw new GeneralException("Authorized user attempted session rating Url meant for unauthorized users.", signOutUser: true);

                if (_context.BookedSessions.ReadOnlyFind(s => (s.ClientId == "anonymous" || s.ClientId == "unauthorized") &&
                                                                              s.Id == bookedSessionVm.BookingId &&
                                                                              s.ClientEmail == bookedSessionVm.ClientEmail) == null)
                {
                    _session.SetToast("Seansa nije pronađena", null, "info");
                    return Json(new
                    {
                        success = false,
                        redirectUrl = Url.Action("Index", "Home", new { Area = "" })
                    });
                }

                if (_sessionsFunctions.SessionAlreadyRated(bookedSessionVm.BookingId) ||
                    _sessionsFunctions.SessionRatingPending(bookedSessionVm.BookingId))
                    return Json(new
                    {
                        success = false,
                        title = "Ova seansa je već ocenjena",
                        body = "",
                        severity = "info"
                    });

                var sessionToRate = (await _sessionsFunctions.GetAnonymousBookedSessionsAsync(bookedSessionVm.ClientEmail, null, null))
                                                          .SingleOrDefault(s => s.BookingId == bookedSessionVm.BookingId);

                if (sessionToRate == default)
                {
                    _session.SetToast("Seansa nije pronađena", "", "info");
                    return Json(new
                    {
                        success = false,
                        redirectUrl = Url.Action("Index", "Home", new { Area = "" })
                    });
                }

                if (await _context.Sessions.ReadOnlyAnyAsync(s => s.Id == sessionToRate.SessionId && s.Booked != 1))
                    return Json(new
                    {
                        success = false,
                        title = "Došlo je do greške",
                        body = "Molimo osvežite stranicu i pokušajte ponovo ili kontaktirajte korisničku podršku.",
                        severity = "error"
                    });

                if (DateTime.UtcNow < sessionToRate.EndTime)
                    return Json(new
                    {
                        success = false,
                        title = "Seansa još nije završena",
                        body = "Sačekajte da se seansa završi, onda ocenite.",
                        severity = "info"
                    });

                string clientId = UnauthenticatedUserRoles.Unauthorized;
                var user = await _userManager.FindByEmailAsync(bookedSessionVm.ClientEmail);

                if (user == null)
                    clientId = UnauthenticatedUserRoles.Anonymous;

                _context.PendingRatings.Insert(new PendingRatings
                {
                    Id = Helper.GenerateNumbersId(),
                    BookedSessionId = bookedSessionVm.BookingId,
                    SessionId = bookedSessionVm.SessionId,
                    TherapistId = bookedSessionVm.TherapistId,
                    ClientId = clientId,
                    Rating = bookedSessionVm.Rating.StarsRating,
                    Comment = bookedSessionVm.Rating.Comment,
                    RatingDate = DateTime.UtcNow
                });

                await _context.SaveAsync();

                return Json(new
                {
                    success = true,
                    title = "Svaka čast!",
                    body = "Uspešno ste oceniti seansu.",
                    severity = "success",
                    starsValue = bookedSessionVm.Rating.StarsRating
                });
            }
            catch(GeneralException ge)
            {
                await HandleErrorJsonAsync(ge);
                return Json(new
                {
                    success = false,
                    redirectUrl = Url.Action("UnratedSessionsUnauthorized", "SessionsController", new { email = bookedSessionVm.ClientEmail, bookingId = bookedSessionVm.BookingId })
                });
            }
            catch(Exception e)
            {
                return await HandleErrorJsonAsync(e);
            }
        }
    }
}
