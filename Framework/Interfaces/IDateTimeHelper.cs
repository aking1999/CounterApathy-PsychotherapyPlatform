using System;

namespace Framework.Interfaces
{
    public interface IDateTimeHelper
    {
        DateTime ConvertDateTimeFromUtcToLocal(DateTime dateTime);
        string ConvertDateTimeFromUtcToLocalString(DateTime utcDateTime);
        string ConvertDateTimeFromUtcToLocalDateString(DateTime utcDateTime);
        string ConvertDateTimeFromUtcToLocalTimeString(DateTime utcDateTime);
        DateTime ConvertDateTimeFromLocalToUtc(DateTime localDateTime);
        DateTime ConvertDateFromUtcToLocal(DateTime utcDate);
        string ConvertDateFromUtcToLocalString(DateTime utcDate);
        TimeSpan ConvertTimeFromUtcToLocal(DateTime utcTime);
        string ConvertTimeFromUtcToLocalString(DateTime utcTime);
        DateTime GetStartOfWeekDate(DayOfWeek startOfWeek);
        string DateTimeStringFromDateTime(DateTime dateTime);
        string DateStringFromDateTime(DateTime dateTime);
        string TimeStringFromDateTime(DateTime dateTime);
        string ConvertDateTimeToDateTimeStringISO(DateTime dateTime);
    }
}
