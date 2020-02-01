using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace iChronoMe.DeviceCalendar
{
    public static partial class DeviceCalendar
    {

        public static async Task<Calendar> GetDefaultCalendar()
        {
            Calendar res = null;
            try
            {

                var calendars = new List<Calendar>(await GetCalendarsAsync());
                foreach (var calendar in calendars)
                {
                    if (calendar.IsPrimary)
                        return calendar;

                    if (res == null && calendar.CanEditEvents)
                        res = calendar;
                }
            }
            catch { }
            return res;
        }
    }
}
