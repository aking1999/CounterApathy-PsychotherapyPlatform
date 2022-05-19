using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class ContactMethods
    {
        public ContactMethods()
        {
            TherapistsContactMethods = new HashSet<TherapistsContactMethods>();
        }

        public string Id { get; set; }
        public string Icon { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }

        public virtual ICollection<TherapistsContactMethods> TherapistsContactMethods { get; set; }
    }
}
