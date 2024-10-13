using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class StripeAccount
    {
        public string Id { get; set; }
        public string TherapistId { get; set; }
        public DateTime CreatedDateTime { get; set; }

        public virtual Therapists Therapist { get; set; }
    }
}
