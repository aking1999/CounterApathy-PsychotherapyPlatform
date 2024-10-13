using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class TherapistsSpecialities
    {
        public string TherapistId { get; set; }
        public string SpecialityId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? Present { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Description { get; set; }

        public virtual Specialities Speciality { get; set; }
        public virtual Therapists Therapist { get; set; }
    }
}
