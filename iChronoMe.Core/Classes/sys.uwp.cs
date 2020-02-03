namespace iChronoMe.Core.Classes
{
    public static partial class sys
    {
        private static void PlatformInit()
        {
            Init(OsType.Windows);
        }

        public static void NotifyCalendarEventsUpdated()
        {

        }
    }
}