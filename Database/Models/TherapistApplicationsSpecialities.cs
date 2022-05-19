using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class TherapistApplicationsSpecialities
    {
        public string TherapistApplicationId { get; set; }
        public string SpecialityId { get; set; }

        public virtual Specialities Speciality { get; set; }
        public virtual TherapistApplications TherapistApplication { get; set; }
    }
}
