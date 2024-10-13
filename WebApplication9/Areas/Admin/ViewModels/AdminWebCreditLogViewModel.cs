using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication9.Areas.Admin.ViewModels
{
    public class AdminWebCreditLogViewModel
    {
        public string WebCreditLogId { get; set; }
        public bool TransactionIsMissing { get; set; }
        public string SenderPhoto { get; set; }
        public string ReceiverPhoto { get; set; }
        public bool SenderIsUser { get; set; }
        public bool ReceiverIsUser { get; set; }
        public string UserId { get; set; }
        public string UserFullName { get; set; }
        public string UserEmail { get; set; }
        public string PaymentTypeLogo { get; set; }
        public double Amount { get; set; }
        public string CurrentAmount { get; set; }
        public string CurrencyCode { get; set; }
        public string ExecutionDateTime { get; set; }
    }
}
