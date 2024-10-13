using DataTransferObjects.ViewModels.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTransferObjects.ViewModels.Client
{
    public class BookedConsultationInviteLinkViewModel
    {
        public string InviteLink { get; set; }
        public string ProfilePhoto { get; set; }
        public string BookingId { get; set; }
        public string TherapistId { get; set; }
        public string TherapistFirstName { get; set; }
        public string TherapistLastName { get; set; }
        public string TherapistEmail { get; set; }
        public string TherapistPhoneNumber { get; set; }
        public string ClientFirstName { get; set; }
        public string ClientLastName { get; set; }
        public string ClientEmail { get; set; }
        public string ClientPhoneNumber { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public DateTime BookingDate { get; set; }
        public bool SessionEnded
        {
            get { return DateTime.UtcNow >= EndDateTime; }
        }
        public ContactMethodViewModel ContactMethod { get; set; }
        public BookedConsultationInviteLinkViewModel()
        {
            ContactMethod = new ContactMethodViewModel();
        }
    }
}
