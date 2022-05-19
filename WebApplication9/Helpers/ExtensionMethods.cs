using Database.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using WebApplication9.PartialViewModels;
using WebApplication9.Areas.Admin.ViewModels;
using WebApplication9.Areas.Therapist.ViewModels;
using WebApplication9.ViewModels;

namespace WebApplication9.Helpers
{
    public static class ExtensionMethods
    {
        public static void MapProfilePhoto(this AdminProfileViewModel adminVm, CustomClient customClient, IWebHostEnvironment environment)
        {
            try
            {
                string folderName = !string.IsNullOrEmpty(customClient.ProfilePhoto) ? @"user-images\" : @"content-images\default-user-image.png";

                using (var stream = System.IO.File.OpenRead($"{environment.ContentRootPath + @"\wwwroot\images\" + folderName + customClient.ProfilePhoto}"))
                {
                    adminVm.ProfilePhoto = new FormFile(stream, 0, stream.Length, null, System.IO.Path.GetFileName(stream.Name));
                }
            }
            catch(Exception)
            {
                string folderName = @"content-images\default-user-image.png";

                using (var stream = System.IO.File.OpenRead($"{environment.ContentRootPath + @"\wwwroot\images\" + folderName}"))
                {
                    adminVm.ProfilePhoto = new FormFile(stream, 0, stream.Length, null, System.IO.Path.GetFileName(stream.Name));
                }
            }
        }

        public static void MapProfilePhoto(this TherapistProfileViewModel therapistVm, CustomClient customClient, IWebHostEnvironment environment)
        {
            try
            {
                string folderName = !string.IsNullOrEmpty(customClient.ProfilePhoto) ? @"user-images\" : @"content-images\default-user-image.png";

                using (var stream = System.IO.File.OpenRead($"{environment.ContentRootPath + @"\wwwroot\images\" + folderName + customClient.ProfilePhoto}"))
                {
                    therapistVm.ProfilePhoto = new FormFile(stream, 0, stream.Length, null, System.IO.Path.GetFileName(stream.Name));
                }
            }
            catch (Exception)
            {
                string folderName = @"content-images\default-user-image.png";

                using (var stream = System.IO.File.OpenRead($"{environment.ContentRootPath + @"\wwwroot\images\" + folderName}"))
                {
                    therapistVm.ProfilePhoto = new FormFile(stream, 0, stream.Length, null, System.IO.Path.GetFileName(stream.Name));
                }
            }
        }

        public static void MapProfilePhoto(this CustomClientViewModel clientVm, CustomClient customClient, IWebHostEnvironment environment)
        {
            try
            {
                string folderName = !string.IsNullOrEmpty(customClient.ProfilePhoto) ? @"user-images\" : @"content-images\default-user-image.png";

                using (var stream = System.IO.File.OpenRead($"{environment.ContentRootPath + @"\wwwroot\images\" + folderName + customClient.ProfilePhoto}"))
                {
                    clientVm.ProfilePhoto = new FormFile(stream, 0, stream.Length, null, System.IO.Path.GetFileName(stream.Name));
                }
            }
            catch (Exception)
            {
                string folderName = @"content-images\default-user-image.png";

                using (var stream = System.IO.File.OpenRead($"{environment.ContentRootPath + @"\wwwroot\images\" + folderName}"))
                {
                    clientVm.ProfilePhoto = new FormFile(stream, 0, stream.Length, null, System.IO.Path.GetFileName(stream.Name));
                }
            }
        }
        public static void MapProfilePhoto(this TherapistApplicationsViewModel applicationVm, CustomClient customClient, IWebHostEnvironment environment)
        {
            //try
            //{
            //    string folderName = !string.IsNullOrEmpty(customClient.ProfilePhoto) ? @"user-images\" : @"content-images\default-user-image.png";

            //    using (var stream = System.IO.File.OpenRead($"{environment.ContentRootPath + @"\wwwroot\images\" + folderName + customClient.ProfilePhoto}"))
            //    {
            //        applicationVm.ProfilePhoto = new FormFile(stream, 0, stream.Length, null, System.IO.Path.GetFileName(stream.Name));
            //    }
            //}
            //catch (Exception)
            //{
                string folderName = @"content-images\default-user-image.png";

                using (var stream = System.IO.File.OpenRead($"{environment.ContentRootPath + @"\wwwroot\images\" + folderName}"))
                {
                    applicationVm.ProfilePhoto = new FormFile(stream, 0, stream.Length, null, System.IO.Path.GetFileName(stream.Name));
                }
            //}
        }

