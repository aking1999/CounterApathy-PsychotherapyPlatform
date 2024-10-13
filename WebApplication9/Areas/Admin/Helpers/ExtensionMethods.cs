using Database.Models;
using Framework.Implementations;
using Framework.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using WebApplication9.Areas.Admin.ViewModels;

namespace WebApplication9.Areas.Admin.Helpers
{
    public static class ExtensionMethods
    {
        private static IFileRepository _files => new FileRepository();
        private static IDateTimeHelper _dateHelper => new HttpContextAccessor().HttpContext.RequestServices.GetRequiredService<IDateTimeHelper>();
        private static IWebHostEnvironment _environment => new HttpContextAccessor().HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();

        public static void MapProfilePhoto(this AdminProfileViewModel adminVm, CustomClient customClient)
        {
            try
            {
                string folderName = !string.IsNullOrEmpty(customClient.ProfilePhoto) ? @"user-images\" : @"content-images\default-user-image.png";

                using (var stream = System.IO.File.OpenRead($"{_environment.ContentRootPath + @"\wwwroot\images\" + folderName + customClient.ProfilePhoto}"))
                {
                    adminVm.ProfilePhoto = new FormFile(stream, 0, stream.Length, null, System.IO.Path.GetFileName(stream.Name));
                }
            }
            catch (Exception)
            {
                string folderName = @"content-images\default-user-image.png";

                using (var stream = System.IO.File.OpenRead($"{_environment.ContentRootPath + @"\wwwroot\images\" + folderName}"))
                {
                    adminVm.ProfilePhoto = new FormFile(stream, 0, stream.Length, null, System.IO.Path.GetFileName(stream.Name));
                }
            }
        }

        public static void Map(this AdminProfileViewModel adminVm, CustomClient customClient)
        {
            MapProfilePhoto(adminVm, customClient);

            adminVm.FirstName = customClient.FirstName;
            adminVm.LastName = customClient.LastName;
            adminVm.Email = customClient.Email;
            adminVm.WebCredit = customClient.WebCredit.ToString();
            adminVm.YearOfBirth = customClient.YearOfBirth;
            adminVm.PhoneNumber = customClient.PhoneNumber;
        }

        public static async Task MapAsync(this CustomClient user, AdminProfileViewModel profile, IWebHostEnvironment environment)
        {
            if (profile.ProfilePhoto != null && !string.IsNullOrWhiteSpace(profile.ProfilePhoto.FileName))
            {
                _files.DeleteUserImage(environment, user.ProfilePhoto);
                user.ProfilePhoto = await _files.CreateUserImageAsync(environment, $"profilna-slika-admina-{user.FirstName}-{user.LastName}", profile.ProfilePhoto);
            }

            user.FirstName = !string.IsNullOrWhiteSpace(profile.FirstName) ? profile.FirstName.Trim() : null;
            user.LastName = !string.IsNullOrWhiteSpace(profile.LastName) ? profile.LastName.Trim() : null;
            user.YearOfBirth = profile.YearOfBirth;
            user.PhoneNumber = !string.IsNullOrWhiteSpace(profile.PhoneNumber) ? profile.PhoneNumber.Trim() : null;
        }

        public static void Map(this WithdrawalViewModel withdrawalVm, Withdrawals withdrawal)
        {
            withdrawalVm.Id = withdrawal.Id;
            withdrawalVm.TherapistId = withdrawal.TherapistId;
            withdrawalVm.FirstName = withdrawal.FirstName;
            withdrawalVm.LastName = withdrawal.LastName;
            withdrawalVm.Street = withdrawal.Street;
            withdrawalVm.HouseNumber = withdrawal.HouseNumber;
            withdrawalVm.City = withdrawal.City;
            withdrawalVm.PostalCode = withdrawal.PostalCode;
            withdrawalVm.Country = withdrawal.Country;
            withdrawalVm.Email = withdrawal.Email;
            withdrawalVm.PhoneNumber = withdrawal.PhoneNumber;
            withdrawalVm.Status = withdrawal.Status;
            withdrawalVm.BankAccountNumber = withdrawal.BankAccountNumber;
            withdrawalVm.Amount = withdrawal.Amount;
            withdrawalVm.RequestDateTime = _dateHelper.ConvertDateTimeFromUtcToLocalString(withdrawal.RequestDateTime);
            withdrawalVm.AcceptDateTime = _dateHelper.ConvertDateTimeFromUtcToLocalString(withdrawal.AcceptDateTime.GetValueOrDefault());
        }

        public static void Map(this PendingRatingViewModel pendingRatingVm, PendingRatings pendingRating)
        {
            pendingRatingVm.Id = pendingRating.Id;
            pendingRatingVm.TherapistId = pendingRating.TherapistId;
            pendingRatingVm.ClientId = pendingRating.ClientId;
            pendingRatingVm.SessionId = pendingRating.SessionId;
            pendingRatingVm.BookedSessionId = pendingRating.BookedSessionId;
            pendingRatingVm.StarsRating = pendingRating.Rating;
            pendingRatingVm.Comment = pendingRating.Comment;
            pendingRatingVm.RatingDate = pendingRating.RatingDate?.ToString("dd/MM/yyyy");
            pendingRatingVm.Refused = pendingRating.Refused;
        }

        public static void Map(this Ratings rating, PendingRatings pendingRating)
        {
            rating.Id = pendingRating.Id;
            rating.TherapistId = pendingRating.TherapistId;
            rating.ClientId = pendingRating.ClientId;
            rating.SessionId = pendingRating.SessionId;
            rating.BookedSessionId = pendingRating.BookedSessionId;
            rating.Rating = pendingRating.Rating;
            rating.Comment = pendingRating.Comment;
            rating.RatingDate = pendingRating.RatingDate;
        }
    }
}
