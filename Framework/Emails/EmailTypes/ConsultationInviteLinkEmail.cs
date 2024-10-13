using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Emails.EmailTypes
{
    public class ConsultationInviteLinkEmail
    {
        public string BookingId { get; set; }
        public string ClientId { get; set; }
        public string ClientEmail { get; set; }
        public string ClientFirstName { get; set; }
        public string TherapistFirstName { get; set; }
        public string TherapistLastName { get; set; }
        public string TherapistEmail { get; set; }
        public string TherapistPhoneNumber { get; set; }
        public DateTime StartDateTime { get; set; }
        public string ContactMethodName { get; set; }
        public string InviteLink { get; set; }
    }
}
