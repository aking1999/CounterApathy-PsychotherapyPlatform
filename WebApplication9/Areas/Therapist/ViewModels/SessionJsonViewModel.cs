using System;

namespace WebApplication9.Areas.Therapist.ViewModels
{
    public class SessionJsonViewModel
    {
        public string SessionId { get; set; }

        public string Subject { get; set; }

        public string Description { get; set; }

        public double Price { get; set; }

        public int Type { get; set; }

        public DateTime StartDateTime { get; set; }

        public DateTime EndDateTime { get; set; }

        public int Booked { get; set; }
    }
}
