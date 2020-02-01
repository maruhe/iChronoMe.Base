﻿using System;
using EventKit;

namespace iChronoMe.DeviceCalendar
{
    public static class EKAlarmExtensions
    {
        public static CalendarEventReminder ToCalendarEventReminder(this EKAlarm alarm)
        {
            return new CalendarEventReminder
            {
                // iOS stores in negative seconds before the event, but CalendarEventReminder.TimeBefore uses
                // a positive TimeSpan before the event
                TimeBefore = -TimeSpan.FromSeconds(alarm.RelativeOffset)
            };
        }
    }
}