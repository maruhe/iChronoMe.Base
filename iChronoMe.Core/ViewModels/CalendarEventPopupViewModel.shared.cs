using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using iChronoMe.Core.Classes;
using iChronoMe.Core.DataBinding;
using iChronoMe.Core.Interfaces;
using iChronoMe.Core.Types;
using iChronoMe.DeviceCalendar;

namespace iChronoMe.Core.ViewModels
{
    public partial class CalendarEventPopupViewModel : BaseObservable, ICanBeReady
    {
        private string cEventID;
        private bool bIsReady = false;
        Calendar cal;
        CalendarEvent calEvent;
        CalendarEventExtention extEvent;
        private static string cLoading = "loading...";
        IProgressChangedHandler mUserIO;
        LocationTimeHolder locationTimeHolder = LocationTimeHolder.LocalInstanceClone;

        public CalendarEventPopupViewModel(string eventID, IProgressChangedHandler userIO)
        {
            cEventID = eventID;
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    calEvent = await DeviceCalendar.DeviceCalendar.GetEventByIdAsync(cEventID);
                    cal = await DeviceCalendar.DeviceCalendar.GetCalendarByIdAsync(cEventID);
                    extEvent = calEvent.Extention;
                    locationTimeHolder.ChangePositionDelay(extEvent.Latitude, extEvent.Longitude);
                    bIsReady = true;
                }
                catch (Exception ex)
                {
                    bIsReady = false;
                    //bHasErrors = true;
                    sys.LogException(ex);
                }
                OnPropertyChanged("*");
            });
        }


        #region Device-Event-Properties
        public string Title
        {
            get
            {
                if (!bIsReady)
                    return cLoading;
                return calEvent.Title;
            }
            set { calEvent.Title = value; OnPropertyChanged(); }
        }

        public string CalendarId { get => calEvent.CalendarId; }

        public xColor CalendarColor { get => calEvent.CalendarColor; }
        public xColor DisplayColor { get => calEvent.DisplayColor; set { calEvent.EventColor = value; OnPropertyChanged(); } }
        public xColor EventColor { get => calEvent.EventColor; set { calEvent.EventColor = value; OnPropertyChanged(); } }
        public string CalendarColorString { get => calEvent.CalendarColorString; }
        public string DisplayColorString { get => calEvent.DisplayColorString; }
        public string EventColorString { get => calEvent.EventColorString; }

        public DateTime SortTime { get => calEvent.SortTime; }

        public DateTime Start
        {
            get => calEvent.Start;
            set
            {
                calEvent.End = value + (calEvent.End - calEvent.Start);
                calEvent.Start = value;
                OnPropertyChanged(nameof(Start));
                OnPropertyChanged(nameof(End));
                OnPropertyChanged(nameof(StartDate));
                OnPropertyChanged(nameof(StartTime));
                OnPropertyChanged(nameof(EndTime));

                OnPropertyChanged(nameof(StartTimeHelper));
                OnPropertyChanged(nameof(EndTimeHelper));
            }
        }
        public DateTime End
        {
            get => calEvent.End;
            set
            {
                if (value <= Start)
                    calEvent.Start = value - (calEvent.End - calEvent.Start);
                calEvent.End = value;
                OnPropertyChanged(nameof(Start));
                OnPropertyChanged(nameof(End));
                OnPropertyChanged(nameof(StartDate));
                OnPropertyChanged(nameof(StartTime));
                OnPropertyChanged(nameof(EndDate));
                OnPropertyChanged(nameof(EndTime));

                OnPropertyChanged(nameof(StartTimeHelper));
                OnPropertyChanged(nameof(EndTimeHelper));
            }
        }

        public DateTime DisplayStart
        {
            get => calEvent.DisplayStart == DateTime.MinValue ? calEvent.Start : calEvent.DisplayStart;
            set
            {
                calEvent.DisplayStart = value;
            }
        }
        public DateTime DisplayEnd
        {
            get => calEvent.DisplayEnd == DateTime.MinValue ? calEvent.End : calEvent.DisplayEnd;
            set
            {
                calEvent.DisplayEnd = value;
            }
        }

        public DateTime StartDate { get => Start.Date; set { Start = value.Date + Start.TimeOfDay; } }
        public DateTime EndDate { get => End.Date; set { End = value.Date + End.TimeOfDay; } }
        public TimeSpan StartTime { get => Start.TimeOfDay; set { Start = Start.Date + value; } }
        public TimeSpan EndTime { get => End.TimeOfDay; set { End = End.Date + value; } }

        public string Location { get => calEvent.Location; set { calEvent.Location = value; OnPropertyChanged(); } }
        public bool AllDay { get => calEvent.AllDay; set { calEvent.AllDay = value; OnPropertyChanged(); } }

        public string Description { get => calEvent.Description; set { calEvent.Description = value; OnPropertyChanged(); } }
        public IList<CalendarEventReminder> Reminders { get => calEvent.Reminders; }

        public string ExternalID { get => calEvent.ExternalID; }

        #endregion

        #region Extention-Properties

        public TimeType TimeType { get => TimeType.MiddleSunTime; }

        #endregion

        #region Model-Properties

        public xColor TextColor
        {
            get
            {
                if (!bIsReady)
                    return xColor.White;
                if (calEvent.DisplayColor.Luminosity < 0.5)
                    return xColor.White;
                return xColor.Black;
            }
        }

        public string StartEndString
        {
            get
            {
                string cBis = " bis ";
                string cTime = Start.ToString("HH:mm") + cBis + End.ToString("HH:mm");
                string cAddonText = "";
                if (AllDay || End - Start > TimeSpan.FromHours(25))
                {
                    cTime = Start.ToString("HH:mm");
                    cAddonText = cBis.TrimStart();// + EndDay.WeekDayNameShort + End.ToString(". HH:mm");
                    if (AllDay)
                    {
                        cTime = "ganztägig";
                    }
                    if (End - Start > TimeSpan.FromHours(25))
                    {
                        if (AllDay)
                        {
                            cTime = "mehrtägig";
                            //cAddonText = StartDay.ToString("d.MM.") + cBis + EndDay.ToString("d.MM.");
                        }
                    }
                }
                return cTime + " " + cAddonText;// + " => " +Start.ToString("dd.MM.yy")+"-"+End.ToString("dd.MM.yy");
            }
        }

        public string StartTimeHelper
        {
            get
            {
                if (TimeType != TimeType.TimeZoneTime)
                {
                    locationTimeHolder.SetTime(calEvent.Start, TimeType);
                    return locationTimeHolder.GetTime(TimeType.TimeZoneTime).ToShortTimeString();
                }
                return "                            ";
            }
        }
        public string EndTimeHelper
        {
            get
            {
                if (TimeType != TimeType.TimeZoneTime)
                {
                    locationTimeHolder.SetTime(calEvent.End, TimeType);
                    return locationTimeHolder.GetTime(TimeType.TimeZoneTime).ToShortTimeString();
                }
                return string.Empty;
            }
        }

        public bool IsReady => bIsReady;

        public bool HasErrors => false;

        #endregion
    }
}
