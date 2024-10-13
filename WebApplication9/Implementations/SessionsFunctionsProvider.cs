using Database.Models;
using Database.RepositoryImplementations;
using Framework.Implementations;
using Framework.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication9.Interfaces;
using Framework.Helpers.ExtensionMethods;
using DataTransferObjects.ViewModels.Therapist;
using DataTransferObjects.ViewModels.Client;
using Microsoft.AspNetCore.Hosting;
using WebApplication9.Helpers;

namespace WebApplication9.Implementations
{
    public class SessionsFunctionsProvider : ISessionsFunctionsProvider
    {
        private readonly IFileRepository _files;
        private readonly IDateTimeHelper _dateHelper;
        private readonly IRatingFunctionsProvider _ratingFunctions;
        private IWebHostEnvironment _environment => new HttpContextAccessor().HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
        private readonly ISystemErrorLogger _systemErrors;
        private readonly UnitOfWork _context;
        private UserManager<CustomClient> _userManager => new HttpContextAccessor().HttpContext.RequestServices.GetRequiredService<UserManager<CustomClient>>();

        public SessionsFunctionsProvider()
        {
            _files = new FileRepository();
            _systemErrors = new SystemErrorLogger();
            _context = new UnitOfWork(new LajsnaProbaContext());
            _dateHelper = new DateTimeHelper();
            _ratingFunctions = new RatingFunctionsProvider();
        }

        public bool SessionAlreadyRated(string bookingId)
        {
            return _context.Ratings.ReadOnlyAny(r => r.BookedSessionId == bookingId);
        }

        public bool SessionRatingPending(string bookingId)
        {
            return _context.PendingRatings.ReadOnlyAny(pr => pr.BookedSessionId == bookingId);
        }

        public List<BookedSessions> GetBookedSessions(string filter, string predicate)
        {
            var bookedSessions = _context.BookedSessions.ReadOnlyGetAll();

            if (string.IsNullOrWhiteSpace(filter) || string.IsNullOrWhiteSpace(predicate))
                return bookedSessions
                               .OrderByDescending(s => s.StartTime)
                               .ToList();

            switch (filter.ToLower())
            {
                case "datum":
                    {
                        switch (predicate.ToLower())
                        {
                            case "danas":
                                {
                                    return bookedSessions
                                                   .Where(s => s.StartTime.Date == DateTime.UtcNow.Date)
                                                   .OrderByDescending(s => s.StartTime)
                                                   .ToList();
                                }
                            case "ova-nedelja":
                                {
                                    var startOfWeek = _dateHelper.GetStartOfWeekDate(DayOfWeek.Monday);
                                    var nextMonday = startOfWeek.AddDays(7);

                                    return bookedSessions
                                                   .Where(s => startOfWeek <= s.StartTime &&
                                                          s.StartTime <= nextMonday)
                                                   .OrderByDescending(s => s.StartTime)
                                                   .ToList();
                                }
                            case "ovaj-mesec":
                                {
                                    var now = DateTime.UtcNow;
                                    var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
                                    var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                                    return bookedSessions
                                                   .Where(s => firstDayOfMonth <= s.StartTime &&
                                                          s.StartTime <= lastDayOfMonth)
                                                   .OrderByDescending(s => s.StartTime)
                                                   .ToList();
                                }
                            case "ova-godina":
                                {
                                    var thisYear = DateTime.UtcNow.Year;
                                    var startOfYear = new DateTime(thisYear, 1, 1);
                                    var endOfYear = new DateTime(thisYear, 12, 31);

                                    return bookedSessions
                                                   .Where(s => startOfYear <= s.StartTime &&
                                                          s.StartTime <= endOfYear)
                                                   .OrderByDescending(s => s.StartTime)
                                                   .ToList();
                                }
                            default: return new List<BookedSessions>();
                        }
                    }
                case "status":
                    {
                        switch (predicate.ToLower())
                        {
                            case "na-čekanju":
                                {
                                    return bookedSessions
                                                   .Where(s => s.EndTime >= DateTime.UtcNow)
                                                   .OrderByDescending(s => s.StartTime)
                                                   .ToList();
                                }
                            case "završeno":
                                {
                                    return bookedSessions
                                                   .Where(s => s.EndTime <= DateTime.UtcNow)
                                                   .OrderByDescending(s => s.StartTime)
                                                   .ToList();
                                }
                            default: return new List<BookedSessions>();
                        }
                    }
                default: return new List<BookedSessions>();
            }
        }

        public List<BookedSessionViewModel> GetTherapistBookedSessions(string therapistId, string filter, string predicate)
        {
            if (string.IsNullOrWhiteSpace(therapistId))
                return new List<BookedSessionViewModel>();

            return _ratingFunctions.MapToApprovedRatings(GetBookedSessions(filter, predicate)
                        .Where(s => s.TherapistId == therapistId)
                        .ToList());
        }

        public async Task<List<BookedSessionAddRatingViewModel>> GetClientBookedSessionsAsync(string userId, string filter, string predicate)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return new List<BookedSessionAddRatingViewModel>();

            return await _ratingFunctions.MapToApprovedOrPendingRatingsAsync(GetBookedSessions(filter, predicate)
                        .Where(s => s.ClientId == userId)
                        .ToList());
        }

        public async Task<List<BookedSessionAddRatingViewModel>> GetAnonymousBookedSessionsAsync(string email, string filter, string predicate)
        {
            if (string.IsNullOrWhiteSpace(email))
                return new List<BookedSessionAddRatingViewModel>();

            return await _ratingFunctions.MapToApprovedOrPendingRatingsAsync(GetBookedSessions(filter, predicate)
                        .Where(s => (s.ClientId == "anonymous" || s.ClientId == "unauthorized") && s.ClientEmail == email)
                        .ToList());
        }

