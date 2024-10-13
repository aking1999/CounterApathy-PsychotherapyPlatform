using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication9.Areas.Admin.ViewModels
{
    public class AdminDashboardViewModel
    {
        public double TotalEarnings { get; set; }
        public double TotalExpenses { get; set; }
        public double TotalProfit { get; set; }
        public double EarningsDuringPeriod { get; set; }
        public double  ExpensesDuringPeriod { get; set; }
        public double ProfitDuringPeriod { get; set; }
        public double SessionsCount { get; set; }
        public double BookedSessionsCount { get; set; }
        public int RatingsCount { get; set; }
        public bool RatingIsPending { get; set; }
        public int PendingRatingCount { get; set; }
        public int WithdrawalsCount { get; set; }
        public bool WithdrawalIsPending { get; set; }
        public int PendingWithdrawalCount { get; set; }
        public int ApplicationsCount { get; set; }
        public bool ApplicationIsPending { get; set; }
        public int PendingApplicationCount { get; set; }
        public double TotalUnusedClientWebCredit { get; set; }
        public int ClientsWebCreditLogsCount { get; set; }
        public double TotalUnusedTherapistsEarnings { get; set; }
        public int TherapistsEarningsLogsCount { get; set; }
    }
}
