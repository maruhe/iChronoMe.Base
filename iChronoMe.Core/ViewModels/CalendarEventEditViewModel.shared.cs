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
    public partial class CalendarEventEditViewModel : BaseObservable, ICanBeReady
    {
        private Task<bool> tskReady { get { return tcsReady == null ? Task.FromResult(false) : tcsReady.Task; } }
        private TaskCompletionSource<bool> tcsReady = null;

        private string cEventID;
        private bool bIsReady = false;
        Calendar cal;
        CalendarEvent calEvent;
        CalendarEventExtention extEvent;
        private static string cLoading = "loading...";
        IProgressChangedHandler mUserIO;
        LocationTimeHolder locationTimeHolder;
        public LocationTimeHolder LocationTimeHolder { get => locationTimeHolder; }

        public CalendarEventEditViewModel(string eventID, IProgressChangedHandler userIO)
        {
            cEventID = eventID;
            ResetLocationTimeHolder();
            tcsReady = new TaskCompletionSource<bool>();

            Task.Factory.StartNew(async () =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(cEventID))
                    {
                        try
                        {
                            calEvent = await DeviceCalendar.DeviceCalendar.GetEventByIdAsync(cEventID);
                            if (calEvent == null)
                                userIO?.ShowToast("event not found: " + cEventID);
                        }
                        catch (Exception ex)
                        {
                            userIO?.ShowToast("error loading event: " + ex.Message);
                        }
                    }
                    if (calEvent == null)
                    {
                        calEvent = new CalendarEvent();
                        calEvent.DisplayStart = DateTime.Today.AddHours(DateTime.Now.Hour + 1);
                        if (calEvent.DisplayStart.Hour > 18)
                            calEvent.DisplayStart = DateTime.Today.AddDays(1).AddHours(10);
                        calEvent.DisplayEnd = calEvent.DisplayStart.AddHours(2);
                        cal = await DeviceCalendar.DeviceCalendar.GetDefaultCalendar();
                    }
                    else
                        cal = await DeviceCalendar.DeviceCalendar.GetCalendarByIdAsync(calEvent.CalendarId);
                    extEvent = calEvent.Extention;
                    if (extEvent.GotCorrectPosition)
                        locationTimeHolder.ChangePositionDelay(extEvent.Latitude, extEvent.Longitude);
                    if (calEvent.Start == DateTime.MinValue)
                        UpdateTimes();
                    if (!string.IsNullOrEmpty(calEvent.ExternalID))
                        EventCollection.UpdateEventDisplayTime(calEvent, calEvent.Extention, locationTimeHolder, extEvent.TimeType, cal);
                    bIsReady = true;
                    tcsReady.TrySetResult(true);
                    Ready?.Invoke(this, new EventArgs());
                }
                catch (Exception ex)
                {
                    bIsReady = false;
                    tcsReady.TrySetResult(false);
                    //bHasErrors = true;
                    sys.LogException(ex);
                }
                OnPropertyChanged("*");
            });
        }

        public async Task<bool> WaitForReady()
        {
            return await tskReady;
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
        public xColor DisplayColor { get => calEvent.DisplayColor; set { calEvent.EventColor = value; OnPropertyChanged(nameof(DisplayColor)); OnPropertyChanged(nameof(EventColor)); } }
        public xColor EventColor { get => calEvent.EventColor; set { calEvent.EventColor = value; OnPropertyChanged(nameof(DisplayColor)); OnPropertyChanged(nameof(EventColor)); } }
        public string CalendarColorString { get => calEvent.CalendarColorString; }
        public string DisplayColorString { get => calEvent.DisplayColorString; }
        public string EventColorString { get => calEvent.EventColorString; }

        public DateTime SortTime { get => calEvent.SortTime; }

        private void UpdateTimes()
        {

            calEvent.Start = locationTimeHolder.GetTime(TimeType.TimeZoneTime, calEvent.DisplayStart, TimeType);
            calEvent.End = locationTimeHolder.GetTime(TimeType.TimeZoneTime, calEvent.DisplayEnd, TimeType);

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
            OnPropertyChanged(nameof(TimeTypeSpinnerPos));
        }

        public void ChangeDisplayTime(DateTime displayStart, DateTime displayEnd, bool allDay)
        {
            calEvent.AllDay = allDay;
            calEvent.DisplayStart = displayStart;
            calEvent.DisplayEnd = displayEnd;
            UpdateTimes();
        }

        public DateTime Start
        {
            get => calEvent.Start;
            set
            {
                calEvent.Start = value;
                OnPropertyChanged();
            }
        }
        public DateTime End
        {
            get => calEvent.End;
            set
            {
                calEvent.End = value;
                OnPropertyChanged();
            }
        }

        public DateTime DisplayStart
        {
            get => calEvent.DisplayStart == DateTime.MinValue ? calEvent.Start : calEvent.DisplayStart;
            set
            {
                calEvent.DisplayEnd = value + (calEvent.DisplayEnd - calEvent.DisplayStart);
                calEvent.DisplayStart = value;
                UpdateTimes();
            }
        }
        public DateTime DisplayEnd
        {
            get => calEvent.DisplayEnd == DateTime.MinValue ? calEvent.End : calEvent.DisplayEnd;
            set
            {
                if (value <= DisplayStart)
                    calEvent.DisplayStart = value - (calEvent.DisplayEnd - calEvent.DisplayStart);
                calEvent.DisplayEnd = value;
                UpdateTimes();
            }
        }

        public DateTime DisplayStartDate { get => DisplayStart.Date; set { DisplayStart = value.Date + DisplayStart.TimeOfDay; } }
        public DateTime DisplayEndDate { get => DisplayEnd.Date; set { DisplayEnd = value.Date + DisplayEnd.TimeOfDay; } }
        public TimeSpan DisplayStartTime { get => DisplayStart.TimeOfDay; set { DisplayStart = DisplayStart.Date + value; } }
        public TimeSpan DisplayEndTime { get => DisplayEnd.TimeOfDay; set { DisplayEnd = DisplayEnd.Date + value; } }

        public string Location { get => calEvent.Location; set { UpdateLocation(value); } }

        public bool AllDay { get => calEvent.AllDay; set { calEvent.AllDay = value; OnPropertyChanged(); OnPropertyChanged(nameof(NotAllDay)); OnPropertyChanged(nameof(ShowTimeHelpers)); } }

        public string Description { get => calEvent.Description; set { calEvent.Description = value; OnPropertyChanged(); } }

        public async Task<bool> SaveEvent()
        {
            try
            {
                if (cal == null)
                    throw new Exception("no selected calendar!");


                if (!cal.CanEditEvents)
                    throw new Exception("selected calendar is read only!");

                if (calEvent.AllDay && (calEvent.Start.TimeOfDay.TotalHours != 0 || calEvent.End.TimeOfDay.TotalHours != 0))
                {
                    calEvent.Start = calEvent.Start.Date;
                    calEvent.End = calEvent.End.Date.AddDays(1);
                }

                extEvent.TimeType = TimeType;
                extEvent.TimeTypeStart = calEvent.DisplayStart;
                extEvent.TimeTypeEnd = calEvent.DisplayEnd;
                extEvent.UseTypedTime = TimeType == TimeType.MiddleSunTime || TimeType == TimeType.RealSunTime;

                calEvent.Start = sys.GetTimeWithoutSeconds(calEvent.Start);
                calEvent.End = sys.GetTimeWithoutSeconds(calEvent.End);
                extEvent.CalendarTimeStart = calEvent.Start;
                extEvent.CalendarTimeEnd = calEvent.End;

                await DeviceCalendar.DeviceCalendar.AddOrUpdateEventAsync(cal, calEvent);

                if (string.IsNullOrEmpty(calEvent.ExternalID))
                    throw new Exception("unknown exeption on saving apppontment!");

                if (extEvent != null)
                {
                    extEvent.EventID = calEvent.ExternalID;
                    if (extEvent.RecNo < 0)
                        db.dbCalendarExtention.Insert(extEvent);
                    else
                        db.dbCalendarExtention.Update(extEvent);
                }

                HasErrors = false;
                ErrorText = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                HasErrors = true;
                ErrorText = ex.Message;
            }
            return false;
        }

        public IList<CalendarEventReminder> Reminders { get => calEvent.Reminders; }

        public string ExternalID { get => calEvent.ExternalID; }

        #endregion

        #region Extention-Properties

        public TimeType TimeType { get => extEvent.TimeType; set { extEvent.TimeType = value; OnPropertyChanged(); UpdateTimes(); } }

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
                    return Start.ToShortTimeString(); ;
                return string.Empty;
                if (TimeType != AppConfigHolder.CalendarViewConfig.TimeType)
                    return locationTimeHolder.GetTime(AppConfigHolder.CalendarViewConfig.TimeType, calEvent.DisplayStart, TimeType).ToShortTimeString();
                return string.Empty;
            }
        }
        public string EndTimeHelper
        {
            get
            {
                if (TimeType != TimeType.TimeZoneTime)
                    return End.ToShortTimeString(); ;
                return string.Empty;
                if (TimeType != AppConfigHolder.CalendarViewConfig.TimeType)
                    return locationTimeHolder.GetTime(AppConfigHolder.CalendarViewConfig.TimeType, calEvent.DisplayEnd, TimeType).ToShortTimeString();
                return string.Empty;
            }
        }

        bool _isSearchingForLocation = false;
        public bool IsSearchingForLocation { get => _isSearchingForLocation; set { _isSearchingForLocation = value; OnPropertyChanged(); OnPropertyChanged(nameof(ShowLocationHelper)); OnPropertyChanged(nameof(LocationHelper)); OnPropertyChanged(nameof(LocationTimeInfo)); } }

        Delayer locationDelayer = null;
        public void UpdateLocation(string cLocationTilte, double nLat = 0, double nLng = 0)
        {
            calEvent.Location = cLocationTilte;
            extEvent.GotCorrectPosition = false;
            OnPropertyChanged(nameof(Location));

            if (string.IsNullOrEmpty(cLocationTilte) || (nLat != 0 && nLng != 0))
                ProcessLocationChange(cLocationTilte, nLat, nLng);
            else
            {
                if (locationDelayer == null || locationDelayer.IsAborted)
                    locationDelayer = new Delayer(0);
                locationDelayer.SetDelay(750, () => ProcessLocationChange(cLocationTilte, nLat, nLng));
            }
        }

        private void ProcessLocationChange(string cLocationTilte, double nLat, double nLng)
        {
            if (string.IsNullOrEmpty(cLocationTilte) || (nLat != 0 && nLng != 0))
            {
                extEvent.LocationString = cLocationTilte;
                extEvent.Latitude = nLat;
                extEvent.Longitude = nLng;
                extEvent.GotCorrectPosition = true;
                if (string.IsNullOrEmpty(cLocationTilte))
                    ResetLocationTimeHolder();
                if (nLat != 0 && nLng != 0)
                    locationTimeHolder.ChangePositionDelay(nLat, nLng);
                IsSearchingForLocation = false;
            }
            else
            {
                SearchPositionByLocation();
            }

            OnPropertyChanged(nameof(LocationHelper));
            OnPropertyChanged(nameof(LocationTimeInfo));
            UpdateTimes();
        }

        private void ResetLocationTimeHolder()
        {
            if (locationTimeHolder != null)
            {
                locationTimeHolder.AreaChanged -= LocationTimeHolder_AreaChanged;
                locationTimeHolder.Dispose();
            }
            locationTimeHolder = LocationTimeHolder.LocalInstanceClone;
            locationTimeHolder.AreaChanged += LocationTimeHolder_AreaChanged;
        }

        private void LocationTimeHolder_AreaChanged(object sender, AreaChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Location));
            OnPropertyChanged(nameof(LocationHelper));
            OnPropertyChanged(nameof(LocationTimeInfo));
            UpdateTimes();
        }

        Task tskPositionSearcher = null;
        DateTime tLastLocationChange = DateTime.MinValue;
        DateTime tLastPositionSearch = DateTime.MinValue;

        public event EventHandler Ready;

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
                    if (!string.IsNullOrEmpty(calEvent.Location) && extEvent.GotCorrectPosition)
                        locationTimeHolder.ChangePositionDelay(extEvent.Latitude, extEvent.Longitude);
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
                string cPosInfo = "";// (extEvent.LocationString.Equals(calEvent.Location) ? "Position und Ortszeit unklar: " : "Position wird ermittelt: ");
                if (extEvent.GotCorrectPosition)
                {
                    TimeSpan tsDiff = LocationTimeHolder.GetUTCGeoLngDiff(extEvent.Longitude - sys.lastUserLocation.Longitude);
                    string cDiffDirection = tsDiff.TotalMilliseconds > 0 ? "+" : "-";
                    if (tsDiff.TotalMilliseconds < 0)
                        tsDiff = TimeSpan.FromMilliseconds(tsDiff.TotalMilliseconds * -1);

                    cPosInfo = cDiffDirection;
                    if (tsDiff.TotalMilliseconds < 0)
                        tsDiff = TimeSpan.FromMilliseconds(tsDiff.TotalMilliseconds * -1);

                    if (tsDiff.TotalHours >= 1)
                        cPosInfo += (int)tsDiff.TotalHours + "h ";
                    if (tsDiff.TotalMinutes > 1)
                        cPosInfo += tsDiff.Minutes + "min ";
                    if (tsDiff.TotalMinutes < 15)
                        cPosInfo += tsDiff.Seconds + "sec";

                    if (tsDiff.TotalSeconds < 3)
                        cPosInfo = "current location";
                    else
                        cPosInfo = cPosInfo.Trim() + " real time offset";
                }
                if (sys.Debugmode && false)
                    cPosInfo = sys.DezimalGradToGrad(locationTimeHolder.Latitude, locationTimeHolder.Longitude) + ", " + locationTimeHolder.AreaName + ", " + locationTimeHolder.CountryName + "  :  " + cPosInfo;
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

        public bool HasErrors { get; private set; } = false;

        public string ErrorText { get; private set; } = string.Empty;

        #endregion
    }
}
