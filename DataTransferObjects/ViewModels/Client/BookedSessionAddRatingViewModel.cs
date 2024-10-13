using DataTransferObjects.PartialViewModels;
using DataTransferObjects.ViewModels.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTransferObjects.ViewModels.Client
{
    public class BookedSessionAddRatingViewModel
    {
        public string InviteLink { get; set; }
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
        public ContactMethodViewModel ContactMethod { get; set; }
        public RatingPartialViewModel Rating { get; set; }
        public bool SessionEnded
        {
            get { return DateTime.UtcNow >= EndTime; }
        }

        public BookedSessionAddRatingViewModel()
        {
            ContactMethod = new ContactMethodViewModel();
            Rating = new RatingPartialViewModel();
        }

        public BookedSessionAddRatingViewModel(string inviteLink, string profilePhoto, string sessionId,
            string bookingId, string therapistId, string therapistFirstName, string therapistLastName,
            string therapistEmail, string therapistPhoneNumber, string therapistStreet,
            string therapistHouseNumber, string therapistCity, string therapistCountry,
            string therapistPostalCode, string clientId, string clientFirstName, string clientLastName,
            string clientEmail, string clientPhoneNumber, double price, int type, DateTime startTime,
            DateTime endTime, DateTime bookingDate, string contactMethodId, string contactMethodName,
            string contactMethodColor, string contactMethodIcon)
        {
            ContactMethod = new ContactMethodViewModel();
            Rating = new RatingPartialViewModel();
            InviteLink = inviteLink;
            ProfilePhoto = profilePhoto;
            SessionId = sessionId;
            BookingId = bookingId;
            TherapistId = therapistId;
            TherapistFirstName = therapistFirstName;
            TherapistLastName = therapistLastName;
            TherapistEmail = therapistEmail;
            TherapistPhoneNumber = therapistPhoneNumber;
            TherapistStreet = therapistStreet;
            TherapistHouseNumber = therapistHouseNumber;
            TherapistCity = therapistCity;
            TherapistCountry = therapistCountry;
            TherapistPostalCode = therapistPostalCode;
            ClientId = clientId;
            ClientFirstName = clientFirstName;
            ClientLastName = clientLastName;
            ClientEmail = clientEmail;
            ClientPhoneNumber = clientPhoneNumber;
            Price = price;
            Type = type;
            StartTime = startTime;
            EndTime = endTime;
            BookingDate = bookingDate;
            ContactMethod.Id = contactMethodId;
            ContactMethod.Name = contactMethodName;
            ContactMethod.Color = contactMethodColor;
            ContactMethod.Icon = contactMethodIcon;
        }
    }
}
