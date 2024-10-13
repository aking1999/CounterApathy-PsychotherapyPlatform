using Database.Models;
using Framework.Helpers.ExtensionMethods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebApplication9.Areas.Therapist.ViewModels;
using WebApplication9.Helpers;
using WebApplication9.ViewModels;
using WebApplication9.Base;
using Framework.Notifications;
using Framework.Emails;
using WebApplication9.Interfaces;
using WebApplication9.Implementations;
using Framework.Interfaces;
using Framework.Models;
using DataTransferObjects.ViewModels.Therapist;
using WebApplication9.Areas.Therapist.Helpers;
using Microsoft.AspNetCore.Http;
using Framework.Implementations;
using Framework.Helpers;

namespace WebApplication9.Areas.Therapist.Controllers
{
    [Area(areaName: "Therapist")]
    [Authorize(Roles = "Therapist")]
    public class AccountController : BaseController
    {
        private readonly IFileRepository _files;
        private readonly IDropdownHelper _dropdown;
        private readonly IWebHostEnvironment _environment;
        private readonly ITherapistFunctionsProvider _therapistFunctions;

        public AccountController(IErrorLogger error,
            IMailService mailService,
            IDateTimeHelper dateHelper,
            IWebHostEnvironment environment,
            IHttpContextAccessor contextAccessor,
            INotificationRepository notificationRepository,
            UserManager<CustomClient> userManager,
            SignInManager<CustomClient> signInManager) : base(error, mailService, dateHelper, contextAccessor, notificationRepository, userManager, signInManager)
        {
            _environment = environment;
            _files = new FileRepository();
            _dropdown = new DropdownHelper();
            _therapistFunctions = new TherapistFunctionsProvider();
        }

