using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class TherapistsContactMethods
    {
        public string TherapistId { get; set; }
        public string ContactMethodId { get; set; }

        public virtual ContactMethods ContactMethod { get; set; }
        public virtual Therapists Therapist { get; set; }
    }
}
