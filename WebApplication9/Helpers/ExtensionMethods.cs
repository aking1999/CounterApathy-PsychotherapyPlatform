using Database.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using WebApplication9.PartialViewModels;
using WebApplication9.ViewModels;
using Framework.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using DataTransferObjects.ViewModels.Therapist;
using DataTransferObjects.ViewModels.Client;
using Framework.Implementations;

namespace WebApplication9.Helpers
{
    public static class ExtensionMethods
    {
        private static IFileRepository _files => new FileRepository();
        private static IWebHostEnvironment _environment => new HttpContextAccessor().HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
        private static IDateTimeHelper _dateHelper => new HttpContextAccessor().HttpContext.RequestServices.GetRequiredService<IDateTimeHelper>();

        public static void MapProfilePhoto(this CustomClientViewModel clientVm, CustomClient customClient)
        {
            try
            {
                string folderName = !string.IsNullOrEmpty(customClient.ProfilePhoto) ? @"user-images\" : @"content-images\default-user-image.png";

                using (var stream = System.IO.File.OpenRead($"{_environment.ContentRootPath + @"\wwwroot\images\" + folderName + customClient.ProfilePhoto}"))
                {
                    clientVm.ProfilePhoto = new FormFile(stream, 0, stream.Length, null, System.IO.Path.GetFileName(stream.Name));
                }
            }
            catch (Exception)
            {
                string folderName = @"content-images\default-user-image.png";

                using (var stream = System.IO.File.OpenRead($"{_environment.ContentRootPath + @"\wwwroot\images\" + folderName}"))
                {
                    clientVm.ProfilePhoto = new FormFile(stream, 0, stream.Length, null, System.IO.Path.GetFileName(stream.Name));
                }
            }
        }

        public static void Map(this CustomClientViewModel clientVm, CustomClient customClient)
        {
            MapProfilePhoto(clientVm, customClient);

            clientVm.FirstName = customClient.FirstName;
            clientVm.LastName = customClient.LastName;
            clientVm.Email = customClient.Email;
            clientVm.WebCredit = customClient.WebCredit != null ? customClient.WebCredit.ToString() : "0";
            clientVm.PhoneNumber = customClient.PhoneNumber;

            if (customClient.YearOfBirth == null)
                clientVm.YearOfBirth = DateTime.UtcNow.Year - 18;
            else clientVm.YearOfBirth = customClient.YearOfBirth;
        }

        public static async Task MapAsync(this CustomClient user, CustomClientViewModel profile)
        {
            if (profile.ProfilePhoto != null && !string.IsNullOrWhiteSpace(profile.ProfilePhoto.FileName))
            {
                _files.DeleteUserImage(_environment, user.ProfilePhoto);
                user.ProfilePhoto = await _files.CreateUserImageAsync(_environment, $"{user.FirstName}-{user.LastName}-{user.Id}", profile.ProfilePhoto);
            }

            user.FirstName = !string.IsNullOrWhiteSpace(profile.FirstName) ? profile.FirstName.Trim() : null;
            user.LastName = !string.IsNullOrWhiteSpace(profile.LastName) ? profile.LastName.Trim() : null;
            user.YearOfBirth = profile.YearOfBirth;
            user.PhoneNumber = !string.IsNullOrWhiteSpace(profile.PhoneNumber) ? profile.PhoneNumber.Trim() : null;
        }

        public static void Map(this TherapistApplicationsViewModel applicationVm, CustomClient customClient)
        {
            applicationVm.FirstName = customClient.FirstName;
            applicationVm.LastName = customClient.LastName;
            applicationVm.Email = customClient.Email;
            applicationVm.YearOfBirth = customClient.YearOfBirth;
            applicationVm.PhoneNumber = customClient.PhoneNumber;
        }

