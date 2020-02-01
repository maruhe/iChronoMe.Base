using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace iChronoMe.Core.DynamicCalendar
{
    public class DynamicDateFormatInfo : ICloneable
    {
        static DynamicDateFormatInfo _invariantInfo;
        [XmlIgnore]
        public static DynamicDateFormatInfo InvariantInfo {
            get {
                if (_invariantInfo == null)
                    _invariantInfo = new DynamicDateFormatInfo(DateTimeFormatInfo.InvariantInfo);
                return _invariantInfo;
            }
        }
        //[XmlIgnore]
        //public static DynamicDateFormatInfo CurrentInfo { get; }

        [XmlIgnore] public string CalendarModelID { get; private set; }

        [XmlAttribute] public DateFormatOrderStyle OrderStyle { get; set; }
        [XmlAttribute] public string ShortDatePattern { get; set; }
        [XmlAttribute] public string MiddleDatePattern { get; set; }        
        [XmlAttribute] public string LongDatePattern { get; set; }

        [XmlAttribute] public string ShortDayPattern { get; set; }
        [XmlAttribute] public string ShortMonthPattern { get; set; }
        [XmlAttribute] public string ShortYearPattern { get; set; }
        [XmlAttribute] public string ShortWeekDayDayPattern { get; set; }
        [XmlAttribute] public string ShortMonthDayPattern { get; set; }
        [XmlAttribute] public string ShortYearMonthPattern { get; set; }

        [XmlAttribute] public string MiddleDayPattern { get; set; }
        [XmlAttribute] public string MiddleMonthPattern { get; set; }
        [XmlAttribute] public string MiddleYearPattern { get; set; }
        [XmlAttribute] public string MiddleWeekDayDayPattern { get; set; }
        [XmlAttribute] public string MiddleMonthDayPattern { get; set; }
        [XmlAttribute] public string MiddleYearMonthPattern { get; set; }

        [XmlAttribute] public string LongDayPattern { get; set; }
        [XmlAttribute] public string LongMonthPattern { get; set; }
        [XmlAttribute] public string LongYearPattern { get; set; }
        [XmlAttribute] public string LongWeekDayDayPattern { get; set; }
        [XmlAttribute] public string LongMonthDayPattern { get; set; }
        [XmlAttribute] public string LongYearMonthPattern { get; set; }

        [XmlAttribute] public int OverrideMaxYearDigits { get; set; }
        [XmlAttribute] public string DateSeparator { get; set; }
        [XmlIgnore] public string RFC1123Pattern { get; private set; }
        [XmlIgnore] public string SortableDatePattern { get; private set; }

        static Dictionary<string, DynamicDateFormatInfo> infoCacheS = new Dictionary<string, DynamicDateFormatInfo>();

        public DynamicDateFormatInfo()
        {

        }

        public DynamicDateFormatInfo(DateTimeFormatInfo dtfi)
        {
            OrderStyle = DateFormatOrderStyle.DMY;

            Regex MyRegex = new Regex("[^a-z]", RegexOptions.IgnoreCase);
            string cOrderStyle = MyRegex.Replace(dtfi.ShortDatePattern, "");
            cOrderStyle = cOrderStyle.Replace("dd", "d").Replace("MM", "M").Replace("yy", "y").Replace("dd", "d").Replace("MM", "M").Replace("yy", "y");
            try
            {
                OrderStyle = (DateFormatOrderStyle)Enum.Parse(typeof(DateFormatOrderStyle), cOrderStyle.ToUpper());
            } catch { }

            MiddleDatePattern = xf(dtfi.ShortDatePattern);
            ShortDatePattern = MiddleDatePattern.Replace("dd", "d").Replace("MM", "M").Replace("yyyy", "yy");
            LongDatePattern = xf(dtfi.LongDatePattern);

            ShortDayPattern = "%d";
            ShortMonthPattern = "%M";
            ShortYearPattern = "yy";
            ShortWeekDayDayPattern = "ddd, d";
            ShortMonthDayPattern = "d.M.";
            ShortYearMonthPattern = "M y";

            MiddleDayPattern = "ddd";
            MiddleMonthPattern = "MMM";
            MiddleYearPattern = "yyyy";
            MiddleWeekDayDayPattern = "ddd, d";
            MiddleMonthDayPattern = dtfi.MonthDayPattern.TrimEnd('.').Replace("MMMM", "MMM");
            MiddleYearMonthPattern = dtfi.YearMonthPattern.Replace("MMMM", "MMM");

            LongDayPattern = "dddd";
            LongMonthPattern = "MMMM";
            LongYearPattern = "yyyy";
            LongWeekDayDayPattern = "dddd, d";
            LongMonthDayPattern = dtfi.MonthDayPattern.TrimEnd('.');
            LongYearMonthPattern = dtfi.YearMonthPattern;

            DateSeparator = dtfi.DateSeparator;
            RFC1123Pattern = dtfi.RFC1123Pattern.Substring(0, dtfi.RFC1123Pattern.IndexOf("HH")).TrimEnd();
            SortableDatePattern = dtfi.SortableDateTimePattern.Substring(0, dtfi.SortableDateTimePattern.IndexOf("HH"));

            string xf(string c)
            {
                if (!c.Contains("MMM"))
                    c = c.Replace('.', '/').Replace('-', '/').Replace('\\', '/');
                return c;
            }
        }

        public void Import(DynamicDateFormatInfo other)
        {
            if (other == null)
                return;
            if (other.OrderStyle != DateFormatOrderStyle.Default)
                OrderStyle = other.OrderStyle;
            if (!string.IsNullOrEmpty(other.ShortDatePattern))
                ShortDatePattern = other.ShortDatePattern;
            if (!string.IsNullOrEmpty(other.ShortDayPattern))
                ShortDayPattern = other.ShortDayPattern;
            if (!string.IsNullOrEmpty(other.ShortMonthPattern))
                ShortMonthPattern = other.ShortMonthPattern;
            if (!string.IsNullOrEmpty(other.ShortYearPattern))
                ShortYearPattern = other.ShortYearPattern;
            if (!string.IsNullOrEmpty(other.ShortWeekDayDayPattern))
                ShortWeekDayDayPattern = other.ShortWeekDayDayPattern;
            if (!string.IsNullOrEmpty(other.ShortMonthDayPattern))
                ShortMonthDayPattern = other.ShortMonthDayPattern;
            if (!string.IsNullOrEmpty(other.ShortYearMonthPattern))
                ShortYearMonthPattern = other.ShortYearMonthPattern;

            if (!string.IsNullOrEmpty(other.MiddleDatePattern))
                MiddleDatePattern = other.MiddleDatePattern;
            if (!string.IsNullOrEmpty(other.MiddleDayPattern))
                MiddleDayPattern = other.MiddleDayPattern;
            if (!string.IsNullOrEmpty(other.MiddleMonthPattern))
                MiddleMonthPattern = other.MiddleMonthPattern;
            if (!string.IsNullOrEmpty(other.MiddleYearPattern))
                MiddleYearPattern = other.MiddleYearPattern;
            if (!string.IsNullOrEmpty(other.MiddleWeekDayDayPattern))
                MiddleWeekDayDayPattern = other.MiddleWeekDayDayPattern;
            if (!string.IsNullOrEmpty(other.MiddleMonthDayPattern))
                MiddleMonthDayPattern = other.MiddleMonthDayPattern;
            if (!string.IsNullOrEmpty(other.MiddleYearMonthPattern))
                MiddleYearMonthPattern = other.MiddleYearMonthPattern;

            if (!string.IsNullOrEmpty(other.LongDatePattern))
                LongDatePattern = other.LongDatePattern;
            if (!string.IsNullOrEmpty(other.LongDayPattern))
                LongDayPattern = other.LongDayPattern;
            if (!string.IsNullOrEmpty(other.LongMonthPattern))
                LongMonthPattern = other.LongMonthPattern;
            if (!string.IsNullOrEmpty(other.LongYearPattern))
                LongYearPattern = other.LongYearPattern;
            if (!string.IsNullOrEmpty(other.LongWeekDayDayPattern))
                LongWeekDayDayPattern = other.LongWeekDayDayPattern;
            if (!string.IsNullOrEmpty(other.LongMonthDayPattern))
                LongMonthDayPattern = other.LongMonthDayPattern;
            if (!string.IsNullOrEmpty(other.LongYearMonthPattern))
                LongYearMonthPattern = other.LongYearMonthPattern;

            if (other.OverrideMaxYearDigits > 0)
                OverrideMaxYearDigits = other.OverrideMaxYearDigits;
            if (!string.IsNullOrEmpty(other.DateSeparator))
                DateSeparator = other.DateSeparator;

            if (!string.IsNullOrEmpty(other.RFC1123Pattern))
                RFC1123Pattern = other.RFC1123Pattern;
            if (!string.IsNullOrEmpty(other.SortableDatePattern))
                SortableDatePattern = other.SortableDatePattern;
        }

        public static DynamicDateFormatInfo GetInstance()
        {
            var provider = CultureInfo.CurrentCulture;
            return GetInstance("", provider);
        }

        public static DynamicDateFormatInfo GetInstance(string calendarModelId, IFormatProvider provider = null)
        {
            if (provider == null)
                provider = CultureInfo.CurrentCulture;
            string cInfId = calendarModelId + "_" + provider.ToString();
            if (infoCacheS.ContainsKey(cInfId))
                return infoCacheS[cInfId];
            DateTimeFormatInfo x = DateTimeFormatInfo.GetInstance(provider);
            var inf = new DynamicDateFormatInfo(x);

            if (string.IsNullOrEmpty(calendarModelId))
               return (DynamicDateFormatInfo)inf.Clone();
            
            var model = DynamicCalendarModel.GetCachedModel(calendarModelId);
            if (model != null && model.FormatInfo != null)
            {
                inf.CalendarModelID = calendarModelId;

                var mi = model.FormatInfo;
                inf.Import(mi);
            }

            return inf;
        }

        public static void ResetInstances(string calendarModelId)
        {
            try
            {
                if (string.IsNullOrEmpty(calendarModelId))
                    infoCacheS.Clear();
                else
                {
                    var keyS = new List<string>(infoCacheS.Keys);
                    foreach (string key in keyS)
                    {
                        try
                        {
                            if (key.StartsWith(calendarModelId))
                                infoCacheS.Remove(key);
                        } catch (Exception ex)
                        {
                            ex.ToString();
                        }
                    }
                }
            }
            catch (Exception ex2)
            {
                ex2.ToString();
            }
        }

        public static string ExpandDynamicPredefinedFormat(String format, DynamicDateFormatInfo ddfi)
        {
            switch (format)
            {
                case "_sdt":
                    return ddfi.ShortDatePattern;
                case "_sd":
                    return ddfi.ShortDayPattern;
                case "_sM":
                    return ddfi.ShortMonthPattern;
                case "_sy":
                    return ddfi.ShortYearPattern;
                case "_swd":
                    return ddfi.ShortWeekDayDayPattern;
                case "_sMd":
                    return ddfi.ShortMonthDayPattern;
                case "_syM":
                    return ddfi.ShortYearMonthPattern;

                case "_mdt":
                    return ddfi.MiddleDatePattern;
                case "_md":
                    return ddfi.MiddleDayPattern;
                case "_mM":
                    return ddfi.MiddleMonthPattern;
                case "_my":
                    return ddfi.MiddleYearPattern;
                case "_mwd":
                    return ddfi.MiddleWeekDayDayPattern;
                case "_mMd":
                    return ddfi.MiddleMonthDayPattern;
                case "_myM":
                    return ddfi.MiddleYearMonthPattern;

                case "_ldt":
                    return ddfi.LongDatePattern;
                case "_ld":
                    return ddfi.LongDayPattern;
                case "_lM":
                    return ddfi.LongMonthPattern;
                case "_ly":
                    return ddfi.LongYearPattern;
                case "_lwd":
                    return ddfi.LongWeekDayDayPattern;                    
                case "_lMd":
                    return ddfi.LongMonthDayPattern;
                case "_lyM":
                    return ddfi.LongYearMonthPattern;
            }
            return format;
        }

        public Object Clone()
        {
            return (DynamicDateFormatInfo)MemberwiseClone();
        }
    }

    public class DateFormatPattern
    {
        public DateFormatPattern(string cFormat, DateFormatPatternType pt, DateFormatPatternLength pl, string cHint = null)
        {
            DateFormat = cFormat;
            Type = pt;
            Length = pl;
            Hint = cHint;
        }

        public string DateFormat { get; }
        public DateFormatPatternType Type { get; }
        public DateFormatPatternLength Length { get; }
        public string Hint { get; }

        public override int GetHashCode()
        {
            unchecked
            {
                return (GetType().GetHashCode() * 16777619) ^ DateFormat.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is DateFormatPattern)
                return GetHashCode() == obj.GetHashCode();
            if (obj is string)
                return DateFormat.Equals(obj);
            return false;
        }

        public override string ToString()
        {
            return DateFormat;
        }

        static List<DateFormatPattern> _dateFormatPatternSamples;
        static Dictionary<string, string> _dateFormatPatternSampleHints;
        public static List<DateFormatPattern> DateFormatPatternSamples
        {
            get
            {
                if (_dateFormatPatternSamples == null)
                {
                    _dateFormatPatternSamples = new List<DateFormatPattern>();
                    _dateFormatPatternSampleHints = new Dictionary<string, string>();

                    AddCulturePattern(CultureInfo.InstalledUICulture);
                    AddCulturePattern(CultureInfo.CurrentUICulture);

                    AddPattern("%d", DateFormatPatternType.Day, DateFormatPatternLength.Short);
                    AddPattern("dd", DateFormatPatternType.Day, DateFormatPatternLength.Short);
                    AddPattern("ddd", DateFormatPatternType.Day, DateFormatPatternLength.Middle, "WeekDayShort");
                    AddPattern("dddd", DateFormatPatternType.Day, DateFormatPatternLength.Long, "WeekDayFull");
                    AddPattern("DDD", DateFormatPatternType.Day, DateFormatPatternLength.Middle, "MonthDayShort");
                    AddPattern("DDDD", DateFormatPatternType.Day, DateFormatPatternLength.Long, "MonthDayFull");
                    AddPattern("%M", DateFormatPatternType.Month, DateFormatPatternLength.Short);
                    AddPattern("MM", DateFormatPatternType.Month, DateFormatPatternLength.Short);
                    AddPattern("MMM", DateFormatPatternType.Month, DateFormatPatternLength.Middle);
                    AddPattern("MMMM", DateFormatPatternType.Month, DateFormatPatternLength.Long);
                    AddPattern("%y", DateFormatPatternType.Year, DateFormatPatternLength.Short);
                    AddPattern("yy", DateFormatPatternType.Year, DateFormatPatternLength.Short);
                    AddPattern("yyy", DateFormatPatternType.Year, DateFormatPatternLength.Middle);
                    AddPattern("yyyyy", DateFormatPatternType.Year, DateFormatPatternLength.Long);

                    //DMY
                    AddPattern("d/M/yy", DateFormatPatternType.Date, DateFormatPatternLength.Short);
                    AddPattern("dd/MM/yy", DateFormatPatternType.Date, DateFormatPatternLength.Short);
                    AddPattern("dd/MM/yyyy", DateFormatPatternType.Date, DateFormatPatternLength.Short);

                    AddPattern("dd/MMM/yyyy", DateFormatPatternType.Date, DateFormatPatternLength.Middle);
                    AddPattern("dd/MMMM yyyy", DateFormatPatternType.Date, DateFormatPatternLength.Long);
                    AddPattern("ddd, dd/MMMM yyyy", DateFormatPatternType.Date, DateFormatPatternLength.Long);
                    AddPattern("dddd, dd/MMMM yyyy", DateFormatPatternType.Date, DateFormatPatternLength.Long);

                    //MDY
                    AddPattern("M/d/yy", DateFormatPatternType.Date, DateFormatPatternLength.Short);
                    AddPattern("MM/dd/yy", DateFormatPatternType.Date, DateFormatPatternLength.Short);
                    AddPattern("MM/dd/yyyy", DateFormatPatternType.Date, DateFormatPatternLength.Short);

                    AddPattern("MMM d, yyyy", DateFormatPatternType.Date, DateFormatPatternLength.Middle);
                    AddPattern("MMMM d, yyyy", DateFormatPatternType.Date, DateFormatPatternLength.Middle | DateFormatPatternLength.Long);
                    AddPattern("ddd, MMMM d, yyyy", DateFormatPatternType.Date, DateFormatPatternLength.Long);
                    AddPattern("dddd, MMMM d, yyyy", DateFormatPatternType.Date, DateFormatPatternLength.Long);

                    //YMD
                    AddPattern("yy/M/d", DateFormatPatternType.Date, DateFormatPatternLength.Short);
                    AddPattern("yy/MM/dd", DateFormatPatternType.Date, DateFormatPatternLength.Short);
                    AddPattern("yyyy/MM/dd", DateFormatPatternType.Date, DateFormatPatternLength.Short);

                    AddPattern("yyyy MMM d", DateFormatPatternType.Date, DateFormatPatternLength.Middle);
                    AddPattern("yyyy MMMM d", DateFormatPatternType.Date, DateFormatPatternLength.Middle | DateFormatPatternLength.Long);

                    AddCulturePattern(CultureInfo.InvariantCulture);

                    //Specials
                    AddPattern("'Kin' &MayaHaabKin&", DateFormatPatternType.Special, DateFormatPatternLength.Middle, "Maya Kin");
                    AddPattern("&MayaDate&", DateFormatPatternType.Special, DateFormatPatternLength.Middle | DateFormatPatternLength.Long, "Maya Langzeitangabe");
                    AddPattern("&MayaDateHaab&", DateFormatPatternType.Special, DateFormatPatternLength.Middle | DateFormatPatternLength.Long, "Maya Haab-Datum");
                    AddPattern("&MayaDateTzolkin&", DateFormatPatternType.Special, DateFormatPatternLength.Middle | DateFormatPatternLength.Long, "Maya Tzolkin-Datum");

                    _dateFormatPatternSamples.ToString();

                }
                return _dateFormatPatternSamples;

                void AddPattern(string cFormat, DateFormatPatternType pt, DateFormatPatternLength pl, string cHint = null)
                {
                    var p = new DateFormatPattern(cFormat, pt, pl, cHint);
                    AddPatternObject(p);
                }
                void AddPatternObject(DateFormatPattern p)
                {
                    if (!_dateFormatPatternSamples.Contains(p))
                    {
                        _dateFormatPatternSamples.Add(p);
                        if (!string.IsNullOrEmpty(p.Hint))
                            _dateFormatPatternSampleHints.Add(p.DateFormat, p.Hint);
                    }
                }

                void AddCulturePattern(IFormatProvider provider)
                {
                    var dtfi = DateTimeFormatInfo.GetInstance(provider);
                    var fi = new DynamicDateFormatInfo(dtfi);
                    foreach (var p in GetDateFormatPatternCultureSamples(provider))
                        AddPatternObject(p);
                }
            }
        }

        public static List<DateFormatPattern> GetDateFormatPatternCultureSamples(string cCultureId)
        {
            return GetDateFormatPatternCultureSamples(CultureInfo.GetCultureInfo(cCultureId));
        }

        public static List<DateFormatPattern> GetDateFormatPatternCultureSamples(IFormatProvider provider)
        {
            var res = new List<DateFormatPattern>();
            try
            {
                var dtfi = DateTimeFormatInfo.GetInstance(provider);
                var fi = new DynamicDateFormatInfo(dtfi);

                res.Add(new DateFormatPattern(fi.ShortDatePattern, DateFormatPatternType.Date, DateFormatPatternLength.Short));
                res.Add(new DateFormatPattern(fi.MiddleDatePattern, DateFormatPatternType.Date, DateFormatPatternLength.Middle));
                res.Add(new DateFormatPattern(fi.LongDatePattern, DateFormatPatternType.Date, DateFormatPatternLength.Long));

                res.Add(new DateFormatPattern(fi.ShortDayPattern, DateFormatPatternType.Day, DateFormatPatternLength.Short));
                res.Add(new DateFormatPattern(fi.ShortMonthPattern, DateFormatPatternType.Month, DateFormatPatternLength.Short));
                res.Add(new DateFormatPattern(fi.ShortYearPattern, DateFormatPatternType.Year, DateFormatPatternLength.Short));
                res.Add(new DateFormatPattern(fi.ShortWeekDayDayPattern, DateFormatPatternType.WeekDayDay, DateFormatPatternLength.Short));
                res.Add(new DateFormatPattern(fi.ShortMonthDayPattern, DateFormatPatternType.MonthDay, DateFormatPatternLength.Short));
                res.Add(new DateFormatPattern(fi.ShortYearMonthPattern, DateFormatPatternType.YearMonth, DateFormatPatternLength.Short));

                res.Add(new DateFormatPattern(fi.MiddleDayPattern, DateFormatPatternType.Day, DateFormatPatternLength.Middle));
                res.Add(new DateFormatPattern(fi.MiddleMonthPattern, DateFormatPatternType.Month, DateFormatPatternLength.Middle));
                res.Add(new DateFormatPattern(fi.MiddleYearPattern, DateFormatPatternType.Year, DateFormatPatternLength.Middle));
                res.Add(new DateFormatPattern(fi.MiddleWeekDayDayPattern, DateFormatPatternType.WeekDayDay, DateFormatPatternLength.Middle));
                res.Add(new DateFormatPattern(fi.MiddleMonthDayPattern, DateFormatPatternType.MonthDay, DateFormatPatternLength.Middle));
                res.Add(new DateFormatPattern(fi.MiddleYearMonthPattern, DateFormatPatternType.YearMonth, DateFormatPatternLength.Middle));

                res.Add(new DateFormatPattern(fi.LongDayPattern, DateFormatPatternType.Day, DateFormatPatternLength.Long));
                res.Add(new DateFormatPattern(fi.LongMonthPattern, DateFormatPatternType.Month, DateFormatPatternLength.Long));
                res.Add(new DateFormatPattern(fi.LongYearPattern, DateFormatPatternType.Year, DateFormatPatternLength.Long));
                res.Add(new DateFormatPattern(fi.LongWeekDayDayPattern, DateFormatPatternType.WeekDayDay, DateFormatPatternLength.Long));
                res.Add(new DateFormatPattern(fi.LongMonthDayPattern, DateFormatPatternType.MonthDay, DateFormatPatternLength.Long));
                res.Add(new DateFormatPattern(fi.LongYearMonthPattern, DateFormatPatternType.YearMonth, DateFormatPatternLength.Long));
            }
            catch { }
            return res;
        }
    }

    [FlagsAttribute]
    public enum DateFormatPatternType
    {
        None = 0,

        Day = 1,
        Week = 2,
        Month = 4,
        Year = 8,

        MonthDay = 64,
        WeekDayDay = 128,
        YearMonth = 512,

        Date = 2048,

        Special = 4096
    }

    [FlagsAttribute]
    public enum DateFormatPatternLength
    {
        None = 0,

        Short = 1,
        Middle = 2,
        Long = 4,
        //Full,
        Sortable = 32,
        Universal = 64,
        //UniversalFull,
        UniversalSortable = 128
    }

    public enum DateFormatOrderStyle
    {
        Default = 0,
        DMY = 1,
        YMD = 2,
        MDY = 3
    }
}
