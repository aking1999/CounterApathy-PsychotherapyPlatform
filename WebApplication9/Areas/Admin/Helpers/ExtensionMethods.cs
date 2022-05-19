using Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication9.Areas.Admin.ViewModels;

namespace WebApplication9.Areas.Admin.Helpers
{
    public static class ExtensionMethods
    {
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
