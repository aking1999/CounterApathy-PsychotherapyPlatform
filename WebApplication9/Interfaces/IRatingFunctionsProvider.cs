using Database.Models;
using DataTransferObjects.ViewModels.Client;
using DataTransferObjects.ViewModels.Therapist;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApplication9.Interfaces
{
    public interface IRatingFunctionsProvider
    {
        List<BookedSessionViewModel> MapToApprovedRatings(List<BookedSessions> bookedSessions);
        Task<BookedSessionAddInviteLinkViewModel> MapToApprovedDetailsRatingAsync(string therapistId, string bookingId);
        //Task<BookedSessionViewModel> MapToApprovedOrPendingRatingAsync(BookedSessions bookedSession);
        //Task<List<BookedSessionViewModel>> MapToApprovedOrPendingRatingsAsync(List<BookedSessions> bookedSessions);
        Task<List<BookedSessionAddRatingViewModel>> MapToApprovedOrPendingRatingsAsync(List<BookedSessions> bookedSessions);
        Task<BookedSessionAddRatingViewModel> MapToApprovedOrPendingDetailsRatingAsync(string userId, string bookingId);
        Task<BookedSessionAddRatingViewModel> MapToApprovedOrPendingDetailsRatingForAnonymousAsync(string email, string bookingId);
    }
}
