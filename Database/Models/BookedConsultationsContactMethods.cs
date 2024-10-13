using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class BookedConsultationsContactMethods
    {
        public string BookedConsultationId { get; set; }
        public string ContactMethodId { get; set; }
        public string InviteLink { get; set; }
        public DateTime? LinkAddedDateTime { get; set; }

        public virtual BookedConsultations BookedConsultation { get; set; }
        public virtual ContactMethods ContactMethod { get; set; }
    }
}
