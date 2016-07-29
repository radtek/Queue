using System;

namespace Engine.Cloud.Core.Utils
{
    public static class TimeHelper
    {
        public static string ConvertDateToUnixDate(DateTime date)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var diff = DateTime.Now - origin;
            return Math.Floor(diff.TotalSeconds).ToString();
        }

        public static DateTime ParseTimestamp(object date)
        {
            DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            long unixTimeStampInTicks = (long)(Convert.ToDouble(date) * TimeSpan.TicksPerSecond);

            return new DateTime(unixStart.Ticks + unixTimeStampInTicks);
        }
    }
}
