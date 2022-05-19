using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Framework.Helpers
{
    public static class Helper
    {
        public static string GetFullDateInfo(string dateTime)
        {
            try
            {
                var dateOnly = dateTime.Split(' ');

                DateTime dt = new DateTime();

                if (DateTime.TryParseExact(dateOnly[0], "MM/d/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                {
                    DateTime date = new DateTime(dt.Year, dt.Month, dt.Day);

                    return date.DayOfWeek + ", " + date.Day + " " + date.ToString("MMMM") + " " + date.Year;
                }

                return dateTime;
            }
            catch (Exception e)
            {
                return dateTime;
            }
        }

        public static string ParseDateForJavascriptCountdown(string dateTime)
        {
            try
            {
                DateTime dt = new DateTime();

                if (DateTime.TryParseExact(dateTime, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                {
                    DateTime date = new DateTime(dt.Year, dt.Month, dt.Day);

                    return date.ToString("MMMM") + " " + date.Day + ", " + date.Year;
                }

                return dateTime;
            }
            catch (Exception e)
            {
                return dateTime;
            }
        }

        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }

        public static string ToString(this DateTime? dt, string format)
            => dt == null ? "n/a" : ((DateTime)dt).ToString(format);

        //// !!! This presumes that weeks start with Monday.
        //// Week 1 is the 1st week of the year with a Thursday in it.
        //public static int GetIso8601WeekOfYear(this DateTime time)
        //{
        //    // If its Monday, Tuesday or Wednesday, then it'll 
        //    // be the same week# as whatever Thursday, Friday or Saturday are,
        //    // and we always get those right
        //    DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
        //    if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
        //    {
        //        time = time.AddDays(3);
        //    }

        //    return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        //}

        public static string[] GetContactMethodIconAndNameAndColor(string iconAndNameAndColorConcatenated)
        {
            try
            {
                if (!string.IsNullOrEmpty(iconAndNameAndColorConcatenated.Trim()))
                {
                    //niz[0] -> fa icon
                    //niz[1] -> name
                    //niz[2] -> color
                    return iconAndNameAndColorConcatenated.Split('|');
                }
                return new string[] { "", "", "" };
            }
            catch (Exception)
            {
                return new string[] { "", "", "" };
            }
        }

        // !!! ovu metodu ne koristim nigde ali9 je cuvam jer mi mozda iz nje bude trebalo nesto !!!
        //public static string GetWwwRootFilePath(IWebHostEnvironment environment, string iconName)
        //{
        //    //if(string.IsNullOrEmpty())
        //    //var favicon = Path.Combine(environment.WebRootPath, iconName);

        //    //// true
        //    //var exists = File.Exists(favicon);
        //    //// false
        //    //var exists2 = File.Exists(favicon2);

        //    try
        //    {
        //        if (!string.IsNullOrEmpty(iconName.Trim()))
        //        {
        //            //var iconWithPath = Path.Combine(environment.WebRootPath, iconName);

        //            var iconWithPath = environment.ContentRootPath + @"\wwwroot\images\content-images\" + iconName;

        //            if (File.Exists(iconWithPath))
        //                return iconWithPath;
        //        }

        //        return "far fa-address-book";
        //    }
        //    catch (Exception)
        //    {
        //        return "far fa-address-book";
        //    }
        //}
    }
}