        public static void Map(this AdminProfileViewModel adminVm, CustomClient customClient, IWebHostEnvironment environment)
        {
            MapProfilePhoto(adminVm, customClient, environment);

            adminVm.Id = customClient.Id;
            adminVm.UserName = customClient.UserName;
            adminVm.NormalizedUserName = customClient.NormalizedUserName;
            adminVm.FirstName = customClient.FirstName;
            adminVm.LastName = customClient.LastName;
            adminVm.Email = customClient.Email;
            adminVm.NormalizedEmail = customClient.NormalizedEmail;
            adminVm.EmailConfirmed = customClient.EmailConfirmed;
            adminVm.PasswordHash = customClient.PasswordHash;
            adminVm.SecurityStamp = customClient.SecurityStamp;
            adminVm.ConcurrencyStamp = customClient.ConcurrencyStamp;
            adminVm.WebCredit = customClient.WebCredit.ToString();
            adminVm.YearOfBirth = customClient.YearOfBirth;
            adminVm.PhoneNumber = customClient.PhoneNumber;
            adminVm.PhoneNumberConfirmed = customClient.PhoneNumberConfirmed;
            adminVm.TwoFactorEnabled = customClient.TwoFactorEnabled;
            adminVm.LockoutEnd = customClient.LockoutEnd;
            adminVm.LockoutEnabled = customClient.LockoutEnabled;
            adminVm.AccessFailedCount = customClient.AccessFailedCount;
        }

        public static void Map(this CustomClientViewModel clientVm, CustomClient customClient, IWebHostEnvironment environment)
        {
            MapProfilePhoto(clientVm, customClient, environment);

            clientVm.Id = customClient.Id;
            clientVm.UserName = customClient.UserName;
            clientVm.NormalizedUserName = customClient.NormalizedUserName;
            clientVm.FirstName = customClient.FirstName;
            clientVm.LastName = customClient.LastName;
            clientVm.Email = customClient.Email;
            clientVm.NormalizedEmail = customClient.NormalizedEmail;
            clientVm.EmailConfirmed = customClient.EmailConfirmed;
            clientVm.PasswordHash = customClient.PasswordHash;
            clientVm.SecurityStamp = customClient.SecurityStamp;
            clientVm.ConcurrencyStamp = customClient.ConcurrencyStamp;
            clientVm.WebCredit = customClient.WebCredit != null ? customClient.WebCredit.ToString() : "0";
            clientVm.YearOfBirth = customClient.YearOfBirth;
            clientVm.PhoneNumber = customClient.PhoneNumber;
            clientVm.PhoneNumberConfirmed = customClient.PhoneNumberConfirmed;
            clientVm.TwoFactorEnabled = customClient.TwoFactorEnabled;
            clientVm.LockoutEnd = customClient.LockoutEnd;
            clientVm.LockoutEnabled = customClient.LockoutEnabled;
            clientVm.AccessFailedCount = customClient.AccessFailedCount;
            //clientVm.AmountDue = customClient.AmountDue != null ? customClient.AmountDue.ToString() : "0";
        }

        public static void Map(this TherapistProfileViewModel therapistVm, CustomClient customClient, IWebHostEnvironment environment)
        {
            MapProfilePhoto(therapistVm, customClient, environment);

            therapistVm.Id = customClient.Id;
            therapistVm.UserName = customClient.UserName;
            therapistVm.NormalizedUserName = customClient.NormalizedUserName;
            therapistVm.FirstName = customClient.FirstName;
            therapistVm.LastName = customClient.LastName;
            therapistVm.Email = customClient.Email;
            therapistVm.NormalizedEmail = customClient.NormalizedEmail;
            therapistVm.EmailConfirmed = customClient.EmailConfirmed;
            therapistVm.PasswordHash = customClient.PasswordHash;
            therapistVm.SecurityStamp = customClient.SecurityStamp;
            therapistVm.ConcurrencyStamp = customClient.ConcurrencyStamp;
            therapistVm.WebCredit = customClient.WebCredit != null ? customClient.WebCredit.ToString() : "0";
            therapistVm.YearOfBirth = customClient.YearOfBirth;
            therapistVm.PhoneNumber = customClient.PhoneNumber;
            therapistVm.PhoneNumberConfirmed = customClient.PhoneNumberConfirmed;
            therapistVm.TwoFactorEnabled = customClient.TwoFactorEnabled;
            therapistVm.LockoutEnd = customClient.LockoutEnd;
            therapistVm.LockoutEnabled = customClient.LockoutEnabled;
            therapistVm.AccessFailedCount = customClient.AccessFailedCount;
            //therapistVm.AmountDue = customClient.AmountDue != null ? customClient.AmountDue.ToString() : "0";
        }

