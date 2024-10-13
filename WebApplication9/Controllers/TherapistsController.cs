using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Database.Models;
using Framework.Emails;
using Framework.Interfaces;
using Framework.Notifications;
using Framework.Helpers.ExtensionMethods;
using WebApplication9.Base;
using WebApplication9.Interfaces;
using WebApplication9.ViewModels;
using WebApplication9.Implementations;
using WebApplication9.Areas.Therapist.ViewModels;
using Framework.Models;
using Microsoft.AspNetCore.Http;
using Framework.Implementations;
using Framework.Providers;
using System.Net;
using System.Collections;
using Framework.Helpers;
using System.Transactions;
using Microsoft.AspNetCore.Hosting;
using Stripe;
using WebApplication9.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BackgroundTasks;

namespace WebApplication9.Controllers
{
    public class TherapistsController : BaseController
    {
        private const int DAYS_BOOKED_IN_ADVANCE = 1;
        private const int PAYMENT_INTENT_MUST_SUCCEED_IN_MINUTES = 10;
        private readonly StripeSettings _stripeSettings;
        private readonly IFileRepository _files;
        private readonly IConfiguration _configuration;
        private readonly IStripeFunctionsProvider _stripeFunctions;
        private readonly ISessionsFunctionsProvider _sessionsFunctions;
        private readonly ITherapistFunctionsProvider _therapistFunctions;
        private readonly ICustomClientFunctionsProvider _clientFunctions;
        private readonly IConsultationsFunctionsProvider _consultationsFunctions;
        private readonly IWebHostEnvironment _environment;

        public TherapistsController(IConfiguration configuration,
            IErrorLogger error,
            IWebHostEnvironment environment,
            IMailService mailService,
            IDateTimeHelper dateHelper,
            IHttpContextAccessor contextAccessor,
            INotificationRepository notificationRepository,
            UserManager<CustomClient> userManager,
            SignInManager<CustomClient> signInManager) : base(error, mailService, dateHelper, contextAccessor, notificationRepository, userManager, signInManager)
        {
            _environment = environment;
            _files = new FileRepository();
            _configuration = configuration;
            _stripeSettings = configuration.GetSection("StripeSettings").Get<StripeSettings>();
            StripeConfiguration.ApiKey = _stripeSettings.ApiKey;
            _stripeFunctions = new StripeFunctionsProvider();
            _sessionsFunctions = new SessionsFunctionsProvider();
            _therapistFunctions = new TherapistFunctionsProvider();
            _clientFunctions = new CustomClientFunctionsProvider(contextAccessor);
            _consultationsFunctions = new ConsultationsFunctionsProvider(dateHelper);
        }

