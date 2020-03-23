using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using iChronoMe.Core.Types;

namespace iChronoMe.Core.Classes
{


    public class LocationConfig
    {
        public LocationConfig()
        {

        }

        public bool UseStaticLocaltion { get; set; } = false;

        public double Latitude { get; set; } = 0;
        public double Longitude { get; set; } = 0;
        public string AreaName { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public string TimeZoneID { get; set; } = TimeZoneInfo.Local.StandardName;
        public string TimeZoneName { get; set; }
        public double TimeZoneOffsetGmt { get; set; } = 0;
        public double TimeZoneOffsetDst { get; set; } = 0;
    }

    public class UsageInfo
    {
        public DateTime FirstAppStart { get; set; } = DateTime.Now;

        public string FirstAppVersion { get; set; } = sys.cAppVersionInfo;

        public string LastAppVersion { get; set; } = string.Empty;

        public int LastAppVersionCode { get; set; } = -1;

        public int AppStartCount { get; set; } = 0;

        public DateTime LastAppStart { get; set; } = DateTime.MinValue;
    }

    public partial class MainConfig
    {
        public MainConfig()
        {

        }

        public TimeType DefaultTimeType
        {
            get => sys.DefaultTimeType;
            set { sys.DefaultTimeType = value; }
        }

        private const int xxDebugDone = 0;

        public int InitScreenTheme { get; set; } = sys.Debugmode ? xxDebugDone : 0;
        public int InitScreenTimeType { get; set; } = sys.Debugmode ? xxDebugDone : 0;
        public int InitScreenPermission { get; set; } = sys.Debugmode ? xxDebugDone : 0;
        public int InitScreenPrivacy { get; set; } = sys.Debugmode ? xxDebugDone : 0;
        public int InitScreenUserLocation { get; set; } = sys.Debugmode ? xxDebugDone : 0;
        public int InitBaseDataDownload { get; set; } = 0;

        public bool AlwaysShowForegroundNotification { get; set; } = false;
        public bool SendErrorLogs { get; set; } = false;
        public bool DenyErrorScreens { get; set; } = false;
        //public DateTime LastCheckClockFaces { get; set; } = DateTime.MinValue;
        //public DateTime LastCheckClockHands { get; set; } = DateTime.MinValue;
        public bool ContinuousLocationUpdates { get; set; } = false;
        public bool CalendarReminderWarningDone { get; set; }

        [XmlIgnore]
        public int DefaultTimeType_SpinnerPosition
        {
            get
            {
                switch (DefaultTimeType) { case TimeType.TimeZoneTime: return 2; case TimeType.MiddleSunTime: return 1; default: return 0; }
            }
            set
            {
                switch (value)
                {
                    case 2:
                        DefaultTimeType = TimeType.TimeZoneTime;
                        break;
                    case 1:
                        DefaultTimeType = TimeType.MiddleSunTime;
                        break;
                    default:
                        DefaultTimeType = TimeType.RealSunTime;
                        break;
                }
            }
        }
    }

    public partial class CalendarViewConfig
    {
        public float WelcomeScreenDone { get; set; } = 0;

        public List<string> HideCalendars { get; set; } = new List<string>();

        public TimeType CalendarTimeType { get; set; } = sys.DefaultTimeType;

        [XmlIgnore]
        public TimeType TimeType
        {
            get => UseAppDefautlTimeType ? sys.DefaultTimeType : CalendarTimeType;
            set
            {
                CalendarTimeType = value;
                UseAppDefautlTimeType = false;
            }
        }

        public bool UseAppDefautlTimeType { get; set; } = true;
        public int LastViewType { get; set; } = 1;
        public int DefaultViewType { get; set; } = -1;

        public SfScheduldeConfig SfScheduldeConfig { get; set; } = new SfScheduldeConfig();

        //public CalendarGroupViewConfig MainGroupViewConfig { get; set; } = new CalendarGroupViewConfig();
        //public List<string> CustomGroupViewConfigS { get; set; } = new List<string>();
    }

    public class SfScheduldeConfig
    {
        public ICollection<int> NonWorkingDays = new List<int>(new int[] { 0, 6 });

        public int TimeLineHourStart { get; set; } = 0;
        public int TimeLineHourEnd { get; set; } = 24;
        public int TimeLineDaysCount { get; set; } = -1;

        public int DayViewHourStart { get; set; } = 0;
        public int DayViewHourEnd { get; set; } = 24;
        public int DayViewWorkHourStart { get; set; } = 8;
        public int DayViewWorkHourEnd { get; set; } = 18;
        public bool DayViewShowAllDay { get; set; } = true;

        public int WeekViewHourStart { get; set; } = 0;
        public int WeekViewHourEnd { get; set; } = 24;
        public int WeekViewWorkHourStart { get; set; } = 8;
        public int WeekViewWorkHourEnd { get; set; } = 18;
        public bool WeekViewShowAllDay { get; set; } = true;

        public int WorkWeekHourStart { get; set; } = 0;
        public int WorkWeekHourEnd { get; set; } = 24;
        public int WorkWeekWorkHourStart { get; set; } = 8;
        public int WorkWeekWorkHourEnd { get; set; } = 18;
        public bool WorkWeekShowAllDay { get; set; } = true;

