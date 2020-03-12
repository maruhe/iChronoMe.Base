using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

using iChronoMe.Core;
using iChronoMe.Core.Classes;
using iChronoMe.Core.DynamicCalendar;
using iChronoMe.Core.Types;

namespace iChronoMe.Widgets
{
    public class WidgetConfigHolder
    {
        public string CfgFile { get; } = System.IO.Path.Combine(sys.PathConfig, "widgetcfg.cfg");

        static bool bTest = false;
        static int iTest = 0;

        public WidgetConfigHolder(bool bIsArchieMode = false)
        {
            if (bIsArchieMode)
                CfgFile = Path.Combine(sys.PathConfig, "widgetcfg_archiv.cfg");
            LoadFromFile();
        }
        public WidgetConfigHolder(string cfgFilePath)
        {
            if (string.IsNullOrEmpty(cfgFilePath))
                cfgFilePath = "_tmp_widgets.cfg";
            if (!cfgFilePath.Contains("/"))
                cfgFilePath = Path.Combine(sys.PathConfig, cfgFilePath);

            CfgFile = cfgFilePath;
            LoadFromFile();
        }

        Dictionary<int, WidgetCfg> WidgetConfigList = null;

        public int WidgetCount { get => WidgetConfigList.Count; }

        public T GetWidgetCfg<T>(int iWidgetId, bool bCreateIfNotExisting = true)
            where T : WidgetCfg
        {
            xLog.Debug("GetWidgetCfg: " + iWidgetId.ToString() + " " + bTest.ToString() + ": " + iTest.ToString());

            bTest = true;
            iTest++;

            if (WidgetConfigList.ContainsKey(iWidgetId))
            {
                object o = WidgetConfigList[iWidgetId];
                if (o.GetType().Equals(typeof(T)) || o.GetType().IsSubclassOf(typeof(T)))
                    return (T)o;
                else
                    this.ToString();
            }

            if (iWidgetId == -101) //Services Notification
                return (T)(object)new WidgetCfg_ClockAnalog() { WidgetId = iWidgetId, ShowHours = true, ShowMinutes = true, ShowSeconds = false };

            if (!bCreateIfNotExisting)
                return default(T);

            T res = (T)Activator.CreateInstance(typeof(T));
            try
            {
                typeof(T).GetField("WidgetId").SetValue(res, iWidgetId);
            }
            catch (Exception ex)
            { ex.ToString(); }
            return res;
        }

        public bool WidgetExists(int iWidgetId)
        {
            return WidgetConfigList.ContainsKey(iWidgetId);
        }
        public bool WidgetExists<T>(int iWidgetId)
        {
            if (WidgetConfigList.ContainsKey(iWidgetId))
            {
                object o = WidgetConfigList[iWidgetId];
                return o.GetType().Equals(typeof(T)) || o.GetType().IsSubclassOf(typeof(T));
            }
            return false;
        }

        public int[] AllIds()
        {
            int[] idS = new int[WidgetConfigList.Count];
            WidgetConfigList.Keys.CopyTo(idS, 0);
            return idS;
        }

        public int[] AllIds<T>()
        {
            List<int> idS = new List<int>();
            foreach (var o in WidgetConfigList.Values)
            {
                if (o.GetType().Equals(typeof(T)) || o.GetType().IsSubclassOf(typeof(T)))
                    idS.Add(o.WidgetId);
            }
            return idS.ToArray();
        }

        public WidgetCfg[] AllCfgs()
        {
            WidgetCfg[] cfgS = new WidgetCfg[WidgetConfigList.Count];
            WidgetConfigList.Values.CopyTo(cfgS, 0);
            return cfgS;
        }


        public void SetWidgetCfg(WidgetCfg cfg, int id, bool bSaveToFile = true)
        {
            cfg.WidgetId = id;
            SetWidgetCfg(cfg, bSaveToFile);
        }

        public void SetWidgetCfg(WidgetCfg cfg, bool bSaveToFile = true)
        {
            if (WidgetConfigList.ContainsKey(cfg.WidgetId))
                WidgetConfigList.Remove(cfg.WidgetId);

            WidgetConfigList.Add(cfg.WidgetId, cfg);
            if (bSaveToFile)
                SaveToFile();
        }

