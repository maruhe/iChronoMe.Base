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
    }
}