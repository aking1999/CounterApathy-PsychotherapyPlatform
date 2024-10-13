using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class PendingRatings
    {
        public string Id { get; set; }
        public string TherapistId { get; set; }
        public string ClientId { get; set; }
        public string SessionId { get; set; }
        public string BookedSessionId { get; set; }
        public double Rating { get; set; }
        public string Comment { get; set; }
        public DateTime? RatingDate { get; set; }
        public int? Refused { get; set; }
    }
}
