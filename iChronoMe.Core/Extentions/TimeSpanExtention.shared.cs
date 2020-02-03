using System;

namespace iChronoMe.Core.Types
{
    public static class TimeSpanExtention
    {
        public static string ToShortString(this TimeSpan ts)
        {
            string cFormat = @"mm\:ss";
            if (ts.TotalHours >= 1)
                cFormat = @"h\:mm\:ss";
            return ts.ToString(cFormat);
        }
    }
}
