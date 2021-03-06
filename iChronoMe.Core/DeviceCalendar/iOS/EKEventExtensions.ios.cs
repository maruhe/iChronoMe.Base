using System.Linq;

using EventKit;

namespace iChronoMe.DeviceCalendar
{
    /// <summary>
    /// iOS EKEvent extensions
    /// </summary>
    internal static class EKEventExtensions
    {
        /// <summary>
        /// Creates a new Calendars.Plugin.Abstractions.CalendarEvent from an EKEvent
        /// </summary>
        /// <param name="ekEvent">Source EKEvent</param>
        /// <returns>Corresponding Calendars.Plugin.Abstraction.CalendarEvent</returns>
        public static CalendarEvent ToCalendarEvent(this EKEvent ekEvent)
        {
            string cClr = ColorConversion.ToHexColor(ekEvent.Calendar.CGColor);

            return new CalendarEvent
            {
                Title = ekEvent.Title,
                Description = ekEvent.Notes,
                Start = ekEvent.StartDate.ToDateTime(),
                EventColorString = cClr,
                CalendarColorString = cClr,
                DisplayColorString = cClr,



                // EventKit treats a one-day AllDay event as starting/ending on the same day,
                // but WinPhone/Android (and thus Calendars.Plugin) define it as ending on the following day.
                //
                End = ekEvent.EndDate.ToDateTime().AddSeconds(ekEvent.AllDay ? 1 : 0),
                AllDay = ekEvent.AllDay,
                Location = ekEvent.Location,
                ExternalID = ekEvent.EventIdentifier,
                Reminders = ekEvent.Alarms?.Select(alarm => alarm.ToCalendarEventReminder()).ToList()
            };
        }
    }
}