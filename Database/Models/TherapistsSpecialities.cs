using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class TherapistsSpecialities
    {
        public string TherapistId { get; set; }
        public string SpecialityId { get; set; }

        public virtual Specialities Speciality { get; set; }
        public virtual Therapists Therapist { get; set; }
    }
}
