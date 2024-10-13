using Database.Models;
using Framework.Emails;
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

namespace WebApplication9.Areas.Admin.Controllers
{
    [Area(areaName: "Admin")]
    [Authorize(Roles = "Admin")]
    public class NotificationsController : BaseController
    {
        public NotificationsController(IErrorLogger error,
            IMailService mailService,
            IDateTimeHelper dateHelper,
            IHttpContextAccessor contextAccessor,
            INotificationRepository notificationRepository,
            UserManager<CustomClient> userManager,
            SignInManager<CustomClient> signInManager) : base(error, mailService, dateHelper, contextAccessor, notificationRepository, userManager, signInManager) { }


        //if route changed here, must also change in Framework.Helpers.IgnoreRoutes
        [HttpGet]
        public async Task<OkObjectResult> GetAll()
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                if (string.IsNullOrWhiteSpace(userId)) throw new GeneralException("User ID is null.");

                var notifications = _notificationRepository.GetAll(userId);
                return Ok(new
                {
                    UserNotification = notifications,
                    UnreadCount = notifications.Where(n => n.Read == false).Count(),
                    AllCount = notifications.Count
                });
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
                var userId = _userManager.GetUserId(User);

                if (string.IsNullOrWhiteSpace(userId)) throw new GeneralException("User ID is null.");

                var notifications = _notificationRepository.GetUnread(userId);
                return Ok(new
                {
                    UserNotification = notifications,
                    UnreadCount = notifications.Count
                });
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
                var userId = _userManager.GetUserId(User);

                if (string.IsNullOrWhiteSpace(userId)) throw new GeneralException("User ID is null.");

                var notifications = _notificationRepository.GetAll(userId);
                var important = notifications.Where(n => n.Important == true).ToList();
                return Ok(new
                {
                    UserNotification = important,
                    UnreadCount = notifications.Where(n => n.Read == false).Count(),
                    ImportantCount = important.Count
                });
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
                var userId = _userManager.GetUserId(User);

                if (string.IsNullOrWhiteSpace(userId)) throw new GeneralException("User ID is null.");

                await _notificationRepository.ReadAsync(notificationId, userId);
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
                var userId = _userManager.GetUserId(User);

                if (string.IsNullOrWhiteSpace(userId)) throw new GeneralException("User ID is null.");

                await _notificationRepository.ImportantAsync(notificationId, userId);
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
                var userId = _userManager.GetUserId(User);

                if (string.IsNullOrWhiteSpace(userId)) throw new GeneralException("User ID is null.");

                await _notificationRepository.UnimportantAsync(notificationId, userId);
            }
            catch (Exception e)
            {
                await HandleErrorJsonAsync(e);
            }

            return Ok();
        }

        [HttpGet("/admin/notifikacije")]
        public async Task<IActionResult> All()
        {
            try
            {
                if ((await _userManager.GetUserAsync(User)) == null) throw new GeneralException("Unable to load user.", signOutUser: true);

                return View();
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpGet("/admin/notifikacije/nepročitano")]
        public async Task<IActionResult> Unread()
        {
            try
            {
                if ((await _userManager.GetUserAsync(User)) == null) throw new GeneralException("Unable to load user.", signOutUser: true);

                return View();
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }

        [HttpGet("/admin/notifikacije/bitno")]
        public async Task<IActionResult> Important()
        {
            try
            {
                if ((await _userManager.GetUserAsync(User)) == null) throw new GeneralException("Unable to load user.", signOutUser: true);

                return View();
            }
            catch (Exception e)
            {
                return await HandleErrorAsync(e);
            }
        }
    }
}
