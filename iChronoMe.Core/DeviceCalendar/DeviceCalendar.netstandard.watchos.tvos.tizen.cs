using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace iChronoMe.DeviceCalendar
{
    public static partial class DeviceCalendar
    {
        public static Task<IList<Calendar>> GetCalendarsAsync() { throw new NotImplementedException(); }
        public static Task<Calendar> GetCalendarByIdAsync(string externalId) { throw new NotImplementedException(); }
        public static Task<IList<CalendarEvent>> GetEventsAsync(Calendar calendar, DateTime start, DateTime end) { throw new NotImplementedException(); }
        public static Task<CalendarEvent> GetEventByIdAsync(string externalId) { throw new NotImplementedException(); }
        public static Task AddOrUpdateCalendarAsync(Calendar calendar) { throw new NotImplementedException(); }
        public static Task AddOrUpdateEventAsync(Calendar calendar, CalendarEvent calendarEvent) { throw new NotImplementedException(); }
        public static Task<bool> DeleteCalendarAsync(Calendar calendar) { throw new NotImplementedException(); }
        public static Task<bool> DeleteEventAsync(Calendar calendar, CalendarEvent calendarEvent) { throw new NotImplementedException(); }
    }
}
