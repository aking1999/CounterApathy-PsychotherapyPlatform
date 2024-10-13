using Microsoft.AspNetCore.Mvc;
using WebApplication9.Base;
using Framework.Notifications;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Database.Models;
using Framework.Helpers.ExtensionMethods;
using Framework.Emails;
using System;
using System.Linq;
using Framework.Interfaces;
using Framework.Models;
using Framework.Implementations;
using WebApplication9.Interfaces;
using WebApplication9.Implementations;
using Framework.ActionFilters;
using DataTransferObjects.ViewModels.Client;
using Microsoft.AspNetCore.Http;
using DataTransferObjects.ViewModels.Shared;
using Framework.Helpers;
using System.Transactions;
using Framework.Providers;

namespace WebApplication9.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IDropdownHelper _dropdown;
        private readonly ICustomClientFunctionsProvider _clientFunctions;
        private readonly ITherapistFunctionsProvider _therapistFunctions;

        public HomeController(IErrorLogger error,
            IMailService mailService,
            IDateTimeHelper dateHelper,
            IHttpContextAccessor contextAccessor,
            INotificationRepository notificationRepository,
            UserManager<CustomClient> userManager,
            SignInManager<CustomClient> signInManager) : base(error, mailService, dateHelper, contextAccessor, notificationRepository, userManager, signInManager)
        {
            _dropdown = new DropdownHelper();

            _clientFunctions = new CustomClientFunctionsProvider(contextAccessor);
            _therapistFunctions = new TherapistFunctionsProvider();
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                ShowToastOnThisPageIfSet();

                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
                {
                    var indexVm = new IndexViewModel
                    {
                        TherapistCount = _context.Therapists.CountEntities(),
                        SessionCount = _context.BookedSessions.CountEntities(),
                        ClientCount = _context.AspNetUsers.CountEntities(),
                        PsychotherapyTechniquesCount = _context.PsychotherapyTechniques.CountEntities(),
                        SpecialtiesCount = _context.Specialities.CountEntities(),
                        TherapistReviewsCount = _context.Ratings.CountEntities() +
                                                _context.PendingRatings.CountEntities(),
                        ContactMethodsCount = _context.ContactMethods.CountEntities(),
                        TherapistsWithConsultations = _therapistFunctions.GetTherapistsWithUpcomingConsultations()
                    };

                    foreach (var tempItem in _context.PsychotherapyTechniques
                                                     .ReadOnlyGetAll()
                                                     .GroupJoin(_context.TherapistPsychotherapyTechniques.ReadOnlyGetAll(),
                                                                psyTech => psyTech.Id,
                                                                thrPsyTech => thrPsyTech.PsychotherapyTechniqueId,
                                                                (technique, therapists) => new
                                                                {
                                                                    Filter = "psihoterapijska-tehnika",
                                                                    Predicate = technique.Id,
                                                                    Name = technique.Name,
                                                                    Color = technique.Color,
                                                                    Icon = technique.Icon,
                                                                    TherapistIds = therapists.Select(i => i.TherapistId)
                                                                }))
                    {
                        var therapistSkill = new SkillTherapistsViewModel
                        {
                            Filter = tempItem.Filter,
                            Predicate = tempItem.Predicate,
                            Name = tempItem.Name,
                            Color = tempItem.Color,
                            Icon = tempItem.Icon
                        };

                        foreach (var therapistId in tempItem.TherapistIds)
                        {
                            //ovo da popravim da stavim _usermanager.findbytherapistaccountid
                            var therapistUser = _context.AspNetUsers.ReadOnlyFind(usr => usr.TherapistAccountId == therapistId).SingleOrDefault();

                            if(therapistUser != default)
                            {
                                therapistSkill.Therapists.Add(new TherapistShowcaseViewModel
                                {
                                    Id = therapistUser.Id,
                                    FirstName = therapistUser.FirstName,
                                    LastName = therapistUser.LastName,
                                    ProfilePhotoPath = therapistUser.ProfilePhoto
                                });
                            }
                        }

                        indexVm.PsychotherapyTechniquesTherapists.Add(therapistSkill);
                    }

                    foreach (var tempItem in _context.Specialities
                                                     .ReadOnlyGetAll()
                                                     .GroupJoin(_context.TherapistsSpecialities.ReadOnlyGetAll(),
                                                                spec => spec.Id,
                                                                thrSpec => thrSpec.SpecialityId,
                                                                (specialty, therapists) => new
                                                                {
                                                                    Filter = "specijalnost",
                                                                    Predicate = specialty.Id,
                                                                    Name = specialty.Name,
                                                                    Color = specialty.Color,
                                                                    Icon = specialty.Icon,
                                                                    TherapistIds = therapists.Select(i => i.TherapistId)
                                                                }))
                    {
                        var therapistSkill = new SkillTherapistsViewModel
                        {
                            Filter = tempItem.Filter,
                            Predicate = tempItem.Predicate,
                            Name = tempItem.Name,
                            Color = tempItem.Color,
                            Icon = tempItem.Icon
                        };

                        foreach(var therapistId in tempItem.TherapistIds)
                        {
                            var therapistUser = _context.AspNetUsers.ReadOnlyFind(usr => usr.TherapistAccountId == therapistId).SingleOrDefault();

                            if (therapistUser != default)
                            {
                                therapistSkill.Therapists.Add(new TherapistShowcaseViewModel
                                {
                                    Id = therapistUser.Id,
                                    FirstName = therapistUser.FirstName,
                                    LastName = therapistUser.LastName,
                                    ProfilePhotoPath = therapistUser.ProfilePhoto
                                });
                            }
                        }

                        indexVm.SpecialtiesTherapists.Add(therapistSkill);
                    }

                    return View(indexVm);
                }
                else if (emailStatus == EmailConfirmationStatus.NotConfirmed)
                    return RedirectToAction("ConfirmEmail", "Authorization", new { Area = "" });
                else throw new GeneralException("Unable to load user.", signOutUser: User.Identity.IsAuthenticated);
            }
            catch (Exception e)
            {
                await HandleErrorAsync(e);
                return View(new IndexViewModel
                {
                    TherapistCount = 10,
                    SessionCount = 35,
                    ClientCount = 12,
                    PsychotherapyTechniquesCount = 10,
                    SpecialtiesCount = 14,
                    TherapistReviewsCount = 20
                });
            }
        }

        [HttpGet("/o-nama")]
        public async Task<IActionResult> About()
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
                {
                    ShowToastOnThisPageIfSet();
                    return View();
                }
                else if (emailStatus == EmailConfirmationStatus.NotConfirmed)
                    return RedirectToAction("ConfirmEmail", "Authorization", new { Area = "" });
                else throw new GeneralException("Unable to load user.", signOutUser: User.Identity.IsAuthenticated);
            }
            catch(Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        //[HttpGet("/laka-navigacija-sajta")]
        //public async Task<IActionResult> EasySiteNavigation()
        //{
        //    try
        //    {
        //        var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
        //        if (emailStatus == EmailConfirmationStatus.Confirmed)
        //        {
        //            ShowToastOnThisPageIfSet();
        //            var role = await User.GetRoleAsync();

        //            if (_signInManager.IsSignedIn(User) && !string.IsNullOrWhiteSpace(role))
        //            {
        //                if (role == UserRoles.Admin)
        //                    return View("EasySiteNavigationAdmin");
        //                else if (role == UserRoles.Therapist)
        //                    return View("EasySiteNavigationTherapist");
        //            }

        //            return View("EasySiteNavigationClientAndAnonymous");
        //        }
        //        else if (emailStatus == EmailConfirmationStatus.NotConfirmed)
        //            return RedirectToAction("ConfirmEmail", "Authorization", new { Area = "" });
        //        else throw new GeneralException("Unable to load user.", signOutUser: User.Identity.IsAuthenticated);
        //    }
        //    catch (Exception e)
        //    {
        //        return await HandleErrorAsync(e);
        //    }
        //}

        [HttpGet("/česta-pitanja")]
        public async Task<IActionResult> FAQ()
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
                {
                    ShowToastOnThisPageIfSet();
                    return View();
                }
                else if (emailStatus == EmailConfirmationStatus.NotConfirmed)
                    return RedirectToAction("ConfirmEmail", "Authorization", new { Area = "" });
                else throw new GeneralException("Unable to load user.", signOutUser: User.Identity.IsAuthenticated);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpGet("/aplikacije-za-terapeutske-naloge")]
        public async Task<IActionResult> TherapistApplications()
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
                {
                    ShowToastOnThisPageIfSet();
                    return View(new TherapistApplicationChoicesViewModel
                    {
                        PsychotherapistCount = _context.Therapists.CountEntities(),
                        PsychotherapistClientCount = _context.AspNetUsers.CountEntities()
                    });
                }
                else if (emailStatus == EmailConfirmationStatus.NotConfirmed)
                    return RedirectToAction("ConfirmEmail", "Authorization", new { Area = "" });
                else throw new GeneralException("Unable to load user.", signOutUser: User.Identity.IsAuthenticated);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpGet("/uslovi-korišćenja")]
        public async Task<IActionResult> TermsOfService()
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
                {
                    ShowToastOnThisPageIfSet();
                    return View();
                }
                else if (emailStatus == EmailConfirmationStatus.NotConfirmed)
                    return RedirectToAction("ConfirmEmail", "Authorization", new { Area = "" });
                else throw new GeneralException("Unable to load user.", signOutUser: User.Identity.IsAuthenticated);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        //[HttpGet("/uslovi-korišćenja-za-psihoterapeute")]
        [HttpGet("/uslovi-za-terapeute")]
        public async Task<IActionResult> TherapistsTerms()
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
                {
                    ShowToastOnThisPageIfSet();

                    var techniquesConcatenated = string.Empty;

                    //foreach (var technique in _context.PsychotherapyTechniques.ReadOnlyGetAll().Select(tech => tech.Name))
                    //{
                    //    techniquesConcatenated = techniquesConcatenated + technique + " | ";
                    //}
                    ViewBag.techniques = string.Join(", ", _context.PsychotherapyTechniques.ReadOnlyGetAll().Select(tech => tech.Name));
                    ViewBag.specialties = string.Join(", ", _context.Specialities.ReadOnlyGetAll().Select(spec => spec.Name));

                    return View();
                }
                else if (emailStatus == EmailConfirmationStatus.NotConfirmed)
                    return RedirectToAction("ConfirmEmail", "Authorization", new { Area = "" });
                else throw new GeneralException("Unable to load user.", signOutUser: User.Identity.IsAuthenticated);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpGet("/politika-privatnosti")]
        public async Task<IActionResult> Privacy()
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
                {
                    ShowToastOnThisPageIfSet();
                    return View();
                }
                else if (emailStatus == EmailConfirmationStatus.NotConfirmed)
                    return RedirectToAction("ConfirmEmail", "Authorization", new { Area = "" });
                else throw new GeneralException("Unable to load user.", signOutUser: User.Identity.IsAuthenticated);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        //[HttpGet("/developer")]
        //public async Task<IActionResult> Developer()
        //{
        //    try
        //    {
        //        var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
        //        if (emailStatus == EmailConfirmationStatus.Confirmed)
        //        {
        //            ShowToastOnThisPageIfSet();
        //            return View();
        //        }
        //        else if (emailStatus == EmailConfirmationStatus.NotConfirmed)
        //            return RedirectToAction("ConfirmEmail", "Authorization", new { Area = "" });
        //        else throw new GeneralException("Unable to load user.", signOutUser: User.Identity.IsAuthenticated);
        //    }
        //    catch (Exception e)
        //    {
        //        return await HandleErrorAsync(e);
        //    }
        //}

        [HttpGet("/korisnička-podrška")]
        [AllowAnonymousAndCustomClient]
        public async Task<IActionResult> CustomerSupport()
        {
            try
            {
                ShowToastOnThisPageIfSet();

                var isSignedIn = User.Identity.IsAuthenticated;
                var user = isSignedIn ? await _userManager.GetUserAsync(User) : null;

                if (isSignedIn && user == null)
                    throw new GeneralException("Unable to load user.", signOutUser: true);

                return View(new CustomerSupportViewModel
                {
                    FirstName = isSignedIn ? user.FirstName : null,
                    LastName = isSignedIn ? user.LastName : null,
                    Email = isSignedIn ? user.Email : null,
                    ToChooseFrom_Topics = isSignedIn ? _clientFunctions.Get_ToChooseFrom_Topics() :
                        _clientFunctions.Get_ToChooseFrom_Topics().Where(t => t.Value != ClientSupportTicketTopicsProvider.RequestAccountDeletion.Id).ToList()
                });
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpPost("/korisnička-podrška")]
        [ValidateAntiForgeryToken]
        [AllowAnonymousAndCustomClient]
        public async Task<JsonResult> CustomerSupport(CustomerSupportViewModel support)
        {
            try
            {
                var isSignedIn = User.Identity.IsAuthenticated;
                var user = isSignedIn ? await _userManager.GetUserAsync(User) : null;

                if (isSignedIn)
                {
                    if (user == null) throw new GeneralException("Unable to load user.", signOutUser: true);

                    support.FirstName = user.FirstName;
                    support.LastName = user.LastName;
                    support.Email = user.Email;
                }

                if (!ModelState.IsValid)
                    return Json(new
                    {
                        success = false,
                        title = "Popunite sva obavezna polja",
                        body = "",
                        severity = "info"
                    });

                if(!isSignedIn && support.Chosen_TopicId == ClientSupportTicketTopicsProvider.RequestAccountDeletion.Id)
                    return Json(new
                    {
                        success = false,
                        title = "Ne možete poslati zahtev za gašenje naloga kad niste ulogovani",
                        body = "Ulogujte se i pošaljite zahtev za gašenje naloga",
                        severity = "info"
                    });

                if (!_dropdown.CustomerSupportTicketTopicExistsInDatabase(support.Chosen_TopicId))
                {
                    _session.SetToast("Izabrana tema nije validna", "Molimo pokušajte ponovo", "info");

                    return Json(new
                    {
                        success = false,
                        redirectUrl = Url.Action("CustomerSupport", "Home", new { Area = "" })
                    });
                }

                _context.ClientSupportTickets.Insert(new ClientSupportTickets
                {
                    Id = Helper.GenerateNumbersId(),
                    UserIdOrAnonymous = isSignedIn ? user.Id : "Anonymous",
                    FirstName = isSignedIn ? user.FirstName : support.FirstName,
                    LastName = isSignedIn ? user.LastName : support.LastName,
                    Email = isSignedIn ? user.Email : support.Email,
                    Text = support.Text,
                    TopicId = support.Chosen_TopicId,
                    TopicName = _context.ClientSupportTicketTopics.GetById(support.Chosen_TopicId).Name,
                    TicketDateTime = DateTime.UtcNow
                });

                await _context.SaveAsync();

                if (isSignedIn)
                    await _notificationRepository.SendAsync(new Notifications
                    {
                        Id = Helper.GenerateNumbersId(),
                        SenderUserId = "System",
                        ReceiverUserId = user.Id,
                        Title = "Vaša poruka je uspešno poslata našem timu za korisničku podršku.",
                        Body = null,
                        Severity = "pink",
                        Read = false,
                        SendingDateTime = DateTime.UtcNow,
                        Icon = "fal fa-user-headset",
                        Important = false
                    });

                return Json(new
                {
                    success = true,
                    title = "Uspešno ste poslali poruku našem timu za korisničku podršku",
                    body = "Naš predstavnik će Vas kontaktirati putem imejla.",
                    severity = "success",
                    redirectUrl = Url.Action("Index", "Home", new { Area = "" })
                });
            }
            catch (Exception e)
            {
                return await HandleErrorJsonAsync(e);
            }
        }

        [HttpPost("/pretplata-na-novosti")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> NewsletterSubscription(NewsletterSubscriptionViewModel subscribe)
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
                            title = "Unesite validan imejl",
                            body = "",
                            severity = "info"
                        });

                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        try
                        {
                            if (_context.NewsletterSubscribers.ReadOnlyAny(l => l.NormalizedEmail == subscribe.Email.ToUpper()))
                                return Json(new
                                {
                                    success = false,
                                    title = "Imejl je već pretplaćen na primanje novosti",
                                    body = "",
                                    severity = "info"
                                });

                            _context.NewsletterSubscribers.Insert(new NewsletterSubscribers
                            {
                                Id = Helper.GenerateNumbersId(),
                                Email = subscribe.Email,
                                NormalizedEmail = subscribe.Email.ToUpper(),
                                NotifiedCount = 0,
                                IpAddress = HttpContext.Connection?.RemoteIpAddress.ToString().TakeMax(256),
                                SubscribeDateTime = DateTime.UtcNow
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

                    return Json(new
                    {
                        success = true,
                        title = "Pretplata na primanje novosti uspešna",
                        body = "",
                        severity = "success"
                    });
                }
                else return Json(new
                {
                    success = false,
                    title = "Ne možete se pretplatiti na primanje novosti",
                    body = "Morate prvo potvrditi imejl.",
                    severity = "error"
                });
            }
            catch (Exception e)
            {
                await HandleErrorJsonAsync(e);

                return Json(new
                {
                    success = false,
                    title = "Došlo je do greške",
                    body = "Molimo osvežite stranicu i pokušajte ponovo.",
                    severity = "error"
                });
            }
        }
    }
}
