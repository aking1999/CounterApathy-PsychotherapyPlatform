using System;
using System.ComponentModel.DataAnnotations;
using Database.Models;

namespace DataTransferObjects.ViewModels.Therapist
{
    public class BookedSessionAddInviteLinkViewModel
    {
        [Required(ErrorMessage = "Polje je obavezno.")]
        [StringLength(1024, MinimumLength = 10, ErrorMessage = "Unesite od 10 do 1024 karaktera.")]
        public string InviteLink { get; set; }
        public Transactions Transaction { get; set; }
        public BookedSessionViewModel BookedSession { get; set; }

        public BookedSessionAddInviteLinkViewModel()
        {
            BookedSession = new BookedSessionViewModel();
        }

        public BookedSessionAddInviteLinkViewModel(string inviteLink, string profilePhoto, string sessionId,
            string bookingId, string therapistId, string therapistFirstName, string therapistLastName,
            string therapistEmail, string therapistPhoneNumber, string therapistStreet,
            string therapistHouseNumber, string therapistCity, string therapistCountry,
            string therapistPostalCode, string clientId, string clientFirstName, string clientLastName,
            string clientEmail, string clientPhoneNumber, double price, int type, DateTime startTime,
            DateTime endTime, DateTime bookingDate, string contactMethodId, string contactMethodName,
            string contactMethodColor, string contactMethodIcon, bool therapistIsPaid,
            double transactionAmount, string transactionCurrencyCode, DateTime transactionDateTime)
        {
            InviteLink = inviteLink;
            Transaction = new Transactions
            {
                Amount = transactionAmount,
                CurrencyCode = transactionCurrencyCode,
                DateTime = transactionDateTime
            };

            BookedSession = new BookedSessionViewModel
            {
                ProfilePhoto = profilePhoto,
                SessionId = sessionId,
                BookingId = bookingId,
                TherapistId = therapistId,
                TherapistFirstName = therapistFirstName,
                TherapistLastName = therapistLastName,
                TherapistEmail = therapistEmail,
                TherapistPhoneNumber = therapistPhoneNumber,
                TherapistStreet = therapistStreet,
                TherapistHouseNumber = therapistHouseNumber,
                TherapistCity = therapistCity,
                TherapistCountry = therapistCountry,
                TherapistPostalCode = therapistPostalCode,
                ClientId = clientId,
                ClientFirstName = clientFirstName,
                ClientLastName = clientLastName,
                ClientEmail = clientEmail,
                ClientPhoneNumber = clientPhoneNumber,
                Price = price,
                Type = type,
                StartTime = startTime,
                EndTime = endTime,
                BookingDate = bookingDate,
                TherapistIsPaid = therapistIsPaid
            };

            BookedSession.ContactMethod = new Shared.ContactMethodViewModel
            {
                Id = contactMethodId,
                Name = contactMethodName,
                Color = contactMethodColor,
                Icon = contactMethodIcon
            };
        }
    }
}
