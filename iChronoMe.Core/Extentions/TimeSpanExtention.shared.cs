using System;

namespace iChronoMe.Core.Types
{
    public static class TimeSpanExtention
    {
        public static string ToShortString(this TimeSpan ts)
        {
            ts = ts.ToPositive();
            int iMins = (int)ts.TotalMinutes;
            int iSecs = ts.Seconds;
            return string.Format("{0:D2}:{1:D2}", iMins, iSecs);

            string cFormat = @"mm\:ss";
            if (ts.TotalHours >= 1)
                cFormat = @"h\:mm\:ss";
            return ts.ToString(cFormat);
        }
        public static string ToDynamicString(this TimeSpan ts)
        {
            ts = ts.ToPositive();
            if (ts.TotalHours >= 1)
                return ts.ToString(@"h\:mm\:ss");

            int iMins = (int)ts.TotalMinutes;
            int iSecs = ts.Seconds;

            if (iMins > 0)
                return string.Format("{0:D2}:{1:D2}", iMins, iSecs);

            if (iSecs >= 10)
                return string.Format("{0:D2}sec", iSecs);

            if (iSecs >= 1)
                return string.Format("{0:D2}.{1:D3}sec", iSecs, ts.Milliseconds);

            return string.Format("{0:D3}ms", ts.Milliseconds);
        }

        public static TimeSpan ToPositive(this TimeSpan value)
        {
            long ticks = value.Ticks;
            if (ticks < 0)
                ticks *= -1;
            return TimeSpan.FromTicks(ticks);
        }
    }
}
