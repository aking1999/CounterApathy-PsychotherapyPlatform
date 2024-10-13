using Database.Models;
using Framework.Helpers.ExtensionMethods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebApplication9.Base;
using WebApplication9.Helpers;
using Framework.Notifications;
using WebApplication9.ViewModels;
using Framework.Emails;
using Framework.Emails.EmailTypes;
using Framework.Models;
using Framework.Interfaces;
using WebApplication9.Interfaces;
using WebApplication9.Implementations;
using Microsoft.AspNetCore.Http;
using Framework.Implementations;
using DataTransferObjects.ViewModels.Client;
using Framework.Helpers;
using System.Transactions;
using WebApplication9.Models;
using Stripe;
using Microsoft.Extensions.Configuration;

namespace WebApplication9.Controllers
{
    [Authorize(Roles = "Client")]
    public class AccountController : BaseController
    {
        private readonly StripeSettings _stripeSettings;
        private readonly IFileRepository _files;
        private readonly IDropdownHelper _dropdown;
        private readonly IWebHostEnvironment _environment;
        private readonly IStripeFunctionsProvider _stripeFunctions;
        private readonly ICustomClientFunctionsProvider _clientFunctions;

        public AccountController(IConfiguration configuration,
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
            _files = new FileRepository();
            _dropdown = new DropdownHelper();
            _clientFunctions = new CustomClientFunctionsProvider(contextAccessor);
            _stripeSettings = configuration.GetSection("StripeSettings").Get<StripeSettings>();
            StripeConfiguration.ApiKey = _stripeSettings.ApiKey;
            _stripeFunctions = new StripeFunctionsProvider();
        }

