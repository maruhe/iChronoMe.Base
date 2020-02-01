using System;
using System.Collections.Generic;
using System.Text;
using iChronoMe.Core;
using iChronoMe.Core.Classes;
using SQLite;

namespace iChronoMe.DeviceCalendar
{
    public class CalendarEventExtention : dbObject
    {
        public static CalendarEventExtention GetExtention(string lEventID, bool bCreateIfNotExists = true)
        {
            if (string.IsNullOrEmpty(lEventID))
                return null;

            var cache = db.dbCalendarExtention.Query<CalendarEventExtention>("select * from CalendarEventExtention where EventID = ?", lEventID);
            if (cache.Count > 0)
                return cache[0];

            if (bCreateIfNotExists)
                return new CalendarEventExtention() { EventID = lEventID, TimeType = TimeType.TimeZoneTime };
            return null;
        }

        public CalendarEventExtention()
        {

        }

        [Indexed]
        public string EventID { get; set; } = "";

        [Indexed]
        public string LocationString { get; set; } = "";

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public bool GotCorrectPosition { get; set; } = false;

        public DateTime CalendarTimeStart { get; set; }
        public DateTime CalendarTimeEnd { get; set; }

        public TimeType TimeType { get; set; } = AppConfigHolder.MainConfig.DefaultTimeType;
        public DateTime TimeTypeStart { get; set; }
        public DateTime TimeTypeEnd { get; set; }

        public bool UseTypedTime { get; set; }

    }
}
