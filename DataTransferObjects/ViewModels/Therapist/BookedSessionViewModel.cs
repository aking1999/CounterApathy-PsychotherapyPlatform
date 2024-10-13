using DataTransferObjects.ViewModels.Shared;
using System;

namespace DataTransferObjects.ViewModels.Therapist
{
    public class BookedSessionViewModel
    {
        public string ProfilePhoto { get; set; }
        public string SessionId { get; set; }
        public string BookingId { get; set; }
        public string TherapistId { get; set; }
        public string TherapistFirstName { get; set; }
        public string TherapistLastName { get; set; }
        public string TherapistEmail { get; set; }
        public string TherapistPhoneNumber { get; set; }
        public string TherapistStreet { get; set; }
        public string TherapistHouseNumber { get; set; }
        public string TherapistCity { get; set; }
        public string TherapistCountry { get; set; }
        public string TherapistPostalCode { get; set; }
        public string ClientId { get; set; }
        public string ClientFirstName { get; set; }
        public string ClientLastName { get; set; }
        public string ClientEmail { get; set; }
        public string ClientPhoneNumber { get; set; }
        public double Price { get; set; }
        public int Type { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime BookingDate { get; set; }
        public bool TherapistIsPaid { get; set; }
        public double StarsRating { get; set; }
        public bool HasRating
        {
            get { return StarsRating > 0; }
        }
        public bool SessionEnded
        {
            get { return DateTime.UtcNow >= EndTime; }
        }
        public ContactMethodViewModel ContactMethod { get; set; }
        public BookedSessionViewModel()
        {
            ContactMethod = new ContactMethodViewModel();
        }
    }
}