        [HttpGet("/nalog")]
        public async Task<IActionResult> Profile()
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
                {
                    ShowToastOnThisPageIfSet();

                    var user = await _userManager.GetUserAsync(User);

                    var viewModel = new CustomClientViewModel
                    {
                        HasAppliedForTherapistAccount = _userManager.HasAppliedForTherapistAccount(user.Id)
                    };

                    viewModel.Map(user);

                    return View(viewModel);
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

        [HttpPost("/nalog/ažuriranje")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CustomClientViewModel profile)
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
                {
                    var user = await _userManager.GetUserAsync(User);

                    if (!ModelState.IsValid)
                    {
                        profile.MapProfilePhoto(user);

                        _session.SetToast("Profil nije ažuriran", "Proverite sve unete podatke i pokušajte ponovo.", "info");
                        ViewBag.toast = _session.GetToast();
                        _session.RemoveToastFromKeys();

                        return View("Profile", profile);
                    }

                    await user.MapAsync(profile);

                    using(var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        try
                        {
                            using (var scope2 = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
                            {
                                try
                                {
                                    var update = await _userManager.UpdateAsync(user);
                                    if (!update.Succeeded) throw new GeneralException(string.Join("|", update.Errors.Select(e => e.Description)));

                                    scope2.Complete();
                                }
                                catch (Exception)
                                {
                                    scope2.Dispose();
                                    throw;
                                }
                            }
                                
                            var stripeCustomerId = _stripeFunctions.GetStripeCustomerId(user.Id);

                            if (!string.IsNullOrWhiteSpace(stripeCustomerId))
                            {
                                var customerInStripe = await new CustomerService().UpdateAsync(stripeCustomerId, new CustomerUpdateOptions
                                {
                                    Name = $"{user.FirstName} {user.LastName} - {user.Id}",
                                    Phone = user.PhoneNumber
                                });

                                _context.StripeCustomers.GetById(stripeCustomerId).PhoneNumber = user.PhoneNumber;
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

                    _session.SetToast("Profil uspešno ažuriran", null, "success");
                    return RedirectToAction("Profile");
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

        [HttpGet("/nalog/promena-lozinke")]
        public async Task<IActionResult> ChangePassword()
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
                {
                    if (!await _userManager.HasPasswordAsync(await _userManager.GetUserAsync(User)))
                        throw new GeneralException("User does not have a password.", signOutUser: true);

                    return View(new ChangePasswordViewModel());
                }
                else if (emailStatus == EmailConfirmationStatus.NotConfirmed)
                    return RedirectToAction("ConfirmEmail", "Authorization", new { Area = "" });
                else throw new GeneralException($"Unable to load user.", signOutUser: true);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpPost("/nalog/promena-lozinke")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel viewModel)
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
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

                    await _mailService.SendPasswordChangedEmailAsync(new PasswordChangedEmail
                    {
                        ToEmail = user.Email,
                        FirstName = user.FirstName
                    }, includeTemplateIfExists: true);

                    return RedirectToAction("Profile");
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

        [HttpPost("/nalog/brisanje-slike-naloga")]
        public async Task<JsonResult> DeleteProfilePhoto()
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
                {
                    var delete = await _userManager.DeleteProfilePhotoIfExists(_environment, User);
                    if (delete.Succeeded)
                        return Json(new
                        {
                            success = true,
                            title = "Profilna slika uspešno obrisana",
                            body = "",
                            severity = "success",
                            path = _files.DefaultProfilePhotoPath
                        });

                    await HandleErrorJsonAsync(string.Join("|", delete.Errors.Select(e => e.Description)));

                    return Json(new
                    {
                        success = false,
                        title = "Došlo je do greške",
                        body = "",
                        severity = "error"
                    });
                }
                else return Json(new
                {
                    success = false,
                    title = "Došlo je do greške",
                    body = "",
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
                    body = "",
                    severity = "error"
                });
            }
        }

        [HttpGet("/nalog/apliciranje-za-nalog-psihoterapeuta")]
        public async Task<IActionResult> Apply()
        {
            try
            {
                ShowToastOnThisPageIfSet();

                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
                {
                    var user = await _userManager.GetUserAsync(User);

                    if (_clientFunctions.HasUpcomingConsultation(user.Id))
                    {
                        _session.SetToast("Imate predstojeće zakazane besplatne konsultacije", "Možete aplicirati nakon završetka zakazanih besplatnih konsultacija.", "info");
                        return RedirectToAction("TherapistApplications", "Home");
                    }

                    if (_clientFunctions.HasUpcomingSession(user.Id))
                    {
                        _session.SetToast("Imate predstojeću zakazanu seansu", "Možete aplicirati nakon završetka svih zakazanih seansi.", "info");
                        return RedirectToAction("TherapistApplications", "Home");
                    }

                    if (_userManager.HasAppliedForTherapistAccount(user.Id))
                    {
                        _session.SetToast("Već ste jednom aplicirali", "Svaki nalog može aplicirati samo jednom.", "info");
                        return RedirectToAction("TherapistApplications", "Home");
                    }

                    var applicationVm = new TherapistApplicationsViewModel
                    {
                        ToChooseFrom_Genders = _dropdown.GetGendersForDropdown(),
                        ToChooseFrom_Specialties = _dropdown.GetSpecialtiesForDropdown(),
                        ToChooseFrom_PsychotherapyTechniques = _dropdown.GetPsychotherapyTechniquesForDropdown()
                    };

                    applicationVm.Map(user);

                    return View(applicationVm);
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

        [HttpPost("/nalog/apliciranje-za-nalog-psihoterapeuta")]
        [ValidateAntiForgeryToken]
        //[Consumes("multipart/form-data")]
        public async Task<JsonResult> Apply([FromForm] TherapistApplicationsViewModel appVm)
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
                {
                    var user = await _userManager.GetUserAsync(User);

                    if (_clientFunctions.HasUpcomingConsultation(user.Id))
                    {
                        _session.SetToast("Imate predstojeće zakazane besplatne konsultacije", "Možete aplicirati nakon završetka zakazanih besplatnih konsultacija.", "info");

                        return Json(new
                        {
                            success = false,
                            redirectUrl = Url.Action("Profile", "Account", new { Area = "" })
                        });
                    }

                    if (_clientFunctions.HasUpcomingSession(user.Id))
                    {
                        _session.SetToast("Imate predstojeću zakazanu seansu", "Možete aplicirati nakon završetka svih zakazanih seansi.", "info");

                        return Json(new
                        {
                            success = false,
                            redirectUrl = Url.Action("Profile", "Account", new { Area = "" })
                        });
                    }

                    if (_userManager.HasAppliedForTherapistAccount(user.Id))
                    {
                        _session.SetToast("Već ste jednom aplicirali", "Svaki nalog može aplicirati samo jednom.", "info");

                        return Json(new
                        {
                            success = false,
                            redirectUrl = Url.Action("TherapistApplications", "Home", new { Area = "" })
                        });
                    }

                    if (!ModelState.IsValid)
                        return Json(new
                        {
                            success = false,
                            title = "Popunite sva obavezna polja",
                            body = "",
                            severity = "info"
                        });

                    var application = new TherapistApplications
                    {
                        Id = Helper.GenerateNumbersId(),
                        UserId = user.Id,
                        ProfilePhoto = await _files.CreateUserImageAsync(_environment, $"psihoterapeut-{user.FirstName}-{user.LastName}", appVm.ProfilePhoto),
                        FirstName = appVm.FirstName,
                        LastName = appVm.LastName,
                        ApplicationDate = DateTime.UtcNow.ToString("dd/MMM/yyyy HH:mm:ss"),
                        PhoneNumber = !string.IsNullOrWhiteSpace(appVm.PhoneNumber) ? "+381" + appVm.PhoneNumber : null,
                        YearOfBirth = appVm.YearOfBirth ?? null,
                        Street = appVm.Street,
                        HouseNumber = appVm.HouseNumber,
                        City = appVm.City,
                        Country = "Srbija",
                        PostalCode = appVm.PostalCode,
                        Gender = appVm.Chosen_GenderId,
                        UnderSupervision = appVm.UnderSupervision,
                        University = appVm.University,
                        PastCompanies = appVm.LegalEntity ? appVm.PastCompanies + " [PRAVNO LICE]" : appVm.PastCompanies,
                        Accepted = 0,
                    };

                    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        try
                        {
                            _context.TherapistApplications.Insert(application);

                            bool hasAtLeastOneSpecialty = false;
                            bool hasAtLeastOnePsyTech = false;

                            foreach (var specialtyId in appVm.Chosen_SpecialtiesIds[0].Split(','))
                            {
                                if (_dropdown.SpecialtyExistsInDatabase(specialtyId.Trim()))
                                {
                                    hasAtLeastOneSpecialty = true;
                                    _context.TherapistApplicationsSpecialities.Insert(new TherapistApplicationsSpecialities
                                    {
                                        TherapistApplicationId = application.Id,
                                        SpecialityId = specialtyId
                                    });
                                }
                            }

                            foreach (var techniqueId in appVm.Chosen_PsychotherapyTechniquesIds[0].Split(','))
                            {
                                if (_dropdown.PsychotherapyTechniqueExistsInDatabase(techniqueId.Trim()))
                                {
                                    hasAtLeastOnePsyTech = true;
                                    _context.TherapistApplicationsPsychotherapyTechniques.Insert(new TherapistApplicationsPsychotherapyTechniques
                                    {
                                        TherapistApplicationId = application.Id,
                                        PsychotherapyTechniqueId = techniqueId
                                    });
                                }
                            }

                            if (!hasAtLeastOnePsyTech)
                                return Json(new
                                {
                                    success = false,
                                    title = "Izabrane psihoterapijske tehnike nisu validne",
                                    body = "Molimo osvežite stranicu i pokušajte ponovo.",
                                    severity = "info"
                                });

                            if (!hasAtLeastOneSpecialty)
                                return Json(new
                                {
                                    success = false,
                                    title = "Izabrane specijalnosti nisu validne",
                                    body = "Molimo osvežite stranicu i pokušajte ponovo.",
                                    severity = "info"
                                });

                            if (!_dropdown.GenderExists(application.Gender))
                                return Json(new
                                {
                                    success = false,
                                    title = "Izabrani pol nije validan",
                                    body = "Molimo osvežite stranicu i pokušajte ponovo.",
                                    severity = "info"
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

                    await _notificationRepository.SendAsync(new Notifications
                    {
                        Id = Helper.GenerateNumbersId(),
                        SenderUserId = "System",
                        ReceiverUserId = user.Id,
                        Title = $"Uspešno ste aplicirali za dobijanje naloga psihoterapeuta. Bićete uskoro kontaktirani putem imejla {user.Email} u vezi nastavka proseca apliciranja.",
                        Body = null,
                        Severity = "success",
                        Read = false,
                        SendingDateTime = DateTime.UtcNow,
                        Icon = "fal fa-clipboard-check",
                        Important = false
                    });

                    await _mailService.SendTherapistApplicationReceivedEmailAsync(new TherapistApplicationStatusEmail
                    {
                        ToEmail = user.Email,
                        FirstName = user.FirstName
                    }, includeTemplateIfExists: true);

                    return Json(new
                    {
                        success = true,
                        title = "Čestitamo!",
                        body = $"Uspešno ste aplicirali za dobijanje naloga psihoterapeuta. Bićete uskoro kontaktirani putem imejla {user.Email} u vezi nastavka proseca apliciranja.",
                        severity = "success",
                        redirectUrl = Url.Action("Profile", "Account", new { Area = "" })
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

        [HttpPost("/nalog/brisanje-naloga")]
        public async Task<JsonResult> DeleteAccount(string password)
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
                {
                    var user = await _userManager.GetUserAsync(User);

                    if (!await _userManager.CheckPasswordAsync(user, password))
                        return Json(new
                        {
                            success = false,
                            title = "Netačna lozinka",
                            body = "Morate uneti tačnu lozinku da bi Vaš nalog bio ugašen.",
                            severity = "error"
                        });

                    if (_clientFunctions.HasUpcomingConsultation(user.Id))
                    {
                        return Json(new
                        {
                            success = false,
                            title = "Imate predstojeće zakazane besplatne konsultacije",
                            body = "Možete aplicirati nakon završetka zakazanih besplatnih konsultacija.",
                            severity = "error"
                        });
                    }

                    if (_clientFunctions.HasUpcomingSession(user.Id))
                        return Json(new
                        {
                            success = false,
                            title = "Imate predstojeću zakazanu seansu",
                            body = "Možete ugasiti nalog nakon završetka svih zakazanih seansi.",
                            severity = "error"
                        });

                    if(user.WebCredit.HasValue &&  user.WebCredit.Value > 0)
                        return Json(new
                        {
                            success = false,
                            hasWebCredit = true,
                            customerSupportUrl = Url.Action("CustomerSupport", "Home"),
                            title = $"Imate RSD {user.WebCredit.Value} Veb kredita",
                            body = "Nalog možete sami ugasiti samo ukoliko na njemu nema nepotrošenog Veb kredita. " +
                            "Kontaktirajte korisničku podršku kako bismo Vam isplatili Veb kredit i ugasili nalog.",
                            severity = "error"
                        });

                    var profilePhotoDeleted = await _userManager.DeleteProfilePhotoIfExists(_environment, User);
                    if (!profilePhotoDeleted.Succeeded)
                    {
                        await HandleErrorJsonAsync($"Error while deleting image '{user.ProfilePhoto}' for user '{user.Id}'. " +
                            $"{string.Join('|', profilePhotoDeleted.Errors.Select(e => e.Description))}");
                    }

                    var app = _context.TherapistApplications.Find(app => app.UserId == user.Id).SingleOrDefault();
                    if (app != default)
                    {
                        if (!_files.DeleteUserImage(_environment, app.ProfilePhoto))
                            await HandleErrorJsonAsync($"Error while deleting image '{app.ProfilePhoto}' for Application '{app.Id}'.");

                        _context.TherapistApplications.Delete(app);
                    }

                    await _signInManager.SignOutAsync();

                    var delete = await _userManager.DeleteAsync(user);

                    if (!delete.Succeeded)
                    {
                        await HandleErrorJsonAsync(string.Join("|", delete.Errors.Select(e => e.Description)));

                        return Json(new
                        {
                            success = false,
                            title = "Došlo je do greške",
                            body = "Molimo osvežite stranicu i pokušajte ponovo ili kontaktirajte korisničku podršku.",
                            severity = "error"
                        });
                    }

                    return Json(new
                    {
                        success = true,
                        title = "Nalog uspešno obrisan",
                        body = "Kliknite bilo gde da biste bili usmereni na početnu stranicu.",
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

        //In paypal controller, "https://counterapathy.com/nalog/veb-kredit" is hardcoded
        //change URL there too if changed here
        [HttpGet("/nalog/veb-kredit")]
        public async Task<IActionResult> AddWebCredit([FromQuery] string Cancel = null)
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
                {
                    ShowToastOnThisPageIfSet();

                    var user = await _userManager.GetUserAsync(User);

                    if (!string.IsNullOrWhiteSpace(Cancel) && Cancel.Trim().ToLower() == "true")
                    {
                        _session.SetToast("Transakcija poništena", null, "error");
                        ViewBag.toast = _session.GetToast();
                        _session.RemoveToastFromKeys();
                    }

                    var userPayPalRequests = _context.PayPalPaymentRequests.Find(req => req.UserId == user.Id).OrderByDescending(req => req.RequestDateTime).ToList();
                    if (userPayPalRequests.Any() && userPayPalRequests[0].Status == 0)
                    {
                        userPayPalRequests[0].Status = -1;
                        await _context.SaveAsync();
                    }

                    return View(new AddWebCreditViewModel
                    {
                        PostOfSerbia = new PostOfSerbiaAddWebCreditViewModel
                        {
                            Id = user.Id,
                            FirstName = user.FirstName,
                            LastName = user.LastName
                        }
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