        public void DeleteWidget(int iWidgetId, bool bSaveToFile = true)
        {
            if (WidgetConfigList.ContainsKey(iWidgetId))
                WidgetConfigList.Remove(iWidgetId);
            if (bSaveToFile)
                SaveToFile();
        }
        public void SaveToFile()
        {
            try
            {
                DateTime swStart = DateTime.Now;

                SmoothXmlSerializer y = new SmoothXmlSerializer();
                TextWriter writer = new StreamWriter(CfgFile + ".new");
                List<WidgetCfg> data = new List<WidgetCfg>(WidgetConfigList.Values);
                y.Serialize(writer, data);
                writer.Flush();
                writer.Close();
                TimeSpan tsSera = DateTime.Now - swStart;
                swStart = DateTime.Now;

                File.Delete(CfgFile);
                File.Move(CfgFile + ".new", CfgFile);

                TimeSpan tsRepl = DateTime.Now - swStart;
                xLog.Debug("StoredWidgetCfg: " + WidgetConfigList.Count.ToString() + " widgets took  " + tsSera.TotalMilliseconds.ToString() + "ms + " + tsRepl.TotalMilliseconds + "ms");
                if (tsSera.TotalMilliseconds > 250)
                    xLog.Warn("StoredWidgetCfg: " + WidgetConfigList.Count.ToString() + " widgets took  " + tsSera.TotalMilliseconds.ToString() + "ms + " + tsRepl.TotalMilliseconds + "ms");
            }
            catch (Exception e)
            {
                e.ToString();
            }
        }

        public void LoadFromFile()
        {
            try
            {
                using (var stream = new StreamReader(CfgFile))
                {
                    var serializer = new SmoothXmlSerializer();
                    var questionData = serializer.Deserialize<List<WidgetCfg>>(stream);

                    WidgetConfigList = new Dictionary<int, WidgetCfg>();
                    foreach (WidgetCfg cfg in questionData)
                    {
                        WidgetConfigList.Add(cfg.WidgetId, cfg);
                    }
                    stream.Close();
                }
                WidgetConfigList.ToString();
            }
            catch (Exception e)
            {
                e.ToString();
                WidgetConfigList = new Dictionary<int, WidgetCfg>();
            }
        }
    }

    [XmlInclude(typeof(WidgetCfg_Clock))]
    [XmlInclude(typeof(WidgetCfg_Lifetime))]
    [XmlInclude(typeof(WidgetCfg_Moon))]
    [XmlInclude(typeof(WidgetCfg_ActionButton))]
    [XmlInclude(typeof(WidgetCfg_CalendarTimetable))]
    [XmlInclude(typeof(WidgetCfg_CalendarMonthView))]
    [XmlInclude(typeof(WidgetCfg_CalendarCircleWave))]
    public abstract class WidgetCfg
    {
        public static readonly xColor tcTransparent = xColor.FromHex("#00000000");
        public static readonly xColor tcWhite = xColor.FromHex("#FFFFFFFF");
        public static readonly xColor tcBlack = xColor.FromHex("#FF000000");
        public static readonly xColor tcLight = xColor.FromHex("#FFD0D0D0");
        public static readonly xColor tcDark = xColor.FromHex("#FF323234");
        public static readonly xColor tcLightGlass1 = xColor.FromHex("#40FFFFFF");
        public static readonly xColor tcDarkGlass1 = xColor.FromHex("#40000000");
        public static readonly xColor tcLightGlass2 = xColor.FromHex("#60FFFFFF");
        public static readonly xColor tcDarkGlass2 = xColor.FromHex("#60000000");
        public static readonly xColor tcLightGlass3 = xColor.FromHex("#80FFFFFF");
        public static readonly xColor tcDarkGlass3 = xColor.FromHex("#80000000");
        public static readonly xColor tcLightGlass4 = xColor.FromHex("#A0FFFFFF");
        public static readonly xColor tcDarkGlass4 = xColor.FromHex("#A0000000");
        public static readonly xColor tcLightGlass5 = xColor.FromHex("#C0FFFFFF");
        public static readonly xColor tcDarkGlass5 = xColor.FromHex("#C0000000");

