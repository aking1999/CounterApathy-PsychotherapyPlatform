using Database.Models;
using System.Collections.Generic;

namespace Framework.Interfaces
{
    public interface IWithdrawalsFunctionsProvider 
    {
        string RequiredDaysPassedBetweenWithdrawals(string therapistId, int REQUIRED_DAYS_BETWEEN_WITHDRAWALS);
        List<Withdrawals> GetWithdrawals(string filter, string predicate);
        List<Withdrawals> GetTherapistWithdrawals(string therapistId, string filter, string predicate);
    }
}
