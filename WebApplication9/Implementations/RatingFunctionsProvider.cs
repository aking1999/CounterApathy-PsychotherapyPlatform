using Database.Models;
using Database.RepositoryImplementations;
using Database.RepositoryInterfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Framework.Helpers.ExtensionMethods;
using System.Collections.Generic;
using System.Linq;
using WebApplication9.Helpers;
using WebApplication9.Interfaces;
using System;
using System.Threading.Tasks;
using Framework.Interfaces;
using DataTransferObjects.ViewModels.Therapist;
using DataTransferObjects.ViewModels.Client;
using Framework.Implementations;
using Microsoft.AspNetCore.Hosting;

namespace WebApplication9.Implementations
{
    public class RatingFunctionsProvider : IRatingFunctionsProvider
    {
        private readonly IFileRepository _files;
        private IDateTimeHelper _dateHelper = new HttpContextAccessor().HttpContext.RequestServices.GetRequiredService<IDateTimeHelper>();
        private IWebHostEnvironment _environment => new HttpContextAccessor().HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
        private readonly ISystemErrorLogger _systemErrors;
        private readonly UnitOfWork _context;
        private UserManager<CustomClient> _userManager => new HttpContextAccessor().HttpContext.RequestServices.GetRequiredService<UserManager<CustomClient>>();

        public RatingFunctionsProvider()
        {
            _files = new FileRepository();
            _systemErrors = new SystemErrorLogger();
            _context = new UnitOfWork(new LajsnaProbaContext());
        }

        // Therapist/BookedSessions
        public List<BookedSessionViewModel> MapToApprovedRatings(List<BookedSessions> bookedSessions)
        {
            try
            {
                var bookedSessionVms = new List<BookedSessionViewModel>();

                foreach (var bookedSession in bookedSessions)
                {
                    // This must be exactly in this order
                    var temp = new BookedSessionViewModel();
                    temp.Map(bookedSession);
                    temp.StartTime = _dateHelper.ConvertDateTimeFromUtcToLocal(bookedSession.StartTime);
                    temp.EndTime = _dateHelper.ConvertDateTimeFromUtcToLocal(bookedSession.EndTime);
                    temp.BookingDate = _dateHelper.ConvertDateTimeFromUtcToLocal(bookedSession.BookingDate);

                    var rating = _context.Ratings.ReadOnlyFind(r => r.BookedSessionId == temp.BookingId).SingleOrDefault();

                    if (rating != default)
                    {
                        temp.StarsRating = rating.Rating;
                    }

                    temp.ProfilePhoto = _files.GetUserProfilePhotoPathOrDefaultPhotoPath(_userManager, _environment, bookedSession.ClientId);

                    var contactMethod = _context.ContactMethods.GetById(temp.ContactMethod.Id);

                    if (contactMethod != null)
                    {
                        temp.ContactMethod.Name = contactMethod.Name;
                        temp.ContactMethod.Color = contactMethod.Color;
                        temp.ContactMethod.Icon = contactMethod.Icon;
                    }

                    bookedSessionVms.Add(temp);
                }

                return bookedSessionVms;
            }
            catch (Exception e)
            {
                _systemErrors.SaveError(e, "WebApplication9", "RatingFunctionsProvider", "MapToApprovedRatings");
                return new List<BookedSessionViewModel>();
            }
        }