        public static void Map(this BookedConsultationViewModel bookedConsultationVm, BookedConsultations bookedConsultation)
        {
            bookedConsultationVm.BookingId = bookedConsultation.Id;
            bookedConsultationVm.ConsultationId = bookedConsultation.ConsultationId;
            bookedConsultationVm.TherapistId = bookedConsultation.TherapistId;
            bookedConsultationVm.TherapistFirstName = bookedConsultation.TherapistFirstName;
            bookedConsultationVm.TherapistLastName = bookedConsultation.TherapistLastName;
            bookedConsultationVm.TherapistEmail = bookedConsultation.TherapistEmail;
            bookedConsultationVm.TherapistPhoneNumber = bookedConsultation.TherapistPhoneNumber;
            bookedConsultationVm.ClientId = bookedConsultation.ClientId;
            bookedConsultationVm.ClientFirstName = bookedConsultation.ClientFirstName;
            bookedConsultationVm.ClientLastName = bookedConsultation.ClientLastName;
            bookedConsultationVm.ClientEmail = bookedConsultation.ClientEmail;
            bookedConsultationVm.ClientPhoneNumber = bookedConsultation.ClientPhoneNumber;
            bookedConsultationVm.StartDateTime = bookedConsultation.StartDateTime;
            bookedConsultationVm.EndDateTime = bookedConsultation.EndDateTime;
            bookedConsultationVm.BookingDate = bookedConsultation.BookingDate;
            bookedConsultationVm.ContactMethod.Id = bookedConsultation.ContactMethodId;
            bookedConsultationVm.ContactMethod.Name = bookedConsultation.ContactMethodName;
            bookedConsultationVm.ContactMethod.Color = bookedConsultation.ContactMethodColor;
            bookedConsultationVm.ContactMethod.Icon = bookedConsultation.ContactMethodIcon;
        }

        public static void Map(this BookedSessionViewModel bookedSessionVm, BookedSessions bookedSession)
        {
            bookedSessionVm.BookingId = bookedSession.Id;
            bookedSessionVm.SessionId = bookedSession.SessionId;
            bookedSessionVm.TherapistId = bookedSession.TherapistId;
            bookedSessionVm.TherapistFirstName = bookedSession.TherapistFirstName;
            bookedSessionVm.TherapistLastName = bookedSession.TherapistLastName;
            bookedSessionVm.TherapistEmail = bookedSession.TherapistEmail;
            bookedSessionVm.TherapistPhoneNumber = bookedSession.TherapistPhoneNumber;
            bookedSessionVm.TherapistStreet = bookedSession.TherapistStreet;
            bookedSessionVm.TherapistHouseNumber = bookedSession.TherapistHouseNumber;
            bookedSessionVm.TherapistCity = bookedSession.TherapistCity;
            bookedSessionVm.TherapistCountry = bookedSession.TherapistCountry;
            bookedSessionVm.TherapistPostalCode = bookedSession.TherapistPostalCode;
            bookedSessionVm.ClientId = bookedSession.ClientId;
            bookedSessionVm.ClientFirstName = bookedSession.ClientFirstName;
            bookedSessionVm.ClientLastName = bookedSession.ClientLastName;
            bookedSessionVm.ClientEmail = bookedSession.ClientEmail;
            bookedSessionVm.ClientPhoneNumber = bookedSession.ClientPhoneNumber;
            bookedSessionVm.Price = bookedSession.Price;
            bookedSessionVm.Type = bookedSession.Type;
            bookedSessionVm.StartTime = bookedSession.StartTime;
            bookedSessionVm.EndTime = bookedSession.EndTime;
            bookedSessionVm.BookingDate = bookedSession.BookingDate;
            bookedSessionVm.ContactMethod.Id = bookedSession.ContactMethodId;
            bookedSessionVm.ContactMethod.Name = bookedSession.ContactMethodName;
            bookedSessionVm.ContactMethod.Color = bookedSession.ContactMethodColor;
            bookedSessionVm.ContactMethod.Icon = bookedSession.ContactMethodIcon;
        }