        public int WidgetId;

        public string WidgetTitle;

        public int WidgetWidth;
        public int WidgetHeight;

        [XmlIgnore] public bool SupportsChangeTimeType = false;
        [XmlIgnore] public bool SupportsWidgetTimeType = false;

        public TimeType CurrentTimeType = sys.DefaultTimeType;
        public TimeType WidgetTimeType = sys.DefaultTimeType;
        public string CalendarModelId;

        public xColor ColorTitleText = xColor.White;
        public xColor ColorBackground = xColor.FromRgba(0, 0, 0, 120);

        public int CornerRadius = 8;
        public bool ShowTitle = true;

        WidgetTheme BaseTheme;

        public Object Clone()
        {
            return MemberwiseClone();
        }

        public virtual void SetTheme(WidgetTheme theme)
        {
            BaseTheme = theme;

            switch (theme)
            {
                case WidgetTheme.TransparentLight:
                    ColorBackground = tcTransparent;
                    ColorTitleText = xColor.White;
                    break;
                case WidgetTheme.TransparentDark:
                    ColorBackground = tcTransparent;
                    ColorTitleText = tcDark;
                    break;
                case WidgetTheme.Light:
                    ColorBackground = tcLight;
                    ColorTitleText = tcDark;
                    break;
                case WidgetTheme.Dark:
                    ColorBackground = tcDark;
                    ColorTitleText = tcLight;
                    break;
                case WidgetTheme.White:
                    ColorBackground = xColor.White;
                    ColorTitleText = xColor.Black;
                    break;
                case WidgetTheme.Black:
                    ColorBackground = xColor.Black;
                    ColorTitleText = xColor.White;
                    break;
                case WidgetTheme.LightGlass1:
                    ColorBackground = tcLightGlass1;
                    ColorTitleText = xColor.Black;
                    break;
                case WidgetTheme.LightGlass2:
                    ColorBackground = tcLightGlass2;
                    ColorTitleText = xColor.Black;
                    break;
                case WidgetTheme.LightGlass3:
                    ColorBackground = tcLightGlass3;
                    ColorTitleText = xColor.Black;
                    break;
                case WidgetTheme.LightGlass4:
                    ColorBackground = tcLightGlass4;
                    ColorTitleText = xColor.Black;
                    break;
                case WidgetTheme.LightGlass5:
                    ColorBackground = tcLightGlass5;
                    ColorTitleText = xColor.Black;
                    break;
                case WidgetTheme.DarkGlass1:
                    ColorBackground = tcDarkGlass1;
                    ColorTitleText = xColor.White;
                    break;
                case WidgetTheme.DarkGlass2:
                    ColorBackground = tcDarkGlass2;
                    ColorTitleText = xColor.White;
                    break;
                case WidgetTheme.DarkGlass3:
                    ColorBackground = tcDarkGlass3;
                    ColorTitleText = xColor.White;
                    break;
                case WidgetTheme.DarkGlass4:
                    ColorBackground = tcDarkGlass4;
                    ColorTitleText = xColor.White;
                    break;
                case WidgetTheme.DarkGlass5:
                    ColorBackground = tcDarkGlass5;
                    ColorTitleText = xColor.White;
                    break;
            }
        }
    }

    public class WidgetCfg_ActionButton : WidgetCfg
    {
        public WidgetCfg_ActionButton()
        {
            WidgetTitle = "iChronoMe";
        }

        public ClickAction ClickAction = new ClickAction(ClickActionType.OpenApp);

        public ActionButton_Style Style = ActionButton_Style.iChronEye;

        public string IconName;

        public xColor IconColor = xColor.White;

        public bool AnimateOnFirstClick = false;

        public float AnimationDuriation = 2;

        public float AnimationRounds = 1;
    }

