using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class BookedSessionsContactMethods
    {
        public string BookedSessionId { get; set; }
        public string ContactMethodId { get; set; }
        public string InviteLink { get; set; }
        public DateTime? LinkAddedDateTime { get; set; }

        public virtual BookedSessions BookedSession { get; set; }
        public virtual ContactMethods ContactMethod { get; set; }
    }
}
