using Database.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Framework.Helpers.ExtensionMethods
{
    public static class HtmlHelperExtensionMethods
    {
        //private static HttpContext _httpContext => new HttpContextAccessor().HttpContext;
        private static IWebHostEnvironment _environment => new HttpContextAccessor().HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
        //private static IActionContextAccessor _actionContext => new HttpContextAccessor().HttpContext.RequestServices.GetRequiredService<IActionContextAccessor>();
        //private static IUrlHelperFactory _urlHelper => new HttpContextAccessor().HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();

        /* !!! ovo mogu kasnije da iskoristim, trenutno nemam zivaca da ispravljam svuda !!! */
        //private static UserManager<CustomClient> _userManager => new HttpContextAccessor().HttpContext.RequestServices.GetRequiredService<UserManager<CustomClient>>();

        public static async Task RenderMenuBasedOnUserRole(this IHtmlHelper htmlHelper, ClaimsPrincipal User, UserManager<CustomClient> userManager)
        {
            try
            {
                var user = await userManager.GetUserAsync(User);

                var userRoles = await userManager.GetRolesAsync(user);

                //svaki user ce svakako imati samo jednu rolu, tako da bez razmisljanja uzimam prvi naziv role
                var userRole = userRoles.ElementAt(0);

                string viewName = "Menues/_" + userRole + "Menu";

                await htmlHelper.RenderPartialAsync(viewName);
            }
            catch (Exception)
            {
                await htmlHelper.RenderPartialAsync("Menues/_ClientMenu");
            }
        }

        public static async Task<string> RenderUserProfilePhotoOrDefaultPhoto(this IHtmlHelper htmlHelper, ClaimsPrincipal User, UserManager<CustomClient> userManager)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await userManager.FindByIdAsync(userId);

            string profilePhotoUrl = !string.IsNullOrEmpty(user.ProfilePhoto) ? @"/images/user-images/" + user.ProfilePhoto : @"/images/content-images/default-user-image.png";

            return profilePhotoUrl;
        }

        public static async Task<string> RenderUserProfilePhotoOrDefaultPhoto(this IHtmlHelper htmlHelper, string userId, UserManager<CustomClient> userManager)
        {
            var user = await userManager.FindByIdAsync(userId);

            string profilePhotoUrl = !string.IsNullOrEmpty(user.ProfilePhoto) ? @"/images/user-images/" + user.ProfilePhoto : @"/images/content-images/default-user-image.png";

            return profilePhotoUrl;
        }

        // !!! ova methoda je losa da se koristi jer ne proverava ako fajl ne postoji a u bazi stoji da ima sliku (), ispraviti.
        public static string RenderUserProfilePhotoOrDefaultPhoto(this IHtmlHelper htmlHelper, string profilePhoto)
        {
            //var profilePhotoTemp = !string.IsNullOrEmpty(profilePhoto) ? @"/images/user-images/" + profilePhoto : @"/images/content-images/default-user-image.png";

            if (!string.IsNullOrEmpty(profilePhoto))
            {
                string path = Path.Combine(_environment.WebRootPath, @"images\user-images\" + profilePhoto);

                FileInfo file = new FileInfo(path);

                if (file.Exists)
                    return @"/images/user-images/" + profilePhoto;
            }

            return @"/images/content-images/default-user-image.png";
        }

        public static async Task<string> GetNotificationsUrlBasedOnRole(this IHtmlHelper htmlHelper, ClaimsPrincipal User, UserManager<CustomClient> userManager)
        {
            try
            {
                var user = await userManager.GetUserAsync(User);

                var userRoles = await userManager.GetRolesAsync(user);

                //svaki user ce svakako imati samo jednu rolu, tako da bez razmisljanja uzimam prvi naziv role
                var userRole = userRoles.ElementAt(0);

                if (userRole.ToLower() == "admin")
                    return "/Admin/Notifications/";

                else if(userRole.ToLower() == "therapist")
                    return "/Therapist/Notifications/";
                else
                    return "/Notifications/";

                //else if (userRole.ToLower() == "client")
                //{
                //    areaName = "Client";
                //}

                //var actionContext = new ActionContext(_httpContext, _httpContext.GetRouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());

                //var urlHelper = _urlHelper.GetUrlHelper(_actionContext.ActionContext);
                //return urlHelper.Action(null, "Notifications", new { Area = areaName }); // Will output the proper link according to routing info
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }

        public static string FixSlashesForJavascriptUrl(this IHtmlHelper htmlHelper, string url)
        {
            return url.Replace(@"\", @"/");
        }

    }
}
