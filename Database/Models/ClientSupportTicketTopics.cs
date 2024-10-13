using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class ClientSupportTicketTopics
    {
        public ClientSupportTicketTopics()
        {
            ClientSupportTickets = new HashSet<ClientSupportTickets>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string Icon { get; set; }

        public virtual ICollection<ClientSupportTickets> ClientSupportTickets { get; set; }
    }
}