        // Therapist/Details
        public async Task<BookedSessionAddInviteLinkViewModel> MapToApprovedDetailsRatingAsync(string therapistId, string bookingId)
        {
            try
            {
                var bookedSessionInviteLink = (from bs in _context.BookedSessions.ReadOnlyFind(s => s.TherapistId == therapistId && s.Id == bookingId)
                                               join bsCm in _context.BookedSessionsContactMethods.ReadOnlyFind(s => s.BookedSessionId == bookingId).ToList() // .ToList() must be here or DataReaderException
                                               on bs.Id equals bsCm.BookedSessionId
                                               where bs.ContactMethodId == bsCm.ContactMethodId
                                               //join tx in _context.Transactions.GetAll()
                                               //on bs.TherapistPaymentTransactionId equals tx.Id
                                               select new BookedSessionAddInviteLinkViewModel(bsCm.InviteLink, _files.GetUserProfilePhotoPathOrDefaultPhotoPath(_userManager, _environment, bs.ClientId),
                                                                                        bs.SessionId, bs.Id, bs.TherapistId,
                                                                                        bs.TherapistFirstName, bs.TherapistLastName,
                                                                                        bs.TherapistEmail, bs.TherapistPhoneNumber,
                                                                                        bs.TherapistStreet, bs.TherapistHouseNumber,
                                                                                        bs.TherapistCity, bs.TherapistCountry,
                                                                                        bs.TherapistPostalCode, bs.ClientId,
                                                                                        bs.ClientFirstName, bs.ClientLastName,
                                                                                        bs.ClientEmail, bs.ClientPhoneNumber, bs.Price,
                                                                                        bs.Type, _dateHelper.ConvertDateTimeFromUtcToLocal(bs.StartTime),
                                                                                        _dateHelper.ConvertDateTimeFromUtcToLocal(bs.EndTime),
                                                                                        _dateHelper.ConvertDateTimeFromUtcToLocal(bs.BookingDate),
                                                                                        bs.ContactMethodId, bs.ContactMethodName, bs.ContactMethodColor, bs.ContactMethodIcon,
                                                                                        bs.TherapistIsPaid.GetValueOrDefault(), default,
                                                                                        default, default)).SingleOrDefault();

                var bookedSession = _context.BookedSessions.GetById(bookingId);

                if (bookedSession == null || bookedSession.TherapistId != therapistId) return default;

                var transactionForTherapistPayment = _context.Transactions.GetById(bookedSession.TherapistPaymentTransactionId);

                if (transactionForTherapistPayment == null)
                    transactionForTherapistPayment = new Transactions();

                if (bookedSessionInviteLink == default)
                {
                    bookedSessionInviteLink = new BookedSessionAddInviteLinkViewModel(null, _files.GetUserProfilePhotoPathOrDefaultPhotoPath(_userManager, _environment, bookedSession.ClientId),
                                                                                bookedSession.SessionId, bookedSession.Id, bookedSession.TherapistId,
                                                                                bookedSession.TherapistFirstName, bookedSession.TherapistLastName,
                                                                                bookedSession.TherapistEmail, bookedSession.TherapistPhoneNumber,
                                                                                bookedSession.TherapistStreet, bookedSession.TherapistHouseNumber,
                                                                                bookedSession.TherapistCity, bookedSession.TherapistCountry,
                                                                                bookedSession.TherapistPostalCode, bookedSession.ClientId,
                                                                                bookedSession.ClientFirstName, bookedSession.ClientLastName,
                                                                                bookedSession.ClientEmail, bookedSession.ClientPhoneNumber, bookedSession.Price,
                                                                                bookedSession.Type, _dateHelper.ConvertDateTimeFromUtcToLocal(bookedSession.StartTime),
                                                                                _dateHelper.ConvertDateTimeFromUtcToLocal(bookedSession.EndTime),
                                                                                _dateHelper.ConvertDateTimeFromUtcToLocal(bookedSession.BookingDate),
                                                                                bookedSession.ContactMethodId, bookedSession.ContactMethodName, bookedSession.ContactMethodColor, bookedSession.ContactMethodIcon,
                                                                                bookedSession.TherapistIsPaid.GetValueOrDefault(), transactionForTherapistPayment.Amount,
                                                                                transactionForTherapistPayment.CurrencyCode, _dateHelper.ConvertDateTimeFromUtcToLocal(transactionForTherapistPayment.DateTime));
                }

                bookedSessionInviteLink.Transaction.Amount = transactionForTherapistPayment.Amount;
                bookedSessionInviteLink.Transaction.CurrencyCode = transactionForTherapistPayment.CurrencyCode;
                bookedSessionInviteLink.Transaction.DateTime = _dateHelper.ConvertDateTimeFromUtcToLocal(transactionForTherapistPayment.DateTime);

                // If the ContactMethod data are changed in the meantime in table ContactMethods,
                // here we update them in BookedSessionAddInviteLink, but if that ContactMethod is deleted,
                // then we leave the ContactMethod data from BookedSessionAddInviteLink
                var contactMethod = _context.ContactMethods.GetById(bookedSessionInviteLink.BookedSession.ContactMethod.Id);
                if (contactMethod != null)
                {
                    bookedSessionInviteLink.BookedSession.ContactMethod.Name = contactMethod.Name;
                    bookedSessionInviteLink.BookedSession.ContactMethod.Color = contactMethod.Color;
                    bookedSessionInviteLink.BookedSession.ContactMethod.Icon = contactMethod.Icon;
                }

                var rating = _context.Ratings.ReadOnlyFind(r => r.BookedSessionId == bookedSessionInviteLink.BookedSession.BookingId).SingleOrDefault();

                if (rating != default)
                    bookedSessionInviteLink.BookedSession.StarsRating = rating.Rating;

                return bookedSessionInviteLink;
            }
            catch (Exception e)
            {
                await _systemErrors.SaveErrorAsync(e, "WebApplication9", "RatingFunctionsProvider", "MapToApprovedDetailsRatingAsync");
                return default;
            }
        }

