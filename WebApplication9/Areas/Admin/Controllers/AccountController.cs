using Database.Models;
using Framework.Helpers.ExtensionMethods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebApplication9.Areas.Admin.ViewModels;
using WebApplication9.Base;
using WebApplication9.Helpers;
using Framework.Notifications;
using WebApplication9.ViewModels;
using Framework.Emails;
using Framework.Interfaces;
using WebApplication9.Interfaces;
using WebApplication9.Implementations;
using Framework.Models;
using System.Linq;
using WebApplication9.Areas.Admin.Helpers;
using Microsoft.AspNetCore.Http;
using Framework.Implementations;
using Framework.Helpers;

namespace WebApplication9.Areas.Admin.Controllers
{
    [Area(areaName: "Admin")]
    [Authorize(Roles = "Admin")]
    public class AccountController : BaseController
    {
        private readonly IFileRepository _files;
        private readonly IDropdownHelper _dropdown;
        private readonly IWebHostEnvironment _environment;

        public AccountController(IErrorLogger error,
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
            _dropdown = new DropdownHelper();
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            try
            {
                ShowToastOnThisPageIfSet();

                var user = await _userManager.GetUserAsync(User);

                if (user == null) throw new GeneralException("Unable to load user.", signOutUser: true);

                var viewModel = new AdminProfileViewModel();
                viewModel.Map(user);

                return View(viewModel);
            }
            catch(Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminProfileViewModel profile)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null) throw new GeneralException("Unable to load user.", signOutUser: true);

                if (!ModelState.IsValid)
                {
                    profile.MapProfilePhoto(user);

                    _session.SetToast("Profile not edited", "Check your entire profile page and try again.", "info");
                    ViewBag.toast = _session.GetToast();
                    _session.RemoveToastFromKeys();

                    return View("Profile", profile);
                }
                    
                await user.MapAsync(profile, _environment);

                var update = await _userManager.UpdateAsync(user);

                if (!update.Succeeded) throw new GeneralException(string.Join("|", update.Errors.Select(e => e.Description)));

                _session.SetToast("Profile edited successfully", null, "success");
                return RedirectToAction("Profile", "Account", new { Area = "Admin" });
            }
            catch(Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null) throw new Exception("Unable to load user.");

                if (!await _userManager.HasPasswordAsync(user)) throw new Exception("User does not have a password.");

                return View(new ChangePasswordViewModel());
            }
            catch(Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(viewModel);

                var user = await _userManager.GetUserAsync(User);

                if (user == null) throw new GeneralException("Unable to load user.", signOutUser: true);

                var change = await _userManager.ChangePasswordAsync(user, viewModel.OldPassword, viewModel.NewPassword);

                if (!change.Succeeded)
                {
                    var errors = string.Empty;

                    foreach (var error in change.Errors)
                    {
                        errors += error.Description + "|";
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    await HandleErrorAsync(errors);
                    return View(viewModel);
                }

                await _signInManager.RefreshSignInAsync(user);

                _session.SetToast("Password changed successfully", null, "success");

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
                    Title = "You have changed your password.",
                    Body = null,
                    Severity = "primary",
                    Read = false,
                    SendingDateTime = DateTime.UtcNow,
                    Icon = "fal fa-sync-alt",
                    Important = false
                });

                await _mailService.SendPasswordChangedEmailAsync(new Framework.Emails.EmailTypes.PasswordChangedEmail
                {
                    ToEmail = user.Email,
                    FirstName = user.FirstName
                }, includeTemplateIfExists: true);

                return RedirectToAction("Profile", "Account", new { Area = "Admin" });
            }
            catch(Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProfilePhoto()
        {
            try
            {
                var delete = await _userManager.DeleteProfilePhotoIfExists(_environment, User);

                if (delete.Succeeded)
                    return Json(new
                    {
                        success = true,
                        title = "Profile photo deleted successfully",
                        body = "",
                        severity = "success",
                        path = _files.DefaultProfilePhotoPath
                    });

                await HandleErrorJsonAsync(string.Join("|", delete.Errors.Select(e => e.Description)));

                return Json(new
                {
                    success = false,
                    title = "An error occurred",
                    body = "",
                    severity = "error"
                });
            }
            catch(Exception e)
            {
                await HandleErrorJsonAsync(e);

                return Json(new
                {
                    success = false,
                    title = "An error occurred",
                    body = "",
                    severity = "error"
                });
            }
        }
    }
}