        public static void Map(this BookedSessionAddRatingViewModel addRating, BookedSessions bookedSession)
        {
            addRating.SessionId = bookedSession.SessionId;
            addRating.BookingId = bookedSession.Id;
            addRating.TherapistId = bookedSession.TherapistId;
            addRating.TherapistFirstName = bookedSession.TherapistFirstName;
            addRating.TherapistLastName = bookedSession.TherapistLastName;
            addRating.TherapistEmail = bookedSession.TherapistEmail;
            addRating.TherapistPhoneNumber = bookedSession.TherapistPhoneNumber;
            addRating.TherapistStreet = bookedSession.TherapistStreet;
            addRating.TherapistHouseNumber = bookedSession.TherapistHouseNumber;
            addRating.TherapistCity = bookedSession.TherapistCity;
            addRating.TherapistCountry = bookedSession.TherapistCountry;
            addRating.TherapistPostalCode = bookedSession.TherapistPostalCode;
            addRating.ClientId = bookedSession.ClientId;
            addRating.ClientFirstName = bookedSession.ClientFirstName;
            addRating.ClientLastName = bookedSession.ClientLastName;
            addRating.ClientEmail = bookedSession.ClientEmail;
            addRating.ClientPhoneNumber = bookedSession.ClientPhoneNumber;
            addRating.Price = bookedSession.Price;
            addRating.Type = bookedSession.Type;
            addRating.StartTime = bookedSession.StartTime;
            addRating.EndTime = bookedSession.EndTime;
            addRating.BookingDate = bookedSession.BookingDate;
            addRating.ContactMethod.Id = bookedSession.ContactMethodId;
            addRating.ContactMethod.Name = bookedSession.ContactMethodName;
            addRating.ContactMethod.Color = bookedSession.ContactMethodColor;
            addRating.ContactMethod.Icon = bookedSession.ContactMethodIcon;
        }

        public static void Map(this ClientReviewPartialViewModel clientReview, Ratings rating)
        {
            clientReview.StarsRating = rating.Rating;
            clientReview.Comment = !string.IsNullOrWhiteSpace(rating.Comment) ? $"“{rating.Comment}”" : string.Empty;
            clientReview.RatingDate = rating.RatingDate.HasValue ? rating.RatingDate.Value.Date.ToString() : null;
        }

        public static void Map(this PayPalPaymentSuccessfulViewModel payPalVm, PayPalPaymentRequests request)
        {
            payPalVm.Id = request.Id;
            payPalVm.PaymentId = request.PayPalPaymentId;
            payPalVm.TransactionId = request.PayPalTransactionId;
            payPalVm.PrimaryCurrencyTotalPaid = (request.PrimaryCurrencyAmount + request.PrimaryCurrencyFeeAmount) > 0 ? (request.PrimaryCurrencyAmount + request.PrimaryCurrencyFeeAmount).ToString("#.##") : "0";
            payPalVm.PrimaryCurrencyCode = request.PrimaryCurrencyCode;
            payPalVm.SecondaryCurrencyCode = request.SecondaryCurrencyCode;
            payPalVm.SecondaryCurrencyAmount = request.SecondaryCurrencyAmount > 0 ? request.SecondaryCurrencyAmount.ToString("#.##") : "0";
            payPalVm.FeePercentage = request.FeePercentage.ToString();
            payPalVm.PrimaryCurrencyFeeAmount = request.PrimaryCurrencyFeeAmount > 0 ? request.PrimaryCurrencyFeeAmount.ToString("#.##") : "0";
            payPalVm.ExchangeRate =  request.ExchangeRate > 0 ? request.ExchangeRate.ToString("#.##") : "0";
            payPalVm.PaymentCompleteDateTime = _dateHelper.ConvertDateTimeFromUtcToLocalString(request.PaymentCompleteDateTime ?? default);
        }
    }
}
