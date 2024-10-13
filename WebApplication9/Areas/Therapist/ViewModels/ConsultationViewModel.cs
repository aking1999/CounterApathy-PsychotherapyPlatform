using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication9.Areas.Therapist.ViewModels
{
    public class ConsultationViewModel
    {
        public string ConsultationId { get; set; }

        public DateTime StartDateTime { get; set; }

        public DateTime EndDateTime { get; set; }

        public int Booked { get; set; }
    }
}