    public enum ClickActionType
    {
        Animate = 1001,
#if DEBUG
        TestActivity = -25,
#endif
        None = -1,
        OpenSettings = 0,
        OpenApp = 100,
        OpenClock = 111,
        OpenCalendar = 112,
        OpenWorldTimeMap = 113,
        CreateEvent = 211,
        CreateAlarm = 251,
        OpenOtherApp = 800
    }
    public enum ActionButton_Style
    {
        iChronEye,
        Icon
    }

    public class ClickAction
    {
        public ClickAction() { }
        public ClickAction(ClickActionType type)
        {
            Type = type;
        }

        public ClickActionType Type = ClickActionType.OpenSettings;

        public string[] Params;
    }

    public class WidgetCfg_Moon : WidgetCfg
    {

    }

    public class WidgetCfg_Lifetime : WidgetCfg
    {
        public DateTime LifeStartTime = DateTime.MinValue;

        public DateTime EndOfLifeTime = DateTime.MinValue;

        public bool ShowLifeTimeProgress = false;

        public bool ShowLifeTimePercentage = false;

        public xColor ColorLifetimeText = xColor.White;

        public xColor ColorLifeTimeProgress = xColor.White;

        public xColor ColorLifeTimePercentage = xColor.White;
    }


    [XmlInclude(typeof(WidgetCfg_CalendarTimetable))]
    [XmlInclude(typeof(WidgetCfg_CalendarMonthView))]
    [XmlInclude(typeof(WidgetCfg_CalendarCircleWave))]
    public abstract class WidgetCfg_Calendar : WidgetCfg
    {
        public bool ShowAllCalendars = true;

        public List<string> ShowCalendars = new List<string>();

        public xColor ColorTitleButtons = xColor.White;
        public xColor ColorErrorText = xColor.DarkRed;

        public xColor ColorTodayText = xColor.Transparent;
        public xColor ColorTodayBackground = xColor.PaleGoldenrod;
        public xColor ColorDayText = xColor.White;
        public xColor ColorDayBackground = xColor.Transparent;

        public bool ShowDateTitle = true;

        public bool ShowButtonConfig = true;
        public bool ShowButtonRefresh = true;
        public bool ShowButtonAdd = true;

        public override void SetTheme(WidgetTheme theme)
        {
            base.SetTheme(theme);
            ColorDayBackground = ColorBackground;
            ColorTitleButtons = ColorDayText = ColorTodayText = ColorTitleText;
            ColorTodayBackground = xColor.PaleGoldenrod;
            switch (theme)
            {
                case WidgetTheme.LightGlass1:
                case WidgetTheme.LightGlass2:
                case WidgetTheme.LightGlass3:
                case WidgetTheme.LightGlass4:
                case WidgetTheme.LightGlass5:
                    ColorTodayBackground = xColor.FromRgba(255, 255, 255, 220);
                    break;
                case WidgetTheme.DarkGlass1:
                case WidgetTheme.DarkGlass2:
                case WidgetTheme.DarkGlass3:
                case WidgetTheme.DarkGlass4:
                case WidgetTheme.DarkGlass5:
                    ColorTodayBackground = xColor.FromRgba(0, 0, 0, 240);
                    break;
            }
        }
    }

    public class WidgetCfg_CalendarCircleWave : WidgetCfg_Calendar
    {
        public WidgetCfg_CalendarCircleWave()
        {
            ColorTodayBackground = xColor.Transparent;
            ColorDayBackground = xColor.PapayaWhip;
            ColorDayText = xColor.Black;
        }

        public FirstDayType FirstDayType = FirstDayType.MonthStart;
        public int FirstDayOffset = 0;
        public TimeUnit TimeUnit = TimeUnit.Month;
        public int TimeUnitCount = 1;

        public RotateBaseDay RotateBaseDay = RotateBaseDay.FirstDay;
        public RotatePosition RotatePosition = RotatePosition.Left;

        public DayNumberStyle DayNumberStyle = DayNumberStyle.CalendarModell;

        public DateGradient DayBackgroundGradient = new DateGradient() { GradientS = { new DynamicGradient(TimeUnit.Month, new xColor[] { xColor.FromHex("#b89284"), xColor.FromHex("#84695f") }) } };

