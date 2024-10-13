using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class TherapistApplications
    {
        public TherapistApplications()
        {
            TherapistApplicationsPsychotherapyTechniques = new HashSet<TherapistApplicationsPsychotherapyTechniques>();
            TherapistApplicationsSpecialities = new HashSet<TherapistApplicationsSpecialities>();
        }

        public string Id { get; set; }
        public string UserId { get; set; }
        public string ApplicationDate { get; set; }
        public int? Accepted { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string Gender { get; set; }
        public string HouseNumber { get; set; }
        public string University { get; set; }
        public string PastCompanies { get; set; }
        public string ProfilePhoto { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? YearOfBirth { get; set; }
        public string PhoneNumber { get; set; }
        public bool? UnderSupervision { get; set; }

        public virtual AspNetUsers User { get; set; }
        public virtual ICollection<TherapistApplicationsPsychotherapyTechniques> TherapistApplicationsPsychotherapyTechniques { get; set; }
        public virtual ICollection<TherapistApplicationsSpecialities> TherapistApplicationsSpecialities { get; set; }
    }
}
