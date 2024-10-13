using Database.Models;
using Framework.Emails;
using Framework.Implementations;
using Framework.Interfaces;
using Framework.Models;
using Framework.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication9.Base;

namespace WebApplication9.Controllers
{
    [Authorize(Roles = "Client")]
    public class NotificationsController : BaseController
    {
        private readonly ICustomClientFunctionsProvider _clientFunctions;

        public NotificationsController(IErrorLogger error,
            IMailService mailService,
            IDateTimeHelper dateHelper,
            IHttpContextAccessor contextAccessor,
            INotificationRepository notificationRepository,
            UserManager<CustomClient> userManager,
            SignInManager<CustomClient> signInManager) : base(error, mailService, dateHelper, contextAccessor, notificationRepository, userManager, signInManager)
        {
            _clientFunctions = new CustomClientFunctionsProvider(contextAccessor);
        }

        //if route changed here, must also change in Framework.Helpers.IgnoreRoutes
        [HttpGet]
        public async Task<OkObjectResult> GetAll()
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
                {
                    var notifications = _notificationRepository.GetAll(_userManager.GetUserId(User));
                    return Ok(new
                    {
                        UserNotification = notifications,
                        UnreadCount = notifications.Where(n => n.Read == false).Count(),
                        AllCount = notifications.Count
                    });
                }
                else if (emailStatus == EmailConfirmationStatus.NotConfirmed)
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
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
                {
                    var notifications = _notificationRepository.GetUnread(_userManager.GetUserId(User));
                    return Ok(new
                    {
                        UserNotification = notifications,
                        UnreadCount = notifications.Count
                    });
                }
                else if (emailStatus == EmailConfirmationStatus.NotConfirmed)
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
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed)
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
                else if (emailStatus == EmailConfirmationStatus.NotConfirmed)
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
                if (await _clientFunctions.HasConfirmedEmailAsync() == EmailConfirmationStatus.Confirmed)
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
                if (await _clientFunctions.HasConfirmedEmailAsync() == EmailConfirmationStatus.Confirmed)
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
                if (await _clientFunctions.HasConfirmedEmailAsync() == EmailConfirmationStatus.Confirmed)
                    await _notificationRepository.UnimportantAsync(notificationId, _userManager.GetUserId(User));
            }
            catch (Exception e)
            {
                await HandleErrorJsonAsync(e);
            }

            return Ok();
        }

        [HttpGet("/notifikacije")]
        public async Task<IActionResult> All()
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed) return View();
                else if (emailStatus == EmailConfirmationStatus.NotConfirmed) return RedirectToAction("ConfirmEmail", "Authorization", new { Area = "" });
                else throw new GeneralException("Unable to load user.", signOutUser: true);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpGet("/notifikacije/nepročitano")]
        public async Task<IActionResult> Unread()
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed) return View();
                else if (emailStatus == EmailConfirmationStatus.NotConfirmed) return RedirectToAction("ConfirmEmail", "Authorization", new { Area = "" });
                else throw new GeneralException("Unable to load user.", signOutUser: true);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpGet("/notifikacije/bitno")]
        public async Task<IActionResult> Important()
        {
            try
            {
                var emailStatus = await _clientFunctions.HasConfirmedEmailAsync();
                if (emailStatus == EmailConfirmationStatus.Confirmed) return View();
                else if (emailStatus == EmailConfirmationStatus.NotConfirmed) return RedirectToAction("ConfirmEmail", "Authorization", new { Area = "" });
                else throw new GeneralException("Unable to load user.", signOutUser: true);
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }
    }
}