        public override void SetTheme(WidgetTheme theme)
        {
            base.SetTheme(theme);

            ColorTodayBackground = xColor.Transparent;
            ColorDayBackground = xColor.PapayaWhip;
            ColorDayText = xColor.Black;
        }
    }

    public class WidgetCfg_CalendarMonthView : WidgetCfg_Calendar
    {
        public WidgetCfg_CalendarMonthView()
        {
            ColorBackground = ColorDayBackground = xColor.White;
            ColorTitleText = ColorTitleButtons = xColor.Black;
            ColorDayText = xColor.Black;
        }

        public DayNumberStyle DayNumberStyle = DayNumberStyle.CalendarModell;

        public xColor ColorGridLines = xColor.Transparent;

        public bool ShowEventColor = true;

        public xColor ColorEventText = xColor.White;

        public override void SetTheme(WidgetTheme theme)
        {
            base.SetTheme(theme);

            ColorGridLines = ColorTitleText;
            switch (theme)
            {
                case WidgetTheme.TransparentLight:
                case WidgetTheme.TransparentDark:
                    ColorGridLines = tcTransparent;
                    break;
                case WidgetTheme.LightGlass1:
                case WidgetTheme.LightGlass2:
                case WidgetTheme.LightGlass3:
                case WidgetTheme.LightGlass4:
                case WidgetTheme.LightGlass5:
                    ColorGridLines = xColor.FromRgba(0, 0, 0, 80);
                    break;
                case WidgetTheme.DarkGlass1:
                case WidgetTheme.DarkGlass2:
                case WidgetTheme.DarkGlass3:
                case WidgetTheme.DarkGlass4:
                case WidgetTheme.DarkGlass5:
                    ColorGridLines = xColor.FromRgba(255, 255, 255, 120);
                    break;
            }
        }
    }

    public enum DayNumberStyle
    {
        [XmlEnum(Name = "X")]
        None,
        [XmlEnum(Name = "0")]
        CalendarModell,
        [XmlEnum(Name = "1")]
        Gregorian,
        [XmlEnum(Name = "10")]
        CalendarModellAndGregorian,
        [XmlEnum(Name = "11")]
        GregorianAndCalendarModell
    }
    public enum FirstDayType
    {
        [XmlEnum(Name = "0")]
        Today,
        [XmlEnum(Name = "1")]
        WeekStart,
        [XmlEnum(Name = "10")]
        TodayLastWeek,
        [XmlEnum(Name = "11")]
        LastWeekStart,
        [XmlEnum(Name = "20")]
        MonthStart,
        [XmlEnum(Name = "30")]
        YearStart
    }

    public enum DayCountType
    {
        [XmlEnum(Name = "1W")]
        OneWeek = 1,
        [XmlEnum(Name = "2W")]
        TwoWeeks = 2,
        [XmlEnum(Name = "3W")]
        ThreeWeeks = 3,
        [XmlEnum(Name = "4W")]
        FourWeeks = 4,
        [XmlEnum(Name = "5W")]
        FiveWeeks = 5,
        [XmlEnum(Name = "6W")]
        SixWeeks = 6,
        [XmlEnum(Name = "1M")]
        OneMonth = 101,
        [XmlEnum(Name = "2M")]
        TwoMonth = 102,
        [XmlEnum(Name = "3M")]
        ThreeMonth = 103,
        [XmlEnum(Name = "4M")]
        FourMonth = 104,
        [XmlEnum(Name = "5M")]
        FiveMonth = 105,
        [XmlEnum(Name = "6M")]
        SixMonth = 106,
        [XmlEnum(Name = "HY")]
        HalfYear = 201,
        [XmlEnum(Name = "Y")]
        Year = 202,
        [XmlEnum(Name = "XD")]
        DayCount = 500
    }

    public enum RotateBaseDay
    {
        [XmlEnum(Name = "FD")]
        FirstDay,
        [XmlEnum(Name = "T")]
        Today
    }

    public enum RotatePosition
    {
        [XmlEnum(Name = "L")]
        Left,
        [XmlEnum(Name = "T")]
        Top,
        [XmlEnum(Name = "R")]
        Right,
        [XmlEnum(Name = "B")]
        Bottom
    }

