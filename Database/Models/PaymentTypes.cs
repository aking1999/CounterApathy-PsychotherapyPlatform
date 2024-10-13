using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class PaymentTypes
    {
        public PaymentTypes()
        {
            TherapistEarningsLogs = new HashSet<TherapistEarningsLogs>();
            WebCreditLogs = new HashSet<WebCreditLogs>();
            Withdrawals = new HashSet<Withdrawals>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Logo { get; set; }

        public virtual ICollection<TherapistEarningsLogs> TherapistEarningsLogs { get; set; }
        public virtual ICollection<WebCreditLogs> WebCreditLogs { get; set; }
        public virtual ICollection<Withdrawals> Withdrawals { get; set; }
    }
}
