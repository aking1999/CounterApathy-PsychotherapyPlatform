using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class ContactMethods
    {
        public ContactMethods()
        {
            BookedConsultationsContactMethods = new HashSet<BookedConsultationsContactMethods>();
            BookedSessionsContactMethods = new HashSet<BookedSessionsContactMethods>();
            TherapistsContactMethods = new HashSet<TherapistsContactMethods>();
        }

        public string Id { get; set; }
        public string Icon { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }

        public virtual ICollection<BookedConsultationsContactMethods> BookedConsultationsContactMethods { get; set; }
        public virtual ICollection<BookedSessionsContactMethods> BookedSessionsContactMethods { get; set; }
        public virtual ICollection<TherapistsContactMethods> TherapistsContactMethods { get; set; }
    }
}