        public static void Map(this TherapistProfileViewModel therapistVm, Therapists therapist)
        {
            therapistVm.About = therapist.About;
            therapistVm.Street = therapist.Street;
            therapistVm.HouseNumber = therapist.HouseNumber;
            therapistVm.City = therapist.City;
            therapistVm.Country = therapist.Country;
            therapistVm.PostalCode = therapist.PostalCode;
            therapistVm.OnVacation = therapist.OnVacation;
            therapistVm.Gender = therapist.Gender;
            therapistVm.Earnings = therapist.Earnings.ToString();
            therapistVm.HasEarnings = therapist.Earnings > 0 ? true : false;
        }

        public static void Map(this Therapists therapist, TherapistProfileViewModel therapistVm)
        {
            therapist.Street = therapistVm.Street;
            therapist.HouseNumber = therapistVm.HouseNumber;
            therapist.City = therapistVm.City;
            therapist.Country = therapistVm.Country;
            therapist.PostalCode = therapistVm.PostalCode;
            therapist.About = therapistVm.About;
            therapist.OnVacation = therapistVm.OnVacation;
        }

        public static void Map(this CustomClient customClient, TherapistProfileViewModel therapistVm)
        {
            //customClient.Id = therapistVm.Id;
            customClient.PhoneNumber = therapistVm.PhoneNumber;
        }

        public static void Map(this CustomClient customClient, CustomClientViewModel clientVm)
        {
            //customClient.ProfilePhoto = clientVm.ProfilePhoto.FileName;

            //!!! OVO Id sam upravo zakomentarisao, ako ne bude radilo, ovde promeniti.
            //customClient.Id = clientVm.Id;
            //customClient.UserName = clientVm.UserName;
            //customClient.NormalizedUserName = clientVm.NormalizedUserName;
            customClient.FirstName = clientVm.FirstName.Trim();
            customClient.LastName = clientVm.LastName.Trim();
            customClient.Email = clientVm.Email.Trim();
            //customClient.NormalizedEmail = clientVm.NormalizedEmail;
            //customClient.EmailConfirmed = clientVm.EmailConfirmed;
            //customClient.PasswordHash = clientVm.PasswordHash;
            //customClient.SecurityStamp = clientVm.SecurityStamp;
            //customClient.ConcurrencyStamp = clientVm.ConcurrencyStamp;
            //customClient.WebCredit = clientVm.WebCredit;
            customClient.YearOfBirth = clientVm.YearOfBirth;
            customClient.PhoneNumber = clientVm.PhoneNumber.Trim();
            //customClient.PhoneNumberConfirmed = clientVm.PhoneNumberConfirmed;
            //customClient.TwoFactorEnabled = clientVm.TwoFactorEnabled;
            //customClient.LockoutEnd = clientVm.LockoutEnd;
            //customClient.LockoutEnabled = clientVm.LockoutEnabled;
            //customClient.AccessFailedCount = clientVm.AccessFailedCount;
        }

        public static void Map(this CustomClient customClient, AdminProfileViewModel adminVm)
        {
            customClient.Id = adminVm.Id;
            customClient.FirstName = adminVm.FirstName.Trim();
            customClient.LastName = adminVm.LastName.Trim();
            customClient.Email = adminVm.Email.Trim();
            customClient.YearOfBirth = adminVm.YearOfBirth;
            customClient.PhoneNumber = adminVm.PhoneNumber.Trim();
        }

