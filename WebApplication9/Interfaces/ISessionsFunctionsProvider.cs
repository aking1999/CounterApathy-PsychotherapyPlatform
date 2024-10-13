using Database.Models;
using DataTransferObjects.ViewModels.Client;
using DataTransferObjects.ViewModels.Therapist;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApplication9.Interfaces
{
    public interface ISessionsFunctionsProvider
    {
        bool SessionAlreadyRated(string bookingId);
        bool SessionRatingPending(string bookingId);
        List<BookedSessions> GetBookedSessions(string filter, string predicate);
        List<BookedSessionViewModel> GetTherapistBookedSessions(string therapistId, string filter, string predicate);
        Task<List<BookedSessionAddRatingViewModel>> GetClientBookedSessionsAsync(string userId, string filter, string predicate);
        Task<List<BookedSessionAddRatingViewModel>> GetAnonymousBookedSessionsAsync(string email, string filter, string predicate);
        Task<CustomClient> GetSessionHostAsync(string sessionId);
        Therapists GetSessionTherapist(string sessionId);
        bool UserHasSessionToAttendDuringPeriod(string userId, DateTime periodStart, DateTime periodEnd);
        bool EmailHasSessionToAttendDuringPeriod(string email, DateTime periodStart, DateTime periodEnd);
        bool TherapistHasSessionDuringPeriod(string therapistId, DateTime periodStart, DateTime periodEnd);
        Task<List<BookedSessionAddRatingViewModel>> GetUnreviewedBookedSessionAddRatingViewModelsAsync(string userId);
        Task<BookedSessionAddRatingViewModel> GetUnreviewedBookedSessionAddRatingViewModelForAnonymousAsync(string email, string bookingId);
    }
}
