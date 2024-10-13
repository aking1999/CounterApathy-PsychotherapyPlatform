using Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication9.ViewModels
{
    public class DayOfWeekNameConsultationsViewModel
    {
        public string DayOfWeekName { get; set; }
        public string DayOfWeekDateTime { get; set; }
        public List<Consultations> Consultations { get; set; }

        public DayOfWeekNameConsultationsViewModel()
        {
            Consultations = new List<Consultations>();
        }
    }
}