        [HttpGet("/terapeut/nalog")]
        public async Task<IActionResult> Profile()
        {
            try
            {
                ShowToastOnThisPageIfSet();

                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                {
                    var user = await _userManager.GetUserAsync(User);

                    var profile = new TherapistProfileViewModel();
                    profile.Map(user);
                    await profile.MapAsync(_context.Therapists.GetById(user.TherapistAccountId));

                    return View(profile);
                }
                else if (completionStatus == TherapistAccountCompletionStatus.NotSetUpYet)
                    return RedirectToAction("AccountSetup");
                else throw new GeneralException("Unable to load user.", signOutUser: true);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpPost("/terapeut/nalog/ažuriranje")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TherapistProfileViewModel profile)
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                {
                    var therapistUser = await _userManager.GetUserAsync(User);

                    if (!ModelState.IsValid)
                    {
                        profile.MapProfilePhoto(therapistUser);

                        profile.PsychotherapyTechniques = _therapistFunctions.GetPsychotherapyTechniques(therapistUser.TherapistAccountId);
                        profile.Specialities = _therapistFunctions.GetSpecialties(therapistUser.TherapistAccountId);
                        profile.ToChooseFrom_ContactMethods = _dropdown.GetContactMethodsForDropdown(therapistUser.TherapistAccountId);

                        _session.SetToast("Profil nije ažuriran", "Proverite sve unete podatke i pokušajte ponovo.", "info");
                        ViewBag.toast = _session.GetToast();
                        _session.RemoveToastFromKeys();

                        return View("Profile", profile);
                    }

                    await therapistUser.MapAsync(profile, _environment);

                    var therapist = _context.Therapists.GetById(therapistUser.TherapistAccountId);
                    therapist.Map(profile);

                    if (_therapistFunctions.HasContactMethods(therapist.Id))
                        await _therapistFunctions.DeleteTherapistContactMethods(therapist.Id);

                    bool hasAtleastOneContactMethod = false;

                    foreach (var contactMethodId in profile.Chosen_ContactMethodsIds)
                    {
                        if (_dropdown.ContactMethodExistsInDatabase(contactMethodId))
                        {
                            hasAtleastOneContactMethod = true;
                            _context.TherapistsContactMethods.Insert(new TherapistsContactMethods
                            {
                                TherapistId = therapist.Id,
                                ContactMethodId = contactMethodId
                            });
                        }
                    }

                    if (!hasAtleastOneContactMethod)
                    {
                        _session.SetToast("Izabrane kontakt metode nisu validne",
                            "Molimo osvežite stranicu i pokušajte ponovo.",
                            "error");
                        return View("Profile", profile);
                    }

                    var update = await _userManager.UpdateAsync(therapistUser);
                    _context.Therapists.Update(therapist);

                    if ((await _context.SaveAsync()) < 1 || !update.Succeeded)
                        throw new GeneralException(string.Join("|", update.Errors.Select(e => e.Description)));

                    _session.SetToast("Profil uspešno ažuriran", null, "success");
                    return RedirectToAction("Profile", "Account", new { Area = "Therapist" });
                }
                else if (completionStatus == TherapistAccountCompletionStatus.NotSetUpYet)
                    return RedirectToAction("AccountSetup");
                else throw new GeneralException("Unable to load user.", signOutUser: true);

            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpGet("/terapeut/nalog/promena-lozinke")]
        public async Task<IActionResult> ChangePassword()
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                {
                    if (!await _userManager.HasPasswordAsync(await _userManager.GetUserAsync(User)))
                        throw new GeneralException("User does not have a password.", signOutUser: true);

                    return View(new ChangePasswordViewModel());
                }
                else if (completionStatus == TherapistAccountCompletionStatus.NotSetUpYet)
                    return RedirectToAction("AccountSetup");
                else throw new GeneralException("Unable to load user.", signOutUser: true);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpPost("/terapeut/nalog/promena-lozinke")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel viewModel)
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                {
                    if (!ModelState.IsValid)
                        return View(viewModel);

                    var user = await _userManager.GetUserAsync(User);

                    var change = await _userManager.ChangePasswordAsync(user, viewModel.OldPassword, viewModel.NewPassword);

                    if (!change.Succeeded)
                    {
                        //Kad se unese pogresna sifra, ovde izbaci gresku Incorrect password., tj.
                        //gresku koja nije prevedena na srpski, ali za sad nije bitno
                        var errors = string.Join('|', change.Errors.Select(e => e.Description));
                        ModelState.AddModelError(string.Empty, errors);

                        await HandleErrorAsync(errors);
                        return View(viewModel);
                    }

                    await _signInManager.RefreshSignInAsync(user);

                    _session.SetToast("Lozinka uspešno promenjena", null, "success");

                    //Setting <hidden> so that UserActivityLoggerAttribute.OnResultExecuted does not
                    //save the raw passwords into column QueryDataJson in database but <hidden>
                    viewModel.OldPassword = "<hidden>";
                    viewModel.NewPassword = "<hidden>";
                    viewModel.ConfirmPassword = "<hidden>";

                    await _notificationRepository.SendAsync(new Notifications
                    {
                        Id = Helper.GenerateNumbersId(),
                        SenderUserId = "System",
                        ReceiverUserId = user.Id,
                        Title = "Lozinka uspešno promenjena.",
                        Body = null,
                        Severity = "primary",
                        Read = false,
                        SendingDateTime = DateTime.UtcNow,
                        Icon = "fal fa-lock",
                        Important = false
                    });

                    await _mailService.SendPasswordChangedEmailAsync(new Framework.Emails.EmailTypes.PasswordChangedEmail
                    {
                        ToEmail = user.Email,
                        FirstName = user.FirstName
                    }, includeTemplateIfExists: true);

                    return RedirectToAction("Profile", "Account", new { Area = "Therapist" });
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

        [HttpGet("/terapeut/nalog/podešavanje-naloga")]
        public async Task<IActionResult> AccountSetup()
        {
            try
            {
                ShowToastOnThisPageIfSet();

                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                {
                    _session.SetToast("Vaš terapeutski nalog je već podešen", "Ukoliko želite ažurirati nalog, učinite to na profilnoj stranici.", "info");
                    return RedirectToAction("Profile", "Account", new { Area = "Therapist" });
                }
                else if(completionStatus == TherapistAccountCompletionStatus.NotSetUpYet)
                {
                    var setup = new TherapistAccountSetupViewModel();

                    foreach (var contactMethod in _context.ContactMethods.ReadOnlyGetAll())
                    {
                        setup.ToChooseFrom_ContactMethods.Add(new SelectListItem
                        {
                            Text = contactMethod.Icon + "|" + contactMethod.Name + "|" + contactMethod.Color,
                            Value = contactMethod.Id
                        });
                    }

                    return View(setup);
                }
                else throw new GeneralException("Unable to load user.", signOutUser: true);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpPost("/terapeut/nalog/podešavanje-naloga")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> AccountSetup([FromForm] TherapistAccountSetupViewModel accSetupVm)
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                {
                    _session.SetToast("Vaš terapeutski nalog je već podešen", "Ukoliko želite ažurirati nalog, učinite to na profilnoj stranici.", "info");
                    return Json(new
                    {
                        success = false,
                        redirectUrl = Url.Action("Profile", "Account", new { Area = "Therapist" })
                    });
                }
                else if (completionStatus == TherapistAccountCompletionStatus.NotSetUpYet)
                {
                    if (!ModelState.IsValid || !accSetupVm.Chosen_ContactMethodsIds.Any())
                        return Json(new
                        {
                            success = false,
                            title = "Popunite sva obavezna polja",
                            body = "",
                            severity = "info"
                        });

                    var user = await _userManager.GetUserAsync(User);

                    var therapist = _context.Therapists.GetById(user.TherapistAccountId);

                    var hasAtleastOneContactMethod = false;

                    foreach (var contactMethodId in accSetupVm.Chosen_ContactMethodsIds[0].Split(','))
                    {
                        if (_dropdown.ContactMethodExistsInDatabase(contactMethodId))
                        {
                            hasAtleastOneContactMethod = true;
                            _context.TherapistsContactMethods.Insert(new TherapistsContactMethods
                            {
                                TherapistId = user.TherapistAccountId,
                                ContactMethodId = contactMethodId
                            });
                        }
                    }

                    if (!hasAtleastOneContactMethod)
                    {
                        return Json(new
                        {
                            success = false,
                            title = "Izabrane kontakt metode nisu validne",
                            body = "",
                            severity = "info"
                        });
                    }

                    if (_therapistFunctions.HasContactMethods(user.TherapistAccountId))
                        await _therapistFunctions.DeleteTherapistContactMethods(user.TherapistAccountId);

                    therapist.About = accSetupVm.About;

                    await _context.SaveAsync();

                    await _notificationRepository.SendAsync(new Notifications
                    {
                        Id = Helper.GenerateNumbersId(),
                        SenderUserId = "System",
                        ReceiverUserId = user.Id,
                        Title = "Vaš terapeutski nalog je uspešno podešen.",
                        Body = null,
                        Severity = "primary",
                        Read = false,
                        SendingDateTime = DateTime.UtcNow,
                        Icon = "fal fa-wrench",
                        Important = false
                    });

                    return Json(new
                    {
                        success = true,
                        title = "Uspešno ste podesili Vaš terapeutski nalog!",
                        body = "Da li biste želeli da sad povežete bankovni račun ili kasnije?",
                        severity = "success",
                        redirectUrlPrimary = Url.Action("PaymentGatewaySetupPage", "Withdrawals", new { Area = "Therapist" }),
                        redirectUrlSecondary = Url.Action("All", "Sessions", new { Area = "" })
                    });
                }
                else throw new GeneralException("Unable to load user.", signOutUser: true);
            }
            catch (Exception e)
            {
                return await HandleErrorJsonAsync(e);
            }
        }

        [HttpPost("/terapeut/nalog/brisanje-naloga")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DeleteAccount()
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                {
                    var therapistUser = await _userManager.GetUserAsync(User);
                    var therapist = _context.Therapists.GetById(therapistUser.TherapistAccountId);

                    if (therapist.Earnings > 0)
                        return Json(new
                        {
                            success = false,
                            hasEarnings = true,
                            title = "Možete zahtevati gašenje naloga nakon što su Vam sva novčana sredstva isplaćena",
                            body = $"Trenutno imate sredstva u iznosu od RSD {therapist.Earnings}. " +
                            $"Pošaljite zahtev za isplatu sredstava i nakon što on bude odobren, " +
                            $"možete poslati zahtev za gašenje naloga.",
                            severity = "warning",
                            withdrawUrl = Url.Action("Withdraw", "Withdrawals", new { Area = "Therapist" })
                        });

                    var now = DateTime.UtcNow;

                    if (_context.BookedConsultations.ReadOnlyAny(c => c.TherapistId == therapist.Id && c.StartDateTime > now))
                        return Json(new
                        {
                            success = false,
                            hasBookedSessions = true,
                            title = "Da biste zahtevali gašenje naloga, sve Vaše zakazane besplatne konsultacije moraju da se završe",
                            body = $"Sačekajte da Vam se sve zakazane besplatne konsultacije završe i pokušajte ponovo.",
                            severity = "warning",
                            allBookedSessionsUrl = Url.Action("BookedConsultations", "Consultations", new { Area = "Therapist" })
                        });

                    if(_context.BookedSessions.ReadOnlyAny(s => s.TherapistId == therapist.Id && s.StartTime > now))
                        return Json(new
                        {
                            success = false,
                            hasBookedSessions = true,
                            title = "Da biste zahtevali gašenje naloga, sve Vaše zakazane seanse moraju da se završe",
                            body = $"Sačekajte da Vam se sve zakazane seanse završe i pokušajte ponovo.",
                            severity = "warning",
                            allBookedSessionsUrl = Url.Action("BookedSessions", "Sessions", new { Area = "Therapist" })
                        });

                    if (_context.BookedSessions.ReadOnlyAny(s => s.TherapistId == therapist.Id && (s.TherapistIsPaid == null || !s.TherapistIsPaid.Value)))
                        return Json(new
                        {
                            success = false,
                            hasBookedSessions = true,
                            title = "Imate neisplaćene seanse",
                            body = $"Da biste zahtevali gašenje naloga, sve Vaše zakazane seanse moraju biti isplaćene.",
                            severity = "warning",
                            allBookedSessionsUrl = Url.Action("BookedSessions", "Sessions", new { Area = "Therapist" })
                        });

                    if(_context.Consultations.ReadOnlyAny(c => c.TherapistId == therapist.Id && c.StartDateTime > now))
                        return Json(new
                        {
                            success = false,
                            hasActiveSessions = true,
                            title = "Da biste zahtevali gašenje naloga, ne smete imati nijedan aktivan termin besplatnih konsultacija",
                            body = $"Obrišite sve aktivne termine besplatnih konsultacija koje imate i pokušajte ponovo.",
                            severity = "warning",
                            allSessionsUrl = Url.Action("BookedConsultations", "Consultations", new { Area = "Therapist" })
                        });

                    if (_context.Sessions.ReadOnlyAny(s => s.TherapistId == therapist.Id && s.StartDateTime > now))
                        return Json(new
                        {
                            success = false,
                            hasActiveSessions = true,
                            title = "Da biste zahtevali gašenje naloga, ne smete imati nijednu aktivnu seansu",
                            body = $"Obrišite sve aktivne seanse koje imate i pokušajte ponovo.",
                            severity = "warning",
                            allSessionsUrl = Url.Action("All", "Sessions", new { Area = "Therapist" })
                        });

                    return Json(new
                    {
                        success = true,
                        therapistSupportUrl = Url.Action("Support", "Account", new { Area = "Therapist" })
                    });
                }
                else if (completionStatus == TherapistAccountCompletionStatus.NotSetUpYet)
                    throw new GeneralException("Therapist account not set up but attempted account deletion.");
                else throw new GeneralException("Unable to load user.", signOutUser: true);
            }
            catch (Exception e)
            {
                return await HandleErrorJsonAsync(e);
                //return Json(new
                //{
                //    success = false,
                //    title = "Greška prilikom slanja zahteva za gašenje naloga",
                //    body = "Molimo pokušajte ponovo ili kontaktirajte podršku za terapeute.",
                //    severity = "error"
                //});
            }
        }

