using Database.Models;
using Database.RepositoryImplementations;
using Framework.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Framework.Helpers.ExtensionMethods
{
    public static class UserManagerExtensionMethods
    {
        public static async Task<IdentityResult> DeleteProfilePhotoIfExists(this UserManager<CustomClient> userManager, ClaimsPrincipal User, IWebHostEnvironment environment)
        {
            var customClient = await userManager.GetUserAsync(User);

            if (!string.IsNullOrEmpty(customClient.ProfilePhoto))
            {
                string path = Path.Combine(environment.WebRootPath, @"images\user-images\" + customClient.ProfilePhoto);

                FileInfo file = new FileInfo(path);

                if (file.Exists)
                {
                    try
                    {
                        file.Delete();

                        customClient.ProfilePhoto = null;

                        var result = await userManager.UpdateAsync(customClient);
                        if (result.Succeeded)
                            return IdentityResult.Success;
                        return IdentityResult.Failed(
                            new IdentityError[1]
                            {
                                new IdentityError
                                {
                                    Code = "500",
                                    Description = "Error while deleting your profile photo. Please try again."
                                }
                            });
                    }
                    catch (Exception)
                    {
                        return IdentityResult.Failed(
                            new IdentityError[1]
                            {
                                new IdentityError
                                {
                                    Code = "500",
                                    Description = "Error while deleting your profile photo. Please try again."
                                }
                            });
                    }

                }

                return IdentityResult.Success;
            }
            return IdentityResult.Success;
        }

        public static bool HasAppliedForTherapistAccount(this UserManager<CustomClient> userManager, CustomClient customClient, UnitOfWork _context)
        {
            var numberOfApplications = _context.TherapistApplications.Find(app => app.UserId == customClient.Id).Count();

            if (numberOfApplications > 0)
                return true;
            return false;
        }

        public static async Task<string> GetUserRoleAsync(this UserManager<CustomClient> userManager, ClaimsPrincipal User)
        {
            var user = await userManager.GetUserAsync(User);

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

        public static async Task<string> GetUserRoleAsync(this UserManager<CustomClient> userManager, string userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            if(user != null)
            {
                var userRoles = await userManager.GetRolesAsync(user);

                if(userRoles != null)
                {
                    return userRoles.ElementAt(0);
                }

                return "404: User's role not found.";
            }

            return "404: User with specified ID not found.";
            
        }
    }
}
