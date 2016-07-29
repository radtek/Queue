using System;

namespace Utils
{
    public static class DateTimeExtentions
    {
        public static bool IsWeekend(this DateTime self)
        {
            return (self.DayOfWeek == DayOfWeek.Sunday || self.DayOfWeek == DayOfWeek.Saturday);
        }

        public static bool IsLeapYear(this DateTime self)
        {
            return DateTime.DaysInMonth(self.Year, 2) == 29;
        }

        public static int Age(this DateTime self)
        {
            return Age(self, DateTime.Today);
        }

        public static int Age(this DateTime self, DateTime laterDate)
        {
            int age;
            age = laterDate.Year - self.Year;
            if (age > 0)
                age -= Convert.ToInt32(laterDate.Date < self.Date.AddYears(age));
            else
                age = 0;

            return age;
        }

        public static long GetJavascriptTimestamp(this DateTime input)
        {
            long DatetimeMinTimeTicks = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks;

            return (long)((input.ToUniversalTime().Ticks - DatetimeMinTimeTicks) / 10000);
        }
    }
}
