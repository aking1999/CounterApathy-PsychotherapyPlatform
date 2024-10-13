using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication9.ViewModels
{
    public class StripePaymentIntentViewModel
    {
        public string PaymentIntentId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Created { get; set; }
        public int Status { get; set; }
    }
}
