using System;
using System.Collections.Generic;
using iChronoMe.Core.DataBinding;
using iChronoMe.Core.Types;

namespace iChronoMe.DeviceCalendar
{
    /// <summary>
    /// Device calendar event/appointment abstraction
    /// </summary>
    public partial class CalendarEvent : BaseObservable
    {
        /// <summary>
        /// Calendar event name/title/subject
        /// </summary>
        public string Title { get; set; }

        public string CalendarId { get; set; }

        /// <summary>
        /// Event display color, as a string in hex notation
        /// </summary>
        /// <remarks>Cannot be changed on WinPhone</remarks>
        /// 

        public xColor CalendarColor { get => xColor.FromHex(DisplayColorString); }
        public xColor DisplayColor { get => xColor.FromHex(DisplayColorString); }
        public xColor EventColor { get => xColor.FromHex(EventColorString); set => DisplayColorString = EventColorString = value.ToHex(); }
        public string CalendarColorString { get; set; } = "#FF00FF00";
        public string DisplayColorString { get; set; } = "#FF00FF00";
        public string EventColorString { get; set; } = "#FF00FF00";


        DateTime? _SortTime = null;
        public DateTime SortTime { get => _SortTime ?? Start; set => _SortTime = value; }

        /// <summary>
        /// Event start time
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// Event end time
        /// </summary>
        public DateTime End { get; set; }

        public DateTime DisplayStart { get; set; } = DateTime.MinValue;

        public DateTime DisplayEnd { get; set; }

        /// <summary>
        /// Gets or sets the location of the event
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Whether or not this is an "all-day" event.
        /// </summary>
        /// <remarks>All-day events end at midnight of the following day</remarks>
        public bool AllDay { get; set; }

        /// <summary>
        /// Optional event description/details
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Event reminders
        /// </summary>
        /// <remarks>Windows only supports a single reminder</remarks>
        public IList<CalendarEventReminder> Reminders { get; set; }

        /// <summary>
        /// Platform-specific unique calendar event identifier
        /// </summary>
        /// <remarks>This ID will be the same for each instance of a recurring event.</remarks>
        public string ExternalID { get; set; }

        public int AccessLevel { get; set; }

        /// <summary>
        /// Simple ToString helper, to assist with debugging.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Name=" + Title + ", AllDay=" + AllDay + ", Start=" + Start + ", End=" + End + ", Location=" + Location;
        }

        private CalendarEventExtention _extention = null;
        public CalendarEventExtention Extention
        {
            get
            {
                if (_extention == null)
                    _extention = CalendarEventExtention.GetExtention(ExternalID, true);
                return _extention;
            }
        }

    }
}
