using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication9.ViewModels
{
    public class TherapistConsultationsViewModel
    {
        public string TherapistId { get; set; }

        public List<DayOfWeekNameConsultationsViewModel> DayOfWeekNameConsultations { get; set; }

        public TherapistConsultationsViewModel()
        {
            DayOfWeekNameConsultations = new List<DayOfWeekNameConsultationsViewModel>();
        }
    }
}
