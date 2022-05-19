using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class Ratings
    {
        public string Id { get; set; }
        public string TherapistId { get; set; }
        public string ClientId { get; set; }
        public string ClientFirstName { get; set; }
        public string ClientLastName { get; set; }
        public string SessionId { get; set; }
        public string BookedSessionId { get; set; }
        public double Rating { get; set; }
        public string Comment { get; set; }
        public DateTime? RatingDate { get; set; }
        public string AdminIdWhoApproved { get; set; }
        public DateTime? ApprovalDate { get; set; }
    }
}