        // Client/BookedSessions
        public async Task<List<BookedSessionAddRatingViewModel>> MapToApprovedOrPendingRatingsAsync(List<BookedSessions> bookedSessions)
        {
            try
            {
                var bookedSessionVms = new List<BookedSessionAddRatingViewModel>();

                foreach (var bookedSession in bookedSessions)
                {
                    // This must be exactly in this order
                    var temp = new BookedSessionAddRatingViewModel();
                    temp.Map(bookedSession);
                    temp.StartTime = _dateHelper.ConvertDateTimeFromUtcToLocal(bookedSession.StartTime);
                    temp.EndTime = _dateHelper.ConvertDateTimeFromUtcToLocal(bookedSession.EndTime);
                    temp.BookingDate = _dateHelper.ConvertDateTimeFromUtcToLocal(bookedSession.BookingDate);

                    var rating = _context.Ratings.ReadOnlyFind(r => r.BookedSessionId == temp.BookingId).SingleOrDefault();

                    if (rating != default)
                    {
                        temp.Rating.StarsRating = rating.Rating;
                    }
                    else
                    {
                        var pendingRating = _context.PendingRatings.ReadOnlyFind(pr => pr.BookedSessionId == temp.BookingId).SingleOrDefault();

                        if (pendingRating != default)
                            temp.Rating.StarsRating = pendingRating.Rating;
                    }

                    temp.ProfilePhoto = _files.GetUserProfilePhotoPathOrDefaultPhotoPath(_userManager, _environment, (await _userManager.FindByTherapistAccountIdAsync(temp.TherapistId)).Id);

                    var contactMethod = _context.ContactMethods.GetById(temp.ContactMethod.Id);

                    if (contactMethod != null)
                    {
                        temp.ContactMethod.Name = contactMethod.Name;
                        temp.ContactMethod.Color = contactMethod.Color;
                        temp.ContactMethod.Icon = contactMethod.Icon;
                    }

                    bookedSessionVms.Add(temp);
                }

                return bookedSessionVms;
            }
            catch (Exception e)
            {
                _systemErrors.SaveError(e, "WebApplication9", "RatingFunctionsProvider", "MapToApprovedOrPendingRatingsAsync");
                return new List<BookedSessionAddRatingViewModel>();
            }
        }

