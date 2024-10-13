using System;
using System.ComponentModel.DataAnnotations;

namespace DataTransferObjects.ViewModels.Therapist
{
    public class BookedConsultationAddInviteLinkViewModel
    {
        [Required(ErrorMessage = "Polje je obavezno.")]
        [StringLength(1024, MinimumLength = 10, ErrorMessage = "Unesite od 10 do 1024 karaktera.")]
        public string InviteLink { get; set; }
        public BookedConsultationViewModel BookedConsultation { get; set; }

        public BookedConsultationAddInviteLinkViewModel()
        {
            BookedConsultation = new BookedConsultationViewModel();
        }

        public BookedConsultationAddInviteLinkViewModel(string inviteLink, string profilePhoto, string consultationId,
            string bookingId, string therapistId, string therapistFirstName, string therapistLastName,
            string therapistEmail, string therapistPhoneNumber, string clientId, string clientFirstName, string clientLastName,
            string clientEmail, string clientPhoneNumber, DateTime startDateTime,
            DateTime endDateTime, DateTime bookingDate, string contactMethodId, string contactMethodName,
            string contactMethodColor, string contactMethodIcon)
        {
            InviteLink = inviteLink;

            BookedConsultation = new BookedConsultationViewModel
            {
                ProfilePhoto = profilePhoto,
                ConsultationId = consultationId,
                BookingId = bookingId,
                TherapistId = therapistId,
                TherapistFirstName = therapistFirstName,
                TherapistLastName = therapistLastName,
                TherapistEmail = therapistEmail,
                TherapistPhoneNumber = therapistPhoneNumber,
                ClientId = clientId,
                ClientFirstName = clientFirstName,
                ClientLastName = clientLastName,
                ClientEmail = clientEmail,
                ClientPhoneNumber = clientPhoneNumber,
                StartDateTime = startDateTime,
                EndDateTime = endDateTime,
                BookingDate = bookingDate
            };

            BookedConsultation.ContactMethod = new Shared.ContactMethodViewModel
            {
                Id = contactMethodId,
                Name = contactMethodName,
                Color = contactMethodColor,
                Icon = contactMethodIcon
            };
        }
    }
}
