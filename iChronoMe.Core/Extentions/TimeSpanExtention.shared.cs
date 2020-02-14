using System;

namespace iChronoMe.Core.Types
{
    public static class TimeSpanExtention
    {
        public static string ToShortString(this TimeSpan ts)
        {
            int iMins = (int)ts.TotalMinutes;
            if (iMins < 0)
                iMins = iMins * -1;
            int iSecs = ts.Seconds;
            if (iSecs < 0)
                iSecs = iSecs * -1;
            return string.Format("{0:D2}:{1:D2}", iMins, iSecs);

            string cFormat = @"mm\:ss";
            if (ts.TotalHours >= 1)
                cFormat = @"h\:mm\:ss";
            return ts.ToString(cFormat);
        }
    }
}
