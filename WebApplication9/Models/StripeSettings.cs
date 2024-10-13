namespace WebApplication9.Models
{
    public class StripeSettings
    {
        public string PublishableKey { get; set; }
        public string ApiKey { get; set; }
        public string WebhookSecret { get; set; }
        public decimal StripeConnectTransferFeePercentage { get; set; }
        public decimal StripeConnectCurrencyConversionFeePercentage { get; set; }
        public decimal StripeConnectFixedFeeAmountInCents { get; set; }
    }
}
