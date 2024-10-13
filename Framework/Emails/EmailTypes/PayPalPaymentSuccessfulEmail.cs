using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Emails.EmailTypes
{
    public class PayPalPaymentSuccessfulEmail
    {
        public string ToEmail { get; set; }
        public string PaymentRequestId { get; set; }
        public string PaymentId { get; set; }
        public string TransactionId { get; set; }
        public string PrimaryCurrencyTotalPaid { get; set; }
        public string PrimaryCurrencyCode { get; set; }
        public string SecondaryCurrencyCode { get; set; }
        public string SecondaryCurrencyAmount { get; set; }
        public string PrimaryCurrencyFeeAmount { get; set; }
        public string ExchangeRate { get; set; }
        public string FeePercentage { get; set; }
        public string TotalWebCreditInAccount { get; set; }
        public string PaymentCompleteDateTime { get; set; }
    }
}
