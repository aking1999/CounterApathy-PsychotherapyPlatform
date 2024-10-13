using Database.Models;

namespace Framework.Providers
{
    public class PaymentTypesProvider
    {
        public static PaymentTypes Withdrawal => new PaymentTypes { Id = "cash-withdrawal", Name = "Withdrawal", Logo = "cashwithdrawal.png" };
        public static PaymentTypes PaymentCards => new PaymentTypes { Id = "payment-cards", Name = "Payment Cards", Logo = "payment-cards.png" };
        public static PaymentTypes BankTransfer => new PaymentTypes { Id = "bank-transfer", Name = "Bank Transfer", Logo = "bank.png" };
        public static PaymentTypes Posta => new PaymentTypes { Id = "posta", Name = "Pošta", Logo = "post-of-serbia.svg" };
        public static PaymentTypes PayPal => new PaymentTypes { Id = "paypal", Name = "PayPal", Logo = "paypal.svg" };
        public static PaymentTypes CounterApathyPayment => new PaymentTypes { Id = "counterapathy-payment", Name = "CounterApathy Payment", Logo = "counterapathy-logo.svg" };
        public static PaymentTypes USDCoin => new PaymentTypes { Id = "usdc", Name = "USD Coin", Logo = "usdc.svg" };
        public static PaymentTypes USDTether => new PaymentTypes { Id = "usdt", Name = "USD Tether", Logo = "usdt.svg" };
    }
}