    public class WidgetCfg_CalendarTimetable : WidgetCfg_Calendar
    {
        public WidgetCfg_CalendarTimetable()
        {
            SupportsWidgetTimeType = SupportsChangeTimeType = true;
        }

        public int MaxFutureDays { get; set; } = 21;

        public int MaxLoadCount { get; set; } = 100;

        public xColor ColorSeparatorText = xColor.White;

        public xColor ColorEventNameText = xColor.White;

        public xColor ColorEventTimeText = xColor.White;

        public xColor ColorEventLocationText = xColor.White;

        public xColor ColorEventLocationOffsetText = xColor.White;

        public xColor ColorEventDescriptionText = xColor.White;

        public xColor ColorEventSymbols = xColor.White;

        public bool ShowEventColor = true;
        public bool ShowLocation = true;
        public bool ShowLocationSunOffset = true;
        public bool ShowDesciption = false;
        public int ShowDesciptionMaxLines = 1;

        public override void SetTheme(WidgetTheme theme)
        {
            base.SetTheme(theme);

            ColorSeparatorText = ColorEventNameText = ColorEventTimeText = ColorEventLocationText = ColorEventLocationOffsetText = ColorEventDescriptionText = ColorEventSymbols = ColorTitleText;
        }
    }

    [XmlInclude(typeof(WidgetCfg_ClockAnalog))]
    public abstract class WidgetCfg_Clock : WidgetCfg
    {
        public ClickAction ClickAction = new ClickAction(ClickActionType.OpenSettings);
        public WidgetCfgPositionType PositionType = WidgetCfgPositionType.None;

        public double Latitude = 0;
        public double Longitude = 0;

        public bool ShowHours = true;
        public bool ShowMinutes = true;
#if DEBUG
        public bool ShowSeconds = true;
#else
        public bool ShowSeconds = false;
#endif

    }

    public class WidgetCfg_ClockAnalog : WidgetCfg_Clock
    {
        public WidgetCfg_ClockAnalog()
        {
            SupportsWidgetTimeType = SupportsChangeTimeType = true;
        }

        public bool FlowHourHand = true;
        public bool FlowMinuteHand = false;
        public bool FlowSecondHand = false;

        [XmlIgnore]
        public string AllHandConfigID
        {
            set
            {
                HourHandConfigID = MinuteHandConfigID = SecondHandConfigID = CapConfigID = value;
            }
        }

        string _hourHandConfigID = string.Empty;
        public string HourHandConfigID { get => _hourHandConfigID; set { _hourHandConfigID = value; _hourHandConfig = null; } }

        ClockHandConfig _hourHandConfig = null;
        [XmlIgnore]
        public ClockHandConfig HourHandConfig
        {
            get
            {
                if (_hourHandConfig == null)
                    _hourHandConfig = ClockHandConfig.Get(_hourHandConfigID);
                return _hourHandConfig;
            }
        }

        string _minuteHandConfigID = string.Empty;
        public string MinuteHandConfigID { get => _minuteHandConfigID; set { _minuteHandConfigID = value; _MinuteHandConfig = null; } }

        ClockHandConfig _MinuteHandConfig = null;
        [XmlIgnore]
        public ClockHandConfig MinuteHandConfig
        {
            get
            {
                if (_MinuteHandConfig == null)
                    _MinuteHandConfig = ClockHandConfig.Get(_minuteHandConfigID);
                return _MinuteHandConfig;
            }
        }

        string _secondHandConfigID = string.Empty;
        public string SecondHandConfigID { get => _secondHandConfigID; set { _secondHandConfigID = value; _secondHandConfig = null; } }

        ClockHandConfig _secondHandConfig = null;
        [XmlIgnore]
        public ClockHandConfig SecondHandConfig
        {
            get
            {
                if (_secondHandConfig == null)
                    _secondHandConfig = ClockHandConfig.Get(_secondHandConfigID);
                return _secondHandConfig;
            }
        }

        string _capConfigID = string.Empty;
        public string CapConfigID { get => _capConfigID; set { _capConfigID = value; _capConfig = null; } }

