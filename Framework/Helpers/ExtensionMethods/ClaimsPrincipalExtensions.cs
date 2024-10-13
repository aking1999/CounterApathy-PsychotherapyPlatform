using Database.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Framework.Providers;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Framework.Interfaces;
using Framework.Implementations;
using Microsoft.AspNetCore.Hosting;

namespace Framework.Helpers.ExtensionMethods
{
    public static class ClaimsPrincipalExtensions
    {
        private static IFileRepository _files;
        private static IUrlHelper _url => new HttpContextAccessor().HttpContext.RequestServices.GetRequiredService<IUrlHelper>();
        private static UserManager<CustomClient> _userManager => new HttpContextAccessor().HttpContext.RequestServices.GetRequiredService<UserManager<CustomClient>>();

        static ClaimsPrincipalExtensions()
        {
            _files = new FileRepository();
        }

        public static async Task<string> GetAreaAsync(this ClaimsPrincipal User)
        {
            try
            {
                if (!User.Identity.IsAuthenticated) return string.Empty;

                var role = await User.GetRoleAsync();

                if (string.IsNullOrEmpty(role)) return string.Empty;

                return role.ToLower() == UserRoles.Client.ToLower() ? string.Empty : role;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static async Task<string> GetRoleAsync(this ClaimsPrincipal User)
        {
            if (!User.Identity.IsAuthenticated) return null;

            var user = await _userManager.GetUserAsync(User);

            if (user == null) return null;

            return (await _userManager.GetRolesAsync(user))?.SingleOrDefault();
        }

        public static bool HasSupportAvailable(this ClaimsPrincipal User)
        {
            if (!User.Identity.IsAuthenticated) return true;

            if (User.IsInRole(UserRoles.Client) || User.IsInRole(UserRoles.Therapist)) return true;

            return false;
        }

        public static async Task<string> GetSupportUrlAsync(this ClaimsPrincipal User)
        {
            if (User.HasSupportAvailable())
            {
                string userRole = await User.GetRoleAsync();

                if (string.IsNullOrWhiteSpace(userRole) || userRole == UserRoles.Client) return new PathString(_url.Action("CustomerSupport", "Home")).Value;
                else if (userRole == UserRoles.Therapist) return new PathString(_url.Action("Support", "Account", new { Area = "Therapist" })).Value;
            }

            return string.Empty;
        }

        public static string GetUserName(this ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.Name);

        public static string GetDafaultProfilePhotoPath(this ClaimsPrincipal User)
        {
            return _files.DefaultProfilePhotoPath;
        }

        public static string GetProfilePhotoPathOrDefaultPhotoPath(this ClaimsPrincipal User, IWebHostEnvironment environment)
        {
            return _files.GetUserProfilePhotoPathOrDefaultPhotoPath(_userManager, environment, User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        public static bool HasAppliedForTherapistAccount(this ClaimsPrincipal User)
        {
            if (!User.Identity.IsAuthenticated) return false;

            //var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var userId = _userManager.GetUserId(User);
            return !string.IsNullOrWhiteSpace(userId) ? _userManager.HasAppliedForTherapistAccount(userId) : false;
        }
    }
}