using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class Transactions
    {
        public Transactions()
        {
            TherapistEarningsLogs = new HashSet<TherapistEarningsLogs>();
            WebCreditLogs = new HashSet<WebCreditLogs>();
            Withdrawals = new HashSet<Withdrawals>();
        }

        public string Id { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public DateTime DateTime { get; set; }
        public double Amount { get; set; }
        public string CurrencyCode { get; set; }

        public virtual ICollection<TherapistEarningsLogs> TherapistEarningsLogs { get; set; }
        public virtual ICollection<WebCreditLogs> WebCreditLogs { get; set; }
        public virtual ICollection<Withdrawals> Withdrawals { get; set; }
    }
}
