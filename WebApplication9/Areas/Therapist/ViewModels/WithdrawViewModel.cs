namespace WebApplication9.Areas.Therapist.ViewModels
{
    public class WithdrawViewModel
    {
        public string StripeConnectTransferFeePercentage { get; set; }
        public string StripeConnectTransferFeePercentageAmount { get; set; }
        public string SubtotalAmount { get; set; }
        public string TotalAmount { get; set; }
        public bool HasSetupStripeAccount { get; set; }
    }
}