        [HttpGet("/terapeut/nalog/podrška-za-terapeute")]
        public async Task<IActionResult> Support()
        {
            try
            {
                ShowToastOnThisPageIfSet();

                var user = await _userManager.GetUserAsync(User) ??
                    throw new GeneralException("Unable to load user.", signOutUser: true); ;

                return View(new TherapistSupportViewModel
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    ToChooseFrom_Topics = _therapistFunctions.Get_ToChooseFrom_Topics()
                });
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpPost("/terapeut/nalog/podrška-za-terapeute")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Support(TherapistSupportViewModel support)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User) ??
                    throw new GeneralException("Unable to load user.", signOutUser: true);

                if (!ModelState.IsValid)
                    return Json(new
                    {
                        success = false,
                        title = "Popunite sva obavezna polja",
                        body = "",
                        severity = "info"
                    });

                if (!_dropdown.TherapistSupportTicketTopicExistsInDatabase(support.Chosen_TopicId))
                {
                    _session.SetToast("Izabrana tema nije validna", "Molimo pokušajte ponovo", "info");

                    return Json(new
                    {
                        success = false,
                        redirectUrl = Url.Action("Support", "Account", new { Area = "Therapist" })
                    });
                }

                if(support.Chosen_TopicId == Framework.Providers.TherapistSupportTicketTopicsProvider.RequestAccountDeletion.Id)
                {
                    var therapist = _context.Therapists.GetById(user.TherapistAccountId) ??
                        throw new GeneralException("Unable to load user.", signOutUser: true);

                    if (_context.TherapistSupportTickets.ReadOnlyAny(t => t.TherapistId == therapist.Id &&
                                                                          t.TopicId == Framework.Providers.TherapistSupportTicketTopicsProvider.RequestAccountDeletion.Id))
                        return Json(new
                        {
                            success = false,
                            accountDeletionAlreadyRequested = true,
                            title = "Zahtev za gašenje naloga može biti poslat samo jednom",
                            body = "Već ste jednom poslali zahtev za gašenje naloga.",
                            severity = "warning"
                        });

                    if (therapist.Earnings > 0)
                        return Json(new
                        {
                            success = false,
                            hasEarnings = true,
                            title = "Možete zahtevati gašenje naloga nakon što su Vam sva novčana sredstva isplaćena",
                            body = $"Trenutno imate sredstva u iznosu od RSD {therapist.Earnings}. " +
                            $"Pošaljite zahtev za isplatu sredstava i nakon što on bude odobren, " +
                            $"možete poslati zahtev za gašenje naloga.",
                            severity = "warning",
                            withdrawUrl = Url.Action("Withdraw", "Withdrawals", new { Area = "Therapist" })
                        });

                    var now = DateTime.UtcNow;

                    if (_context.BookedConsultations.ReadOnlyAny(c => c.TherapistId == therapist.Id && c.StartDateTime > now))
                        return Json(new
                        {
                            success = false,
                            hasBookedSessions = true,
                            title = "Da biste zahtevali gašenje naloga, sve Vaše zakazane besplatne konsultacije moraju da se završe",
                            body = $"Sačekajte da Vam se sve zakazane besplatne konsultacije završe i pokušajte ponovo.",
                            severity = "warning",
                            allBookedSessionsUrl = Url.Action("BookedConsultations", "Consultations", new { Area = "Therapist" })
                        });

                    if (_context.BookedSessions.ReadOnlyAny(s => s.TherapistId == therapist.Id && s.StartTime > now))
                        return Json(new
                        {
                            success = false,
                            hasBookedSessions = true,
                            title = "Da biste zahtevali gašenje naloga, sve Vaše zakazane seanse moraju da se završe",
                            body = $"Sačekajte da Vam se sve zakazane seanse završe i pokušajte ponovo.",
                            severity = "warning",
                            allBookedSessionsUrl = Url.Action("BookedSessions", "Sessions", new { Area = "Therapist" })
                        });

                    if (_context.BookedSessions.ReadOnlyAny(s => s.TherapistId == therapist.Id && (s.TherapistIsPaid == null || !s.TherapistIsPaid.Value)))
                        return Json(new
                        {
                            success = false,
                            hasBookedSessions = true,
                            title = "Imate neisplaćene seanse",
                            body = $"Da biste zahtevali gašenje naloga, sve Vaše zakazane seanse moraju biti isplaćene.",
                            severity = "warning",
                            allBookedSessionsUrl = Url.Action("BookedSessions", "Sessions", new { Area = "Therapist" })
                        });

                    if (_context.Consultations.ReadOnlyAny(c => c.TherapistId == therapist.Id && c.StartDateTime > now))
                        return Json(new
                        {
                            success = false,
                            hasActiveSessions = true,
                            title = "Da biste zahtevali gašenje naloga, ne smete imati nijedan aktivan termin besplatnih konsultacija",
                            body = $"Obrišite sve aktivne termine besplatnih konsultacija koje imate i pokušajte ponovo.",
                            severity = "warning",
                            allSessionsUrl = Url.Action("BookedConsultations", "Consultations", new { Area = "Therapist" })
                        });

                    if (_context.Sessions.ReadOnlyAny(s => s.TherapistId == therapist.Id && s.StartDateTime > now))
                        return Json(new
                        {
                            success = false,
                            hasActiveSessions = true,
                            title = "Da biste zahtevali gašenje naloga, ne smete imati nijednu aktivnu seansu",
                            body = $"Obrišite sve aktivne seanse koje imate i pokušajte ponovo.",
                            severity = "warning",
                            allSessionsUrl = Url.Action("All", "Sessions", new { Area = "Therapist" })
                        });
                }