        ClockHandConfig _capConfig = null;
        [XmlIgnore]
        public ClockHandConfig CapConfig
        {
            get
            {
                if (_capConfig == null)
                    _capConfig = ClockHandConfig.Get(_capConfigID);
                return _capConfig;
            }
        }


        public string BackgroundImage;
        public xColor BackgroundImageTint = xColor.Transparent;
        public TickMarkStyle TickMarkStyle = TickMarkStyle.Dotts;

        public xColor ColorTickMarks = xColor.White;
        public xColor ColorHourHandStroke = xColor.White;
        public xColor ColorHourHandFill = xColor.FromHex("#FF620000");
        public xColor ColorMinuteHandStroke = xColor.White;
        public xColor ColorMinuteHandFill = xColor.FromHex("#FF930000");
        public xColor ColorSecondHandStroke = xColor.White;
        public xColor ColorSecondHandFill = xColor.FromHex("#FFBC0000");
        public xColor ColorCenterCapStroke = xColor.White;
        public xColor ColorCenterCapFill = xColor.FromHex("#FFBC0000");

        public void SetDefaultColors()
        {
            ColorHourHandStroke = xColor.MakeEmptyColor(ColorHourHandStroke);
            ColorHourHandFill = xColor.MakeEmptyColor(ColorHourHandStroke);
            ColorMinuteHandStroke = xColor.MakeEmptyColor(ColorHourHandStroke);
            ColorMinuteHandFill = xColor.MakeEmptyColor(ColorHourHandStroke);
            ColorSecondHandStroke = xColor.MakeEmptyColor(ColorHourHandStroke);
            ColorSecondHandFill = xColor.MakeEmptyColor(ColorHourHandStroke);
            ColorCenterCapStroke = xColor.MakeEmptyColor(ColorCenterCapStroke);
            ColorCenterCapFill = xColor.MakeEmptyColor(ColorCenterCapFill);

            if (!string.IsNullOrEmpty(BackgroundImage) && BackImageAllowsBackColor)
                ColorBackground = xColor.White;

            if (string.IsNullOrEmpty(BackgroundImage) && ColorTickMarks.A > 0)
            {
                if (ColorBackground.IsSimilar(ColorTickMarks))
                    ColorTickMarks = ColorTickMarks.Invert();
            }
        }

        public void ApplyUserColors()
        {
            ColorHourHandStroke = xColor.MakeNonEmptyColor(ColorHourHandStroke);
            ColorHourHandFill = xColor.MakeNonEmptyColor(ColorHourHandStroke);
            ColorMinuteHandStroke = xColor.MakeNonEmptyColor(ColorHourHandStroke);
            ColorMinuteHandFill = xColor.MakeNonEmptyColor(ColorHourHandStroke);
            ColorSecondHandStroke = xColor.MakeNonEmptyColor(ColorHourHandStroke);
            ColorSecondHandFill = xColor.MakeNonEmptyColor(ColorHourHandStroke);
            ColorCenterCapStroke = xColor.MakeNonEmptyColor(ColorCenterCapStroke);
            ColorCenterCapFill = xColor.MakeNonEmptyColor(ColorCenterCapFill);
        }

        public bool BackImageAllowsBackColor { get => !string.IsNullOrEmpty(BackgroundImage) && BackgroundImage.Contains("01_simple_face"); }
    }

    public enum WidgetCfgPositionType
    {
        [XmlEnum(Name = "0")]
        None = 0,
        [XmlEnum(Name = "10")]
        StaticPosition = 10,
        [XmlEnum(Name = "20")]
        LivePosition = 20
    }

    public enum TickMarkStyle
    {
        None = 0,
        Dotts = 1,
        Circle = 5,
        LinesRound = 10,
        LinesSquare = 11,
        LinesButt = 12
    }

    public enum WidgetTheme
    {
        TransparentLight,
        TransparentDark,
        Light,
        Dark,
        White,
        Black,
        LightGlass1,
        LightGlass2,
        LightGlass3,
        LightGlass4,
        LightGlass5,
        DarkGlass1,
        DarkGlass2,
        DarkGlass3,
        DarkGlass4,
        DarkGlass5,
    }
}