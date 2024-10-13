using Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Framework.Notifications;
using WebApplication9.Base;
using System.Linq;
using Framework.Emails;
using System.Threading.Tasks;
using Framework.Interfaces;
using System;
using System.Collections.Generic;
using WebApplication9.Interfaces;
using WebApplication9.Implementations;
using Framework.Models;
using Microsoft.AspNetCore.Http;

namespace WebApplication9.Areas.Therapist.Controllers
{
    [Area(areaName: "Therapist")]
    [Authorize(Roles = "Therapist")]
    public class NotificationsController : BaseController
    {
        private readonly ITherapistFunctionsProvider _therapistFunctions;

        public NotificationsController(IErrorLogger error,
            IMailService mailService,
            IDateTimeHelper dateHelper,
            IHttpContextAccessor contextAccessor,
            INotificationRepository notificationRepository,
            UserManager<CustomClient> userManager,
            SignInManager<CustomClient> signInManager) : base(error, mailService, dateHelper, contextAccessor, notificationRepository, userManager, signInManager)
        {
            _therapistFunctions = new TherapistFunctionsProvider();
        }

        //if route changed here, must also change in Framework.Helpers.IgnoreRoutes
        [HttpGet]
        public async Task<OkObjectResult> GetAll()
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                {
                    var notifications = _notificationRepository.GetAll(_userManager.GetUserId(User));
                    return Ok(new
                    {
                        UserNotification = notifications,
                        UnreadCount = notifications.Where(n => n.Read == false).Count(),
                        AllCount = notifications.Count
                    });
                }
                else if (completionStatus == TherapistAccountCompletionStatus.NotSetUpYet)
                    return Ok(new
                    {
                        UserNotification = new List<NotificationsViewModel>(),
                        UnreadCount = 0,
                        AllCount = 0
                    });
                else throw new GeneralException("Unable to load user.");
            }
            catch (Exception e)
            {
                await HandleErrorJsonAsync(e);
                return Ok(new
                {
                    UserNotification = new List<NotificationsViewModel>(),
                    UnreadCount = 0,
                    AllCount = 0
                });
            }
        }

        //if route changed here, must also change in Framework.Helpers.IgnoreRoutes
        [HttpGet]
        public async Task<OkObjectResult> GetUnread()
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                {
                    var notifications = _notificationRepository.GetUnread(_userManager.GetUserId(User));
                    return Ok(new
                    {
                        UserNotification = notifications,
                        UnreadCount = notifications.Count
                    });
                }
                else if (completionStatus == TherapistAccountCompletionStatus.NotSetUpYet)
                    return Ok(new
                    {
                        UserNotification = new List<NotificationsViewModel>(),
                        UnreadCount = 0
                    });
                else throw new GeneralException("Unable to load user.");
            }
            catch (Exception e)
            {
                await HandleErrorJsonAsync(e);
                return Ok(new
                {
                    UserNotification = new List<NotificationsViewModel>(),
                    UnreadCount = 0
                });
            }
        }

        //if route changed here, must also change in Framework.Helpers.IgnoreRoutes
        [HttpGet]
        public async Task<OkObjectResult> GetImportant()
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed)
                {
                    var notifications = _notificationRepository.GetAll(_userManager.GetUserId(User));
                    var important = notifications.Where(n => n.Important == true).ToList();
                    return Ok(new
                    {
                        UserNotification = important,
                        UnreadCount = notifications.Where(n => n.Read == false).Count(),
                        ImportantCount = important.Count
                    });
                }
                else if (completionStatus == TherapistAccountCompletionStatus.NotSetUpYet)
                    return Ok(new
                    {
                        UserNotification = new List<NotificationsViewModel>(),
                        UnreadCount = 0,
                        ImportantCount = 0
                    });
                else throw new GeneralException("Unable to load user.");
            }
            catch (Exception e)
            {
                await HandleErrorJsonAsync(e);
                return Ok(new
                {
                    UserNotification = new List<NotificationsViewModel>(),
                    UnreadCount = 0,
                    ImportantCount = 0
                });
            }
        }

        //if route changed here, must also change in Framework.Helpers.IgnoreRoutes
        [HttpPost]
        public async Task<OkResult> SetRead(string notificationId)
        {
            try
            {
                if (await _therapistFunctions.TherapistHasCompletedAccountSetupAsync() == TherapistAccountCompletionStatus.Completed)
                    await _notificationRepository.ReadAsync(notificationId, _userManager.GetUserId(User));
            }
            catch (Exception e)
            {
                await HandleErrorJsonAsync(e);
            }

            return Ok();
        }

        //if route changed here, must also change in Framework.Helpers.IgnoreRoutes
        [HttpPost]
        public async Task<OkResult> SetImportant(string notificationId)
        {
            try
            {
                if (await _therapistFunctions.TherapistHasCompletedAccountSetupAsync() == TherapistAccountCompletionStatus.Completed)
                    await _notificationRepository.ImportantAsync(notificationId, _userManager.GetUserId(User));
            }
            catch (Exception e)
            {
                await HandleErrorJsonAsync(e);
            }

            return Ok();
        }

        //if route changed here, must also change in Framework.Helpers.IgnoreRoutes
        [HttpPost]
        public async Task<OkResult> SetUnimportant(string notificationId)
        {
            try
            {
                if (await _therapistFunctions.TherapistHasCompletedAccountSetupAsync() == TherapistAccountCompletionStatus.Completed)
                    await _notificationRepository.UnimportantAsync(notificationId, _userManager.GetUserId(User));
            }
            catch (Exception e)
            {
                await HandleErrorJsonAsync(e);
            }

            return Ok();
        }

        [HttpGet("/terapeut/notifikacije")]
        public async Task<IActionResult> All()
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed) return View();
                else if (completionStatus == TherapistAccountCompletionStatus.NotSetUpYet) return RedirectToAction("AccountSetup", "Account", new { Area = "Therapist" });
                else throw new GeneralException("Unable to load user.", signOutUser: true);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpGet("/terapeut/notifikacije/nepročitano")]
        public async Task<IActionResult> Unread()
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed) return View();
                else if (completionStatus == TherapistAccountCompletionStatus.NotSetUpYet) return RedirectToAction("AccountSetup", "Account", new { Area = "Therapist" });
                else throw new GeneralException("Unable to load user.", signOutUser: true);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpGet("/terapeut/notifikacije/bitno")]
        public async Task<IActionResult> Important()
        {
            try
            {
                var completionStatus = await _therapistFunctions.TherapistHasCompletedAccountSetupAsync();
                if (completionStatus == TherapistAccountCompletionStatus.Completed) return View();
                else if (completionStatus == TherapistAccountCompletionStatus.NotSetUpYet) return RedirectToAction("AccountSetup", "Account", new { Area = "Therapist" });
                else throw new GeneralException("Unable to load user.", signOutUser: true);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }
    }
}
