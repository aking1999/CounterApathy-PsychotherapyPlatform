using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class ClientSupportTickets
    {
        public string Id { get; set; }
        public string UserIdOrAnonymous { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Text { get; set; }
        public string TopicId { get; set; }
        public string TopicName { get; set; }
        public DateTime TicketDateTime { get; set; }
        public string AdminIdWhoAnswered { get; set; }
        public bool? Answered { get; set; }
        public DateTime? AnsweredDateTime { get; set; }

        public virtual ClientSupportTicketTopics Topic { get; set; }
    }
}
