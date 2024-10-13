using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class WebCreditLogs
    {
        public WebCreditLogs()
        {
            PayPalPaymentRequests = new HashSet<PayPalPaymentRequests>();
            StripePaymentIntents = new HashSet<StripePaymentIntents>();
        }

        public string Id { get; set; }
        public string UserId { get; set; }
        public double Amount { get; set; }
        public DateTime? ExecutionDateTime { get; set; }
        public string PaymentTypeId { get; set; }
        public string PaymentTypeName { get; set; }
        public double CurrentAmount { get; set; }
        public string TransactionId { get; set; }
        public string Email { get; set; }

        public virtual PaymentTypes PaymentType { get; set; }
        public virtual Transactions Transaction { get; set; }
        public virtual ICollection<PayPalPaymentRequests> PayPalPaymentRequests { get; set; }
        public virtual ICollection<StripePaymentIntents> StripePaymentIntents { get; set; }
    }
}