        // Client/SessionDetails
        public async Task<BookedSessionAddRatingViewModel> MapToApprovedOrPendingDetailsRatingAsync(string userId, string bookingId)
        {
            try
            {
                var bookedSessionAddRating = (from bs in _context.BookedSessions.ReadOnlyFind(s => s.ClientId == userId && s.Id == bookingId)
                                              join bsCm in _context.BookedSessionsContactMethods.ReadOnlyFind(s => s.BookedSessionId == bookingId).ToList() // .ToList() must be here or DataReaderException
                                              on bs.Id equals bsCm.BookedSessionId
                                              where bs.ContactMethodId == bsCm.ContactMethodId
                                              select new BookedSessionAddRatingViewModel(bsCm.InviteLink, _files.GetUserProfilePhotoPathOrDefaultPhotoPath(_userManager, _environment, _userManager.FindByTherapistAccountIdAsync(bs.TherapistId).Result.Id),
                                              bs.SessionId, bs.Id, bs.TherapistId,
                                              bs.TherapistFirstName, bs.TherapistLastName,
                                              bs.TherapistEmail, bs.TherapistPhoneNumber,
                                              bs.TherapistStreet, bs.TherapistHouseNumber,
                                              bs.TherapistCity, bs.TherapistCountry,
                                              bs.TherapistPostalCode, bs.ClientId,
                                              bs.ClientFirstName, bs.ClientLastName,
                                              bs.ClientEmail, bs.ClientPhoneNumber, bs.Price,
                                              bs.Type, _dateHelper.ConvertDateTimeFromUtcToLocal(bs.StartTime),
                                              _dateHelper.ConvertDateTimeFromUtcToLocal(bs.EndTime),
                                              _dateHelper.ConvertDateTimeFromUtcToLocal(bs.BookingDate),
                                              bs.ContactMethodId, bs.ContactMethodName, bs.ContactMethodColor, bs.ContactMethodIcon)).SingleOrDefault();

                if (bookedSessionAddRating == default)
                {
                    var bookedSession = _context.BookedSessions.GetById(bookingId);

                    if (bookedSession == null || bookedSession.ClientId != userId) return default;

                    bookedSessionAddRating = new BookedSessionAddRatingViewModel(null, _files.GetUserProfilePhotoPathOrDefaultPhotoPath(_userManager, _environment, (await _userManager.FindByTherapistAccountIdAsync(bookedSession.TherapistId)).Id),
                                                                           bookedSession.SessionId, bookedSession.Id, bookedSession.TherapistId,
                                                                           bookedSession.TherapistFirstName, bookedSession.TherapistLastName,
                                                                           bookedSession.TherapistEmail, bookedSession.TherapistPhoneNumber,
                                                                           bookedSession.TherapistStreet, bookedSession.TherapistHouseNumber,
                                                                           bookedSession.TherapistCity, bookedSession.TherapistCountry,
                                                                           bookedSession.TherapistPostalCode, bookedSession.ClientId,
                                                                           bookedSession.ClientFirstName, bookedSession.ClientLastName,
                                                                           bookedSession.ClientEmail, bookedSession.ClientPhoneNumber, bookedSession.Price,
                                                                           bookedSession.Type, _dateHelper.ConvertDateTimeFromUtcToLocal(bookedSession.StartTime),
                                                                           _dateHelper.ConvertDateTimeFromUtcToLocal(bookedSession.EndTime),
                                                                           _dateHelper.ConvertDateTimeFromUtcToLocal(bookedSession.BookingDate),
                                                                           bookedSession.ContactMethodId, bookedSession.ContactMethodName, bookedSession.ContactMethodColor, bookedSession.ContactMethodIcon);
                }

                // If PhoneNumber is deleted in meantime in BookedSessions, we start
                // taking the PhoneNumber info from AspNetUsers for that client.
                if (string.IsNullOrWhiteSpace(bookedSessionAddRating.ClientPhoneNumber))
                    bookedSessionAddRating.ClientPhoneNumber = (await _userManager.FindByIdAsync(bookedSessionAddRating.ClientId)).PhoneNumber;

                // If the ContactMethod data are changed in the meantime in table ContactMethods,
                // here we update them in BookedSessionAddRating, but if that ContactMethod is deleted,
                // then we leave the ContactMethod data from BookedSessions
                var contactMethod = _context.ContactMethods.GetById(bookedSessionAddRating.ContactMethod.Id);
                if (contactMethod != null)
                {
                    bookedSessionAddRating.ContactMethod.Name = contactMethod.Name;
                    bookedSessionAddRating.ContactMethod.Color = contactMethod.Color;
                    bookedSessionAddRating.ContactMethod.Icon = contactMethod.Icon;
                }

                var rating = _context.Ratings.ReadOnlyFind(r => r.BookedSessionId == bookedSessionAddRating.BookingId).SingleOrDefault();

                if (rating != default)
                    bookedSessionAddRating.Rating.StarsRating = rating.Rating;
                else
                {
                    var pendingRating = _context.PendingRatings.ReadOnlyFind(pr => pr.BookedSessionId == bookedSessionAddRating.BookingId).SingleOrDefault();

                    if (pendingRating != default)
                        bookedSessionAddRating.Rating.StarsRating = pendingRating.Rating;
                }

                return bookedSessionAddRating;
            }
            catch (Exception e)
            {
                await _systemErrors.SaveErrorAsync(e, "WebApplication9", "RatingFunctionsProvider", "MapToApprovedOrPendingDetailsRatingAsync");
                return default;
            }
        }