        public bool MonthViewShowWeekNumber { get; set; } = true;
        public int MonthViewAppointmentDisplayMode { get; set; } = 0;
        public int MonthViewAppointmentIndicatorCount { get; set; } = 4;
        public int MonthViewNavigationDirection { get; set; } = 0;
        public bool MonthViewShowInlineEvents { get; set; } = false;
        public bool MonthViewShowAgenda { get; set; } = true;

        public void CheckStartEndTimes()
        {
            if (TimeLineHourStart >= TimeLineHourEnd)
            {
                TimeLineHourStart = 0;
                TimeLineHourEnd = 24;
            }
            if (DayViewHourStart >= DayViewHourEnd)
            {
                DayViewHourStart = 0;
                DayViewHourEnd = 24;
            }
            if (DayViewWorkHourStart >= DayViewWorkHourEnd)
            {
                DayViewWorkHourStart = 8;
                DayViewWorkHourEnd = 16;
            }
            if (WeekViewHourStart >= WeekViewHourEnd)
            {
                WeekViewHourStart = 0;
                WeekViewHourEnd = 24;
            }
            if (WeekViewWorkHourStart >= WeekViewWorkHourEnd)
            {
                WeekViewWorkHourStart = 8;
                WeekViewWorkHourEnd = 16;
            }
            if (WorkWeekHourStart >= WorkWeekHourEnd)
            {
                WorkWeekHourStart = 0;
                WorkWeekHourEnd = 24;
            }
            if (WorkWeekWorkHourStart >= WorkWeekWorkHourEnd)
            {
                WorkWeekWorkHourStart = 8;
                WorkWeekWorkHourEnd = 16;
            }
        }
    }

    public class CalendarGroupViewConfig
    {
        public DualCalendarModelType DualCalendarModelType { get; set; }
        public List<int> DualCalendarWeekDayS { get; set; } = new List<int>();
        public string DualCalendarModelID { get; set; }
        public bool ShowTitle { get; set; } = true;
        public bool ShowMonthNames { get; set; } = true;
        public bool ShowWeekDays { get; set; } = true;
        public bool ShowWeekNumbers { get; set; } = true;

        public xColor BackgroundColor { get; set; } = xColor.WhiteSmoke;
        public xColor TitleColor { get; set; } = xColor.Black;
        public xColor MonthNamesColor { get; set; } = xColor.Black;
        public xColor WeekDaysColor { get; set; } = xColor.Black;
        public xColor WeekNumbersColor { get; set; } = xColor.Black;
        public xColor DayNamesColor { get; set; } = xColor.Black;
        public xColor DayNamesBackgroundColor { get; set; } = xColor.AliceBlue;
        public xColor TodayBackgroundColor { get; set; } = xColor.Khaki;

        [XmlIgnore]
        public bool ShowDualCalendarModel
        {
            get => DualCalendarModelType != DualCalendarModelType.None;
        }

        [XmlIgnore]
        public bool ShowCustomDualCalendarModel
        {
            get => DualCalendarModelType == DualCalendarModelType.Custom;
        }
    }

    public enum DualCalendarModelType
    {
        None = 0,
        Gregorian = 1,
        Custom = 2
    }

    public class DashboardConfig
    {
        public DashboardConfig()
        {

        }

        public List<DashboardItemConfig> Items { get; set; } = new List<DashboardItemConfig>();
    }

    [XmlInclude(typeof(dbiDigitalTimes))]
    [XmlInclude(typeof(dbiAnalogClock))]
    [XmlInclude(typeof(dbiDynamicDateInfo))]
    [XmlInclude(typeof(dbiEventList))]
    //[XmlInclude(typeof(dbiAstroOffset))]    
    public class DashboardItemConfig
    {
        public string Title { get; set; }
        public TimeType TimeType { get; set; } = sys.DefaultTimeType;
        public string CalendarModelID { get; set; }
        public DashboardItemPositionType PositionType { get; set; } = DashboardItemPositionType.LivePosition;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string CountryName { get; set; }
        public string CityName { get; set; }
        public string RegionName { get; set; }
        public string AreaName { get; set; }
        public string TimeZoneName { get; set; }
        public double gmtOffset { get; set; }
        public double dstOffset { get; set; }
    }

    public class dbiDelete : DashboardItemConfig { }

    public class dbiDigitalTimes : DashboardItemConfig
    {
        public bool ShowRealSunTime { get; set; } = true;
        public bool ShowMiddleSunTime { get; set; } = false;
        public bool ShowTimeZoneTime { get; set; } = true;
        public bool ShowUtcTime { get; set; } = false;

        public string TimeFormatString { get; set; } = "HH:mm:ss";
    }

    public class dbiAnalogClock : DashboardItemConfig
    {
        public bool ShowHourHand { get; set; } = true;
        public bool ShowMinuteHand { get; set; } = true;
        public bool ShowSecondHand { get; set; } = true;
    }

    public class dbiDynamicDateInfo : DashboardItemConfig
    {

    }

    public class dbiEventList : DashboardItemConfig
    {
        public int MaxDayCount { get; set; } = 3;
    }

    /*public class dbiAstroOffset : DashboardItemConfig
    {
        public TimeOffsetAstronomic AstroOffset { get; set; }
    }*/

    public enum DashboardItemPositionType
    {
        [XmlEnum(Name = "0")]
        None = 0,
        [XmlEnum(Name = "10")]
        FixedPosition = 10,
        [XmlEnum(Name = "20")]
        LivePosition = 20
    }
}