using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Emails.EmailTypes
{
    public class SessionInviteLinkEmail
    {
        public string BookingId { get; set; }
        public int Type { get; set; }
        public string ClientId { get; set; }
        public string ClientEmail { get; set; }
        public string ClientFirstName { get; set; }
        public string TherapistFirstName { get; set; }
        public string TherapistLastName { get; set; }
        public string TherapistEmail { get; set; }
        public string TherapistPhoneNumber { get; set; }
        public DateTime StartTime { get; set; }
        public string ContactMethodName { get; set; }
        public string InviteLink { get; set; }
    }
}
