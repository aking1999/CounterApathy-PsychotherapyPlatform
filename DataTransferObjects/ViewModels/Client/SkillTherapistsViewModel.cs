using System;
using System.Collections.Generic;
using System.Text;

namespace DataTransferObjects.ViewModels.Client
{
    public class SkillTherapistsViewModel
    {
        public string Filter { get; set; }
        public string Predicate { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string Icon { get; set; }
        public List<TherapistShowcaseViewModel> Therapists { get; set; }

        public SkillTherapistsViewModel()
        {
            Therapists = new List<TherapistShowcaseViewModel>();
        }
    }
}
