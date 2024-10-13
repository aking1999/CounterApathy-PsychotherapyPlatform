using Database.Models;
using Framework.Implementations;
using Framework.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using WebApplication9.Areas.Therapist.ViewModels;
using WebApplication9.Implementations;
using WebApplication9.Interfaces;

namespace WebApplication9.Areas.Therapist.Helpers
{
    public static class ExtensionMethods
    {
        private static IFileRepository _files => new FileRepository();
        private static IDropdownHelper _dropdown => new DropdownHelper();
        private static IStripeFunctionsProvider _stripeFunctions => new StripeFunctionsProvider();
        private static ITherapistFunctionsProvider _therapistFunctions => new TherapistFunctionsProvider();
        private static IDateTimeHelper _dateHelper => new HttpContextAccessor().HttpContext.RequestServices.GetRequiredService<IDateTimeHelper>();
        private static IWebHostEnvironment _environment => new HttpContextAccessor().HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();

        public static void MapProfilePhoto(this TherapistProfileViewModel therapistVm, CustomClient customClient)
        {
            try
            {
                string folderName = !string.IsNullOrEmpty(customClient.ProfilePhoto) ? (@"/images/user-images/" + customClient.ProfilePhoto) : _files.DefaultProfilePhotoPath;

                using (var stream = System.IO.File.OpenRead($"{_environment.ContentRootPath + @"/wwwroot" + folderName}"))
                {
                    therapistVm.ProfilePhoto = new FormFile(stream, 0, stream.Length, null, System.IO.Path.GetFileName(stream.Name));
                }
            }
            catch (Exception)
            {
                using (var stream = System.IO.File.OpenRead($"{_environment.ContentRootPath + @"/wwwroot" + _files.DefaultProfilePhotoPath}"))
                {
                    therapistVm.ProfilePhoto = new FormFile(stream, 0, stream.Length, null, System.IO.Path.GetFileName(stream.Name));
                }
            }
        }

        public static void Map(this TherapistProfileViewModel therapistVm, CustomClient customClient)
        {
            MapProfilePhoto(therapistVm, customClient);

            therapistVm.FirstName = customClient.FirstName;
            therapistVm.LastName = customClient.LastName;
            therapistVm.Email = customClient.Email;
            //therapistVm.WebCredit = customClient.WebCredit != null ? customClient.WebCredit.ToString() : "0";
            therapistVm.YearOfBirth = customClient.YearOfBirth;
            therapistVm.PhoneNumber = customClient.PhoneNumber;
            //therapistVm.AmountDue = customClient.AmountDue != null ? customClient.AmountDue.ToString() : "0";
        }

        public static async Task MapAsync(this TherapistProfileViewModel profile, Therapists therapist)
        {
            profile.UnderSupervision = therapist.UnderSupervision.GetValueOrDefault();
            profile.PsychotherapyTechniques = _therapistFunctions.GetPsychotherapyTechniques(therapist.Id);
            profile.Specialities = _therapistFunctions.GetSpecialties(therapist.Id);
            profile.ToChooseFrom_ContactMethods = _dropdown.GetContactMethodsForDropdown(therapist.Id);
            profile.HasSetupStripeAccount = (await _stripeFunctions.TherapistHasCompletedStripeAccountSetupAsync(therapist)) == TherapistStripeAccountCompletionStatus.Completed;

            profile.Id = therapist.Id;
            profile.About = therapist.About;
            profile.Street = therapist.Street;
            profile.HouseNumber = therapist.HouseNumber;
            profile.City = therapist.City;
            profile.Country = therapist.Country;
            profile.PostalCode = therapist.PostalCode;
            profile.Earnings = therapist.Earnings;
        }

        public static void Map(this Therapists therapist, TherapistProfileViewModel therapistVm)
        {
            therapist.Street = !string.IsNullOrWhiteSpace(therapistVm.Street) ? therapistVm.Street.Trim() : null;
            therapist.HouseNumber = !string.IsNullOrWhiteSpace(therapistVm.HouseNumber) ? therapistVm.HouseNumber.Trim() : null;
            therapist.City = !string.IsNullOrWhiteSpace(therapistVm.City) ? therapistVm.City.Trim() : null;
            //therapist.Country = therapistVm.Country;
            therapist.PostalCode = !string.IsNullOrWhiteSpace(therapistVm.PostalCode) ? therapistVm.PostalCode.Trim() : null;
            therapist.About = !string.IsNullOrWhiteSpace(therapistVm.About) ? therapistVm.About.Trim() : null;
        }

        public static async Task MapAsync(this CustomClient user, TherapistProfileViewModel profile, IWebHostEnvironment environment)
        {
            if (profile.ProfilePhoto != null && !string.IsNullOrWhiteSpace(profile.ProfilePhoto.FileName))
            {
                _files.DeleteUserImage(environment, user.ProfilePhoto);
                user.ProfilePhoto = await _files.CreateUserImageAsync(environment, $"psihoterapeut-{user.FirstName}-{user.LastName}", profile.ProfilePhoto);
            }

            user.PhoneNumber = !string.IsNullOrWhiteSpace(profile.PhoneNumber) ? profile.PhoneNumber.Trim() : null;
        }
    }
}
