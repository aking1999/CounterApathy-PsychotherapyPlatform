using Framework.Interfaces;
using System;

namespace Framework.Implementations
{
    public class DateTimeHelper : IDateTimeHelper
    {
        public DateTime ConvertDateTimeFromUtcToLocal(DateTime utcDateTime)
        {
            //local is serbian
            var serbianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");

            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, serbianTimeZone);
        }

        public string ConvertDateTimeFromUtcToLocalString(DateTime utcDateTime)
        {
            var localDateTime = ConvertDateTimeFromUtcToLocal(utcDateTime);

            try
            {
                return localDateTime.ToString("dd/MMM/yyyy HH:mm");
            }
            catch (Exception)
            {
                return "UTC " + localDateTime.ToString();
            }
        }

        public string ConvertDateTimeFromUtcToLocalDateString(DateTime utcDateTime)
        {
            var localDateTime = ConvertDateTimeFromUtcToLocal(utcDateTime);

            try
            {
                return localDateTime.ToString("dd/MMM/yyyy");
            }
            catch (Exception)
            {
                return localDateTime.ToString();
            }
        }

        public string ConvertDateTimeFromUtcToLocalTimeString(DateTime utcDateTime)
        {
            var localDateTime = ConvertDateTimeFromUtcToLocal(utcDateTime);

            try
            {
                return localDateTime.ToString("HH:mm");
            }
            catch (Exception)
            {
                return "UTC " + localDateTime.ToString();
            }
        }

        public DateTime ConvertDateTimeFromLocalToUtc(DateTime localDateTime)
        {
            var serbianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");

            return TimeZoneInfo.ConvertTimeToUtc(localDateTime, serbianTimeZone);
        }

        public DateTime ConvertDateFromUtcToLocal(DateTime utcDate)
        {
            var serbianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");

            return TimeZoneInfo.ConvertTimeFromUtc(utcDate.Date, serbianTimeZone);
        }

        public string ConvertDateFromUtcToLocalString(DateTime utcDate)
        {
            var localDate = ConvertDateFromUtcToLocal(utcDate);

            try
            {
                return localDate.ToString("dd/MMM/yyyy");
            }
            catch (Exception)
            {
                return localDate.ToString();
            }
        }

        public TimeSpan ConvertTimeFromUtcToLocal(DateTime dateTime)
        {
            var serbianTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");

            var localDatetime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, serbianTimeZone);

            return localDatetime.TimeOfDay;
        }

        public string ConvertTimeFromUtcToLocalString(DateTime dateTime)
        {
            var localTime = ConvertTimeFromUtcToLocal(dateTime);

            try
            {
                var localTimeString = localTime.ToString();
                return localTimeString.Remove(localTimeString.Length - 3, 3);
                //return string.Format("{0}:{1}", localTime.TotalHours.ToString(), localTime.Minutes.ToString());
            }
            catch (Exception)
            {
                return localTime.ToString();
            }
        }

        public DateTime GetStartOfWeekDate(DayOfWeek startOfWeek)
        {
            // !!! ovde mozda izbaci error ili ne radi tacno racunanje.
            // ako tako bude, skloniti ConvertDateTimeFromUtcToLocal
            var localDateTime = ConvertDateTimeFromUtcToLocal(DateTime.UtcNow);
            int diff = (7 + (localDateTime.DayOfWeek - startOfWeek)) % 7;
            return localDateTime.AddDays(-1 * diff).Date;
        }

        public string DateTimeStringFromDateTime(DateTime dateTime)
        {
            try
            {
                return dateTime.ToString("dd/MMM/yyyy HH:mm");
            }
            catch (Exception)
            {
                return dateTime.ToString();
            }
        }

        public string DateStringFromDateTime(DateTime dateTime)
        {
            try
            {
                return dateTime.Date.ToString("dd/MMM/yyyy");
            }
            catch (Exception)
            {
                return dateTime.Date.ToString();
            }
        }

        public string TimeStringFromDateTime(DateTime dateTime)
        {
            try
            {
                return dateTime.ToString("HH:mm");
            }
            catch (Exception)
            {
                return dateTime.ToString();
            }
        }

        public string ConvertDateTimeToDateTimeStringISO(DateTime dateTime)
        {
            return dateTime.ToString("o");
        }
    }
}