using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class TherapistSupportTicketTopics
    {
        public TherapistSupportTicketTopics()
        {
            TherapistSupportTickets = new HashSet<TherapistSupportTickets>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string Icon { get; set; }

        public virtual ICollection<TherapistSupportTickets> TherapistSupportTickets { get; set; }
    }
}
