using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class Specialities
    {
        public Specialities()
        {
            TherapistApplicationsSpecialities = new HashSet<TherapistApplicationsSpecialities>();
            TherapistsSpecialities = new HashSet<TherapistsSpecialities>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string Icon { get; set; }

        public virtual ICollection<TherapistApplicationsSpecialities> TherapistApplicationsSpecialities { get; set; }
        public virtual ICollection<TherapistsSpecialities> TherapistsSpecialities { get; set; }
    }
}