        public static void Map(this TherapistApplicationsViewModel applicationVm, CustomClient customClient, IWebHostEnvironment environment)
        {
            // !!! ovo treba kasnije da popravim, tj da umesto ovog ogromnog dela koda
            // stoji samo MapProfilePhoto(applicationVm, customClient, environment); 
            // kao sto stoji kod ostalih metoda
            //string folderName = !string.IsNullOrEmpty(customClient.ProfilePhoto) ? @"user-images\" : @"content-images\default-user-image.png";

            //using (var stream = System.IO.File.OpenRead($"{environment.ContentRootPath + @"\wwwroot\images\" + folderName + customClient.ProfilePhoto}"))
            //{
            //    applicationVm.ProfilePhoto = new FormFile(stream, 0, stream.Length, null, System.IO.Path.GetFileName(stream.Name));
            //}

            //MapProfilePhoto(applicationVm, customClient, environment); ne radi mapiranje slike nikako.

            //applicationVm.UserName = customClient.UserName;
            //applicationVm.NormalizedUserName = customClient.NormalizedUserName;
            applicationVm.FirstName = customClient.FirstName;
            applicationVm.LastName = customClient.LastName;
            applicationVm.Email = customClient.Email;
            //applicationVm.NormalizedEmail = customClient.NormalizedEmail;
            //applicationVm.EmailConfirmed = customClient.EmailConfirmed;
            //applicationVm.PasswordHash = customClient.PasswordHash;
            //applicationVm.SecurityStamp = customClient.SecurityStamp;
            //applicationVm.ConcurrencyStamp = customClient.ConcurrencyStamp;
            //applicationVm.WebCredit = customClient.WebCredit.ToString();
            applicationVm.YearOfBirth = customClient.YearOfBirth;
            applicationVm.PhoneNumber = customClient.PhoneNumber;
            //applicationVm.PhoneNumberConfirmed = customClient.PhoneNumberConfirmed;
            //applicationVm.TwoFactorEnabled = customClient.TwoFactorEnabled;
            //applicationVm.LockoutEnd = customClient.LockoutEnd;
            //applicationVm.LockoutEnabled = customClient.LockoutEnabled;
            //applicationVm.AccessFailedCount = customClient.AccessFailedCount;
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
            bookedSessionVm.ClientFirstName = bookedSession.ClientFirstName;
            bookedSessionVm.ClientLastName = bookedSession.ClientLastName;
            bookedSessionVm.ClientEmail = bookedSession.ClientEmail;
            bookedSessionVm.ClientPhoneNumber = bookedSession.ClientPhoneNumber;
            bookedSessionVm.Subject = bookedSession.Subject;
            bookedSessionVm.Price = bookedSession.Price;
            bookedSessionVm.Type = bookedSession.Type;
            bookedSessionVm.SessionDate = bookedSession.SessionDate;
            bookedSessionVm.StartTime = bookedSession.StartTime;
            bookedSessionVm.EndTime = bookedSession.EndTime;
            bookedSessionVm.BookingDate = bookedSession.BookingDate;
            bookedSessionVm.ContactMethodName = bookedSession.ContactMethodName;
            bookedSessionVm.ContactInfo = bookedSession.ContactInfo;
            bookedSessionVm.Status = bookedSession.Status;
        }

        public static void Map(this ClientReviewPartialViewModel clientReview, Ratings rating)
        {
            clientReview.FullName = rating.ClientFirstName + " " + rating.ClientLastName;
            clientReview.StarsRating = rating.Rating;
            clientReview.Comment = rating.Comment;
            clientReview.RatingDate = rating.RatingDate.HasValue ? rating.RatingDate.Value.Date.ToString("dd/MM/yyyy") : null;
        }

        public static void Map(this Withdrawals withdrawal, WithdrawViewModel withdrawVm)
        {
            withdrawal.FirstName = withdrawVm.FirstName;
            withdrawal.LastName = withdrawVm.LastName;
            withdrawal.Street = withdrawVm.Street;
            withdrawal.HouseNumber = withdrawVm.HouseNumber;
            withdrawal.City = withdrawVm.City;
            withdrawal.PostalCode = withdrawVm.PostalCode;
            withdrawal.Country = withdrawVm.Country;
            withdrawal.BankAccountNumber = withdrawVm.BankAccountNumber;
        }
    }
}
