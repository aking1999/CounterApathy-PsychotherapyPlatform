using Database.Models;
using Framework.Emails.EmailTypes;
using Framework.Helpers.ExtensionMethods;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebApplication9.Base;
using Framework.Notifications;
using WebApplication9.ViewModels;
using Framework.Emails;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using Framework.Interfaces;
using System;
using System.Linq;
using Framework.Models;
using Framework.Implementations;
using DataTransferObjects.ViewModels.Client;
using Framework.Helpers;
using Framework.Providers;
using Microsoft.AspNetCore.Hosting;

namespace WebApplication9.Controllers
{
    public class AuthorizationController : BaseController
    {
        private const int PASSWORD_MIN_LENGTH = 6;
        private readonly IFileRepository _files;
        private readonly IContextSetup _contextSetup;
        private readonly IWebHostEnvironment _environment;

        public AuthorizationController(
            IServiceProvider serviceProvider,
            IWebHostEnvironment environment,
            IMailService mailService,
            IDateTimeHelper dateHelper,
            IHttpContextAccessor contextAccessor,
            INotificationRepository notificationRepository,
            UserManager<CustomClient> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<CustomClient> signInManager) : base(mailService, dateHelper, contextAccessor, notificationRepository, userManager, roleManager, signInManager)
        {
            _environment = environment;
            _files = new FileRepository();
            _contextSetup = new ContextSetup(serviceProvider);
        }

        [HttpGet("/autorizacija/logovanje")]
        public async Task<IActionResult> SignIn(string returnUrl = null)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    _session.SetToast("Već ste ulogovani", null, "info");
                    return RedirectToAction("Index", "Home");
                }

                var loginInput = new LoginInputViewModel();

                if (!string.IsNullOrEmpty(loginInput.ErrorMessage))
                    ModelState.AddModelError(string.Empty, loginInput.ErrorMessage);

                returnUrl ??= Url.Content("~/");

                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

                //loginInput.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

                loginInput.ReturnUrl = returnUrl;

