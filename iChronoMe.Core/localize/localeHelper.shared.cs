namespace iChronoMe.Core
{
    public static class localeHelper
    {
        public static string GetTimeTypeText(TimeType tt)
        {
            switch (tt)
            {
                case TimeType.RealSunTime:
                    return localize.TimeType_RealSunTime;

                case TimeType.MiddleSunTime:
                    return localize.TimeType_MiddleSunTime;

                case TimeType.TimeZoneTime:
                    return localize.TimeType_TimeZoneTime;
            }
            return tt.ToString();
        }
    }
}
