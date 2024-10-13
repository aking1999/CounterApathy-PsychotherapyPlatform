using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class StripePaymentIntents
    {
        public string Id { get; set; }
        public string UserIdOrAnonymous { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string CustomerId { get; set; }
        public int Status { get; set; }
        public string SystemEventTypeName { get; set; }
        public string StripeEventTypeName { get; set; }
        public DateTime? MustSucceedUntil { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string WebCreditLogId { get; set; }

        public virtual WebCreditLogs WebCreditLog { get; set; }
    }
}
