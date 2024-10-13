using Database.Models;
using Framework.Interfaces;
using Framework.Models;
using Framework.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Framework.Helpers.ExtensionMethods
{
    public static class UrlHelperExtensions
    {
        private static IErrorLogger _errors => new HttpContextAccessor().HttpContext?.RequestServices?.GetRequiredService<IErrorLogger>();
        private static UserManager<CustomClient> _userManager => new HttpContextAccessor().HttpContext?.RequestServices?.GetRequiredService<UserManager<CustomClient>>();
        private static ClaimsPrincipal User => new HttpContextAccessor().HttpContext?.User;

        public static async Task<string> GetNotificationsUrlBasedOnRole(this IUrlHelper url)
        {
            try
            {
                var userRole = await User.GetRoleAsync();

                if (userRole == default) throw new GeneralException("User does not have a role or has multiple.", signOutUser: true);

                if (userRole.ToLower() == UserRoles.Admin.ToLower())
                    return "/admin/notifications/";

                else if (userRole.ToLower() == UserRoles.Therapist.ToLower())
                    return "/therapist/notifications/";
                else
                    return "/notifications/";
            }
            catch (Exception e)
            {
                _errors.SaveError(e, "Framework", "UrlHelperExtensions", "GetNotificationsUrlBasedOnRole");
                return string.Empty;
            }
        }

        public static async Task<string> AuthenticatedPsychotherapistPublicProfileUrlAsync(this IUrlHelper url)
        {
            try
            {
                if (!User.Identity.IsAuthenticated || !User.IsInRole(UserRoles.Therapist))
                    throw new Exception($"User is not in {UserRoles.Therapist} role.");

                return url.PsychotherapistPublicProfileUrl((await _userManager.GetUserAsync(User)).TherapistAccountId);
            }
            catch(Exception e)
            {
                _errors.SaveError(e, "Framework", "UrlHelperExtensions", "AuthenticatedPsychotherapistPublicProfileUrlAsync");
                return string.Empty;
            }
        }

        public static string AllPsychotherapistsUrl(this IUrlHelper url, string filter = "", string predicate = "")
        {
            try
            {
                return url.Action("All", "Therapists", new { filter = filter, predicate = predicate });
            }
            catch (Exception e)
            {
                _errors.SaveError(e, "Framework", "UrlHelperExtensions", "AllPsychotherapistsUrl");
                return string.Empty;
            }
        }

        public static string PsychotherapistPublicProfileUrl(this IUrlHelper url, string therapistId)
        {
            try
            {
                return url.Action("Profile", "Therapists", new { therapistId = therapistId });
            }
            catch(Exception e)
            {
                _errors.SaveError(e, "Framework", "UrlHelperExtensions", "PsychotherapistPublicProfileUrl");
                return string.Empty;
            }
        }

        public static string PsychotherapistPublicProfileUrlWithName(this IUrlHelper url, string therapistId, string therapistFirstName, string therapistLastName)
        {
            try
            {
                return url.Action("Profile", "Therapists", new { therapistId = therapistId, therapistName = $"{therapistFirstName}-{therapistLastName}".ToLower() });
            }
            catch (Exception e)
            {
                _errors.SaveError(e, "Framework", "UrlHelperExtensions", "PsychotherapistPublicProfileUrlWithName");
                return string.Empty;
            }
        }

        public static string AllBookedConsultationsUrl(this IUrlHelper url, string filter = "", string predicate = "")
        {
            return url.Action("BookedConsultations", "Consultations", new { filter = filter, predicate = predicate });
        }

        public static string AllPsychotherapistBookedConsultationsUrl(this IUrlHelper url, string filter = "", string predicate = "")
        {
            return url.Action("BookedConsultations", "Consultations", new { Area = "Therapist", filter = filter, predicate = predicate });
        }

        public static string AllBookedSessionsUrl(this IUrlHelper url, string filter = "", string predicate = "")
        {
            return url.Action("BookedSessions", "Sessions", new { filter = filter, predicate = predicate });
        }

        public static string AllPsychotherapistBookedSessionsUrl(this IUrlHelper url, string filter = "", string predicate = "")
        {
            return url.Action("BookedSessions", "Sessions", new { Area = "Therapist", filter = filter, predicate = predicate });
        }

        public static string AllTherapistWithdrawalsUrl(this IUrlHelper url, string filter = "", string predicate = "")
        {
            try
            {
                return url.Action("All", "Withdrawals", new { Area = "Therapist", filter = filter, predicate = predicate });
            }
            catch(Exception e)
            {
                _errors.SaveError(e, "Framework", "UrlHelperExtensions", "AllTherapistWithdrawalsUrl");
                return string.Empty;
            }
        }

        public static async Task<string> ProfileUrlAsync(this IUrlHelper url)
        {
            try
            {
                return url.Action("Profile", "Account", new { Area = await User.GetAreaAsync() });
            }
            catch (Exception e)
            {
                _errors.SaveError(e, "Framework", "UrlHelperExtensions", "ProfileUrlAsync");
                return string.Empty;
            }
        }
    }
}
