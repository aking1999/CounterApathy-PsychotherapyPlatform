using Database.Models;
using Framework.Helpers;
using Framework.Helpers.ExtensionMethods;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebApplication9.Base;
using Framework.Notifications;
using WebApplication9.ViewModels;

namespace WebApplication9.Controllers
{
    public class AuthorizationController : BaseController
    {
        private readonly IWebHostEnvironment _environment;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthorizationController(INotificationRepository notificationRepository,
            UserManager<CustomClient> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<CustomClient> signInManager,
            IWebHostEnvironment environment) : base(notificationRepository, userManager, signInManager)
        {
            _roleManager = roleManager;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> SignIn(string returnUrl = null)
        {
            if (_signInManager.IsSignedIn(User))
                return RedirectToAction("Index", "Home");

            LoginInputViewModel loginInput = new LoginInputViewModel();

            if (!string.IsNullOrEmpty(loginInput.ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, loginInput.ErrorMessage);
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            loginInput.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            loginInput.ReturnUrl = returnUrl;

            return View(loginInput);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(LoginInputViewModel loginInput, string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(loginInput.Email, loginInput.Password, loginInput.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    //_logger.LogInformation("User logged in.");

                    _session.SetToast("Signed in successfully", null, "success");

                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    //return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    //_logger.LogWarning("User account locked out.");
                    //return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid username or password.");
                    return View(loginInput);
                    //return Page();
                }
            }
            return View(loginInput);
        }

        [HttpGet]
        public async Task<IActionResult> Register(string returnUrl = null)
        {
            ShowToastOnThisPageIfSet();

            if (_signInManager.IsSignedIn(User))
                return RedirectToAction("Index", "Home");

            RegisterInputViewModel registerInput = new RegisterInputViewModel();

            registerInput.ReturnUrl = returnUrl;

            registerInput.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            return View(registerInput);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterInputViewModel registerInput, string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            registerInput.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            bool clientRoleExists = await _roleManager.RoleExistsAsync("Client");
            if (!clientRoleExists)
            {
                _session.SetToast("An error occured. Please try again.", null, "error");
                return RedirectToAction("Register");
            }

            if (ModelState.IsValid)
            {
                string fileName = null;
                string fileExtension = string.Empty;
                string uniqueFileName = string.Empty;
                string uniquieFileNameWithExtension = null;
                string saveToPath = string.Empty;

                if (registerInput.ProfilePhoto != null)
                {
                    fileName = registerInput.ProfilePhoto.FileName;
                    fileExtension = Path.GetExtension(fileName);
                    uniqueFileName = Guid.NewGuid().ToString();
                    uniquieFileNameWithExtension = uniqueFileName + fileExtension;

                    saveToPath = Path.Combine(_environment.WebRootPath, @"\images\user-images") + $@"\{uniquieFileNameWithExtension}";

                    using (FileStream stream = System.IO.File.Create(_environment.ContentRootPath + @"\wwwroot" + saveToPath))
                    {
                        await registerInput.ProfilePhoto.CopyToAsync(stream);
                        await stream.FlushAsync();
                    }
                }

                var user = new CustomClient
                {
                    ProfilePhoto = uniquieFileNameWithExtension,
                    FirstName = registerInput.FirstName,
                    LastName = registerInput.LastName,
                    UserName = registerInput.Email,
                    Email = registerInput.Email,
                    PhoneNumber = registerInput.PhoneNumber,
                    YearOfBirth = registerInput.YearOfBirth,
                    WebCredit = 0
                };

                var result = await _userManager.CreateAsync(user, registerInput.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Client");

                    //_logger.LogInformation("User created a new account with password.");

                    /*var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");*/

                    ///////////////////////////////////////////////
                    ///ovde dodajem novu rolu korisniku koji se registrovao
                    /*
                    var userIdOfLoggedUser = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var loggedClient = await _userManager.FindByIdAsync(userIdOfLoggedUser);
                    var r = await _userManager.AddToRoleAsync(loggedClient, "Client");*/
                    //treba jos da dodam error da ako se ne doda u odredjenu rolu, izbacu gresku i redirect
                    ////////////////////////////////////////////////

                    //if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    //{
                    //return RedirectToPage("RegisterConfirmation", new { email = registerInput.Email, returnUrl = returnUrl });
                    //}
                    //else
                    //{

                    await _signInManager.SignInAsync(user, isPersistent: false);

                    _session.SetToast("Registered successfully", null, "success");

                    return LocalRedirect(returnUrl);

                    //}
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(registerInput);
        }

        [HttpPost]
        public async Task<IActionResult> SignOut(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();

            if (returnUrl != null)
            {
                _session.SetToast("Signed out successfully", null, "success");
                return LocalRedirect(returnUrl);
            }
            else
            {
                _session.SetToast("Signed out successfully", null, "success");
                return RedirectToPage("Index");
            }
        }
    }
}
