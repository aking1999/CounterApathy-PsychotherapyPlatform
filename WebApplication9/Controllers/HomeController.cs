using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using WebApplication9.Models;
using Microsoft.AspNetCore.Http;
using WebApplication9.Base;
using Framework.Notifications;

namespace WebApplication9.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(INotificationRepository notificationRepository,
            ILogger<HomeController> logger) : base(notificationRepository)
        {
            _logger = logger;
        }
        
        public IActionResult Index()
        {
            ShowToastOnThisPageIfSet();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
