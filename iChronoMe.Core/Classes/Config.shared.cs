using iChronoMe.Core.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace iChronoMe.Core.Classes
{
    public static class AppConfigHolder
    {
        private static MainConfig _mainConfig = null;
        private static DashboardConfig _dashboardConfig = null;
        private static CalendarViewConfig _calendarViewConfig = null;
        public static MainConfig MainConfig
        {
            get
            {
                if (_mainConfig == null) {
                    _mainConfig = LoadFromFile<MainConfig>();
                }
                return _mainConfig;
            }
        }

        public static void SaveMainConfig()
        {
            if (_mainConfig != null)
                SaveConfig(_mainConfig);
        }

        public static DashboardConfig DashboardConfig
        {
            get
            {
                if (_dashboardConfig == null)
                {
                    _dashboardConfig = LoadFromFile<DashboardConfig>();
                }
                return _dashboardConfig;
            }
        }

        public static void SaveDashboardConfig()
        {
            if (_dashboardConfig != null)
                SaveConfig(_dashboardConfig);
        }

        public static CalendarViewConfig CalendarViewConfig
        {
            get
            {
                if (_calendarViewConfig == null)
                {
                    _calendarViewConfig = LoadFromFile<CalendarViewConfig>();
                }
                return _calendarViewConfig;
            }
        }

        public static void SaveCalendarViewConfig()
        {
            if (_calendarViewConfig != null)
                SaveConfig(_calendarViewConfig);
        }

        private static void SaveConfig<T>(T cfg)
        {
            string cfgFile = Path.Combine(sys.PathConfig, typeof(T).Name + ".cfg");
            try
            {
                try
                {
                    if (File.Exists(cfgFile))
                        File.Copy(cfgFile, cfgFile + ".bak", true);
                }
                catch { }
                SmoothXmlSerializer x = new SmoothXmlSerializer();
                TextWriter writer = new StreamWriter(cfgFile + ".new");
                x.Serialize(writer, cfg);
                writer.Flush();
                writer.Close();

                File.Delete(cfgFile);
                File.Move(cfgFile + ".new", cfgFile);

            }
            catch (Exception e)
            {
                e.ToString();
                try
                {
                    if (File.Exists(cfgFile + ".bak"))
                        File.Copy(cfgFile + ".bak", cfgFile, true);
                }
                catch { }
            }
        }
        private static T LoadFromFile<T>()
        {
            string cfgFile = Path.Combine(sys.PathConfig, typeof(T).Name + ".cfg");
            try
            {
                using (var stream = new StreamReader(cfgFile))
                {
                    var serializer = new SmoothXmlSerializer();
                    var data = serializer.Deserialize<T>(stream);
                    stream.Close();
                    return data;
                }
            }
            catch (Exception e)
            {
                e.ToString();
                return (T)Activator.CreateInstance(typeof(T));
            }
        }
    }

    public class MainConfig
    {
        public MainConfig()
        {

        }

        public TimeType DefaultTimeType { get; set; } = TimeType.RealSunTime;

        public float WelcomeScreenDone { get; set; } = 0;

        public string ThemeName { get; set; }
        public bool AlwaysShowForegroundNotification { get; set; } = false;
        public bool SendErrorLogs { get; set; } = false;
        public DateTime LastCheckClockFaces { get; set; } = DateTime.MinValue;
        public WidgetCfg_ClockAnalog MainClock { get; set; } = new WidgetCfg_ClockAnalog();
    }

    public class CalendarViewConfig
    {
        public CalendarGroupViewConfig MainGroupViewConfig { get; set; } = new CalendarGroupViewConfig();

        public List<string> CustomGroupViewConfigS { get; set; } = new List<string>();
    }

    public class CalendarGroupViewConfig : INotifyPropertyChanged
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

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
        public TimeType TimeType { get; set; } = AppConfigHolder.MainConfig.DefaultTimeType;
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
