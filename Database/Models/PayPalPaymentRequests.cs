using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class PayPalPaymentRequests
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string WebCreditLogId { get; set; }
        public string PayPalPayerId { get; set; }
        public string PayPalPaymentId { get; set; }
        public string PayPalTransactionId { get; set; }
        public string PrimaryCurrencyCode { get; set; }
        public string SecondaryCurrencyCode { get; set; }
        public double PrimaryCurrencyAmount { get; set; }
        public double SecondaryCurrencyAmount { get; set; }
        public double ExchangeRate { get; set; }
        public DateTime RequestDateTime { get; set; }
        public int Status { get; set; }
        public DateTime? PaymentCompleteDateTime { get; set; }
        public double FeePercentage { get; set; }
        public double PrimaryCurrencyFeeAmount { get; set; }
        public double SecondaryCurrencyFeeAmount { get; set; }

        public virtual AspNetUsers User { get; set; }
        public virtual WebCreditLogs WebCreditLog { get; set; }
    }
}