        public async Task<CustomClient> GetSessionHostAsync(string sessionId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sessionId))
                    return null;

                var therapistId = (from sess in _context.Sessions.ReadOnlyFind(s => s.Id == sessionId)
                                   join th in _context.Therapists.ReadOnlyGetAll()
                                   on sess.TherapistId equals th.Id
                                   select th.Id).Single();

                return await _userManager.FindByTherapistAccountIdAsync(therapistId);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Therapists GetSessionTherapist(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                return null;

            return (from sess in _context.Sessions.ReadOnlyFind(s => s.Id == sessionId)
                    join th in _context.Therapists.ReadOnlyGetAll()
                    on sess.TherapistId equals th.Id
                    select th).Single();
        }

        public bool UserHasSessionToAttendDuringPeriod(string userId, DateTime periodStart, DateTime periodEnd)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return false;

            return _context.BookedSessions.ReadOnlyAny(s => s.StartTime < periodEnd &&
                                                            s.EndTime > periodStart);
        }

        public bool EmailHasSessionToAttendDuringPeriod(string email, DateTime periodStart, DateTime periodEnd)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            email = email.ToUpperInvariant();
            return _context.BookedSessions.ReadOnlyAny(s => s.StartTime < periodEnd &&
                                                            s.EndTime > periodStart);
        }

        public bool TherapistHasSessionDuringPeriod(string therapistId, DateTime periodStart, DateTime periodEnd)
        {
            if (string.IsNullOrWhiteSpace(therapistId))
                return false;

            return _context.Sessions.ReadOnlyAny(s => s.TherapistId == therapistId &&
                                                      s.StartDateTime < periodEnd &&
                                                      s.EndDateTime > periodStart);
        }

        public async Task<List<BookedSessionAddRatingViewModel>> GetUnreviewedBookedSessionAddRatingViewModelsAsync(string userId)
        {
            var now = DateTime.UtcNow;
            var unreviewedSessions = new List<BookedSessionAddRatingViewModel>();

            var bookedSessions = _context.BookedSessions
                .ReadOnlyFind(s => s.ClientId == userId && (s.EndTime <= now));

            if (!bookedSessions.Any())
                return unreviewedSessions;

            foreach (var bookedSession in bookedSessions
                .Select(s =>
                new
                {
                    s.Id,
                    s.SessionId,
                    s.TherapistId,
                    s.TherapistFirstName,
                    s.TherapistLastName,
                    s.Type,
                    s.ContactMethodName,
                    s.StartTime,
                    s.EndTime
                }).ToList())
            {
                if (!_context.PendingRatings.ReadOnlyAny(r => r.BookedSessionId == bookedSession.Id) &&
                    !_context.Ratings.ReadOnlyAny(r => r.BookedSessionId == bookedSession.Id))
                {
                    unreviewedSessions.Add(new BookedSessionAddRatingViewModel
                    {
                        BookingId = bookedSession.Id,
                        SessionId = bookedSession.SessionId,
                        TherapistId = bookedSession.TherapistId,
                        ProfilePhoto = _files.GetUserProfilePhotoPathOrDefaultPhotoPath(_userManager, _environment, (await _userManager.FindByTherapistAccountIdAsync(bookedSession.TherapistId)).Id),
                        TherapistFirstName = bookedSession.TherapistFirstName,
                        TherapistLastName = bookedSession.TherapistLastName,
                        Type = bookedSession.Type,
                        ContactMethod = new DataTransferObjects.ViewModels.Shared.ContactMethodViewModel { Name = bookedSession.ContactMethodName },
                        StartTime = bookedSession.StartTime,
                        EndTime = bookedSession.EndTime
                    });
                }
            }

            return unreviewedSessions;
        }

        public async Task<BookedSessionAddRatingViewModel> GetUnreviewedBookedSessionAddRatingViewModelForAnonymousAsync(string email, string bookingId)
        {
            var now = DateTime.UtcNow;
            BookedSessionAddRatingViewModel unreviewedSession = default;

            var bookedSession = _context.BookedSessions
                .ReadOnlyFind(s => (s.ClientId == "anonymous" || s.ClientId == "unauthorized") && (s.EndTime <= now) && s.ClientEmail == email && s.Id == bookingId).SingleOrDefault();

            if (bookedSession == default)
                return default;

            if (!_context.PendingRatings.ReadOnlyAny(r => r.BookedSessionId == bookedSession.Id) &&
                !_context.Ratings.ReadOnlyAny(r => r.BookedSessionId == bookedSession.Id))
            {
                unreviewedSession = new BookedSessionAddRatingViewModel
                {
                    BookingId = bookedSession.Id,
                    SessionId = bookedSession.SessionId,
                    TherapistId = bookedSession.TherapistId,
                    ProfilePhoto = _files.GetUserProfilePhotoPathOrDefaultPhotoPath(_userManager, _environment, (await _userManager.FindByTherapistAccountIdAsync(bookedSession.TherapistId)).Id),
                    TherapistFirstName = bookedSession.TherapistFirstName,
                    TherapistLastName = bookedSession.TherapistLastName,
                    ClientEmail = bookedSession.ClientEmail,
                    Type = bookedSession.Type,
                    ContactMethod = new DataTransferObjects.ViewModels.Shared.ContactMethodViewModel { Name = bookedSession.ContactMethodName },
                    StartTime = bookedSession.StartTime,
                    EndTime = bookedSession.EndTime
                };
            }

            return unreviewedSession;
        }
    }
}
