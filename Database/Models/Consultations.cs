using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class Consultations
    {
        public string Id { get; set; }
        public string TherapistId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int Booked { get; set; }

        public virtual Therapists Therapist { get; set; }
    }
}
