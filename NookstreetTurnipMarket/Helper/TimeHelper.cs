using System;
using System.Collections.Generic;
using System.Text;

namespace NookstreetTurnipMarket.Helper
{
    class TimeHelper
    {
        public static string SanitizeTime(string aTime, out int aHour, out int aMinutes)
        {
            int hours = 0;
            int minutes = 0;

            try
            {
                hours = Int32.Parse(aTime.Substring(0, aTime.IndexOf(':')));
                minutes = Int32.Parse(aTime.Substring(aTime.IndexOf(':') + 1, 2));
            }
            catch
            {
                aHour = 0;
                aMinutes = 0;
                return "Unable to parse time. Example of time format: 11:11PM.";
            }

            if (hours > 12)
            {
                aHour = 0;
                aMinutes = 0;
                return "Max hours is 12. Example of time format: 11:11PM.";
            }

            if (hours < 1)
            {
                aHour = 0;
                aMinutes = 0;
                return "Use 12 instead of 0 for hours. Example of time format: 11:11PM.";
            }

            if (minutes > 59)
            {
                aHour = 0;
                aMinutes = 0;
                return "Max minutes in an hour is 59. Example of time format: 11:11PM.";
            }

            if (minutes < 0)
            {
                aHour = 0;
                aMinutes = 0;
                return "Minimum minutes is 0. Example of time format: 11:11PM.";
            }

            aHour = hours;
            aMinutes = minutes;
            return string.Empty;
        }

        public static string ConvertDateTime(DateTime aTime)
        {
            return aTime.ToString("yyyy-MM-dd HH:mm");
        }
    }
}
