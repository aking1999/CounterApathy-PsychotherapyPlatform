using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class BookedConsultations
    {
        public BookedConsultations()
        {
            BookedConsultationsContactMethods = new HashSet<BookedConsultationsContactMethods>();
        }

        public string Id { get; set; }
        public string ConsultationId { get; set; }
        public string TherapistId { get; set; }
        public string TherapistFirstName { get; set; }
        public string TherapistLastName { get; set; }
        public string TherapistEmail { get; set; }
        public string TherapistPhoneNumber { get; set; }
        public string ClientId { get; set; }
        public string ClientFirstName { get; set; }
        public string ClientLastName { get; set; }
        public string ClientEmail { get; set; }
        public string ClientPhoneNumber { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public DateTime BookingDate { get; set; }
        public string ContactMethodId { get; set; }
        public string ContactMethodName { get; set; }
        public string ContactMethodColor { get; set; }
        public string ContactMethodIcon { get; set; }

        public virtual ICollection<BookedConsultationsContactMethods> BookedConsultationsContactMethods { get; set; }
    }
}
