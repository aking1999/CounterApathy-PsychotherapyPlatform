using Database.Models;
using Database.RepositoryImplementations;
using Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Framework.Implementations
{
    public class WithdrawalsFunctionsProvider : IWithdrawalsFunctionsProvider
    {
        private readonly UnitOfWork _context;
        private readonly IDateTimeHelper _dateHelper;

        public WithdrawalsFunctionsProvider()
        {
            _context = new UnitOfWork(new LajsnaProbaContext());
            _dateHelper = new DateTimeHelper();
        }

        public string RequiredDaysPassedBetweenWithdrawals(string therapistId, int REQUIRED_DAYS_BETWEEN_WITHDRAWALS)
        {
            var withdrawals = _context.Withdrawals.ReadOnlyFind(w => w.TherapistId == therapistId).ToList();

            if (withdrawals.Any())
            {
                var latestWithdrawal = withdrawals.Max(w => w.AcceptDateTime != null ? w.AcceptDateTime : w.RequestDateTime);

                var checkDate = latestWithdrawal.Value.AddDays(REQUIRED_DAYS_BETWEEN_WITHDRAWALS);
                var checkDateLocal = _dateHelper.ConvertDateTimeFromUtcToLocal(checkDate);
                var utcNow = DateTime.UtcNow;
                var utcNowToLocal = _dateHelper.ConvertDateTimeFromUtcToLocal(utcNow);

                if (checkDate >= utcNow)
                {
                    var timeDiff = checkDate - utcNow;
                    return "Sledeća isplata dostupna za " +
                         string.Format("{0} dana, {1} sati, {2} minuta",
                         timeDiff.Days, timeDiff.Hours, timeDiff.Minutes);
                }
            }

            return string.Empty;
        }

        public List<Withdrawals> GetWithdrawals(string filter, string predicate)
        {
            var withdrawals = _context.Withdrawals.ReadOnlyGetAll();

            if (string.IsNullOrWhiteSpace(filter) || string.IsNullOrWhiteSpace(predicate))
                return withdrawals.OrderByDescending(w => w.RequestDateTime).ToList();

            switch (filter.ToLower())
            {
                case "datum":
                    {
                        switch (predicate.ToLower())
                        {
                            case "danas":
                                {
                                    return withdrawals.Where(w => w.RequestDateTime.Date == DateTime.UtcNow.Date)
                                                      .OrderByDescending(w => w.RequestDateTime)
                                                      .ToList();
                                }
                            case "ova-nedelja":
                                {
                                    var startOfWeek = _dateHelper.GetStartOfWeekDate(DayOfWeek.Monday);
                                    var nextMonday = startOfWeek.AddDays(7);

                                    return withdrawals.Where(w => startOfWeek <= w.RequestDateTime && w.RequestDateTime <= nextMonday)
                                                      .OrderByDescending(w => w.RequestDateTime)
                                                      .ToList();
                                }
                            case "ovaj-mesec":
                                {
                                    var now = DateTime.UtcNow;
                                    var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
                                    var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                                    return withdrawals.Where(w => firstDayOfMonth <= w.RequestDateTime && w.RequestDateTime <= lastDayOfMonth)
                                                      .OrderByDescending(w => w.RequestDateTime)
                                                      .ToList();
                                }
                            case "ova-godina":
                                {
                                    var thisYear = DateTime.UtcNow.Year;
                                    var startOfYear = new DateTime(thisYear, 1, 1);
                                    var endOfYear = new DateTime(thisYear, 12, 31);

                                    return withdrawals.Where(w => startOfYear <= w.RequestDateTime && w.RequestDateTime <= endOfYear)
                                                      .OrderByDescending(w => w.RequestDateTime)
                                                      .ToList();
                                }
                            default: return new List<Withdrawals>();
                        }
                    }
                case "status":
                    {
                        switch (predicate.ToLower())
                        {
                            case "na-čekanju":
                                {
                                    return withdrawals.Where(w => w.Status == 0)
                                                      .OrderByDescending(w => w.RequestDateTime)
                                                      .ToList();
                                }
                            case "isplaćeno":
                                {
                                    return withdrawals.Where(w => w.Status == 1)
                                                      .OrderByDescending(w => w.RequestDateTime)
                                                      .ToList();
                                }
                            default: return new List<Withdrawals>();
                        }
                    }
                default: return new List<Withdrawals>();
            }
        }

        public List<Withdrawals> GetTherapistWithdrawals(string therapistId, string filter, string predicate)
        {
            return !string.IsNullOrWhiteSpace(therapistId) ? GetWithdrawals(filter, predicate)
                        .Where(w => w.TherapistId == therapistId)
                        .OrderByDescending(w => w.RequestDateTime)
                        .ToList() : new List<Withdrawals>();
        }
    }
}