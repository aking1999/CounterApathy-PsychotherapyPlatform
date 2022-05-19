using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class Therapists
    {
        public Therapists()
        {
            AspNetUsers = new HashSet<AspNetUsers>();
            Sessions = new HashSet<Sessions>();
            TherapistsContactMethods = new HashSet<TherapistsContactMethods>();
            TherapistsSpecialities = new HashSet<TherapistsSpecialities>();
            Withdrawals = new HashSet<Withdrawals>();
        }

        public string Id { get; set; }
        public string TherapistsContactMethodsId { get; set; }
        public int? OnVacation { get; set; }
        public string Gender { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string About { get; set; }
        public string University { get; set; }
        public string PastCompanies { get; set; }
        public string HouseNumber { get; set; }
        public double Earnings { get; set; }

        public virtual ICollection<AspNetUsers> AspNetUsers { get; set; }
        public virtual ICollection<Sessions> Sessions { get; set; }
        public virtual ICollection<TherapistsContactMethods> TherapistsContactMethods { get; set; }
        public virtual ICollection<TherapistsSpecialities> TherapistsSpecialities { get; set; }
        public virtual ICollection<Withdrawals> Withdrawals { get; set; }
    }
}
