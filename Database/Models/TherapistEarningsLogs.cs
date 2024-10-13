using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class TherapistEarningsLogs
    {
        public string Id { get; set; }
        public string TherapistId { get; set; }
        public double Amount { get; set; }
        public DateTime? EarningsDateTime { get; set; }
        public double CurrentAmount { get; set; }
        public string TransactionId { get; set; }
        public string PaymentTypeId { get; set; }
        public string PaymentTypeName { get; set; }

        public virtual PaymentTypes PaymentType { get; set; }
        public virtual Therapists Therapist { get; set; }
        public virtual Transactions Transaction { get; set; }
    }
}