        [HttpGet("/licencirani-psihoterapeuti/{filter?}/{predicate?}")]
        public async Task<IActionResult> All(string filter = null, string predicate = null)
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
                {
                    ShowToastOnThisPageIfSet();

                    var therapistVms = new List<TherapistCardViewModel>();

                    foreach (var therapist in _therapistFunctions.GetTherapistsWithSetUpAccount(filter, predicate))
                    {
                        var therapistUser = await _userManager.FindByTherapistAccountIdAsync(therapist.Id);

                        if (therapistUser == null) continue;

                        // If therapist does not have any session, we add him to the end of the list
                        // so he will be displayed after all the therapists that have active sessions
                        var therapistSessionPriceString = _therapistFunctions.GetPriceRangeString(therapistUser.TherapistAccountId);
                        if (string.IsNullOrWhiteSpace(therapistSessionPriceString))
                        {
                            therapistVms.Add(new TherapistCardViewModel()
                            {
                                Id = therapistUser.TherapistAccountId,
                                ProfilePhoto = _files.GetUserProfilePhotoPathOrDefaultPhotoPath(_userManager, _environment, therapistUser.Id),
                                FirstName = therapistUser.FirstName,
                                LastName = therapistUser.LastName,
                                SessionPrice = therapistSessionPriceString,
                                Rating = _therapistFunctions.GetTherapistAverageRating(therapistUser.TherapistAccountId),
                                ContactMethods = _therapistFunctions.GetContactMethodsViewModel(therapistUser.TherapistAccountId),
                                SpecialtyNamesForDisplay = _therapistFunctions.GetSpecialtiesNamesForDisplay(therapistUser.TherapistAccountId),
                                PsychotherapyTechniqueNamesForDisplay = _therapistFunctions.GetPsychotherapyTechniquesNamesForDisplay(therapistUser.TherapistAccountId),
                                NumberOfBookedSessions = _therapistFunctions.GetNumberOfBookedSessions(therapistUser.TherapistAccountId),
                                NumberOfUniqueClients = _therapistFunctions.GetNumberOfUniqueClients(therapistUser.TherapistAccountId),
                                HasUpcomingConsultation = await _therapistFunctions.HasAnyUpcomingConsultationAsync(therapist.Id)
                            });
                        }
                        else
                        {
                            // If therapist has session(s), we add him to the beginning of the list
                            // so he will be displayed before all the therapists that don't have active sessions
                            therapistVms.Insert(0, new TherapistCardViewModel()
                            {
                                Id = therapistUser.TherapistAccountId,
                                ProfilePhoto = _files.GetUserProfilePhotoPathOrDefaultPhotoPath(_userManager, _environment, therapistUser.Id),
                                FirstName = therapistUser.FirstName,
                                LastName = therapistUser.LastName,
                                SessionPrice = therapistSessionPriceString,
                                Rating = _therapistFunctions.GetTherapistAverageRating(therapistUser.TherapistAccountId),
                                ContactMethods = _therapistFunctions.GetContactMethodsViewModel(therapistUser.TherapistAccountId),
                                SpecialtyNamesForDisplay = _therapistFunctions.GetSpecialtiesNamesForDisplay(therapistUser.TherapistAccountId),
                                PsychotherapyTechniqueNamesForDisplay = _therapistFunctions.GetPsychotherapyTechniquesNamesForDisplay(therapistUser.TherapistAccountId),
                                NumberOfBookedSessions = _therapistFunctions.GetNumberOfBookedSessions(therapistUser.TherapistAccountId),
                                NumberOfUniqueClients = _therapistFunctions.GetNumberOfUniqueClients(therapistUser.TherapistAccountId),
                                HasUpcomingConsultation = await _therapistFunctions.HasAnyUpcomingConsultationAsync(therapist.Id)
                            });
                        }
                    }

                    ViewBag.techniques = _context.PsychotherapyTechniques.ReadOnlyGetAll().Select(i =>
                        new DataTransferObjects.ViewModels.Client.PsychotherapyTechniqueListItem
                        {
                            Id = i.Id,
                            Name = i.Name
                        }).ToList();
                    ViewBag.specialties = _context.Specialities.ReadOnlyGetAll().Select(i =>
                        new DataTransferObjects.ViewModels.Client.SpecialtyListItem
                        {
                            Id = i.Id,
                            Name = i.Name
                        }).ToList();

                    //if (string.IsNullOrWhiteSpace(filter) && string.IsNullOrWhiteSpace(predicate))
                    //    ViewBag.IsCanonical = true;

                    if (!therapistVms.Any()) Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return View(therapistVms);
                }
                else if (emailStatus == EmailConfirmationStatus.NotConfirmed)
                    return RedirectToAction("ConfirmEmail", "Authorization", new { Area = "" });
                else throw new GeneralException("Unable to load user.", signOutUser: true);
            }
            catch (Exception e)
            {
                await HandleErrorAsync(e);
                return RedirectToAction("Index", "Home", new { Area = "" });
            }
        }

        [HttpGet("/licencirani-psihoterapeuti/profil/{therapistId}/{therapistName?}")]
        public async Task<IActionResult> Profile(string therapistId, string therapistName = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(therapistId))
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return RedirectToAction("All", "Therapists", new { Area = "" });
                }

                var therapistUser = await _userManager.FindByTherapistAccountIdAsync(therapistId);
                var therapist = _therapistFunctions.GetTherapistWithSetUpAccount(therapistId);

                if (therapistUser == null || therapist == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;
                    _session.SetToast("Psihoterapeut nije pronađen", null, "info");
                    return RedirectToAction("All", "Therapists", new { Area = "" });
                }

                if (User.Identity.IsAuthenticated)
                {
                    if (User.IsInRole(UserRoles.Client))
                    {
                        var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                        if (emailStatus == EmailConfirmationStatus.Confirmed)
                        {
                            ShowToastOnThisPageIfSet();

                            return View("ProfileForClientVisitor", new TherapistPublicProfileForClientVisitorViewModel()
                            {
                                UserId = therapistUser.Id,
                                TherapistAccountId = therapistUser.TherapistAccountId,
                                FirstName = therapistUser.FirstName,
                                LastName = therapistUser.LastName,
                                YearOfBirth = therapistUser.YearOfBirth.ToString(),
                                City = therapist.City,
                                Country = therapist.Country,
                                About = therapist.About,
                                ProfilePhoto = _files.GetUserProfilePhotoPathOrDefaultPhotoPath(_userManager, _environment, therapistUser.Id),
                                Gender = _therapistFunctions.GetGender(therapistId),
                                UnderSupervision = therapist.UnderSupervision.Value,
                                SessionPrice = _therapistFunctions.GetPriceRangeString(therapistId),
                                Rating = _therapistFunctions.GetTherapistAverageRating(therapistId),
                                ClientReviews = _therapistFunctions.GetClientReviews(therapistId),
                                PsychotherapyTechniquesExperiences = _therapistFunctions.GetPsychotherapyTechniquesExperiencesForProfile(therapistId),
                                SpecialtiesExperiences = _therapistFunctions.GetSpecialtiesExperiencesForProfile(therapistId),
                                ToChooseFrom_ContactMethods = _therapistFunctions.Get_ToChooseFrom_ContactMethods(therapistId),
                                Sessions = _therapistFunctions.GetUpcomingSessionsViewModels(therapistId),
                                Address = "Molimo da 5 minuta pre početka seanse budete na adresi psihoterapeuta:\n" +
                                           therapistUser.FirstName + " " + therapistUser.LastName + ", \n" +
                                           therapist.Street + " " + therapist.HouseNumber + ", \n" +
                                           therapist.City + " " + therapist.PostalCode + ", " + "Srbija" + ".\n",
                                HasUpcomingConsultation = await _therapistFunctions.HasAnyUpcomingConsultationAsync(therapistId)
                            });
                        }
                        else if (emailStatus == EmailConfirmationStatus.NotConfirmed)
                            return RedirectToAction("ConfirmEmail", "Authorization", new { Area = "" });
                        else throw new GeneralException("Unable to load user.", signOutUser: true);
                    }
                    else
                    {
                        ShowToastOnThisPageIfSet();

                        return View("ProfileForAuthenticatedNonClientVisitor", new TherapistPublicProfileForAuthenticatedNonClientVisitorViewModel
                        {
                            UserId = therapistUser.Id,
                            TherapistAccountId = therapistUser.TherapistAccountId,
                            IsSelf = User.IsInRole(UserRoles.Therapist) &&
                                     (await _userManager.GetUserAsync(User)).TherapistAccountId == therapistId,
                            FirstName = therapistUser.FirstName,
                            LastName = therapistUser.LastName,
                            YearOfBirth = therapistUser.YearOfBirth.ToString(),
                            City = therapist.City,
                            Country = therapist.Country,
                            About = therapist.About,
                            ProfilePhoto = _files.GetUserProfilePhotoPathOrDefaultPhotoPath(_userManager, _environment, therapistUser.Id),
                            Gender = _therapistFunctions.GetGender(therapistId),
                            UnderSupervision = therapist.UnderSupervision.Value,
                            SessionPrice = _therapistFunctions.GetPriceRangeString(therapistId),
                            Rating = _therapistFunctions.GetTherapistAverageRating(therapistId),
                            ClientReviews = _therapistFunctions.GetClientReviews(therapistId),
                            PsychotherapyTechniquesExperiences = _therapistFunctions.GetPsychotherapyTechniquesExperiencesForProfile(therapistId),
                            SpecialtiesExperiences = _therapistFunctions.GetSpecialtiesExperiencesForProfile(therapistId),
                            ToChooseFrom_ContactMethods = _therapistFunctions.Get_ToChooseFrom_ContactMethods(therapistId),
                            Sessions = _therapistFunctions.GetUpcomingSessionsViewModels(therapistId),
                            Address = "Molimo da 5 minuta pre početka seanse budete na adresi psihoterapeuta:\n" +
                                       therapistUser.FirstName + " " + therapistUser.LastName + ", \n" +
                                       therapist.Street + " " + therapist.HouseNumber + ", \n" +
                                       therapist.City + " " + therapist.PostalCode + ", " + "Srbija" + ".\n",
                            HasUpcomingConsultation = await _therapistFunctions.HasAnyUpcomingConsultationAsync(therapistId)
                        });
                    }
                }
                else
                {
                    ShowToastOnThisPageIfSet();

                    return View("ProfileForAnonymousVisitor", new TherapistPublicProfileForAnonymousVisitorViewModel
                    {
                        UserId = therapistUser.Id,
                        TherapistAccountId = therapistUser.TherapistAccountId,
                        FirstName = therapistUser.FirstName,
                        LastName = therapistUser.LastName,
                        YearOfBirth = therapistUser.YearOfBirth.ToString(),
                        City = therapist.City,
                        Country = therapist.Country,
                        About = therapist.About,
                        ProfilePhoto = _files.GetUserProfilePhotoPathOrDefaultPhotoPath(_userManager, _environment, therapistUser.Id),
                        Gender = _therapistFunctions.GetGender(therapistId),
                        UnderSupervision = therapist.UnderSupervision.Value,
                        SessionPrice = _therapistFunctions.GetPriceRangeString(therapistId),
                        Rating = _therapistFunctions.GetTherapistAverageRating(therapistId),
                        ClientReviews = _therapistFunctions.GetClientReviews(therapistId),
                        PsychotherapyTechniquesExperiences = _therapistFunctions.GetPsychotherapyTechniquesExperiencesForProfile(therapistId),
                        SpecialtiesExperiences = _therapistFunctions.GetSpecialtiesExperiencesForProfile(therapistId),
                        ToChooseFrom_ContactMethods = _therapistFunctions.Get_ToChooseFrom_ContactMethods(therapistId),
                        Sessions = _therapistFunctions.GetUpcomingSessionsViewModels(therapistId),
                        Address = "Molimo da 5 minuta pre početka seanse budete na adresi psihoterapeuta:\n" +
                                   therapistUser.FirstName + " " + therapistUser.LastName + ", \n" +
                                   therapist.Street + " " + therapist.HouseNumber + ", \n" +
                                   therapist.City + " " + therapist.PostalCode + ", " + "Srbija" + ".\n",
                        HasUpcomingConsultation = await _therapistFunctions.HasAnyUpcomingConsultationAsync(therapistId)
                    });
                }  
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpGet("/licencirani-psihoterapeuti/iskustva-sa-psihoterapije")]
        public async Task<IActionResult> PsychotherapistsExperiences()
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
                {
                    var therapistsExperiencesVm = new List<PsychotherapistExperienceViewModel>();

                    foreach (var therapist in _therapistFunctions.GetTherapistsWithSetUpAccount().Select(thr => new { thr.Id, thr.UnderSupervision }))
                    {
                        var therapistUser = await _userManager.FindByTherapistAccountIdAsync(therapist.Id);

                        if (therapistUser == null) continue;

                        // If therapist does not have any session, we add him to the end of the list
                        // so he will be displayed after all the therapists that have active sessions
                        if (!await _therapistFunctions.HasAnyUpcomingSessionAsync(therapist.Id))
                        {
                            therapistsExperiencesVm.Add(new PsychotherapistExperienceViewModel
                            {
                                TherapistId = therapist.Id,
                                TherapistProfilePhotoPath = _files.GetUserProfilePhotoPathOrDefaultPhotoPath(_userManager, _environment, therapistUser.Id),
                                TherapistFirstName = therapistUser.FirstName,
                                TherapistLastName = therapistUser.LastName,
                                UnderSupervision = therapist.UnderSupervision.GetValueOrDefault(),
                                SpecialtyNamesForDisplay = _therapistFunctions.GetSpecialtiesNamesForDisplay(therapistUser.TherapistAccountId),
                                PsychotherapyTechniqueNamesForDisplay = _therapistFunctions.GetPsychotherapyTechniquesNamesForDisplay(therapistUser.TherapistAccountId),
                                Rating = _therapistFunctions.GetTherapistAverageRating(therapist.Id),
                                ClientReviews = _therapistFunctions.GetClientReviews(therapist.Id),
                                HasUpcomingConsultation = await _therapistFunctions.HasAnyUpcomingConsultationAsync(therapist.Id)
                            });
                        }
                        else
                        {
                            // If therapist has session(s), we add him to the beginning of the list
                            // so he will be displayed before all the therapists that don't have active sessions
                            therapistsExperiencesVm.Insert(0, new PsychotherapistExperienceViewModel
                            {
                                TherapistId = therapist.Id,
                                TherapistProfilePhotoPath = _files.GetUserProfilePhotoPathOrDefaultPhotoPath(_userManager, _environment, therapistUser.Id),
                                TherapistFirstName = therapistUser.FirstName,
                                TherapistLastName = therapistUser.LastName,
                                UnderSupervision = therapist.UnderSupervision.GetValueOrDefault(),
                                SpecialtyNamesForDisplay = _therapistFunctions.GetSpecialtiesNamesForDisplay(therapistUser.TherapistAccountId),
                                PsychotherapyTechniqueNamesForDisplay = _therapistFunctions.GetPsychotherapyTechniquesNamesForDisplay(therapistUser.TherapistAccountId),
                                Rating = _therapistFunctions.GetTherapistAverageRating(therapist.Id),
                                ClientReviews = _therapistFunctions.GetClientReviews(therapist.Id),
                                HasUpcomingConsultation = await _therapistFunctions.HasAnyUpcomingConsultationAsync(therapist.Id)
                            });
                        }
                    }

                    if (!therapistsExperiencesVm.Any()) Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return View(therapistsExperiencesVm);
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

        [HttpPost("/anonimno/licencirani-psihoterapeuti/zakaži-seansu")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> BookSessionUnauthorized([FromForm] TherapistPublicProfileForAnonymousVisitorViewModel publicVm, string sessionId)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                    throw new Exception("Authorized user attempted session booking Url meant for unauthorized users.");

                var therapistCC = await _userManager.FindByIdAsync(publicVm.UserId);
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

                publicVm.ClientEmail = publicVm.ClientEmail.ToLowerInvariant();

                // Check if non-client is booking a session
                var user = await _userManager.FindByEmailAsync(publicVm.ClientEmail);
                if (user != null && (await _userManager.GetUserRoleAsync(user.Id)) != UserRoles.Client)
                    return Json(new
                    {
                        success = false,
                        title = "Trenutno samo klijentski imejl može zakazati seansu",
                        body = "Unesite drugi imejl i pokušajte ponovo",
                        severity = "info"
                    });

                //if has an un-reviewed session
                //treba da modifikujem ovo da trazi po emailu
                //if (_clientFunctions.HasUnreviewedBookedSession(loggedUser.Id))
                //{
                //    var therapistWithRateSessionButtonPartialViews = new ArrayList();

                //    foreach (var session in _sessionsFunctions.GetUnreviewedBookedSessionAddRatingViewModels(loggedUser.Id))
                //    {
                //        therapistWithRateSessionButtonPartialViews.Add(new
                //        {
                //            bookingId = session.BookingId,
                //            therapistProfilePhoto = session.ProfilePhoto,
                //            therapistFirstName = session.TherapistFirstName,
                //            therapistLastName = session.TherapistLastName,
                //            type = session.Type == 0 ? "Individualna seansa" : "Grupna seansa",
                //            contactMethod = session.ContactMethod.Name,
                //            startTime = _dateHelper.ConvertDateTimeFromUtcToLocalString(session.StartTime),
                //            endTime = _dateHelper.ConvertDateTimeFromUtcToLocalString(session.EndTime),
                //            partialView = await RenderPartialViewToStringAsync("_RateSessionButtonOnBooking", session)
                //        });
                //    }

                //    return Json(new
                //    {
                //        success = false,
                //        hasUnreviewedSessions = true,
                //        title = "Imate neocenjene seanse od ranije",
                //        body = "Molimo ocenite sve seanse ispod da biste mogli zakazati narednu seansu",
                //        unreviewedSessions = therapistWithRateSessionButtonPartialViews
                //    });
                //}

                // Check if user has applied for getting therapist account
                if (user != null && await _userManager.HasPendingApplicationForTherapistAccountCheckByEmail(publicVm.ClientEmail))
                    return Json(new
                    {
                        success = false,
                        title = "Nije moguće zakazati seansu koristeći ovaj imejl",
                        body = "Unesite drugi imejl i pokušajte ponovo.",
                        severity = "info"
                    });

                // Will never execute this code because above we have _therapistFunctions.GetTherapistWithSetUpAccount
                // but leave it here in case later we remove _therapistFunctions.GetTherapistWithSetUpAccount and just 
                // search for the therapist normally by _context.Therapists.GetById
                // Check if user is booking a session with therapist who has uncolpleted account
                if (!_therapistFunctions.HasCompletedTherapistAccountSetup(therapist))
                    return Json(new
                    {
                        success = false,
                        title = "Trenutno nije moguće zakazati seansu sa ovim psihoterapeutom",
                        body = "Izaberite drugog psihoterapeuta i pokušajte ponovo.",
                        severity = "info"
                    });

                // Check if all the info are provided
                if (!ModelState.IsValid)
                    return Json(new
                    {
                        success = false,
                        title = "Popunite sva obavezna polja",
                        body = "Proverite sva polja i pokušajte ponovo.",
                        severity = "info"
                    });

                // Check if session exists
                // Check if session belongs to therapist with whom we are booking
                var sessionToBook = _context.Sessions.Find(s => s.Id == sessionId &&
                                                                s.TherapistId == therapist.Id &&
                                                                s.Booked == 0).SingleOrDefault();

                if (sessionToBook == default || _context.BookedSessions.ReadOnlyAny(s => s.SessionId == sessionToBook.Id))
                    return Json(new
                    {
                        success = false,
                        title = "Ova seansa je već zakazana",
                        body = "Molimo izaberite drugu seansu za zakazivanje.",
                        severity = "info"
                    });

                // Check if session is overlapping with consultation
                if (_consultationsFunctions.EmailHasConsultationToAttendDuringPeriod(publicVm.ClientEmail, sessionToBook.StartDateTime, sessionToBook.EndDateTime))
                    return Json(new
                    {
                        success = false,
                        title = "Termini se preklapaju",
                        body = "Već imate zakazan termin besplatnih konsultacija u ovom vremenskom rasponu. " +
                        "Molimo izaberite seansu sa drugim vremenom početka.",
                        severity = "info"
                    });

                // Check if sessions are overlapping
                if (user != null && _sessionsFunctions.EmailHasSessionToAttendDuringPeriod(publicVm.ClientEmail, sessionToBook.StartDateTime, sessionToBook.EndDateTime))
                    return Json(new
                    {
                        success = false,
                        title = "Termini se preklapaju",
                        body = "Već imate zakazanu seansu u ovom vremenskom rasponu. " +
                        "Molimo izaberite seansu sa drugim vremenom početka.",
                        severity = "info"
                    });

                // Check if session is being booked less than 1 day in advance
                if (sessionToBook.StartDateTime < DateTime.UtcNow.AddDays(DAYS_BOOKED_IN_ADVANCE))
                    return Json(new
                    {
                        success = false,
                        title = $"Seansa mora biti zakazana bar {24 * DAYS_BOOKED_IN_ADVANCE} časa pre vremena početka",
                        body = "Molimo izaberite seansu koja najranije počinje za 24 časa.",
                        severity = "info"
                    });

                // Check if user is trying to hack by changing frontend Contact Method Id in dropdown
                var contactMethod = _context.ContactMethods.GetById(publicVm.Chosen_ContactMethodId);
                if (contactMethod == null || !_therapistFunctions.HasContactMethod(therapist.Id, contactMethod.Id))
                    return Json(new
                    {
                        success = false,
                        title = "Izaberite kontakt metodu",
                        body = "Izborom kontakt metode, birate gde biste želeli da Vaša seansa bude održana.",
                        severity = "info"
                    });

                StripePaymentIntents paymentIntentInDatabase;
                PaymentIntent paymentIntent;
                var paymentIntentClientSecret = string.Empty;
                var bookingId = Helper.GenerateNumbersId();
                publicVm.ClientPhoneNumber = !publicVm.ClientPhoneNumber.StartsWith("381") ? "381" + publicVm.ClientPhoneNumber : publicVm.ClientPhoneNumber;

                UnbookUnpaidSessionService hostedService;
                DateTime nextHostedServiceExecution;

                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    try
                    {
                        var paymentIntentOptions = new PaymentIntentCreateOptions
                        {
                            Amount = Convert.ToInt64(sessionToBook.Price * 100),
                            Currency = "rsd",
                            SetupFutureUsage = "off_session",
                            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                            {
                                Enabled = true
                            },
                            ReceiptEmail = publicVm.ClientEmail,
                            Metadata = new Dictionary<string, string>
                            {
                                { "UserId", user == null ? "anonymous" : "unauthorized" },
                                { "SessionId", sessionToBook.Id },
                                { "BookingId", bookingId },
                                { "TherapistUserId", therapistCC.Id },
                                { "TherapistId", therapist.Id },
                                { "TherapistFirstName", therapistCC.FirstName },
                                { "TherapistLastName", therapistCC.LastName },
                                { "TherapistEmail", therapistCC.Email },
                                { "TherapistPhoneNumber", therapistCC.PhoneNumber },
                                { "TherapistStreet", therapist.Street },
                                { "TherapistHouseNumber", therapist.HouseNumber },
                                { "TherapistCity", therapist.City },
                                { "TherapistPostalCode", therapist.PostalCode },
                                { "TherapistCountry", therapist.Country },
                                { "ClientFirstName", publicVm.ClientFirstName },
                                { "ClientLastName", publicVm.ClientLastName },
                                { "ContactMethodId", contactMethod.Id },
                                { "ContactMethodName", contactMethod.Name },
                                { "ContactMethodColor", contactMethod.Color },
                                { "ContactMethodIcon", contactMethod.Icon }
                            },
                            Description = user != null ? "Not logged in" : "anonymous",
                            StatementDescriptor = _configuration.GetSection("Application:AppName")?.Value
                        };

                        var stripeCustomerId = _stripeFunctions.GetStripeCustomerIdByEmail(publicVm.ClientEmail);

                        if (!string.IsNullOrWhiteSpace(stripeCustomerId))
                        {
                            paymentIntentOptions.Customer = stripeCustomerId;

                            var customerInStripe = await new CustomerService().UpdateAsync(stripeCustomerId, new CustomerUpdateOptions
                            {
                                Name = user == null ? $"{publicVm.ClientFirstName} {publicVm.ClientLastName}" : $"{publicVm.ClientFirstName} {publicVm.ClientLastName} - {user.Id}",
                                Phone = publicVm.ClientPhoneNumber,
                                Metadata = new Dictionary<string, string>
                                {
                                    { "UserId", user == null ? "anonymous" : "unauthorized" }
                                }
                            });

                            // Here updating phone in StripeCustomers is not needed because we do not
                            // want the user to be able to update his Account's PhoneNumber while
                            // not logged in.
                        }
                        else
                        {
                            var customer = await new CustomerService().CreateAsync(new CustomerCreateOptions
                            {
                                Name = user == null ? $"{publicVm.ClientFirstName} {publicVm.ClientLastName}" : $"{publicVm.ClientFirstName} {publicVm.ClientLastName} - {user.Id}",
                                Email = publicVm.ClientEmail,
                                Phone = publicVm.ClientPhoneNumber,
                                Description = "Generated at Therapists/BookSessionUnauthorized",
                                Metadata = new Dictionary<string, string>
                                {
                                    { "UserId", user == null ? "anonymous" : "unauthorized" }
                                }
                            });

                            _context.StripeCustomers.Insert(new StripeCustomers
                            {
                                Id = customer.Id,
                                UserIdOrAnonymous = user == null ? "anonymous" : "unauthorized",
                                Email = publicVm.ClientEmail,
                                CreatedDateTime = DateTime.UtcNow,
                                PhoneNumber = publicVm.ClientPhoneNumber
                            });

                            paymentIntentOptions.Customer = customer.Id;
                        }

                        paymentIntentInDatabase = _stripeFunctions.GetLatestPaymentIntentByEmail(publicVm.ClientEmail);
                        var newPaymentIntentInserted = false;

                        if(paymentIntentInDatabase != null &&
                            !string.IsNullOrWhiteSpace(paymentIntentInDatabase.CustomerId) &&
                            DateTime.UtcNow < paymentIntentInDatabase.MustSucceedUntil &&
                            paymentIntentInDatabase.Status == 0)
                        {
                            var paymentIntentInStripe = await new PaymentIntentService().GetAsync(paymentIntentInDatabase.Id);

                            if(paymentIntentInStripe != null &&
                                paymentIntentInStripe.Status != "succeeded")
                            {
                                paymentIntent = await new PaymentIntentService().UpdateAsync(paymentIntentInDatabase.Id, new PaymentIntentUpdateOptions
                                {
                                    Amount = Convert.ToInt64(sessionToBook.Price * 100),
                                    Currency = "rsd",
                                    SetupFutureUsage = "off_session",
                                    ReceiptEmail = publicVm.ClientEmail,
                                    Metadata = new Dictionary<string, string>
                                    {
                                        { "UserId", user == null ? "anonymous" : "unauthorized" },
                                        { "SessionId", sessionToBook.Id },
                                        { "BookingId", bookingId },
                                        { "TherapistUserId", therapistCC.Id },
                                        { "TherapistId", therapist.Id },
                                        { "TherapistFirstName", therapistCC.FirstName },
                                        { "TherapistLastName", therapistCC.LastName },
                                        { "TherapistEmail", therapistCC.Email },
                                        { "TherapistPhoneNumber", therapistCC.PhoneNumber },
                                        { "TherapistStreet", therapist.Street },
                                        { "TherapistHouseNumber", therapist.HouseNumber },
                                        { "TherapistCity", therapist.City },
                                        { "TherapistPostalCode", therapist.PostalCode },
                                        { "TherapistCountry", therapist.Country },
                                        { "ClientFirstName", publicVm.ClientFirstName },
                                        { "ClientLastName", publicVm.ClientLastName },
                                        { "ContactMethodId", contactMethod.Id },
                                        { "ContactMethodName", contactMethod.Name },
                                        { "ContactMethodColor", contactMethod.Color },
                                        { "ContactMethodIcon", contactMethod.Icon }
                                    },
                                    StatementDescriptor = _configuration.GetSection("Application:AppName")?.Value
                                });

                                paymentIntentInDatabase.PhoneNumber = publicVm.ClientPhoneNumber;
                                paymentIntentInDatabase.SystemEventTypeName = StripePaymentIntentSystemEventTypeNameProvider.SessionInstantPayment;
                                paymentIntentInDatabase.MustSucceedUntil = DateTime.UtcNow.AddMinutes(PAYMENT_INTENT_MUST_SUCCEED_IN_MINUTES);
                            }
                            else
                            {
                                paymentIntent = await new PaymentIntentService().CreateAsync(paymentIntentOptions);

                                paymentIntentInDatabase = new StripePaymentIntents
                                {
                                    Id = paymentIntent.Id,
                                    UserIdOrAnonymous = user == null ? "anonymous" : "unauthorized",
                                    Email = publicVm.ClientEmail,
                                    PhoneNumber = publicVm.ClientPhoneNumber,
                                    CustomerId = paymentIntent.CustomerId,
                                    Status = 0,
                                    SystemEventTypeName = StripePaymentIntentSystemEventTypeNameProvider.SessionInstantPayment,
                                    MustSucceedUntil = DateTime.UtcNow.AddMinutes(PAYMENT_INTENT_MUST_SUCCEED_IN_MINUTES),
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
                                UserIdOrAnonymous = user == null ? "anonymous" : "unauthorized",
                                Email = publicVm.ClientEmail,
                                PhoneNumber = publicVm.ClientPhoneNumber,
                                CustomerId = paymentIntent.CustomerId,
                                Status = 0,
                                SystemEventTypeName = StripePaymentIntentSystemEventTypeNameProvider.SessionInstantPayment,
                                MustSucceedUntil = DateTime.UtcNow.AddMinutes(PAYMENT_INTENT_MUST_SUCCEED_IN_MINUTES),
                                CreatedDateTime = DateTime.UtcNow
                            };

                            _context.StripePaymentIntents.Insert(paymentIntentInDatabase);
                            newPaymentIntentInserted = true;
                        }

                        sessionToBook.Booked = 1;
                        paymentIntentClientSecret = paymentIntent.ClientSecret;

                        if (!newPaymentIntentInserted)
                            _context.StripePaymentIntents.Update(paymentIntentInDatabase);

                        hostedService = HttpContext.RequestServices.GetRequiredService<UnbookUnpaidSessionService>();
                        nextHostedServiceExecution = hostedService.GetNextExecution();
                        if (nextHostedServiceExecution < paymentIntentInDatabase.MustSucceedUntil)
                            nextHostedServiceExecution = hostedService.GetNextExecution(DateTime.UtcNow.AddMinutes(PAYMENT_INTENT_MUST_SUCCEED_IN_MINUTES));

                        _context.Sessions.Update(sessionToBook);
                        await _context.SaveAsync();
                        scope.Complete();
                    }
                    catch(Exception)
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
                    paymentSuccessfulReturnUrl = Url.Action("SessionDetailsUnauthorized", "Sessions", new { Area = "", email = publicVm.ClientEmail, bookingId }, HttpContext.Request.Scheme),
                    mustCompletePaymentUntil = _dateHelper.ConvertDateTimeToDateTimeStringISO(_dateHelper.ConvertDateTimeFromUtcToLocal(nextHostedServiceExecution)),
                    title = "Čestitamo!",
                    body = $"Uspešno ste zakazali seansu sa psihoterapeutom {therapistCC.FirstName} {therapistCC.LastName}.",
                    severity = "success"
                });
            }
            catch(StripeException se)
            {
                if (se.GetRootException().Message.ToLowerInvariant().Contains("email"))
                    return Json(new
                    {
                        success = false,
                        title = "Uneta imejl adresa nije validna",
                        body = $"Imejl adresa {publicVm.ClientEmail} nije validna, molimo unesite drugu adresu i pokušajte ponovo.",
                        severity = "info"
                    });
                else if(se.GetRootException().Message.ToLowerInvariant().Contains("phone"))
                    return Json(new
                    {
                        success = false,
                        title = "Uneti broj telefona nije validan",
                        body = $"Broj telefona adresa {publicVm.ClientPhoneNumber} nije validan, molimo unesite drugi broj i pokušajte ponovo.",
                        severity = "info"
                    });
                else return await HandleErrorJsonAsync(se);
            }
            catch (Exception e)
            {
                return await HandleErrorJsonAsync(e);
            }
        }

        [HttpPost("/licencirani-psihoterapeuti/zakaži-seansu")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookSession([FromForm] TherapistPublicProfileForAuthenticatedNonClientVisitorViewModel publicVm, string sessionId)
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
                {
                    if (!User.Identity.IsAuthenticated)
                    {
                        _session.SetToast("Molimo ulogujte se prvo", "", "info");
                        return Json(new
                        {
                            success = false,
                            redirectUrl = Url.Action("SignIn", "Authorization", new { Area = "" })
                        });
                    }

                    var loggedUser = await _userManager.GetUserAsync(User);

                    var therapistCC = await _userManager.FindByIdAsync(publicVm.UserId);
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

                    // Check if non-client is booking a session
                    if (await _userManager.IsInRoleAsync(loggedUser, UserRoles.Therapist) || await _userManager.IsInRoleAsync(loggedUser, UserRoles.Admin))
                        return Json(new
                        {
                            success = false,
                            title = "Trenutno samo klijenti mogu zakazivati seanse sa psihoterapeutima",
                            body = "Ulogujte se koristeći nalog klijenta da biste mogli zakazati seansu.",
                            severity = "info"
                        });

                    //if has an un-reviewed session
                    if (_clientFunctions.HasUnreviewedBookedSession(loggedUser.Id))
                    {
                        await HandleErrorJsonAsync("**This is not an error: user has attempted to book a session while having unreviewed session(s). Check if he booked a session at the end or gave up.");

                        var therapistWithRateSessionButtonPartialViews = new ArrayList();

                        foreach (var session in await _sessionsFunctions.GetUnreviewedBookedSessionAddRatingViewModelsAsync(loggedUser.Id))
                        {
                            therapistWithRateSessionButtonPartialViews.Add(new
                            {
                                bookingId = session.BookingId,
                                therapistProfilePhoto = session.ProfilePhoto,
                                therapistFirstName = session.TherapistFirstName,
                                therapistLastName = session.TherapistLastName,
                                type = session.Type == 0 ? "Individualna seansa" : "Grupna seansa",
                                contactMethod = session.ContactMethod.Name,
                                startTime = _dateHelper.ConvertDateTimeFromUtcToLocalString(session.StartTime),
                                endTime = _dateHelper.ConvertDateTimeFromUtcToLocalString(session.EndTime),
                                partialView = await RenderPartialViewToStringAsync("_RateSessionButtonOnBooking", session)
                            });
                        }

                        return Json(new
                        {
                            success = false,
                            hasUnreviewedSessions = true,
                            title = "Imate neocenjene seanse od ranije",
                            body = "Molimo ocenite sve seanse ispod da biste mogli zakazati narednu seansu",
                            unreviewedSessions = therapistWithRateSessionButtonPartialViews
                        });
                    }

                    // Check if user has applied for getting therapist account
                    if (_userManager.HasPendingApplicationForTherapistAccount(loggedUser.Id))
                        return Json(new
                        {
                            success = false,
                            title = "Ne možete zakazati seansu dok ste u toku procesa apliciranja za nalog terapeuta",
                            body = "Ulogujte se drugim nalogom i pokušajte ponovo.",
                            severity = "info"
                        });

                    // Will never execute this code because above we have _therapistFunctions.GetTherapistWithSetUpAccount
                    // but leave it here in case later we remove _therapistFunctions.GetTherapistWithSetUpAccount and just 
                    // search for the therapist normally by _context.Therapists.GetById
                    // Check if user is booking a session with therapist who has uncolpleted account
                    if (!_therapistFunctions.HasCompletedTherapistAccountSetup(therapist))
                        return Json(new
                        {
                            success = false,
                            title = "Trenutno nije moguće zakazati seansu sa ovim psihoterapeutom",
                            body = "Izaberite drugog psihoterapeuta i pokušajte ponovo.",
                            severity = "info"
                        });

                    // Check if all the info are provided
                    if (!ModelState.IsValid)
                        return Json(new
                        {
                            success = false,
                            title = "Popunite sva obavezna polja",
                            body = "Proverite sva polja i pokušajte ponovo.",
                            severity = "info"
                        });

                    // Check if session exists
                    // Check if session belongs to therapist with whom we are booking
                    var sessionToBook = _context.Sessions.Find(s => s.Id == sessionId &&
                                                                    s.TherapistId == therapist.Id &&
                                                                    s.Booked == 0).SingleOrDefault();

                    if (sessionToBook == default || _context.BookedSessions.ReadOnlyAny(s => s.SessionId == sessionToBook.Id))
                        return Json(new
                        {
                            success = false,
                            title = "Ova seansa je već zakazana",
                            body = "Molimo izaberite drugu seansu za zakazivanje.",
                            severity = "info"
                        });

                    // Check if session is overlapping with consultation
                    if(_consultationsFunctions.UserHasConsultationToAttendDuringPeriod(loggedUser.Id, sessionToBook.StartDateTime, sessionToBook.EndDateTime))
                        return Json(new
                        {
                            success = false,
                            title = "Termini se preklapaju",
                            body = "Već imate zakazan termin besplatnih konsultacija u ovom vremenskom rasponu. " +
                            "Molimo izaberite seansu sa drugim vremenom početka.",
                            severity = "info"
                        });

                    // Check if sessions are overlapping
                    if (_sessionsFunctions.UserHasSessionToAttendDuringPeriod(loggedUser.Id, sessionToBook.StartDateTime, sessionToBook.EndDateTime))
                        return Json(new
                        {
                            success = false,
                            title = "Termini se preklapaju",
                            body = "Već imate zakazanu seansu u ovom vremenskom rasponu. " +
                            "Molimo izaberite seansu sa drugim vremenom početka.",
                            severity = "info"
                        });

                    // Check if session is being booked less than 1 day in advance
                    if (sessionToBook.StartDateTime < DateTime.UtcNow.AddDays(DAYS_BOOKED_IN_ADVANCE))
                        return Json(new
                        {
                            success = false,
                            title = $"Seansa mora biti zakazana bar {24 * DAYS_BOOKED_IN_ADVANCE} časa pre vremena početka",
                            body = "Molimo izaberite seansu koja najranije počinje za 24 časa.",
                            severity = "info"
                        });

                    // Check if user is trying to hack by changing frontend Contact Method Id in dropdown
                    var contactMethod = _context.ContactMethods.GetById(publicVm.Chosen_ContactMethodId);
                    if (contactMethod == null || !_therapistFunctions.HasContactMethod(therapist.Id, contactMethod.Id))
                        return Json(new
                        {
                            success = false,
                            title = "Izaberite kontakt metodu",
                            body = "Izborom kontakt metode, birate gde biste želeli da Vaša seansa bude održana.",
                            severity = "info"
                        });

                    var bookingId = Helper.GenerateNumbersId();

                    // Check if user who is booking has enough credits
                    loggedUser.WebCredit = loggedUser.WebCredit != null ? loggedUser.WebCredit : 0;
                    var webCreditBeforeReduction = loggedUser.WebCredit;
                    if (loggedUser.WebCredit < sessionToBook.Price)
                    {
                        StripePaymentIntents paymentIntentInDatabase;
                        PaymentIntent paymentIntent;
                        var paymentIntentClientSecret = string.Empty;

                        UnbookUnpaidSessionService hostedService;
                        DateTime nextHostedServiceExecution;

                        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            try
                            {
                                var paymentIntentOptions = new PaymentIntentCreateOptions
                                {
                                    Amount = Convert.ToInt64(sessionToBook.Price * 100),
                                    Currency = "rsd",
                                    SetupFutureUsage = "off_session",
                                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                                    {
                                        Enabled = true
                                    },
                                    ReceiptEmail = loggedUser.Email,
                                    Metadata = new Dictionary<string, string>
                                    {
                                        { "UserId", loggedUser.Id },
                                        { "SessionId", sessionToBook.Id },
                                        { "BookingId", bookingId },
                                        { "TherapistUserId", therapistCC.Id },
                                        { "TherapistId", therapist.Id },
                                        { "TherapistFirstName", therapistCC.FirstName },
                                        { "TherapistLastName", therapistCC.LastName },
                                        { "TherapistEmail", therapistCC.Email },
                                        { "TherapistPhoneNumber", therapistCC.PhoneNumber },
                                        { "TherapistStreet", therapist.Street },
                                        { "TherapistHouseNumber", therapist.HouseNumber },
                                        { "TherapistCity", therapist.City },
                                        { "TherapistPostalCode", therapist.PostalCode },
                                        { "TherapistCountry", therapist.Country },
                                        { "ClientFirstName", loggedUser.FirstName },
                                        { "ClientLastName", loggedUser.LastName },
                                        { "ContactMethodId", contactMethod.Id },
                                        { "ContactMethodName", contactMethod.Name },
                                        { "ContactMethodColor", contactMethod.Color },
                                        { "ContactMethodIcon", contactMethod.Icon }
                                    },
                                    Description = "authorized",
                                    StatementDescriptor = _configuration.GetSection("Application:AppName")?.Value,
                                };

                                var stripeCustomerId = _stripeFunctions.GetStripeCustomerId(loggedUser.Id);

                                if (!string.IsNullOrWhiteSpace(stripeCustomerId))
                                {
                                    paymentIntentOptions.Customer = stripeCustomerId;

                                    var customerInStripe = await new CustomerService().UpdateAsync(stripeCustomerId, new CustomerUpdateOptions
                                    {
                                        Name = $"{loggedUser.FirstName} {loggedUser.LastName} - {loggedUser.Id}",
                                        Phone = loggedUser.PhoneNumber,
                                        Metadata = new Dictionary<string, string>
                                        {
                                            { "UserId", loggedUser.Id }
                                        }
                                    });

                                    _context.StripeCustomers.GetById(stripeCustomerId).PhoneNumber = loggedUser.PhoneNumber;
                                }
                                else
                                {
                                    var customer = await new CustomerService().CreateAsync(new CustomerCreateOptions
                                    {
                                        Name = $"{loggedUser.FirstName} {loggedUser.LastName} - {loggedUser.Id}",
                                        Email = loggedUser.Email,
                                        Phone = loggedUser.PhoneNumber,
                                        Description = "Generated at Therapists/BookSession",
                                        Metadata = new Dictionary<string, string>
                                        {
                                            { "UserId", loggedUser.Id }
                                        }
                                    });

                                    _context.StripeCustomers.Insert(new StripeCustomers
                                    {
                                        Id = customer.Id,
                                        UserIdOrAnonymous = loggedUser.Id,
                                        Email = loggedUser.Email,
                                        CreatedDateTime = DateTime.UtcNow,
                                        PhoneNumber = customer.Phone
                                    });

                                    paymentIntentOptions.Customer = customer.Id;
                                }

                                paymentIntentInDatabase = _stripeFunctions.GetLatestPaymentIntentByUserId(loggedUser.Id);
                                var newPaymentIntentInserted = false;

                                if(paymentIntentInDatabase != null &&
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
                                            Amount = Convert.ToInt64(sessionToBook.Price * 100),
                                            Currency = "rsd",
                                            SetupFutureUsage = "off_session",
                                            ReceiptEmail = loggedUser.Email,
                                            Metadata = new Dictionary<string, string>
                                            {
                                                { "UserId", loggedUser.Id },
                                                { "SessionId", sessionToBook.Id },
                                                { "BookingId", bookingId },
                                                { "TherapistUserId", therapistCC.Id },
                                                { "TherapistId", therapist.Id },
                                                { "TherapistFirstName", therapistCC.FirstName },
                                                { "TherapistLastName", therapistCC.LastName },
                                                { "TherapistEmail", therapistCC.Email },
                                                { "TherapistPhoneNumber", therapistCC.PhoneNumber },
                                                { "TherapistStreet", therapist.Street },
                                                { "TherapistHouseNumber", therapist.HouseNumber },
                                                { "TherapistCity", therapist.City },
                                                { "TherapistPostalCode", therapist.PostalCode },
                                                { "TherapistCountry", therapist.Country },
                                                { "ClientFirstName", loggedUser.FirstName },
                                                { "ClientLastName", loggedUser.LastName },
                                                { "ContactMethodId", contactMethod.Id },
                                                { "ContactMethodName", contactMethod.Name },
                                                { "ContactMethodColor", contactMethod.Color },
                                                { "ContactMethodIcon", contactMethod.Icon }
                                            },
                                            Description = "authorized",
                                            StatementDescriptor = _configuration.GetSection("Application:AppName")?.Value
                                        });

                                        paymentIntentInDatabase.PhoneNumber = loggedUser.PhoneNumber;
                                        paymentIntentInDatabase.SystemEventTypeName = StripePaymentIntentSystemEventTypeNameProvider.SessionInstantPayment;
                                        paymentIntentInDatabase.MustSucceedUntil = DateTime.UtcNow.AddMinutes(PAYMENT_INTENT_MUST_SUCCEED_IN_MINUTES);
                                    }
                                    else
                                    {
                                        paymentIntent = await new PaymentIntentService().CreateAsync(paymentIntentOptions);

                                        paymentIntentInDatabase = new StripePaymentIntents
                                        {
                                            Id = paymentIntent.Id,
                                            UserIdOrAnonymous = loggedUser.Id,
                                            Email = loggedUser.Email,
                                            PhoneNumber = loggedUser.PhoneNumber,
                                            CustomerId = paymentIntent.CustomerId,
                                            Status = 0,
                                            SystemEventTypeName = StripePaymentIntentSystemEventTypeNameProvider.SessionInstantPayment,
                                            MustSucceedUntil = DateTime.UtcNow.AddMinutes(PAYMENT_INTENT_MUST_SUCCEED_IN_MINUTES),
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
                                        UserIdOrAnonymous = loggedUser.Id,
                                        Email = loggedUser.Email,
                                        PhoneNumber = loggedUser.PhoneNumber,
                                        CustomerId = paymentIntent.CustomerId,
                                        Status = 0,
                                        SystemEventTypeName = StripePaymentIntentSystemEventTypeNameProvider.SessionInstantPayment,
                                        MustSucceedUntil = DateTime.UtcNow.AddMinutes(PAYMENT_INTENT_MUST_SUCCEED_IN_MINUTES),
                                        CreatedDateTime = DateTime.UtcNow
                                    };

                                    _context.StripePaymentIntents.Insert(paymentIntentInDatabase);
                                    newPaymentIntentInserted = true;
                                }

                                sessionToBook.Booked = 1;
                                paymentIntentClientSecret = paymentIntent.ClientSecret;

                                if (!newPaymentIntentInserted)
                                    _context.StripePaymentIntents.Update(paymentIntentInDatabase);

                                hostedService = HttpContext.RequestServices.GetRequiredService<UnbookUnpaidSessionService>();
                                nextHostedServiceExecution = hostedService.GetNextExecution();
                                if (nextHostedServiceExecution < paymentIntentInDatabase.MustSucceedUntil)
                                    nextHostedServiceExecution = hostedService.GetNextExecution(DateTime.UtcNow.AddMinutes(PAYMENT_INTENT_MUST_SUCCEED_IN_MINUTES));

                                _context.Sessions.Update(sessionToBook);
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
                            success = false,
                            notEnoughWebCredit = true,
                            publicKey = _stripeSettings.PublishableKey,
                            clientSecret = paymentIntentClientSecret,
                            amount = paymentIntent.Amount / 100,
                            currency = paymentIntent.Currency,
                            paymentSuccessfulReturnUrl = Url.Action("SessionDetails", "Sessions", new { Area = "", bookingId }, HttpContext.Request.Scheme),
                            mustCompletePaymentUntil = _dateHelper.ConvertDateTimeToDateTimeStringISO(_dateHelper.ConvertDateTimeFromUtcToLocal(nextHostedServiceExecution)),
                            title = "Čestitamo!",
                            body = $"Uspešno ste zakazali seansu sa psihoterapeutom {therapistCC.FirstName} {therapistCC.LastName}.",
                            severity = "success"
                        });
                    } 

                    BookedSessions bookedSession;
                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        try
                        {
                            var transactionId = Helper.GenerateNumbersId();
                            _context.Transactions.Insert(new Transactions
                            {
                                Id = transactionId,
                                SenderId = loggedUser.Id,
                                ReceiverId = PaymentTypesProvider.CounterApathyPayment.Id,
                                DateTime = DateTime.UtcNow,
                                Amount = sessionToBook.Price,
                                CurrencyCode = "RSD"
                            });

                            bookedSession = new BookedSessions
                            {
                                Id = bookingId,
                                SessionId = sessionId,
                                TherapistId = therapist.Id,
                                TherapistFirstName = therapistCC.FirstName,
                                TherapistLastName = therapistCC.LastName,
                                TherapistEmail = therapistCC.Email,
                                TherapistPhoneNumber = therapistCC.PhoneNumber,
                                TherapistStreet = therapist.Street,
                                TherapistHouseNumber = therapist.HouseNumber,
                                TherapistCity = therapist.City,
                                TherapistPostalCode = therapist.PostalCode,
                                TherapistCountry = therapist.Country,
                                ClientId = loggedUser.Id,
                                ClientFirstName = loggedUser.FirstName,
                                ClientLastName = loggedUser.LastName,
                                ClientEmail = loggedUser.Email,
                                ClientPhoneNumber = loggedUser.PhoneNumber,
                                Subject = "Session",
                                Description = null,
                                Price = sessionToBook.Price,
                                Type = sessionToBook.Type,
                                StartTime = sessionToBook.StartDateTime,
                                EndTime = sessionToBook.EndDateTime,
                                BookingDate = DateTime.UtcNow,
                                ContactMethodId = contactMethod.Id,
                                ContactMethodName = contactMethod.Name,
                                ContactMethodColor = contactMethod.Color,
                                ContactMethodIcon = contactMethod.Icon,
                                //ContactInfo = !string.IsNullOrWhiteSpace(publicVm.ContactInfo) ? publicVm.ContactInfo.Trim() : null,
                                Status = 0,
                                TherapistIsPaid = false,
                                ClientBookingTransactionId = transactionId
                            };

                            _context.BookedSessions.Insert(bookedSession);
                            _context.BookedSessionsContactMethods.Insert(new BookedSessionsContactMethods
                            {
                                BookedSessionId = bookedSession.Id,
                                ContactMethodId = contactMethod.Id
                            });

                            // must be placed here
                            loggedUser.WebCredit -= sessionToBook.Price;

                            _context.WebCreditLogs.Insert(new WebCreditLogs
                            {
                                Id = Helper.GenerateNumbersId(),
                                UserId = loggedUser.Id,
                                Email = loggedUser.Email,
                                Amount = sessionToBook.Price * -1,
                                ExecutionDateTime = DateTime.UtcNow,
                                PaymentTypeId = PaymentTypesProvider.CounterApathyPayment.Id,
                                PaymentTypeName = PaymentTypesProvider.CounterApathyPayment.Name,
                                CurrentAmount = loggedUser.WebCredit.Value,
                                TransactionId = transactionId
                            });

                            sessionToBook.Booked = 1;

                            using (var scope2 = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
                            {
                                try
                                {
                                    var webCreditUpdated = await _userManager.UpdateAsync(loggedUser);

                                    if (!webCreditUpdated.Succeeded)
                                        throw new Exception(string.Join("|", webCreditUpdated.Errors.Select(e => e.Description)));

                                    scope2.Complete();
                                }
                                catch (Exception)
                                {
                                    scope2.Dispose();
                                    throw;
                                }
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

                    // Send email to user
                    await _mailService.SendClientBookedSessionEmailAsync(new Framework.Emails.EmailTypes.BookedSessionEmail
                    {
                        BookedSession = bookedSession
                    }, includeTemplateIfExists: true);

                    // Send email to therapist
                    await _mailService.SendTherapistSessionBookedEmailAsync(new Framework.Emails.EmailTypes.BookedSessionEmail
                    {
                        BookedSession = bookedSession
                    }, includeTemplateIfExists: true);

                    var sessionStart = _dateHelper.ConvertDateTimeFromUtcToLocal(sessionToBook.StartDateTime);

                    // Send notification to user
                    await _notificationRepository.SendAsync(new Notifications
                    {
                        Id = Helper.GenerateNumbersId(),
                        SenderUserId = SystemInformation.Name,
                        ReceiverUserId = loggedUser.Id,
                        Title = $"Uspešno ste zakazali seansu sa psihoterapeutom {therapistCC.FirstName} {therapistCC.LastName}, kontakt metoda je {contactMethod.Name}. " +
                        $"Seansa počinje datuma {_dateHelper.DateStringFromDateTime(sessionStart)}, u {_dateHelper.TimeStringFromDateTime(sessionStart)}h. " +
                        "Za više detalja, proverite imejl koji Vam je upravo stigao u inboks ili spam.",
                        Body = null,
                        Severity = "success",
                        Read = false,
                        SendingDateTime = DateTime.UtcNow,
                        Icon = "fal fa-calendar-check",
                        Important = false
                    });

                    // Send notification to therapist
                    await _notificationRepository.SendAsync(new Notifications
                    {
                        Id = Helper.GenerateNumbersId(),
                        SenderUserId = SystemInformation.Name,
                        ReceiverUserId = therapistCC.Id,
                        Title = $"Klijent {loggedUser.FirstName} {loggedUser.LastName} je zakazao seansu sa Vama. Kontakt metoda je {contactMethod.Name}. " +
                        $"Seansa počinje datuma {_dateHelper.DateStringFromDateTime(sessionStart)}, u {_dateHelper.TimeStringFromDateTime(sessionStart)}h. " +
                        "Za više detalja, proverite imejl koji Vam je upravo stigao u inboks ili spam.",
                        Body = null,
                        Severity = "success",
                        Read = false,
                        SendingDateTime = DateTime.UtcNow,
                        Icon = "fal fa-calendar-check",
                        Important = false
                    });

                    return Json(new
                    {
                        success = true,
                        title = "Čestitamo!",
                        body = $"Uspešno ste zakazali seansu sa psihoterapeutom {therapistCC.FirstName} {therapistCC.LastName}.",
                        severity = "success",
                        redirectUrl = Url.Action("SessionDetails", "Sessions", new { Area = "", bookingId })
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
            catch (StripeException se)
            {
                if (se.GetRootException().Message.ToLowerInvariant().Contains("email"))
                    return Json(new
                    {
                        success = false,
                        title = "Vaša imejl adresa nije validna",
                        body = $"Molimo kontaktirajte korisničku podršku.",
                        severity = "info"
                    });
                else if (se.GetRootException().Message.ToLowerInvariant().Contains("phone"))
                    return Json(new
                    {
                        success = false,
                        title = "Vaš broj telefona nije validan",
                        body = $"Molimo da na svom profilu ažurirate broj telefona i pokušate ponovo.",
                        severity = "info"
                    });
                else return await HandleErrorJsonAsync(se);
            }
            catch (Exception e)
            {
                await HandleErrorJsonAsync(e);

                return Json(new
                {
                    success = false,
                    title = "Došlo je do greške",
                    body = "Molimo osvežite stranicu i pokušajte ponovo ili kontaktirajte korisničku podršku.",
                    severity = "error"
                });
            }
        }
    }
}
