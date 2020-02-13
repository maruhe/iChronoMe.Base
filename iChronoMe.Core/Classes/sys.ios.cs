using System;

namespace iChronoMe.Core.Classes
{
    public static partial class sys
    {
        private static void PlatformInit()
        {
            Init(OsType.iOS);
        }

        public static void NotifyCalendarEventsUpdated()
        {

        }

        public static void AfterExceptionLog(Exception ex, bool bTryShowUser, string cLogFilePath)
        {

        }

        public static string ConvertTimeZoneToSystem(string cTimeZoneID)
            => cTimeZoneID;
    }
}