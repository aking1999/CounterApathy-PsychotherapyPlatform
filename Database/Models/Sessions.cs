using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class Sessions
    {
        public string Id { get; set; }
        public string TherapistId { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int Type { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string Color { get; set; }
        public string Icon { get; set; }
        public int Booked { get; set; }

        public virtual Therapists Therapist { get; set; }
    }
}
