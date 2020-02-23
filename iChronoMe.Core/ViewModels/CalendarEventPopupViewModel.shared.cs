using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using iChronoMe.Core.Classes;
using iChronoMe.Core.DataBinding;
using iChronoMe.Core.DynamicCalendar;
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
        public LocationTimeHolder LocationTimeHolder { get => locationTimeHolder; }

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
                    if (extEvent.GotCorrectPosition)
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
        
        private void UpdateTimes()
        {

            if (calEvent.AllDay && (calEvent.Start.TimeOfDay.TotalHours != 0 || calEvent.End.TimeOfDay.TotalHours != 0))
            {
                calEvent.Start = calEvent.Start.Date;
                calEvent.End = calEvent.End.Date.AddDays(1);
            }

            calEvent.DisplayStart = locationTimeHolder.GetTime(TimeType, calEvent.Start, TimeType.TimeZoneTime);
            calEvent.DisplayEnd = locationTimeHolder.GetTime(TimeType, calEvent.End, TimeType.TimeZoneTime);

            extEvent.TimeTypeStart = calEvent.DisplayStart;
            extEvent.TimeTypeEnd = calEvent.DisplayEnd;
            extEvent.UseTypedTime = TimeType == TimeType.MiddleSunTime || TimeType == TimeType.RealSunTime;
            
            extEvent.CalendarTimeStart = calEvent.Start;
            extEvent.CalendarTimeEnd = calEvent.End;

            OnPropertyChanged(nameof(Start));
            OnPropertyChanged(nameof(End));
            OnPropertyChanged(nameof(DisplayStart));
            OnPropertyChanged(nameof(DisplayEnd));
            OnPropertyChanged(nameof(DisplayStartDate));
            OnPropertyChanged(nameof(DisplayStartTime));
            OnPropertyChanged(nameof(DisplayEndDate));
            OnPropertyChanged(nameof(DisplayEndTime));

            OnPropertyChanged(nameof(StartTimeHelper));
            OnPropertyChanged(nameof(EndTimeHelper));

            OnPropertyChanged(nameof(ShowTimeHelpers));

        }

        public DateTime Start
        {
            get => calEvent.Start;
            set
            {
                calEvent.End = value + (calEvent.End - calEvent.Start);
                calEvent.Start = value;
                UpdateTimes();
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
                UpdateTimes();
            }
        }

        public DateTime DisplayStart
        {
            get => calEvent.DisplayStart == DateTime.MinValue ? calEvent.Start : calEvent.DisplayStart;
            set
            {
                calEvent.DisplayStart = value;
                Start = sys.GetTimeWithoutSeconds(locationTimeHolder.GetTime(TimeType.TimeZoneTime, value, TimeType));
            }
        }
        public DateTime DisplayEnd
        {
            get => calEvent.DisplayEnd == DateTime.MinValue ? calEvent.End : calEvent.DisplayEnd;
            set
            {
                calEvent.DisplayEnd = value;
                End = sys.GetTimeWithoutSeconds(locationTimeHolder.GetTime(TimeType.TimeZoneTime, value, TimeType));
            }
        }

        public DateTime DisplayStartDate { get => DisplayStart.Date; set { DisplayStart = value.Date + DisplayStart.TimeOfDay; } }
        public DateTime DisplayEndDate { get => DisplayEnd.Date; set { DisplayEnd = value.Date + DisplayEnd.TimeOfDay; } }
        public TimeSpan DisplayStartTime { get => DisplayStart.TimeOfDay; set { DisplayStart = DisplayStart.Date + value; } }
        public TimeSpan DisplayEndTime { get => DisplayEnd.TimeOfDay; set { DisplayEnd = DisplayEnd.Date + value; } }

        public string Location { get => calEvent.Location; set { UpdateLocation(value); } }        

        public bool AllDay { get => calEvent.AllDay; set { calEvent.AllDay = value; OnPropertyChanged(); OnPropertyChanged(nameof(NotAllDay)); OnPropertyChanged(nameof(ShowTimeHelpers)); } }

        public string Description { get => calEvent.Description; set { calEvent.Description = value; OnPropertyChanged(); } }
        public IList<CalendarEventReminder> Reminders { get => calEvent.Reminders; }

        public string ExternalID { get => calEvent.ExternalID; }

        #endregion

        #region Extention-Properties

        public TimeType TimeType { get => extEvent.TimeType; set { extEvent.TimeType = value; OnPropertyChanged(); OnPropertyChanged(nameof(TimeTypeSpinnerPos)); UpdateTimes(); } }

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

        bool _isSearchingForLocation = false;
        public bool IsSearchingForLocation { get => _isSearchingForLocation; set { _isSearchingForLocation = value;  OnPropertyChanged(); OnPropertyChanged(nameof(LocationHelper)); OnPropertyChanged(nameof(LocationTimeInfo)); } }

        public void UpdateLocation(string cLocationTilte, double nLat = 0, double nLng = 0)
        {
            calEvent.Location = cLocationTilte;
            
            if (string.IsNullOrEmpty(cLocationTilte) || (nLat != 0 && nLng != 0))
            {
                extEvent.LocationString = cLocationTilte;
                extEvent.Latitude = nLat;
                extEvent.Longitude = nLng;
                extEvent.GotCorrectPosition = true;
                locationTimeHolder = LocationTimeHolder.LocalInstanceClone;
                if (nLat != 0 && nLng != 0)
                    locationTimeHolder.ChangePositionDelay(nLat, nLng);
            }
            else
            {
                SearchPositionByLocation();
            }
            
            OnPropertyChanged(nameof(Location));
            OnPropertyChanged(nameof(LocationHelper));
            OnPropertyChanged(nameof(LocationTimeInfo));
            UpdateTimes();
        }

        Task tskPositionSearcher = null;
        DateTime tLastLocationChange = DateTime.MinValue;
        DateTime tLastPositionSearch = DateTime.MinValue;
        private void SearchPositionByLocation()
        {
            tLastLocationChange = DateTime.Now;
            if (tskPositionSearcher != null)
                return;
            
            IsSearchingForLocation = true;
            extEvent.LocationString = "-";
            extEvent.Latitude = 0;
            extEvent.Longitude = 0;
            extEvent.GotCorrectPosition = false;
            tskPositionSearcher = Task.Factory.StartNew(() =>
            {
                DateTime tSearchStart = DateTime.Now;
                DateTime tMaxWait = tSearchStart.AddSeconds(5);
                try
                {
                    int iSame = 0;
                    string cLoc = calEvent.Location;
                    int iWordCount = cLoc.Split(' ').Length;
                    while (tMaxWait > DateTime.Now)
                    {
                        Task.Delay(250).Wait();
                        if (string.IsNullOrEmpty(calEvent.Location))
                            break;
                        if (cLoc.Equals(calEvent.Location))
                        {
                            iSame++;
                            if (iSame >= 4)
                            {
                                mUserIO?.ShowToast("a second without change => go");
                                break;
                            }
                        }
                        else
                        {
                            if (iWordCount != calEvent.Location.Split(' ').Length)
                            {
                                mUserIO?.ShowToast("found new word => go");
                                break;
                            }
                        }
                        cLoc = calEvent.Location;
                    }
                    tSearchStart = DateTime.Now;
                    if (!string.IsNullOrEmpty(calEvent.Location))
                        EventCollection.UpdateEventLocationPosition(calEvent, extEvent);
                    if (tLastLocationChange > tSearchStart)
                        return;
                    OnPropertyChanged(nameof(Location));
                    OnPropertyChanged(nameof(ShowTimeHelpers));
                } 
                catch (Exception ex)
                {
                    xLog.Error(ex);
                }
                finally
                {
                    IsSearchingForLocation = false;
                    UpdateTimes();

                    tskPositionSearcher = null;
                    if (tLastLocationChange > tSearchStart)
                    {
                        SearchPositionByLocation();
                    }
                }
            });
        }

        public string LocationHelper
        {
            get
            {
                if (_isSearchingForLocation)
                    return "_isSearchingForLocation...";
                if (string.IsNullOrEmpty(Location))
                    return "";
                if (extEvent.GotCorrectPosition)
                    return sys.DezimalGradToGrad(extEvent.Latitude, extEvent.Longitude) + ", " + extEvent.ConfirmedAddress;
                return "unknown location!";
            }
        }

        public string LocationTimeInfo
        {
            get
            {
                string cPosInfo = (extEvent.LocationString.Equals(calEvent.Location) ? "Position und Ortszeit unklar: " : "Position wird ermittelt: ");
                if (extEvent.GotCorrectPosition)
                {
                    TimeSpan tsDiff = LocationTimeHolder.GetUTCGeoLngDiff(extEvent.Longitude - sys.lastUserLocation.Longitude);
                    string cDiffDirection = tsDiff.TotalMilliseconds > 0 ? "+" : "-";
                    if (tsDiff.TotalMilliseconds < 0)
                        tsDiff = TimeSpan.FromMilliseconds(tsDiff.TotalMilliseconds * -1);

                    cPosInfo = cDiffDirection;
                    if (tsDiff.TotalMilliseconds < 0)
                        tsDiff = TimeSpan.FromMilliseconds(tsDiff.TotalMilliseconds * -1);

                    if (tsDiff.TotalHours > 1)
                        cPosInfo += ((int)tsDiff.TotalHours).ToString() + ":" + tsDiff.Minutes.ToString("00") + "h";
                    else if (tsDiff.TotalMinutes > 1)
                        cPosInfo += ((int)tsDiff.TotalHours).ToString() + ":" + tsDiff.Minutes.ToString("00") + "h";
                    else if (tsDiff.TotalSeconds > 3)
                        cPosInfo += tsDiff.Seconds.ToString("00") + "sec";
                    else
                        cPosInfo += ":-)";
                    cPosInfo = cPosInfo.Trim();
                }
                return cPosInfo;
            }
        }

        public bool ShowTimeHelpers { get => !AllDay && TimeType != TimeType.TimeZoneTime; }

        public bool ShowLocationHelper { get => !string.IsNullOrEmpty(Location); }

        public int TimeTypeSpinnerPos
        {
            get
            {
                switch (TimeType) { case TimeType.TimeZoneTime: return 2; case TimeType.MiddleSunTime: return 1; default: return 0; }
            }
            set
            {
                switch (value)
                {
                    case 2:
                        TimeType = TimeType.TimeZoneTime;
                        break;
                    case 1:
                        TimeType = TimeType.MiddleSunTime;
                        break;
                    default:
                        TimeType = TimeType.RealSunTime;
                        break;
                }                
            }
        }

        public bool NotAllDay { get => !calEvent.AllDay; }

        public bool IsReady => bIsReady;

        public bool HasErrors => false;

        #endregion
    }
}
