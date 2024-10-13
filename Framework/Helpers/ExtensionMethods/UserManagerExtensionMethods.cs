using Database.Models;
using Database.RepositoryImplementations;
using Framework.Implementations;
using Framework.Interfaces;
using Framework.Providers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Framework.Helpers.ExtensionMethods
{
    public static class UserManagerExtensionMethods
    {
        private const string PROJECT = "Framework";
        private const string CLASS = "UserManagerExtensionMethods";

        private static IFileRepository _files;
        private static ISystemErrorLogger _systemError;
        private static UnitOfWork _context;

        static UserManagerExtensionMethods()
        {
            _files = new FileRepository();
            _context = new UnitOfWork(new LajsnaProbaContext());
            _systemError = new SystemErrorLogger();
        }

        public static async Task<IdentityResult> DeleteProfilePhotoIfExists(this UserManager<CustomClient> userManager, IWebHostEnvironment environment, ClaimsPrincipal User)
        {
            var user = await userManager.GetUserAsync(User);

            _files.DeleteUserImage(environment, user.ProfilePhoto);
            user.ProfilePhoto = null;
            return await userManager.UpdateAsync(user);
        }

        public static bool HasAppliedForTherapistAccount(this UserManager<CustomClient> userManager, string userId)
        {
            return _context.TherapistApplications.ReadOnlyAny(app => app.UserId == userId);
        }

        public static bool HasPendingApplicationForTherapistAccount(this UserManager<CustomClient> userManager, string userId)
        {
            return _context.TherapistApplications.ReadOnlyAny(app => app.UserId == userId && app.Accepted == 0);
        }

        public static async Task<bool> HasPendingApplicationForTherapistAccountCheckByEmail(this UserManager<CustomClient> userManager, string email)
        {
            var userId = (await userManager.FindByEmailAsync(email)).Id;
            return !string.IsNullOrWhiteSpace(userId) && _context.TherapistApplications.ReadOnlyAny(app => app.UserId == userId && app.Accepted == 0);
        }

        public static async Task<string> GetUserRoleAsync(this UserManager<CustomClient> userManager, string userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user != null)
            {
                var userRoles = await userManager.GetRolesAsync(user);

                if (userRoles != null)
                {
                    return userRoles.ElementAt(0);
                }

                return "404: User's role not found.";
            }

            return "404: User with specified ID not found.";
        }

        public static async Task<CustomClient> FindByTherapistAccountIdAsync(this UserManager<CustomClient> userManager, string therapistId)
        {
            try
            {
                var user = userManager?.Users?.SingleOrDefault(x => x.TherapistAccountId == therapistId);

                if (user == null) return null;

                return await userManager.IsInRoleAsync(user, UserRoles.Therapist) ? user : null;
            }
            catch(Exception e)
            {
                _systemError.SaveError(e, PROJECT, CLASS, "FindByTherapistAccountIdAsync");
                return null;
            }
        }
    }
}