        // Anonimno/SessionDetailsUnauthorized
        public async Task<BookedSessionAddRatingViewModel> MapToApprovedOrPendingDetailsRatingForAnonymousAsync(string email, string bookingId)
        {
            try
            {
                var bookedSessionAddRating = (from bs in _context.BookedSessions.ReadOnlyFind(s => s.ClientEmail == email && s.Id == bookingId && (s.ClientId == "anonymous" || s.ClientId == "unauthorized"))
                                              join bsCm in _context.BookedSessionsContactMethods.ReadOnlyFind(s => s.BookedSessionId == bookingId).ToList() // .ToList() must be here or DataReaderException
                                              on bs.Id equals bsCm.BookedSessionId
                                              where bs.ContactMethodId == bsCm.ContactMethodId
                                              select new BookedSessionAddRatingViewModel(bsCm.InviteLink, _files.GetUserProfilePhotoPathOrDefaultPhotoPath(_userManager, _environment, _userManager.FindByTherapistAccountIdAsync(bs.TherapistId).Result.Id),
                                              bs.SessionId, bs.Id, bs.TherapistId,
                                              bs.TherapistFirstName, bs.TherapistLastName,
                                              bs.TherapistEmail, bs.TherapistPhoneNumber,
                                              bs.TherapistStreet, bs.TherapistHouseNumber,
                                              bs.TherapistCity, bs.TherapistCountry,
                                              bs.TherapistPostalCode, bs.ClientId,
                                              bs.ClientFirstName, bs.ClientLastName,
                                              bs.ClientEmail, bs.ClientPhoneNumber, bs.Price,
                                              bs.Type, _dateHelper.ConvertDateTimeFromUtcToLocal(bs.StartTime),
                                              _dateHelper.ConvertDateTimeFromUtcToLocal(bs.EndTime),
                                              _dateHelper.ConvertDateTimeFromUtcToLocal(bs.BookingDate),
                                              bs.ContactMethodId, bs.ContactMethodName, bs.ContactMethodColor, bs.ContactMethodIcon)).SingleOrDefault();

                if(bookedSessionAddRating == default)
                {
                    var bookedSession = _context.BookedSessions.GetById(bookingId);

                    if (bookedSession == null || bookedSession.ClientEmail != email) return default;

                    bookedSessionAddRating = new BookedSessionAddRatingViewModel(null, _files.GetUserProfilePhotoPathOrDefaultPhotoPath(_userManager, _environment, (await _userManager.FindByTherapistAccountIdAsync(bookedSession.TherapistId)).Id),
                                                                                 bookedSession.SessionId, bookedSession.Id, bookedSession.TherapistId,
                                                                                 bookedSession.TherapistFirstName, bookedSession.TherapistLastName,
                                                                                 bookedSession.TherapistEmail, bookedSession.TherapistPhoneNumber,
                                                                                 bookedSession.TherapistStreet, bookedSession.TherapistHouseNumber,
                                                                                 bookedSession.TherapistCity, bookedSession.TherapistCountry,
                                                                                 bookedSession.TherapistPostalCode, bookedSession.ClientId,
                                                                                 bookedSession.ClientFirstName, bookedSession.ClientLastName,
                                                                                 bookedSession.ClientEmail, bookedSession.ClientPhoneNumber, bookedSession.Price, bookedSession.Type,
                                                                                 _dateHelper.ConvertDateTimeFromUtcToLocal(bookedSession.StartTime),
                                                                                 _dateHelper.ConvertDateTimeFromUtcToLocal(bookedSession.EndTime),
                                                                                 _dateHelper.ConvertDateTimeFromUtcToLocal(bookedSession.BookingDate),
                                                                                 bookedSession.ContactMethodId, bookedSession.ContactMethodName, bookedSession.ContactMethodColor, bookedSession.ContactMethodIcon);
                }

                // If the ContactMethod data are changed in the meantime in table ContactMethods,
                // here we update them in BookedSessionAddInviteLink, but if that ContactMethod is deleted,
                // then we leave the ContactMethod data from BookedSessionAddInviteLink
                var contactMethod = _context.ContactMethods.GetById(bookedSessionAddRating.ContactMethod.Id);
                if (contactMethod != null)
                {
                    bookedSessionAddRating.ContactMethod.Name = contactMethod.Name;
                    bookedSessionAddRating.ContactMethod.Color = contactMethod.Color;
                    bookedSessionAddRating.ContactMethod.Icon = contactMethod.Icon;
                }

                var rating = _context.Ratings.ReadOnlyFind(r => r.BookedSessionId == bookedSessionAddRating.BookingId).SingleOrDefault();

                if (rating != default)
                    bookedSessionAddRating.Rating.StarsRating = rating.Rating;
                else
                {
                    var pendingRating = _context.PendingRatings.ReadOnlyFind(pr => pr.BookedSessionId == bookedSessionAddRating.BookingId).SingleOrDefault();

                    if (pendingRating != default)
                        bookedSessionAddRating.Rating.StarsRating = pendingRating.Rating;
                }

                return bookedSessionAddRating;
            }
            catch (Exception e)
            {
                await _systemErrors.SaveErrorAsync(e, "WebApplication9", "RatingFunctionsProvider", "MapToApprovedOrPendingDetailsRatingForAnonymousAsync");
                return default;
            }
        }
    }
}