                return View(loginInput);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpPost("/autorizacija/logovanje")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(LoginInputViewModel loginInput, string returnUrl = null)
        {
            try
            {
                returnUrl ??= Url.Content("~/");

                if (User.Identity.IsAuthenticated)
                {
                    _session.SetToast("Već ste ulogovani", null, "info");
                    return RedirectToAction("Index", "Home");
                }

                if (ModelState.IsValid)
                {
                    // This doesn't count login failures towards account lockout
                    // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                    // var result = await _signInManager.PasswordSignInAsync(loginInput.Email, loginInput.Password, loginInput.RememberMe, lockoutOnFailure: false);
                    var user = await _userManager.FindByEmailAsync(loginInput.Email);
                    if (user == null)
                    {
                        ModelState.AddModelError(string.Empty, "Nevalidan imejl ili lozinka.");
                        return View(loginInput);
                    }

                    var signIn = await _signInManager.PasswordSignInAsync(user.UserName, loginInput.Password, true, lockoutOnFailure: false);

                    if (signIn.Succeeded)
                    {
                        //Setting <hidden> so that UserActivityLoggerAttribute.OnResultExecuted does not
                        //save the raw password into column QueryDataJson in database but <hidden>
                        loginInput.Password = "<hidden>";

                        //Users dont like getting SignInSuccessful emails so commenting this
                        //await _mailService.SendSignInSuccessfulEmailAsync(new SignInSuccessfulEmail
                        //{
                        //    ToEmail = loginInput.Email
                        //}, includeTemplateIfExists: true);

                        _session.SetToast("Logovanje uspešno", null, "success");

                        var bookConsultationRequested = _session.GetString("BookConsultationRequested");
                        if (!string.IsNullOrWhiteSpace(bookConsultationRequested) && bookConsultationRequested == "true")
                        {
                            _session.Remove("BookConsultationRequested");
                            return RedirectToAction("Index", "Consultations");
                        }

                        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                            return LocalRedirect(returnUrl);

                        return RedirectToAction("Index", "Home");
                    }
                    if (signIn.RequiresTwoFactor)
                    {
                        //return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                    }
                    if (signIn.IsLockedOut)
                    {
                        //return RedirectToPage("./Lockout");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Nevalidni imejl ili lozinka.");
                        return View(loginInput);
                    }
                }

                return View(loginInput);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpGet("/autorizacija/registracija")]
        public async Task<IActionResult> Register(string returnUrl = null)
        {
            try
            {
                ShowToastOnThisPageIfSet();

                if (User.Identity.IsAuthenticated)
                {
                    _session.SetToast("Već ste registrovani i ulogovani", null, "info");
                    return RedirectToAction("Index", "Home");
                }

                returnUrl ??= Url.Content("~/");

                return View(new RegisterInputViewModel
                {
                    ReturnUrl = returnUrl
                    //ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
                });
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpPost("/autorizacija/registracija")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterInputViewModel registerInput, string returnUrl = null)
        {
            try
            {
                if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");

                returnUrl ??= Url.Content("~/");

                //registerInput.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

                if (!_contextSetup.AddClientRole())
                {
                    _session.SetToast("Došlo je do greške", "Molimo osvežite stranicu i pokušajte ponovo.", "error");
                    return RedirectToAction("Register");
                }

                if (ModelState.IsValid)
                {
                    var user = new CustomClient
                    {
                        Id = Helper.GenerateNumbersId(),
                        ProfilePhoto = null,
                        FirstName = !string.IsNullOrWhiteSpace(registerInput.FirstName) ? registerInput.FirstName.Trim() : null,
                        LastName = !string.IsNullOrWhiteSpace(registerInput.LastName) ? registerInput.LastName.Trim() : null,
                        UserName = !string.IsNullOrWhiteSpace(registerInput.Email) ? registerInput.Email.Trim() : null,
                        Email = !string.IsNullOrWhiteSpace(registerInput.Email) ? registerInput.Email.Trim() : null,
                        PhoneNumber = !string.IsNullOrWhiteSpace(registerInput.PhoneNumber) ? "+381" + registerInput.PhoneNumber.Trim() : null,
                        YearOfBirth = null,
                        WebCredit = 0
                    };

                    var register = await _userManager.CreateAsync(user, registerInput.Password);

                    if (register.Succeeded)
                    {
                        var added = await _userManager.AddToRoleAsync(user, "Client");

                        if (!added.Succeeded) throw new Exception(string.Join("|", added.Errors.Select(e => e.Description)));

                        await _signInManager.SignInAsync(user, isPersistent: true);

                        _session.SetToast("Registracija uspešna", null, "success");

                        //Setting <hidden> so that UserActivityLoggerAttribute.OnResultExecuted does not
                        //save the raw passwords into column QueryDataJson in database but <hidden>
                        registerInput.Password = "<hidden>";
                        registerInput.ConfirmPassword = "<hidden>";

                        return RedirectToAction("ConfirmEmail", "Authorization", new { Area = "" });
                    }

                    var errors = register.Errors.Select(e => e.Description).ToList();
                    

                    if (errors.Where(e => e.ToLower().Contains("user name") && e.Contains("is already taken")).Any())
                    {
                        errors.RemoveAll(e => e.ToLower().Contains("user name") && e.Contains("is already taken"));
                        errors.Add($"Imejl je zauzet. Unesite drugi.");
                    }

                    var errorsConcat = string.Join('|', errors);

                    ModelState.AddModelError(string.Empty, errorsConcat);

                    await HandleErrorAsync(errorsConcat);
                }

                return View(registerInput);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpPost("/autorizacija/odjava")]
        public async Task<IActionResult> SignOut(string returnUrl = null)
        {
            try
            {
                await _signInManager.SignOutAsync();

                _session.SetToast("Odjava uspešna", null, "success");
                return LocalRedirect(returnUrl);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [Authorize]
        [HttpGet("/autorizacija/potvrdite-imejl")]
        public async Task<IActionResult> ConfirmEmail()
        {
            try
            {
                ShowToastOnThisPageIfSet();

                var user = await _userManager.GetUserAsync(User);

                if (user == null) throw new GeneralException("Unable to load user.", signOutUser: true);

                if (await _userManager.IsEmailConfirmedAsync(user))
                {
                    _session.SetToast("Imejl je već potvrđen", null, "info");
                    return RedirectToAction("Index", "Home", new { Area = "" });
                }

                return View((object)user.Email);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [Authorize]
        [HttpPost("/autorizacija/slanje-linka-za-potvrdu-imejla")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SendEmailConfirmationLink()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null) throw new GeneralException("Unable to load user.", signOutUser: true);

                if (await _userManager.IsEmailConfirmedAsync(user))
                {
                    _session.SetToast("Imejl je već potvrđen", null, "info");
                    return Json(new
                    {
                        success = false,
                        redirectUrl = Url.Action("Index", "Home", new { Area = "" })
                    }); ;
                }

                await _mailService.SendEmailConfirmationEmailAsync(new ConfirmationEmail
                {
                    UserId = user.Id,
                    // WebUtility.Encode is not needed because LinkGenerator in MailService already
                    // encodes the token, so we will get double encoding and Token Invalid error.
                    Token = await _userManager.GenerateEmailConfirmationTokenAsync(user),
                    FirstName = user.FirstName,
                    ToEmail = user.Email
                }, includeTemplateIfExists: true);

                return Json(new
                {
                    success = true
                });
            }
            catch (Exception e)
            {
                return await HandleErrorJsonAsync(e);
            }
        }

        [HttpGet("/autorizacija/potvrda-imejla")]
        public async Task<IActionResult> EmailConfirmation(string uid, string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(uid) || string.IsNullOrWhiteSpace(token))
                {
                    await HandleErrorAsync($"Invalid confirmation link. UserId: '{uid ?? "null"}', token: '{token ?? "null"}'.");
                    _session.SetToast("Link za potvrdu imejla nije validan", "Kliknite na dugme ispod da bi na Vaš imejl bio poslat novi link za potvrdu.", "error");
                    return RedirectToAction("ConfirmEmail", "Authorization", new { Area = "" });
                }

                var user = await _userManager.FindByIdAsync(uid);

                if (user == null)
                {
                    await HandleErrorAsync($"User with ID '{uid}' not found.");
                    _session.SetToast("Link za potvrdu imejla nije validan", "Kliknite na dugme ispod da bi na Vaš imejl bio poslat novi link za potvrdu.", "error");
                    return RedirectToAction("ConfirmEmail", "Authorization", new { Area = "" });
                }

                if (await _userManager.IsEmailConfirmedAsync(user))
                {
                    _session.SetToast("Imejl je već potvrđen", null, "info");
                    return RedirectToAction("Index", "Home", new { Area = "" });
                }

                var confirm = await _userManager.ConfirmEmailAsync(user, WebUtility.HtmlDecode(token));

                if (confirm.Succeeded)
                {
                    await _mailService.SendWelcomeEmailAsync(new WelcomeEmail
                    {
                        FirstName = user.FirstName,
                        ToEmail = user.Email
                    }, includeTemplateIfExists: false);

                    _context.NewsletterSubscribers.Insert(new NewsletterSubscribers
                    {
                        Id = Helper.GenerateNumbersId(),
                        Email = user.Email,
                        NormalizedEmail = user.Email.ToUpperInvariant(),
                        IpAddress = HttpContext.Connection?.RemoteIpAddress.ToString().TakeMax(256),
                        NotifiedCount = 0,
                        SubscribeDateTime = DateTime.UtcNow
                    });

                    await _context.SaveAsync();

                    var redirectToUrl = Url.Action("Index", "Home", new { Area = "" });

                    var bookConsultationRequested = _session.GetString("BookConsultationRequested");
                    if (!string.IsNullOrWhiteSpace(bookConsultationRequested) && bookConsultationRequested == "true")
                    {
                        _session.Remove("BookConsultationRequested");
                        redirectToUrl = Url.Action("Index", "Consultations", new { Area = "" });
                    }

                    return View("EmailConfirmed", redirectToUrl);
                }

                await HandleErrorAsync(string.Join("|", confirm.Errors.Select(e => e.Description)));
                _session.SetToast("Link za potvrdu imejla nije validan", "Kliknite na dugme ispod da bi na Vaš imejl bio poslat novi link za potvrdu ili kontaktirajte korisničku podršku.", "error");
                return RedirectToAction("ConfirmEmail", "Authorization", new { Area = "" });
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpGet("/autorizacija/zaboravljena-lozinka")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword()
        {
            try
            {
                ShowToastOnThisPageIfSet();

                if (User.Identity.IsAuthenticated)
                    return RedirectToAction("Index", "Home", new { Area = "" });

                return View(new ForgotPasswordViewModel());
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpPost("/autorizacija/slanje-linka-za-obnovu-lozinke")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SendPasswordResetLink([FromForm] ForgotPasswordViewModel reset)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                    return Json(new
                    {
                        success = false,
                        redirectUrl = Url.Action("Index", "Home")
                    });

                if (!ModelState.IsValid)
                    return Json(new
                    {
                        success = false,
                        title = "Unesite Vaš imejl",
                        body = "",
                        severity = "info"
                    });

                var user = await _userManager.FindByEmailAsync(reset.Email);

                if (user == null)
                    return Json(new
                    {
                        //it is true here on purpose
                        //so there is no difference between user
                        //exists and doesnt exist, so hacker cant mill
                        //existing accounts
                        success = true
                    });

                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    await HandleErrorJsonAsync("Cannot reset the password of an account which has an unconfirmed email.");
                    return Json(new
                    {
                        success = false,
                        title = "Obnova lozinke nije moguća",
                        body = "Obnova lozinke je moguća samo za nalog sa potvrđenom imejl adresom.",
                        severity = "error"
                    });
                }

                await _mailService.SendPasswordResetEmailAsync(new PasswordResetEmail
                {
                    UserId = user.Id,
                    // WebUtility.Encode is not needed because LinkGenerator in MailService already
                    // encodes the token, so we will get double encoding and Token Invalid error.
                    Token = await _userManager.GeneratePasswordResetTokenAsync(user),
                    FirstName = user.FirstName,
                    ToEmail = user.Email
                }, includeTemplateIfExists: true);

                await _notificationRepository.SendAsync(new Notifications
                {
                    Id = Helper.GenerateNumbersId(),
                    SenderUserId = "System",
                    ReceiverUserId = user.Id,
                    Title = "Uspešno ste poslali zahtev za obnovu Vaše lozinke. Uputstva za obnovu su poslata na Vaš imejl.",
                    Body = null,
                    Severity = "primary",
                    Read = false,
                    SendingDateTime = DateTime.UtcNow,
                    Icon = "fal fa-sync-alt",
                    Important = false
                });

                return Json(new
                {
                    success = true
                });
            }
            catch (Exception e)
            {
                return await HandleErrorJsonAsync(e);
            }
        }

        [HttpGet("/autorizacija/obnova-lozinke")]
        public async Task<IActionResult> PasswordReset(string uid, string token)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    _session.SetToast("Morate se odjaviti pre obnove lozinke", "Odjavite se i zatim ponovo kliknite na link u Vašem mejlu.", "info");
                    return RedirectToAction("Index", "Home");
                }

                if (string.IsNullOrWhiteSpace(uid) || string.IsNullOrWhiteSpace(token))
                {
                    await HandleErrorAsync($"Invalid password reset link. UserId: '{uid ?? "null"}', token: '{token ?? "null"}'.");
                    _session.SetToast("Link za obnovu lozinke nije validan", "Kliknite na dugme ispod da bi na Vaš imejl bio poslat novi link za obnovu.", "error");
                    return RedirectToAction("ForgotPassword", "Authorization", new { Area = "" });
                }

                var user = await _userManager.FindByIdAsync(uid);

                if (user == null)
                {
                    await HandleErrorAsync($"User with ID '{uid}' not found.");
                    _session.SetToast("Link za obnovu lozinke nije validan", "Kliknite na dugme ispod da bi na Vaš imejl bio poslat novi link za obnovu.", "error");
                    return RedirectToAction("ForgotPassword", "Authorization", new { Area = "" });
                }

                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    await HandleErrorJsonAsync("Cannot reset the password of an account which has an unconfirmed email.");
                    _session.SetToast("Obnova lozinke nije moguća", "Obnova lozinke je moguća samo za nalog sa potvrđenom imejl adresom.", "error");
                    return RedirectToAction("Index", "Home", new { Area = "" });
                }

                return View(new PasswordResetViewModel
                {
                    ProfilePhoto = _files.GetUserProfilePhotoPathOrDefaultPhotoPath(_userManager, _environment, uid),
                    FullName = user.FirstName + " " + user.LastName,
                    Uid = uid,
                    Token = token
                });
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpPost("/autorizacija/obnova-lozinke")]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> PasswordReset([FromForm] PasswordResetViewModel reset)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    _session.SetToast("Morate se odjaviti pre obnove lozinke", "Odjavite se i zatim ponovo kliknite na link u Vašem mejlu.", "info");
                    return Json(new
                    {
                        success = false,
                        redirectUrl = Url.Action("Index", "Home")
                    });
                }

                if (string.IsNullOrWhiteSpace(reset.Uid) || string.IsNullOrWhiteSpace(reset.Token))
                {
                    await HandleErrorAsync($"Invalid password reset link. UserId: '{reset.Uid ?? "null"}', token: '{reset.Token ?? "null"}'.");
                    _session.SetToast("Link za obnovu lozinke nije validan", "Kliknite na dugme ispod da bi na Vaš imejl bio poslat novi link za obnovu.", "error");
                    return Json(new
                    {
                        success = false,
                        redirectUrl = Url.Action("ForgotPassword", "Authorization", new { Area = "" })
                    });
                }

                var user = await _userManager.FindByIdAsync(reset.Uid);

                if (user == null)
                {
                    await HandleErrorAsync($"User with ID '{reset.Uid}' not found.");
                    _session.SetToast("Link za obnovu lozinke nije validan", "Kliknite na dugme ispod da bi na Vaš imejl bio poslat novi link za obnovu.", "error");
                    return Json(new
                    {
                        success = false,
                        redirectUrl = Url.Action("ForgotPassword", "Authorization", new { Area = "" })
                    });
                }

                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    await HandleErrorJsonAsync("Cannot reset the password of an account which has an unconfirmed email.");
                    _session.SetToast("Obnova lozinke nije moguća", "Obnova lozinke je moguća samo za nalog sa potvrđenom imejl adresom.", "error");
                    return Json(new
                    {
                        success = false,
                        redirectUrl = Url.Action("Index", "Home", new { Area = "" })
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

                var result = await _userManager.ResetPasswordAsync(user, reset.Token, reset.Password);
                if (result.Succeeded)
                {
                    await _mailService.SendPasswordChangedEmailAsync(new PasswordChangedEmail
                    {
                        ToEmail = user.Email,
                        FirstName = user.FirstName
                    }, includeTemplateIfExists: true);

                    await _notificationRepository.SendAsync(new Notifications
                    {
                        Id = Helper.GenerateNumbersId(),
                        SenderUserId = "System",
                        ReceiverUserId = user.Id,
                        Title = "Lozinka uspešno obnovljena.",
                        Body = null,
                        Severity = "success",
                        Read = false,
                        SendingDateTime = DateTime.UtcNow,
                        Icon = "far fa-shield-check",
                        Important = false
                    });

                    return Json(new
                    {
                        success = true,
                        title = "Čestitamo!",
                        body = "Lozinka uspešno obnovljena.",
                        severity = "success"
                    });
                }

                if (result.Errors.Any(e => e.Code == "PasswordTooShort"))
                {
                    return Json(new
                    {
                        success = false,
                        title = $"Dužina lozinke mora biti od {PASSWORD_MIN_LENGTH} do 100 karaktera",
                        body = "",
                        severity = "info"
                    });
                }

                await HandleErrorJsonAsync(string.Join("|", result.Errors.Select(e => e.Description)));
                _session.SetToast("Link za obnovu lozinke nije validan", "Kliknite na dugme ispod da bi na Vaš imejl bio poslat novi link za obnovu.", "error");
                return Json(new
                {
                    success = false,
                    redirectUrl = Url.Action("ForgotPassword", "Authorization", new { Area = "" })
                });
            }
            catch (Exception e)
            {
                return await HandleErrorJsonAsync(e);
            }
        }
    }
}
