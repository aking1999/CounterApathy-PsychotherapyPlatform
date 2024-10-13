using System;
using System.Collections.Generic;
using System.Text;

namespace DataTransferObjects.ViewModels.Client
{
    public class PayPalPaymentSuccessfulViewModel
    {
        public string Id { get; set; }
        public string PaymentId { get; set; }
        public string TransactionId { get; set; }
        public string PaymentType { get; set; }
        public string PaymentTypeLogo { get; set; }
        public string PrimaryCurrencyTotalPaid { get; set; }
        public string PrimaryCurrencyCode { get; set; }
        public string SecondaryCurrencyCode { get; set; }
        public string PrimaryCurrencyAmount { get; set; }
        public string SecondaryCurrencyAmount { get; set; }
        public string PrimaryCurrencyFeeAmount { get; set; }
        //public string SecondaryCurrencyFeeAmount { get; set; }
        public string ExchangeRate { get; set; }
        public string FeePercentage { get; set; }
        public string RequestDateTime { get; set; }
        public string PaymentCompleteDateTime { get; set; }
    }
}
