using System.Collections.Generic;
using System.Threading.Tasks;
using iChronoMe.Core.Types;

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

        public static Task CreateDefaulCalendar()
        {
            return AddOrUpdateCalendarAsync(new Calendar { Name = localize.AppName, IsPrimary = true, CanEditCalendar = true, CanEditEvents = true, Color = xColor.MaterialTeal.HexString });
        }

        public async static Task<IList<Calendar>> GetEditableCalendarsAsync()
        {
            List<Calendar> res = new List<Calendar>();

            foreach (var cal in await GetCalendarsAsync())
            {
                if (cal.CanEditEvents)
                    res.Add(cal);
            }

            return res;
        }
    }
}
