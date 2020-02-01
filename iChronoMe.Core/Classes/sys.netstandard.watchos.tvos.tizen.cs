using System;
using System.Collections.Generic;
using System.Text;

namespace iChronoMe.Core.Classes
{
    public static partial class sys
    {
        private static void PlatformInit()
        {
            Init(OsType.Undefined);
        }

        public static void NotifyCalendarEventsUpdated()
        {

        }
    }
}