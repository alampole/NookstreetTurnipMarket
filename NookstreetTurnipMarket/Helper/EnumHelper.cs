using System;

namespace NookstreetTurnipMarket.Helper
{
    public enum Day
    {
        Sunday,
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday,
        Unknown
    }

    public enum DayPeriod
    {
        AM,
        PM,
        Unknown
    }

    class EnumHelper
    {
        public static Day StringToDay(string aInput)
        {
            Day result;

            if (Enum.TryParse(aInput, true, out result))
            {
                return result;
            }

            return Day.Unknown;
        }

        public static DayPeriod StringToDayPeriod(string aInput)
        {
            DayPeriod result;

            if (Enum.TryParse(aInput, true, out result))
            {
                return result;
            }

            return DayPeriod.Unknown;
        }
    }
}