                _context.TherapistSupportTickets.Insert(new TherapistSupportTickets
                {
                    Id = Helper.GenerateNumbersId(),
                    TherapistId = user.TherapistAccountId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Text = support.Text,
                    TopicId = support.Chosen_TopicId,
                    TopicName = _context.TherapistSupportTicketTopics.GetById(support.Chosen_TopicId).Name,
                    TicketDateTime = DateTime.UtcNow
                });

                await _context.SaveAsync();

                await _notificationRepository.SendAsync(new Notifications
                {
                    Id = Helper.GenerateNumbersId(),
                    SenderUserId = "System",
                    ReceiverUserId = user.Id,
                    Title = "Vaša poruka je uspešno poslata našem timu za podršku.",
                    Body = null,
                    Severity = "orange",
                    Read = false,
                    SendingDateTime = DateTime.UtcNow,
                    Icon = "fal fa-headset",
                    Important = false
                });

                return Json(new
                {
                    success = true,
                    title = "Uspešno ste poslali poruku našem timu za podršku",
                    body = "",
                    severity = "success",
                    redirectUrl = Url.Action("Index", "Home", new { Area = "" })
                });
            }
            catch (Exception e)
            {
                return await HandleErrorJsonAsync(e);
            }
        }

        [HttpGet("/terapeut/nalog/iskustva-sa-psihoterapijskim-tehnikama")]
        public async Task<IActionResult> PsychotherapyTechniquesExperiences()
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                    return View(_therapistFunctions.GetPsychotherapyTechniquesExperiences((await _userManager.GetUserAsync(User)).TherapistAccountId));
                else if (completionStatus == TherapistAccountCompletionStatus.NotSetUpYet)
                    return RedirectToAction("AccountSetup");
                else throw new GeneralException("Unable to load user.", signOutUser: true);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpPost("/terapeut/nalog/iskustva-sa-psihoterapijskim-tehnikama")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> EditPsychotherapyTechniquesExperience([FromForm] PsychotherapyTechniqueExperienceViewModel exp)
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                {
                    if (exp.Present)
                        ModelState.Remove("exp.ToDate");

                    if (!ModelState.IsValid)
                        return Json(new
                        {
                            success = false,
                            showToast = true,
                            title = "Popunite sva obavezna polja",
                            body = "",
                            severity = "info"
                        });

                    if (_dateHelper.ConvertDateTimeFromLocalToUtc(exp.FromDate.GetValueOrDefault()) > DateTime.UtcNow.Date)
                        return Json(new
                        {
                            success = false,
                            title = "Datum početka prakse mora biti pre današnjeg datuma",
                            body = "Za datum početka prakse izaberite datum koji je pre današnjeg datuma.",
                            severity = "info"
                        });

                    if (_dateHelper.ConvertDateTimeFromLocalToUtc(exp.ToDate.GetValueOrDefault()) > DateTime.UtcNow.Date)
                        return Json(new
                        {
                            success = false,
                            title = "Datum završetka prakse mora biti pre današnjeg datuma",
                            body = "Za datum završetka prakse izaberite datum koji je pre današnjeg datuma.",
                            severity = "info"
                        });

                    if (!exp.Present && !(exp.FromDate < exp.ToDate))
                        return Json(new
                        {
                            success = false,
                            title = "Pogrešno postavljen vremenski raspon",
                            body = "Datum početka mora biti pre datuma završetka.",
                            severity = "info"
                        });

                    var therapistUser = await _userManager.GetUserAsync(User);

                    var psyTechToUpdate = _context.TherapistPsychotherapyTechniques.Find(p => p.PsychotherapyTechniqueId == exp.Id && p.TherapistId == therapistUser.TherapistAccountId).SingleOrDefault();

                    if (psyTechToUpdate == default)
                    {
                        _session.SetToast("Došlo je do greške", "Molimo pokušajte ponovo ili kontaktirajte podršku za terapeute.", "error");
                        await HandleErrorJsonAsync($"Therapist '{therapistUser.TherapistAccountId}' does not have a psychotherapy technique with id '{exp.Id}' or has multiple, instead of one.");
                        return Json(new
                        {
                            success = false,
                            redirectUrl = Url.Action("PsychotherapyTechniquesExperiences", "Account", new { Area = "Therapist" })
                        });
                    }

                    psyTechToUpdate.FromDate = _dateHelper.ConvertDateTimeFromLocalToUtc(exp.FromDate.GetValueOrDefault().Date);
                    psyTechToUpdate.ToDate = exp.Present ? null : (DateTime?)_dateHelper.ConvertDateTimeFromLocalToUtc(exp.ToDate.GetValueOrDefault().Date);
                    psyTechToUpdate.Present = exp.Present;
                    psyTechToUpdate.City = exp.City;
                    psyTechToUpdate.Country = exp.Country;
                    psyTechToUpdate.Description = exp.Description;

                    if (await _context.SaveAsync() < 1)
                    {
                        _session.SetToast("Došlo je do greške", "Molimo pokušajte ponovo ili kontaktirajte podršku za terapeute.", "error");
                        await HandleErrorJsonAsync($"Unable to update psychotherapy technique '{psyTechToUpdate.PsychotherapyTechniqueId}' in database.");
                        return Json(new
                        {
                            success = false,
                            redirectUrl = Url.Action("PsychotherapyTechniquesExperiences", "Account", new { Area = "Therapist" })
                        });
                    }

                    return Json(new
                    {
                        success = true,
                        psyTech = new PsychotherapyTechniqueExperienceViewModel
                        {
                            Id = psyTechToUpdate.PsychotherapyTechniqueId,
                            //FromDate = exp.FromDate.GetValueOrDefault().Date,
                            //ToDate = exp.ToDate.GetValueOrDefault().Date,
                            Present = psyTechToUpdate.Present.GetValueOrDefault(),
                            City = psyTechToUpdate.City,
                            Country = psyTechToUpdate.Country,
                            Description = psyTechToUpdate.Description
                        },
                        title = "Čestitamo!",
                        body = "Uspešno ste dodali iskustvo.",
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

        [HttpGet("/terapeut/nalog/iskustva-sa-specijalnostima")]
        public async Task<IActionResult> SpecialtiesExperiences()
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                    return View(_therapistFunctions.GetSpecialtiesExperiences((await _userManager.GetUserAsync(User)).TherapistAccountId));
                else if (completionStatus == TherapistAccountCompletionStatus.NotSetUpYet)
                    return RedirectToAction("AccountSetup");
                else throw new GeneralException("Unable to load user.", signOutUser: true);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpPost("/terapeut/nalog/iskustva-sa-specijalnostima")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> EditSpecialtiesExperience([FromForm] SpecialtyExperienceViewModel exp)
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                {
                    if (exp.Present)
                        ModelState.Remove("exp.ToDate");

                    if (!ModelState.IsValid)
                        return Json(new
                        {
                            success = false,
                            showToast = true,
                            title = "Popunite sva obavezna polja",
                            body = "",
                            severity = "info"
                        });

                    if (_dateHelper.ConvertDateTimeFromLocalToUtc(exp.FromDate.GetValueOrDefault()) > DateTime.UtcNow.Date)
                        return Json(new
                        {
                            success = false,
                            title = "Datum početka prakse mora biti pre današnjeg datuma",
                            body = "Za datum početka prakse izaberite datum koji je pre današnjeg datuma.",
                            severity = "info"
                        });

                    if (_dateHelper.ConvertDateTimeFromLocalToUtc(exp.ToDate.GetValueOrDefault()) > DateTime.UtcNow.Date)
                        return Json(new
                        {
                            success = false,
                            title = "Datum završetka prakse mora biti pre današnjeg datuma",
                            body = "Za datum završetka prakse izaberite datum koji je pre današnjeg datuma.",
                            severity = "info"
                        });

                    if (!exp.Present && !(exp.FromDate < exp.ToDate))
                        return Json(new
                        {
                            success = false,
                            title = "Pogrešno postavljen vremenski raspon",
                            body = "Datum početka mora biti pre datuma završetka.",
                            severity = "info"
                        });

                    var therapistUser = await _userManager.GetUserAsync(User);

                    var specToUpdate = _context.TherapistsSpecialities.Find(s => s.SpecialityId == exp.Id && s.TherapistId == therapistUser.TherapistAccountId).SingleOrDefault();

                    if (specToUpdate == default)
                    {
                        _session.SetToast("Došlo je do greške", "Molimo pokušajte ponovo ili kontaktirajte podršku za terapeute.", "error");
                        await HandleErrorJsonAsync($"Therapist '{therapistUser.TherapistAccountId}' does not have a specialty with id '{exp.Id}', or has multiple, instead of one.");
                        return Json(new
                        {
                            success = false,
                            redirectUrl = Url.Action("SpecialtiesExperiences", "Account", new { Area = "Therapist" })
                        });
                    }

                    specToUpdate.FromDate = _dateHelper.ConvertDateTimeFromLocalToUtc(exp.FromDate.GetValueOrDefault().Date);
                    specToUpdate.ToDate = exp.Present ? null : (DateTime?)_dateHelper.ConvertDateTimeFromLocalToUtc(exp.ToDate.GetValueOrDefault().Date);
                    specToUpdate.Present = exp.Present;
                    specToUpdate.City = exp.City;
                    specToUpdate.Country = exp.Country;
                    specToUpdate.Description = exp.Description;

                    if (await _context.SaveAsync() < 1)
                    {
                        _session.SetToast("Došlo je do greške", "Molimo pokušajte ponovo ili kontaktirajte podršku za terapeute.", "error");
                        await HandleErrorJsonAsync($"Unable to update specialty '{specToUpdate.SpecialityId}' in database.");
                        return Json(new
                        {
                            success = false,
                            redirectUrl = Url.Action("SpecialtiesExperiences", "Account", new { Area = "Therapist" })
                        });
                    }

                    return Json(new
                    {
                        success = true,
                        spec = new SpecialtyExperienceViewModel
                        {
                            Id = specToUpdate.SpecialityId,
                            Present = specToUpdate.Present.GetValueOrDefault(),
                            City = specToUpdate.City,
                            Country = specToUpdate.Country,
                            Description = specToUpdate.Description
                        },
                        title = "Čestitamo!",
                        body = "Uspešno ste dodali iskustvo.",
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
    }
}
