using Database.Models;
using Framework.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication9.Base;

namespace WebApplication9.Areas.Admin.Controllers
{
    [Area(areaName: "Admin")]
    [Authorize(Roles = "Admin")]
    public class NotificationsController : BaseController
    {
        public NotificationsController(INotificationRepository notificationRepository,
            UserManager<CustomClient> userManager) : base(notificationRepository, userManager) { }

        [HttpGet]
        public IActionResult Get()
        {
            var userId = _userManager.GetUserId(User);
            var notification = _notificationRepository.Get(userId);
            return Ok(new { UserNotification = notification, Count = notification.Count });
        }

        [HttpPost]
        public IActionResult Read(string notificationId)
        {
            _notificationRepository.Read(notificationId, _userManager.GetUserId(HttpContext.User));

            return Ok();
        }
    }
}
