using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using Database.Models;
using Database.RepositoryImplementations;
using Framework.Emails;
using Framework.Interfaces;
using Framework.Notifications;
using Framework.Helpers.ExtensionMethods;
using Framework.Models;
using Framework.Implementations;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace WebApplication9.Base
{
    public class BaseController : Controller
    {
        private ICompositeViewEngine _viewEngine => HttpContext.RequestServices.GetRequiredService<ICompositeViewEngine>();

        protected readonly ISession _session;
        protected readonly IErrorLogger _errors;
        protected readonly IMailService _mailService;
        protected readonly IDateTimeHelper _dateHelper;
        protected readonly INotificationRepository _notificationRepository;
        protected readonly UnitOfWork _context;
        protected readonly UserManager<CustomClient> _userManager;
        protected readonly SignInManager<CustomClient> _signInManager;
        protected readonly RoleManager<IdentityRole> _roleManager;
        private string AREA_NAME => HttpContext?.GetRouteData()?.Values["area"]?.ToString();
        private string CONTROLLER_NAME => HttpContext?.GetRouteValue("controller")?.ToString();
        private string ACTION_NAME => HttpContext?.GetRouteValue("action")?.ToString();

        /*protected const string TOAST_HEADER_ERROR = "An error occurred";
        protected const string TOAST_BODY_ERROR = "Please log in again.";
        protected const string TOAST_SEVERITY_ERROR = "error";
        protected const string REDIRECT_ACTION_ERROR = ""*/

        protected BaseController(IErrorLogger error,
            IHttpContextAccessor contextAccessor)
        {
            _session = contextAccessor.HttpContext.Session;
            _errors = error;
        }

        protected BaseController(IErrorLogger error,
            IMailService mailService,
            IDateTimeHelper dateHelper,
            IHttpContextAccessor contextAccessor,
            INotificationRepository notificationRepository,
            UserManager<CustomClient> userManager,
            SignInManager<CustomClient> signInManager)
        {
            _session = contextAccessor.HttpContext.Session;
            _errors = error;
            _mailService = mailService;
            _dateHelper = dateHelper;
            _notificationRepository = notificationRepository;
            _context = new UnitOfWork(new LajsnaProbaContext());
            _userManager = userManager;
            _signInManager = signInManager;
        }

        protected BaseController(IErrorLogger errors,
            IMailService mailService,
            IDateTimeHelper dateHelper,
            IHttpContextAccessor contextAccessor,
            INotificationRepository notificationRepository,
            UserManager<CustomClient> userManager)
        {
            _session = contextAccessor.HttpContext.Session;
            _errors = errors;
            _mailService = mailService;
            _dateHelper = dateHelper;
            _notificationRepository = notificationRepository;
            _context = new UnitOfWork(new LajsnaProbaContext());
            _userManager = userManager;
        }

        protected BaseController(IMailService mailService,
            IDateTimeHelper dateHelper,
            IHttpContextAccessor contextAccessor,
            INotificationRepository notificationRepository,
            UserManager<CustomClient> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<CustomClient> signInManager)
        {
            _session = contextAccessor.HttpContext.Session;
            _errors = new ErrorLogger(contextAccessor);
            _mailService = mailService;
            _dateHelper = dateHelper;
            _notificationRepository = notificationRepository;
            _context = new UnitOfWork(new LajsnaProbaContext());
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        [NonAction]
        protected void ShowToastOnThisPageIfSet()
        {
            if (_session.HasToast())
            {
                ViewBag.toast = _session.GetToast();
                _session.RemoveToastFromKeys();
            }
        }

        [NonAction]
        protected async Task<RedirectToActionResult> HandleErrorAsync(string errorMessage)
        {
            try
            {
                await _errors.SaveErrorAsync(errorMessage, AREA_NAME, CONTROLLER_NAME, ACTION_NAME);
            }
            catch (Exception e)
            {
                _errors.SaveError(e, "WebApplication9", "BaseController", "HandleErrorAsync");
            }

            TempData["isSearchedByUrl"] = false;
            return RedirectToAction("Error", "Error", new { Area = "" });
        }

        [NonAction]
        protected async Task<JsonResult> HandleErrorJsonAsync(string errorMessage)
        {
            try
            {
                await _errors.SaveErrorAsync(errorMessage, AREA_NAME, CONTROLLER_NAME, ACTION_NAME);
            }
            catch (Exception e)
            {
                _errors.SaveError(e, "WebApplication9", "BaseController", "HandleErrorJsonAsync");
            }

            TempData["isSearchedByUrl"] = false;
            return Json(new
            {
                success = false,
                redirectUrl = Url.Action("Error", "Error", new { Area = "" })
            });
        }

        [NonAction]
        protected async Task<RedirectToActionResult> HandleErrorAsync(Exception e)
        {
            var signOutUser = false;

            if (e.GetType() == typeof(GeneralException))
                signOutUser = ((GeneralException)e).SignOutUser;

            try
            {
                await _errors.SaveErrorAsync(e, AREA_NAME, CONTROLLER_NAME, ACTION_NAME);

                if (signOutUser && User.Identity.IsAuthenticated) await _signInManager.SignOutAsync();
            }
            catch (Exception ex)
            {
                _errors.SaveError(ex, "WebApplication9", "BaseController", "HandleErrorAsync");

                if (signOutUser && User.Identity.IsAuthenticated) _signInManager.SignOutAsync().Wait();
            }

            TempData["isSearchedByUrl"] = false;
            return RedirectToAction("Error", "Error", new { Area = "" });
        }

        [NonAction]
        protected async Task<JsonResult> HandleErrorJsonAsync(Exception e)
        {
            var signOutUser = false;

            if (e.GetType() == typeof(GeneralException))
                signOutUser = ((GeneralException)e).SignOutUser;

            try
            {
                await _errors.SaveErrorAsync(e, AREA_NAME, CONTROLLER_NAME, ACTION_NAME);

                if (signOutUser && User.Identity.IsAuthenticated) await _signInManager.SignOutAsync();
            }
            catch (Exception ex)
            {
                _errors.SaveError(ex, "WebApplication9", "BaseController", "HandleErrorJsonAsync");

                if (signOutUser && User.Identity.IsAuthenticated) _signInManager.SignOutAsync().Wait();
            }

            TempData["isSearchedByUrl"] = false;
            return Json(new
            {
                success = false,
                redirectUrl = Url.Action("Error", "Error", new { Area = "" })
            });
        }

        protected async Task<string> RenderPartialViewToStringAsync(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.ActionDescriptor.ActionName;

            ViewData.Model = model;

            using (var writer = new StringWriter())
            {
                ViewEngineResult viewResult =
                    _viewEngine.FindView(ControllerContext, viewName, false);

                ViewContext viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);

                return writer.GetStringBuilder().ToString();
            }
        }
    }
}