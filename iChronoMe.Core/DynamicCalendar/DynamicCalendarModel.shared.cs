using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Xml.Serialization;

using iChronoMe.Core.Classes;
using iChronoMe.Core.Types;

namespace iChronoMe.Core.DynamicCalendar
{
    public class DynamicCalendarModel
    {
        public static DynamicCalendarModel CurrentModel { get; set; }
        static Dictionary<string, DynamicCalendarModel> modelCache = new Dictionary<string, DynamicCalendarModel>();
        static Dictionary<string, string> modelCacheVersionS = new Dictionary<string, string>();

        public static IComputingDialog ComputingDialog { get; set; }

        public static void AddCachedModel(DynamicCalendarModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.ID))
                return;
            if (modelCache.ContainsKey(model.ID))
                modelCache.Remove(model.ID);
            if (modelCacheVersionS.ContainsKey(model.ID))
                modelCacheVersionS.Remove(model.ID);

            modelCache.Add(model.ID, model);
            modelCacheVersionS.Add(model.ID, model.VersionID);
        }
        public static DynamicCalendarModel GetCachedModel(string cId)
        {
            if (modelCache.ContainsKey(cId))
                return modelCache[cId];
            return new CalendarModelCfgHolder().GetModelCfg(cId, true);
        }

        public static string GetCachedModelVersion(string cId)
        {
            if (modelCacheVersionS.ContainsKey(cId))
                return modelCacheVersionS[cId];
            return null;
        }

        [XmlIgnore]
        mySQLiteConnection dbCache = null;
        [XmlIgnore]
        public mySQLiteConnection dbUserData = null;
        [XmlIgnore]
        public DateTime _LastChange = DateTime.MinValue;
        [XmlIgnore]
        internal string ID { get; set; }
        [XmlIgnore]
        internal string BackupID { get; set; } //for Editing-Mode
        [XmlAttribute]
        public string ModelID
        {
            get
            {
                if (!string.IsNullOrEmpty(BackupID))
                    return BackupID;
                return ID;
            }
            set
            {
                ID = value;
            }
        }
        [XmlAttribute]
        public string VersionID { get; set; } = Guid.NewGuid().ToString();
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Description { get; set; }
        [XmlAttribute]
        public string BaseSample { get; set; }
        [XmlAttribute]
        public WeekStartType YearWeekStartType { get; set; } = WeekStartType.OngoingWeeks;
        [XmlAttribute]
        public WeekStartType MonthWeekStartType { get; set; } = WeekStartType.OngoingWeeks;
        [XmlAttribute]
        public int WeekLength { get; set; } = 7;
        [XmlAttribute]
        public int FirstDayOfWeek { get; set; } = 1;
        [XmlAttribute]
        public int FirstFullWeekMinLength { get; set; } = 4;
        [XmlAttribute]
        public bool ContinueWeeksOutOfTime { get; set; }
        [XmlElement]
        public List<WeekDay> WeekDays { get; set; } = new List<WeekDay>();
        [XmlAttribute]
        public FirstMonthFirstDayType FirstMonthFirstDayType { get; set; } = FirstMonthFirstDayType.NumberOne;
        [XmlElement]
        public TimePoint ModelStartPoint { get; set; }
        [XmlAttribute]
        public int ModelStartYearID { get; set; } = 0;
        [XmlAttribute]
        public int ModelStartYearNumber { get; set; } = 0;
        [XmlAttribute]
        public int ModelStartDayOfWeek { get; set; } = -1;
        [XmlAttribute]
        public int ModelStartFirstMonthFirstDayNumber { get; set; } = -1;
        [XmlAttribute]
        public MayaCorrelation MayaCorrelation { get; set; } = MayaCorrelation.Default;
        [XmlAttribute]
        public DateTime ValidFrom { get; set; }
        [XmlAttribute]
        public string PreValidCalendarModelID { get; set; }
        [XmlAttribute]
        public DateTime ValidUntil { get; set; }
        [XmlAttribute]
        public string PostValidCalendarModelID { get; set; }
        [XmlAttribute]
        public string MonthsTitle { get; set; }
        [XmlAttribute]
        public string MonthFullNameTemplate { get; set; }
        [XmlAttribute]
        public string MonthShortNameTemplate { get; set; }
        [XmlAttribute]
        public MonthNumberType MonthNumberType { get; set; } = MonthNumberType.OneBased;
        [XmlElement]
        public List<Month> Months { get; set; } = new List<Month>();
        [XmlElement]
        public List<OutOfTimeSection> OutOfTimeSections { get; set; } = new List<OutOfTimeSection>();
        [XmlAttribute]
        public ExtraDaysAssignment YearLengthAssignment { get; set; } = ExtraDaysAssignment.None;
        [XmlAttribute]
        public int YearLengthAssigBeforeMonth { get; set; } = -1;
        [XmlElement]
        public TimeOffset CommonYearLength { get; set; }
        [XmlElement]
        public LeapModel LeapModel { get; set; } = new LeapModel();
        [XmlElement]
        public DynamicDateFormatInfo FormatInfo { get; set; } = new DynamicDateFormatInfo();

        private Dictionary<int, OutOfTimeSection> _outOfTimeSectionsDict = null;
        [XmlIgnore]
        public Dictionary<int, OutOfTimeSection> OutOfTimeSectionsDict
        {
            get
            {
                if (_outOfTimeSectionsDict == null)
                {
                    _outOfTimeSectionsDict = new Dictionary<int, OutOfTimeSection>();
                    foreach (var o in OutOfTimeSections)
                        _outOfTimeSectionsDict.Add(o.BeforeMonth, o);
                }
                return _outOfTimeSectionsDict;
            }
        }

        public bool IsLeapYear(int iYearID)
        {
            bool bRes = false;
            if (_leapYears.TryGetValue(iYearID, out bRes))
                return bRes;

            if (LeapModel != null)
            {
                try
                {
                    switch (LeapModel.ConditionType)
                    {
                        case LeapConditionType.Algorithm:

                            foreach (var yc in LeapModel.LeapYearAlgo)
                            {
                                if (GetYearNumber(iYearID) % yc.Dividor == 0)
                                    bRes = yc.IsLeap;
                            }
                            break;
                        case LeapConditionType.AstronomicCondition:
                            if (LeapModel.AstroConditionOffset != null)
                            {
                                int iAstroDays = LeapModel.AstroConditionOffset.GetOffset(GetYearInfo(iYearID).UtcStart, this).Days;
                                switch (LeapModel.AstroConditionOperator)
                                {
                                    case CompareOperator.Equal:
                                        bRes = iAstroDays == LeapModel.AstroConditionYearDay;
                                        break;
                                    case CompareOperator.NotEqual:
                                        bRes = iAstroDays != LeapModel.AstroConditionYearDay;
                                        break;
                                    case CompareOperator.Smaller:
                                        bRes = iAstroDays < LeapModel.AstroConditionYearDay;
                                        break;
                                    case CompareOperator.SmallerOrEqual:
                                        bRes = iAstroDays <= LeapModel.AstroConditionYearDay;
                                        break;
                                    case CompareOperator.Greater:
                                        bRes = iAstroDays > LeapModel.AstroConditionYearDay;
                                        break;
                                    case CompareOperator.GreaterOrEqual:
                                        bRes = iAstroDays >= LeapModel.AstroConditionYearDay;
                                        break;
                                }
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    AddException(ex);
                }
            }

            _leapYears.Add(iYearID, bRes);

            return bRes;
        }

        bool? _hasAstronomics = null;
        public bool HasAstronomics
        {
            get
            {
                if (_hasAstronomics == null)
                {
                    _hasAstronomics = CommonYearLength.HasAstronomics || (ModelStartPoint.Offset?.HasAstronomics ?? false);
                    if (!_hasAstronomics.Value && LeapModel != null)
                    {
                        _hasAstronomics = LeapModel.AstroConditionOffset != null || (LeapModel.LeapOffset?.HasAstronomics ?? false);
                    }
                    if (!_hasAstronomics.Value)
                    {
                        foreach (var m in Months)
                        {
                            if (m.Length.HasAstronomics)
                            {
                                _hasAstronomics = true;
                                break;
                            }
                        }
                    }
                }
                return _hasAstronomics.Value;
            }
        }

        List<int> _loadetMonthCacheYears = new List<int>();
        SortedDictionary<int, int> _daysOfYears = new SortedDictionary<int, int>();
        SortedDictionary<int, int> _monthsOfYears = new SortedDictionary<int, int>();
        SortedDictionary<int, int> _daysOfMonths = new SortedDictionary<int, int>();
        SortedDictionary<int, int> _daysOutOfTime = new SortedDictionary<int, int>();
        SortedDictionary<int, int> _beginsOfMonths = new SortedDictionary<int, int>();
        SortedDictionary<int, YearInfo> _yearInfos = new SortedDictionary<int, YearInfo>();
        SortedDictionary<int, OutOfTimeInfo> _outOfTimeInfos = new SortedDictionary<int, OutOfTimeInfo>();
        SortedDictionary<int, MonthInfo> _monthInfos = new SortedDictionary<int, MonthInfo>();
        SortedDictionary<int, MonthInfo> _monthUserData = new SortedDictionary<int, MonthInfo>();
        SortedDictionary<int, bool> _leapYears = new SortedDictionary<int, bool>();
        ObservableCollection<Exception> _exceptions = new ObservableCollection<Exception>();

        DynamicCalendarModel _preValidCalendarModel;
        DynamicCalendarModel _postValidCalendarModel;

        [XmlIgnore]
        public ObservableCollection<Exception> Exceptions { get => _exceptions; }

        internal void EnterEditableMode()
        {
            if (!string.IsNullOrEmpty(BackupID))
                throw new Exception("Model is already in EditableMode!");
            BackupID = ID;
            ID = "_EditMode_" + BackupID;
            AddCachedModel(this);
        }

        internal void EnterTemporaryMode()
        {
            if (!string.IsNullOrEmpty(BackupID))
                throw new Exception("Model is already in EditableMode!");
            BackupID = ID;
            ID = "_TempMode_" + Guid.NewGuid().ToString();
            AddCachedModel(this);
        }

        public void ClearCache()
        {
            lock (_yearInfos)
            {
                _hasAstronomics = null;
                _outOfTimeSectionsDict = null;
                _exceptions.Clear();

                _preValidCalendarModel = null;
                _postValidCalendarModel = null;

                _loadetMonthCacheYears.Clear();
                _daysOfYears.Clear();
                _monthsOfYears.Clear();
                _daysOfMonths.Clear();
                _daysOutOfTime.Clear();
                _beginsOfMonths.Clear();
                _yearInfos.Clear();
                _monthInfos.Clear();
                _leapYears.Clear();

                _monthUserData.Clear();

                if (dbCache != null)
                {
                    dbCache.DropTable<YearInfo>();
                    dbCache.CreateTable<YearInfo>();
                    dbCache.DropTable<MonthInfo>();
                    dbCache.CreateTable<MonthInfo>();
                }
            }
        }

        public int GetDaysOfMonth(int iYear, int iMonth)
        {
            if (iMonth < 0)
                return 0;
            bool bSaveToCache = HasAstronomics;
            lock (_yearInfos)
            {
                if (_monthsOfYears.ContainsKey(iYear))
                {
                    while (iMonth >= GetMonthsOfYear(iYear))
                    {
                        iMonth -= GetMonthsOfYear(iYear);
                        iYear++;
                    }
                }
                int iYearMonth = GetYearMonth(iYear, iMonth);

                if (dbUserData == null || _yearInfos.Count == 0)
                {
                    xLog.Debug("Load dbUserData");
                    //Load Cache on first Access
                    dbUserData = db.GetCalendarModelUserData(ModelID);

                    var yCache = dbUserData.Query<YearInfo>("select * from YearInfo", new object[0]);
                    foreach (var x in yCache)
                        try { _yearInfos.Add(x.Year, x); } catch { };
                }
                if (dbCache == null || _yearInfos.Count == 0)
                {
                    xLog.Debug("Load dbCache");
                    //Load Cache on first Access
                    dbCache = db.GetCalendarModelCache(ModelID);

                    //sqlite problem with datetimes!!!
                    //var yCache = dbCache.Query<YearInfo>("select * from YearInfo", new object[0]);
                    //foreach (var x in yCache)
                    //    try { _yearInfos.Add(x.Year, x); } catch { };
                }
                if (!_loadetMonthCacheYears.Contains(iYear))
                {
                    xLog.Debug("Load Months from DB " + iYear);
                    _loadetMonthCacheYears.Add(iYear);
                    //Load UserData
                    var mData = dbUserData.Query<MonthInfo>("select * from MonthInfo where Year = ?", iYear);
                    foreach (var x in mData)
                    {
                        try { _monthInfos.Add(x.YearMonth, x); } catch { };
                        try { _monthUserData.Add(x.YearMonth, x); } catch { };
                    }

                    //Load Cache
                    var mCache = dbCache.Query<MonthInfo>("select * from MonthInfo where Year = ?", iYear);
                    foreach (var x in mCache)
                        try { _monthInfos.Add(x.YearMonth, x); }
                        catch (Exception ex)
                        { ex.ToString(); };
                }

                YearInfo yi = null;
                if (!_yearInfos.TryGetValue(iYear, out yi) || yi == null)
                {
                    xLog.Debug("compute new year " + iYear);
                    ComputingDialog?.Prepare("computing year " + GetYearNumber(iYear) + "...");
                    //ein neues Jahr berechnen..
                    yi = new YearInfo();
                    yi.Year = iYear;

                    //Jahresanfang und Dauer
                    if (iYear == ModelStartYearID)
                    {
                        yi.UtcStart = ModelStartPoint.GetTimePoint(this);
                        yi.DayCount = CommonYearLength.GetOffsetDays(yi.UtcStart, this, 90);
                        if (IsLeapYear(iYear))
                            yi.DayCount += LeapModel.LeapOffset.GetOffsetDays(yi.UtcStart.AddDays(yi.DayCount), this);

                        if (ModelStartDayOfWeek >= 0)
                            yi.DayOfWeekStart = ModelStartDayOfWeek;
                        else
                            yi.DayOfWeekStart = (int)yi.UtcStart.DayOfWeek;
                    }
                    else if (iYear > ModelStartYearID)
                    {
                        var lastYi = GetYearInfo(iYear - 1);
                        int lastDays = GetDaysOfYear(iYear - 1);
                        yi.UtcStart = lastYi.UtcStart.AddDays(lastDays);
                        yi.DayCount = CommonYearLength.GetOffsetDays(yi.UtcStart, this, 180);
                        if (yi.DayCount < 200)
                            yi.ToString();
                        if (IsLeapYear(iYear))
                            yi.DayCount += LeapModel.LeapOffset.GetOffsetDays(yi.UtcStart.AddDays(yi.DayCount), this);
                        if (yi.DayCount < 200)
                            yi.ToString();

                        yi.DayOfWeekStart = lastYi.DayOfWeekStart + lastDays;
                        if (!ContinueWeeksOutOfTime)
                            yi.DayOfWeekStart -= GetOutOfTimeDays(iYear - 1);
                        while (yi.DayOfWeekStart >= WeekLength)
                            yi.DayOfWeekStart -= WeekLength;
                    }
                    else if (iYear < ModelStartYearID)
                    {
                        //todo set YearStart here for Calcling => set real start later.. ~~

                        var yiNext = GetYearInfo(iYear + 1);
                        int iDays = 0;
                        if (CommonYearLength.IsFullStatic)
                        {
                            iDays = CommonYearLength.GetOffsetDays(DateTime.Now, this);
                            if (IsLeapYear(iYear))
                                iDays += LeapModel.LeapOffset.GetOffsetDays(DateTime.Now, this);

                        }
                        else if (CommonYearLength is TimeOffsetAstronomic || CommonYearLength is TimeOffsetGregorianDate)
                        {
                            var tClalc = yiNext.UtcStart.AddDays(CommonYearLength.GetMaxOffsetDays(this) * -1.5);
                            var off = CommonYearLength.GetOffset(tClalc, this);
                            iDays = (yiNext.UtcStart - (tClalc + off)).Days;
                        }
                        else
                            throw new Exception("unsupported Calendar-Model-Config");

                        yi.UtcStart = GetYearInfo(iYear + 1).UtcStart.AddDays(iDays * -1);
                        yi.DayCount = iDays;
                    }

                    if (YearWeekStartType == WeekStartType.FristDayIsFirstWeekDay)
                        yi.DayOfWeekStart = FirstDayOfWeek;

                    if (yi.DayCount < 200)
                    {
                        AddException(new Exception("Year Length under 200 Days! " + yi.DayCount + " for Year " + yi.Year));
                        yi.DayCount = 200;
                    }
                    if (yi.DayCount > 500)
                    {
                        AddException(new Exception("Year Length over 500 Days! " + yi.DayCount + " for Year " + yi.Year));
                        yi.DayCount = 500;
                    }

                    //Dynamischer Erster Tag des Jahres
                    if (FirstMonthFirstDayType == FirstMonthFirstDayType.ContinueLastYear)
                    {
                        var toLast = Months[Months.Count - 1].Length;
                        if (iYear == ModelStartYearID && !(toLast is TimeOffsetAstronomic))
                        {
                            yi.FirstMonthFirstDayNumber = ModelStartFirstMonthFirstDayNumber;
                            if (yi.FirstMonthFirstDayNumber < 0)
                                yi.FirstMonthFirstDayNumber = 0;
                        }
                        else if (iYear > ModelStartYearID)
                        {
                            var yiLast = GetYearInfo(iYear - 1);
                            int iLastYearLastMonth = GetMonthsOfYear(iYear - 1) - 1;
                            int iLastYearLastDay = GetDaysOfMonth(iYear - 1, iLastYearLastMonth) - 1;
                            var tLastYearLastMonthStart = yiLast.UtcStart.AddDays(_beginsOfMonths[GetYearMonth(iYear - 1, iLastYearLastMonth)]);
                            int iLastYearLastMonthLength = Months[iLastYearLastMonth - 1].Length.GetOffsetDays(tLastYearLastMonthStart, this);
                            if (iLastYearLastMonthLength > iLastYearLastDay)
                                yi.FirstMonthFirstDayNumber = iLastYearLastDay + 1;
                            else if (toLast is TimeOffsetAstronomic)
                            {
                                var lastYearLastMonthStart = yiLast.UtcStart.AddDays(_beginsOfMonths[GetYearMonth(iYear - 1, GetMonthsOfYear(iYear - 1) - 1)]);
                                if (toLast.GetOffsetDays(lastYearLastMonthStart, this) > iLastYearLastDay)
                                    yi.FirstMonthFirstDayNumber = iLastYearLastDay + 1;
                            }
                        }
                        else if (iYear <= ModelStartYearID)
                        {
                            if (toLast is TimeOffsetAstronomic)
                            {
                                var tClacStart = yi.UtcStart.AddDays(toLast.GetMaxOffsetDays(this) * -1.5);
                                var off = toLast.GetOffsetDays(tClacStart, this);
                                int iTry = 0;
                                while (tClacStart.AddDays(off) < yi.UtcStart && iTry < 3)
                                {
                                    tClacStart = tClacStart.AddDays(off);
                                    off = toLast.GetOffsetDays(tClacStart, this);
                                    iTry++;
                                }
                                TimeSpan tsDiff = yi.UtcStart - tClacStart;
                                if (tsDiff.TotalDays < off)
                                {
                                    int iLastYearLastDay = (int)Math.Truncate(tsDiff.TotalDays);
                                    yi.FirstMonthFirstDayNumber = iLastYearLastDay;
                                }
                            }
                            else
                            {
                                try
                                {
                                    var yiNext = GetYearInfo(iYear + 1);
                                    int iDay = yiNext.FirstMonthFirstDayNumber;
                                    if (iDay > 0)
                                    {
                                        int iRestDays = yi.DayCount - iDay;
                                        int iM = 0;
                                        while (Months[iM].Length.GetMaxOffsetDays(this) < iRestDays)
                                        {
                                            iRestDays -= Months[iM].Length.GetMaxOffsetDays(this);
                                            iM++;
                                        }
                                        yi.FirstMonthFirstDayNumber = Months[0].Length.GetMaxOffsetDays(this) - iRestDays;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    ex.ToString();
                                }
                            }
                        }
                    }

                    _yearInfos.Add(iYear, yi);
                    if (bSaveToCache && false)
                    {
                        dbCache.Insert(yi);
                        var zCache = dbCache.Query<YearInfo>("select * from YearInfo", new object[0]);
                        zCache.ToString();
                    }
                    ComputingDialog?.SetDone();
                }

                /* war ein schöner gedanke..
                if (yi.UtcStart < ValidFrom)
                {
                    if (_preValidCalendarModel == null)
                    {
                        _preValidCalendarModel = new CalendarModelCfgHolder().GetTemporaryModelCfg(PreValidCalendarModelID);
                        _preValidCalendarModel.ValidUntil = ValidFrom.AddDays(-1);
                    }
                    return _preValidCalendarModel.GetDaysOfMonth(iYear, iMonth);
                    _preValidCalendarModel._
                }*/

                int iRes = -1;

                if (_daysOfMonths.TryGetValue(iYearMonth, out iRes))
                    return iRes;

                if (_daysOfMonths.TryGetValue(GetYearMonth(iYear, 1), out iRes))
                {
                    if (GetMonthsOfYear(iYear) < iMonth + 1)
                        return 0;
                }

                //Start computing months...
                try
                {
                    MonthInfo mi = null;
                    int iDaysOfYear = 0;
                    bool bIsLeapYear = IsLeapYear(iYear);
                    int iAddMonth = -1;
                    int myYearLengthTarget = yi.DayCount;
                    var MonthInfosToSave = new List<MonthInfo>();

                    void aOutATime(OutOfTimeSection o, DateTime tStart)
                    {
                        int iOat = o.Length.GetOffsetDays(tStart, this);
                        if (iOat > 0)
                        {
                            if (_daysOutOfTime.ContainsKey(GetYearMonth(iYear, o.BeforeMonth)))
                            {
                                o.ToString();
                            }
                            else
                            {
                                iDaysOfYear += iOat;
                                _daysOutOfTime.Add(GetYearMonth(iYear, o.BeforeMonth), iOat);
                            }
                        }
                    }

                    foreach (var m in Months)
                    {
                        ComputingDialog?.Prepare("computing year " + GetYearNumber(iYear) + "...");

                        if (iDaysOfYear < yi.DayCount && OutOfTimeSectionsDict.ContainsKey(iAddMonth + 1))
                            aOutATime(OutOfTimeSectionsDict[iAddMonth + 1], yi.UtcStart.AddDays(iDaysOfYear - 1));

                        if (iDaysOfYear < yi.DayCount)
                        {
                            iAddMonth++;

                            int iDays = 0;
                            mi = null;
                            if (_monthInfos.TryGetValue(GetYearMonth(iYear, iAddMonth), out mi) && mi != null)
                            {
                                iDays = mi.Length;
                            }
                            else
                            {
                                DateTime tCalcStart = yi.UtcStart.AddDays(iDaysOfYear);
                                int iOffsetDays = 0;
                                if (iAddMonth > 0)
                                    iOffsetDays = m.Length.GetMaxOffsetDays(this) / 5;
                                if (tCalcStart.Equals(new DateTime(2019, 12, 12)))
                                    this.ToString();
                                iDays = m.Length.GetOffsetDays(tCalcStart, this, iOffsetDays);
                                if (iDays > 40)
                                    iDays.ToString();
                                if (iDays < 1)
                                    iDays = m.Length.GetOffsetDays(tCalcStart.AddDays((iDays * -1) + 1), this);

                                if (iAddMonth > 0 && iDays < 1)
                                {
                                    if (iDays < 0)
                                        iDays = iDays * -1;
                                    iDays = m.Length.GetOffsetDays(tCalcStart.AddDays(iDays + 3), this);
                                }
                                if (iAddMonth > 0 && iDays < 1 && m.Length.HasAstronomics)
                                {
                                    m.ToString();
                                }
                                if (iDays > 40)
                                    iDays.ToString();

                                if (bIsLeapYear)
                                {
                                    //Schaltjahre
                                    switch (LeapModel.DaysAssignment)
                                    {
                                        case ExtraDaysAssignment.AttachToMonth:
                                            if (iAddMonth == LeapModel.LeapBeforeMonth - 1 || (iAddMonth == 0 && LeapModel.LeapBeforeMonth == 0) || (LeapModel.LeapBeforeMonth < 0 && iDaysOfYear + iDays >= yi.DayCount))
                                            {
                                                int iAdd = LeapModel.LeapOffset.GetOffsetDays(yi.UtcStart.AddDays(iDaysOfYear + iDays), this);
                                                iDays += iAdd;
                                            }
                                            break;
                                    }
                                }

                                //Dynamischer Erster Tag des Jahres
                                if (iAddMonth == 0 && FirstMonthFirstDayType == FirstMonthFirstDayType.ContinueLastYear)
                                {
                                    if (yi.FirstMonthFirstDayNumber != 0 && (m.Length is TimeOffsetStatic && m.Length.AddOnOffset == null)) //toDo ersten Monatsanfang schöner gestalten
                                    {
                                        iDays -= yi.FirstMonthFirstDayNumber - 1;
                                    }
                                }

                                if (iDays < 1)
                                    iDays = 20;

                                mi = new MonthInfo(iYear, iAddMonth, iDays);
                                MonthInfosToSave.Add(mi);
                            }

                            //Monat zwischenspeichern
                            _daysOfMonths.Add(GetYearMonth(iYear, iAddMonth), iDays);
                            iDaysOfYear += iDays;

                            //extra-Tage für Schaltjahre
                            if (bIsLeapYear && (int)LeapModel.DaysAssignment > 1 && iAddMonth == LeapModel.LeapBeforeMonth - 1)
                            {
                                int iAdd = LeapModel.LeapOffset.GetOffsetDays(yi.UtcStart.AddDays(iDaysOfYear), this);
                                if (LeapModel.DaysAssignment == ExtraDaysAssignment.CreateExtraMonth)
                                {
                                    iAddMonth++;
                                    if (iDaysOfYear < yi.DayCount && OutOfTimeSectionsDict.ContainsKey(iAddMonth))
                                        aOutATime(OutOfTimeSectionsDict[iAddMonth], yi.UtcStart);
                                    xLog.Debug("compute CreateExtraLeapMonth " + iYear);
                                    _daysOfMonths.Add(GetYearMonth(iYear, iAddMonth), iAdd);
                                    mi = new MonthInfo(iYear, iAddMonth, iAdd);
                                    MonthInfosToSave.Add(mi);
                                }
                                else if (LeapModel.DaysAssignment == ExtraDaysAssignment.OutOfTime)
                                {
                                    xLog.Debug("compute CreateExtraLeapMonth " + iYear);
                                    int ym = GetYearMonth(iYear, LeapModel.LeapBeforeMonth);
                                    if (_daysOutOfTime.ContainsKey(ym))
                                        _daysOutOfTime[ym] += iDays;
                                    else
                                        _daysOutOfTime.Add(ym, iAdd);
                                }
                                else
                                    Exceptions.Add(new Exception("Enum-Zähl-Feher!!!"));
                                iDaysOfYear += iAdd;
                            }
                        }
                    }

                    if (iDaysOfYear < yi.DayCount && OutOfTimeSectionsDict.ContainsKey(iAddMonth + 1))
                        aOutATime(OutOfTimeSectionsDict[iAddMonth + 1], yi.UtcStart.AddDays(iDaysOfYear - 1));

                    //for MonthEndType.ManualEndOrOpen: Aktuellen Monat finden und eventuell Länge anpassen
                    if (iYear >= ModelStartYearID && yi.UtcStart < DateTime.Now)
                    {
                        xLog.Debug("check ManualEndOrOpen " + iYear);
                        int iOpenMonth = -1;
                        foreach (var m in Months)
                        {
                            iOpenMonth++;
                            if (iOpenMonth > iAddMonth)
                            {
                                break;
                            }
                            if (m.EndType == MonthEndType.ManualEndOrOpen)
                            {
                                if (!_monthUserData.ContainsKey(GetYearMonth(iYear, iOpenMonth)))
                                {
                                    //das sollte der aktuelle Monat sein => Verlängern bis heute!
                                    int iDaysBeforeCurrentMonth = 0;
                                    for (int i = 0; i < iOpenMonth; i++)
                                    {
                                        iDaysBeforeCurrentMonth += _daysOfMonths[GetYearMonth(iYear, i)];
                                    }
                                    int iDaysSinceYearStart = (DateTime.Now.Date - yi.UtcStart).Days + 1;

                                    if (iDaysBeforeCurrentMonth + _daysOfMonths[GetYearMonth(iYear, iOpenMonth)] > iDaysSinceYearStart)
                                        break; //Alles Okay, aktueller Monat ist innerhalb seiner standard länge

                                    int iCurrentDayLength = iDaysSinceYearStart - iDaysBeforeCurrentMonth;
                                    if (iCurrentDayLength > 99)
                                        iCurrentDayLength = 99; //ToDo: auch hier eine Exception bzw. einen Hinweis!
                                    int iLengthDiff = iCurrentDayLength - _daysOfMonths[GetYearMonth(iYear, iOpenMonth)];
                                    if (iLengthDiff <= 0)
                                    {
                                        //ToDo: eigentlich eine Exception!!!
                                        break;
                                    }

                                    //Aktuellen Monat verlängern
                                    _daysOfMonths[GetYearMonth(iYear, iOpenMonth)] = iCurrentDayLength;

                                    //und dann diese Tage am Jahresende abziehen
                                    while (_daysOfMonths[GetYearMonth(iYear, iAddMonth)] <= iLengthDiff)
                                    {
                                        //Ganze Monate entfernen, der Letzte wird dann beim Anpassen der Jahreslänge gekürzt
                                        //ob das nächste Jahr dann korrekt anfängt??? 
                                        iLengthDiff -= _daysOfMonths[GetYearMonth(iYear, iAddMonth)];
                                        _daysOfMonths.Remove(GetYearMonth(iYear, iAddMonth));
                                        iAddMonth--;
                                    }
                                }
                            }
                        }
                    }

                    if (myYearLengthTarget > 100 && myYearLengthTarget != iDaysOfYear)
                    {
                        int iAssiMonth = YearLengthAssigBeforeMonth;
                        if (iAssiMonth < 0)
                            iAssiMonth = iAddMonth;
                        xLog.Debug("ajust year-length " + iYear);
                        var mYearLengthAssignment = YearLengthAssignment;
                        int iDiff = myYearLengthTarget - iDaysOfYear;
                        if (iDiff < 0 && (mYearLengthAssignment == ExtraDaysAssignment.OutOfTime || mYearLengthAssignment == ExtraDaysAssignment.None))
                            mYearLengthAssignment = ExtraDaysAssignment.AttachToMonth;
                        if (iDiff > 0 && mYearLengthAssignment == ExtraDaysAssignment.None)
                            mYearLengthAssignment = ExtraDaysAssignment.AttachToMonth;
                        switch (mYearLengthAssignment)
                        {
                            case ExtraDaysAssignment.AttachToMonth:
                                _daysOfMonths[GetYearMonth(iYear, iAssiMonth)] += iDiff;
                                mi.Length += iDiff;
                                iDaysOfYear += iDiff;
                                break;
                            case ExtraDaysAssignment.CreateExtraMonth:
                                iAddMonth++;
                                iAssiMonth++;
                                _daysOfMonths.Add(GetYearMonth(iYear, iAssiMonth), iDiff);
                                mi = new MonthInfo(iYear, iAssiMonth, iDiff);
                                MonthInfosToSave.Add(mi);
                                iDaysOfYear += iDiff;
                                break;
                            case ExtraDaysAssignment.OutOfTime:
                                if (YearLengthAssigBeforeMonth < 0)
                                    iAssiMonth = iAddMonth + 1;
                                _daysOutOfTime.Add(GetYearMonth(iYear, iAssiMonth), iDiff);
                                iDaysOfYear += iDiff;
                                break;
                            case ExtraDaysAssignment.None:
                                break;
                            default:
                                throw new NotImplementedException("ExtraDaysAssignment-Type " + mYearLengthAssignment.ToString() + " currently not supportet here");
                        }
                    }

                    //Jahresanfang vor Modell-Start festlegen
                    if (iYear < ModelStartYearID)
                        yi.UtcStart = GetYearInfo(iYear + 1).UtcStart.AddDays(iDaysOfYear * -1);

                    //restliche Infos zwischenspeichern
                    if (!_monthsOfYears.ContainsKey(iYear))
                        _monthsOfYears.Add(iYear, iAddMonth + 1);
                    if (!_daysOfYears.ContainsKey(iYear))
                        _daysOfYears.Add(iYear, iDaysOfYear);

                    if (iDaysOfYear != yi.DayCount)
                        yi.DayCount.ToString();

                    int iStartDay = 0;
                    if (_daysOutOfTime.ContainsKey(GetYearMonth(iYear, 0)))
                        iStartDay += _daysOutOfTime[GetYearMonth(iYear, 0)];
                    for (int iM = 0; iM <= iAddMonth; iM++)
                    {
                        int iYM = GetYearMonth(iYear, iM);
                        _beginsOfMonths.Add(iYM, iStartDay);
                        iStartDay += _daysOfMonths[iYM];
                        if (_daysOutOfTime.ContainsKey(iYM))
                            iStartDay += _daysOutOfTime[iYM];
                    }

                    if (iYear < ModelStartYearID)
                    {
                        var yiNext = GetYearInfo(iYear + 1);

                        yi.DayOfWeekStart = yiNext.DayOfWeekStart - yi.DayCount;
                        if (!ContinueWeeksOutOfTime)
                            yi.DayOfWeekStart += GetOutOfTimeDays(iYear);
                        while (yi.DayOfWeekStart < 0)
                            yi.DayOfWeekStart += WeekLength;

                        _yearInfos[iYear] = yi;
                        if (bSaveToCache && false)
                            dbCache.InsertOrReplace(yi);
                    }

                    if (bSaveToCache && MonthInfosToSave.Count > 0)
                        dbCache.InsertAll(MonthInfosToSave);

                    xLog.Debug("computing year done " + iYear);

                    if (_daysOfMonths.TryGetValue(iYearMonth, out iRes))
                        return iRes;
                }
                catch (Exception ex)
                {
                    Exceptions.Add(ex);
                    ex.ToString();
                }
                finally
                {
                    ComputingDialog?.SetDone();
                }
                return -1;
            }
        }

        private void AddException(Exception exception)
        {
            _exceptions.Add(exception);
        }

        public int GetDaysOfYear(int iYear)
        {
            int iRes = -1;
            if (!_daysOfYears.ContainsKey(iYear))
                GetDaysOfMonth(iYear, 1);
            if (_daysOfYears.TryGetValue(iYear, out iRes))
                return iRes;
            return -1;
        }

        public int GetMonthsOfYear(int iYear)
        {
            int iRes = -1;
            if (!_monthsOfYears.ContainsKey(iYear))
                GetDaysOfMonth(iYear, 1);
            if (_monthsOfYears.TryGetValue(iYear, out iRes))
                return iRes;
            return -1;
        }

        public int GetYearNumber(int iYearID)
            => iYearID + ModelStartYearNumber;
        public int GetYearID(int iYearNumber)
            => iYearNumber - ModelStartYearNumber;

        public YearInfo GetYearInfo(int iYear)
        {
            if (!_yearInfos.ContainsKey(iYear))
                GetDaysOfMonth(iYear, 1);
            YearInfo res = null;
            if (_yearInfos.TryGetValue(iYear, out res))
                return res;
            throw new Exception("@GetYearInfo: Das is ja schräg.. should not happen!!");
        }

        public MonthInfo GetMonthInfo(int iYear, int iMonth)
        {
            MonthInfo res;
            if (_monthInfos.TryGetValue(GetYearMonth(iYear, iMonth), out res))
                return res;
            return null;
        }

        public DateTime GetMonthStartUtc(int iYear, int iMonth)
        {
            return GetYearInfo(iYear).UtcStart.AddDays(GetMonthStartDayNumber(iYear, iMonth) - 1);
        }

        public int GetMonthStartDayNumber(int iYear, int iMonth)
        {
            int iYM = GetYearMonth(iYear, iMonth);
            if (!_beginsOfMonths.ContainsKey(iYM))
                GetDaysOfMonth(iYear, iMonth);
            if (_beginsOfMonths.ContainsKey(iYM))
                return _beginsOfMonths[iYM];
            return -1;
        }

        public int GetMonthNumber(int iMonthID)
        {
            if (iMonthID < 0)
                return -1;

            switch (MonthNumberType)
            {
                case MonthNumberType.ZeroBased:
                    return iMonthID;
                case MonthNumberType.OneBased:
                    return iMonthID + 1;
                case MonthNumberType.Dynamic:
                    try
                    {
                        return Months[iMonthID].Number;
                    }
                    catch
                    {
                        return -3;
                    }
            }

            return -2;
        }

        public int GetMonthID(int iMonthNumber)
        {
            if (iMonthNumber < 0)
                return -1;

            switch (MonthNumberType)
            {
                case MonthNumberType.ZeroBased:
                    return iMonthNumber;
                case MonthNumberType.OneBased:
                    return iMonthNumber - 1;
                case MonthNumberType.Dynamic:
                    foreach (Month m in Months)
                    {
                        if (m.Number.Equals(iMonthNumber))
                            return Months.IndexOf(m);
                    }
                    return -3;
            }

            return -2;
        }

        public int GetOutOfTimeDays(int iYear)
        {
            int iCount = 0;
            for (int i = 0; i <= GetMonthsOfYear(iYear); i++)
                iCount += GetOutOfTimeDays(iYear, i);
            return iCount;
        }

        public int GetOutOfTimeDays(int iYear, int iBeforeMonth)
        {
            int iYM = GetYearMonth(iYear, iBeforeMonth);
            if (_daysOutOfTime.ContainsKey(iYM))
                return _daysOutOfTime[iYM];
            return 0;
        }

        public void AddMonthUserDate(MonthInfo info)
        {
            try
            {
                _monthUserData.Add(info.YearMonth, info);
            }
            catch { }
        }

        public int GetWeekDay(int iYear, int iMonth, int iDay)
        {
            if (MonthWeekStartType == WeekStartType.FristDayIsFirstWeekDay)
            {
                if (!ContinueWeeksOutOfTime)
                    iDay -= GetOutOfTimeDays(iYear, iMonth);
                iDay = iDay + FirstDayOfWeek;
                while (iDay >= WeekLength)
                    iDay -= WeekLength;
                return iDay;
            }
            else
            {
                var yi = GetYearInfo(iYear);
                int iWeekDay = yi.DayOfWeekStart;
                for (int i = 0; i < iMonth; i++)
                {
                    iWeekDay += GetDaysOfMonth(iYear, i);
                    if (ContinueWeeksOutOfTime)
                        iWeekDay += GetOutOfTimeDays(iYear, i);
                }
                iWeekDay += iDay;
                while (iWeekDay >= WeekLength)
                    iWeekDay -= WeekLength;
                return iWeekDay;
            }
        }

        public WeekDay GetWeekDay(int iWeekDay)
        {
            if (iWeekDay < 0)
                return null;
            if (WeekDays.Count == 0)
            {
                DateTime tDay = DateTime.Today;
                tDay = tDay.AddDays((int)tDay.DayOfWeek * -1);
                for (int i = 0; i < 7; i++)
                {
                    var wd = new WeekDay { FullName = tDay.ToString("dddd"), ShortName = tDay.ToString("ddd"), OneCharName = tDay.ToString("dddd").Substring(0, 1) };
                    if (i == 0)
                    {
                        wd.HasSpecialTextColor = true;
                        wd.SpecialTextColor = xColor.FromHex("#ffba4862");
                    }
                    WeekDays.Add(wd);
                    tDay = tDay.AddDays(1);
                }
            }
            while (WeekDays.Count < WeekLength)
                WeekDays.Add(new WeekDay { FullName = WeekDays.Count.ToString() + " ????", ShortName = WeekDays.Count.ToString() + "?", OneCharName = WeekDays.Count.ToString() });

            while (iWeekDay >= WeekLength)
                iWeekDay -= WeekLength;

            return WeekDays[iWeekDay];
        }

        public MonthDay GetMonthDay(int iYear, int iMonth, int iDay)
        {
            int iMonthDays = GetDaysOfMonth(iYear, iMonth);
            if (iMonth == 0 || iDay >= iMonthDays)
            {
                iDay = -iMonthDays;
                if (OutOfTimeSectionsDict.ContainsKey(iMonth))
                {
                    if (OutOfTimeSectionsDict[iMonth].Days.Count >= iDay)
                        return OutOfTimeSectionsDict[iMonth].Days[iDay];
                }
            }
            else
            {
                if (Months.Count > iMonth)
                {
                    if (Months[iMonth].Days.Count > iDay)
                        return Months[iMonth].Days[iDay];
                }
            }
            return null;
        }

        public int GetMonthLengthSum()
        {
            int iRes = 0;
            foreach (var m in Months)
                iRes += m.Length.GetMaxOffsetDays(this);
            return iRes;
        }
        public int GetMonthMaxLength(int iMonth)
        {
            if (iMonth < 1 || iMonth >= Months.Count)
            {
                if (Months.Count > 0)
                    iMonth = 1;
                else
                    return -1;
            }
            int iRes = Months[iMonth - 1].Length.GetMaxOffsetDays(this);
            if (LeapModel.DaysAssignment == ExtraDaysAssignment.AttachToMonth && LeapModel.LeapBeforeMonth - 1 == iMonth)
                iRes += LeapModel.LeapOffset.GetMaxOffsetDays(this);
            //ToDo if (YearLengthAssignment == ExtraDaysAssignment.AttachToMonth && YearLengthAssigAfterMonthNo == iMonth)
            //  iRes += CommonYearLength - GetMonthLengthSum();

            if (Months[iMonth - 1].EndType == MonthEndType.ManualEndOrOpen)
                iRes = iRes * 2;

            return iRes;
        }

        public static int GetYearMonth(int iYear, int iMonth)
        {
            /*while (iYear > 0 && iYear < 10000)
                iYear = iYear * 10;
            while (iYear < 0 && iYear > -10000)
                iYear = iYear * 10;*/
            return iYear * 100 + iMonth;
        }

        public YearInfo GetYearFromUtcDate(DateTime tUtcDate)
        {
            tUtcDate = tUtcDate.Date;
            int iYearNumber = ModelStartYearID;
            YearInfo yi = GetYearInfo(iYearNumber);
            if (yi.UtcStart < tUtcDate)
            {
                while (yi.UtcStart.AddDays(yi.DayCount - 1) < tUtcDate)
                { //ToDo Check +-1
                    iYearNumber++;
                    yi = GetYearInfo(iYearNumber);
                }
                //iYearNumber--;
                //yi = GetYearInfo(iYearNumber);
            }
            else if (yi.UtcStart > tUtcDate)
            {
                while (yi.UtcStart > tUtcDate)
                {
                    iYearNumber--;
                    yi = GetYearInfo(iYearNumber);
                }
            }
            return yi;
        }

        public async Task<DynamicDate> GetDateFromUtcDateAsync(DateTime tUtcDate)
        {
            DynamicDate dDate = new DynamicDate();
            await Task.Factory.StartNew(() =>
            {
                dDate = GetDateFromUtcDate(tUtcDate);
            });
            return dDate;
        }

        public DynamicDate GetDateFromUtcDate(DateTime tUtcDate)
        {
            tUtcDate = tUtcDate.Date;
            YearInfo yi = GetYearFromUtcDate(tUtcDate);
            int iDays = (tUtcDate - yi.UtcStart).Days;
            int iMonth = 0;
            //iDays -= GetOutOfTimeDays(yi.Year, 0);
            while (iDays > GetDaysOfMonth(yi.Year, iMonth) + GetOutOfTimeDays(yi.Year, iMonth))
            {
                iDays -= _daysOfMonths[GetYearMonth(yi.Year, iMonth)];
                iDays -= GetOutOfTimeDays(yi.Year, iMonth);
                iMonth++;
            }
            return new DynamicDate(this, yi.Year, iMonth, iDays, tUtcDate);
        }

        public DateTime GetUtcDateFromDate(DynamicDate dDate)
        {
            return GetUtcDateFromDate(dDate.Year, dDate.Month, dDate.Day);
        }

        public DateTime GetUtcDateFromDate(int iYear, int iMonth, int iDay)
        {
            var yi = GetYearInfo(iYear);
            DateTime tUtcDate = yi.UtcStart;
            for (int i = 0; i < iMonth; i++)
            {
                tUtcDate = tUtcDate.AddDays(GetDaysOfMonth(iYear, i) + GetOutOfTimeDays(iYear, i));
            }
            return tUtcDate.AddDays(iDay);
        }

        public static string GetMonthNameFromTemplate(string cTemplate, int iMonth, MonthNumberType MonthNumberType, int[] DynamicMonthNumbers)
        {
            if (string.IsNullOrEmpty(cTemplate))
                return "";
            switch (MonthNumberType)
            {
                case MonthNumberType.OneBased:
                    iMonth++;
                    break;
                case MonthNumberType.Dynamic:
                    try
                    {
                        iMonth = DynamicMonthNumbers[iMonth];
                    }
                    catch
                    {
                        iMonth++;
                    }
                    break;
            }

            return cTemplate.Replace("%M", iMonth.ToString());
        }
    }

    public enum WeekStartType
    {
        [XmlEnum("OW")]
        OngoingWeeks,
        [XmlEnum("FIF")]
        FristDayIsFirstWeekDay
    }

    public enum MonthNumberType
    {
        [XmlEnum("Z")]
        ZeroBased,
        [XmlEnum("1")]
        OneBased,
        [XmlEnum("D")]
        Dynamic
    }

    public enum FirstMonthFirstDayType
    {
        [XmlEnum("NO")]
        NumberOne,
        [XmlEnum("CLY")]
        ContinueLastYear
    }

    public class Month
    {
        [XmlAttribute]
        public int Number { get; set; }
        [XmlElement]
        public TimeOffset Length { get; set; }
        [XmlAttribute]
        public string FullName { get; set; }
        [XmlAttribute]
        public string ShortName { get; set; }
        [XmlElement]
        public List<MonthDay> Days { get; set; } = new List<MonthDay>();
        [XmlAttribute]
        public DayNumberType DayNumberType { get; set; } = DayNumberType.OneBased;
        [XmlAttribute]
        public MonthEndType EndType { get; set; } = MonthEndType.FixedLength;

        public string DayNamesList
        {
            get
            {
                int iMax = Days.Count;
                if (iMax > 3)
                    iMax = 3;
                string cRes = "";
                for (int i = 0; i < iMax; i++)
                {
                    if (!string.IsNullOrEmpty(cRes))
                        cRes += ", ";
                    cRes += Days[i].FullName;
                }
                return cRes;
            }
        }
    }

    public class OutOfTimeSection
    {
        [XmlAttribute]
        public int BeforeMonth { get; set; }
        [XmlElement]
        public TimeOffset Length { get; set; }
        [XmlAttribute]
        public string FullName { get; set; }
        [XmlAttribute]
        public string ShortName { get; set; }
        [XmlElement]
        public List<MonthDay> Days { get; set; } = new List<MonthDay>();
        [XmlAttribute]
        public DayNumberType DayNumberType { get; set; } = DayNumberType.OneBased;
        public string DayNamesList
        {
            get
            {
                int iMax = Days?.Count ?? 0;
                if (iMax > 3)
                    iMax = 3;
                string cRes = "";
                for (int i = 0; i < iMax; i++)
                {
                    if (!string.IsNullOrEmpty(cRes))
                        cRes += ", ";
                    cRes += Days[i].FullName;
                }
                return cRes;
            }
        }
    }

    public class WeekDay
    {
        [XmlAttribute("FN")]
        public string FullName { get; set; }
        [XmlAttribute("SN")]
        public string ShortName { get; set; }
        [XmlAttribute("OCN")]
        public string OneCharName { get; set; }
        [XmlAttribute("DT")]
        public DayType DayType { get; set; } = DayType.CommonDay;
        [XmlAttribute("HSTC")]
        public bool HasSpecialTextColor { get; set; }
        [XmlElement("STC")]
        public xColor SpecialTextColor { get; set; } = xColor.Black;
        [XmlAttribute("HSBC")]
        public bool HasSpecialBackgroundColor { get; set; }
        [XmlElement("SBC")]
        public xColor SpecialBackgroundColor { get; set; } = xColor.Transparent;
    }

    public enum DayType
    {
        [XmlEnum("0")]
        CommonDay,
        [XmlEnum("10")]
        SpecialDay,
        [XmlEnum("20")]
        Holyday,
        [XmlEnum("25")]
        RestingDay,
        [XmlEnum("30")]
        FeastDay,
    }

    public class MonthDay
    {
        [XmlAttribute("Nr")]
        public int Number { get; set; }
        [XmlAttribute("FN")]
        public string FullName { get; set; }
        [XmlAttribute("SN")]
        public string ShortName { get; set; }
        [XmlAttribute("DT")]
        public DayType DayType { get; set; } = DayType.CommonDay;
        [XmlAttribute("HSTC")]
        public bool HasSpecialTextColor { get; set; }
        [XmlElement("STC")]
        public xColor SpecialTextColor { get; set; } = xColor.Black;
        [XmlAttribute("HSBC")]
        public bool HasSpecialBackgroundColor { get; set; }
        [XmlElement("SBC")]
        public xColor SpecialBackgroundColor { get; set; } = xColor.Transparent;
    }

    public class MonthInfo : dbObject
    {
        public MonthInfo() { } //für SQL create on load
        public MonthInfo(int iYear, int iMonth, int iLength)
        {
            if (iMonth < 0 || iLength < 0)
                throw new Exception("invalid Month Number or Length on " + iYear + " " + iMonth + " => " + iLength);
            Year = iYear;
            Month = iMonth;
            Length = iLength;
            YearMonth = DynamicCalendarModel.GetYearMonth(iYear, iMonth);
        }

        [SQLite.Indexed(Unique = true)]
        public int YearMonth { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Length { get; set; }
        //public DateTime UtcStart { get; set; }

        public override string ToString()
        {
            return Year + "\\" + Month + ": " + Length;
        }
    }

    public class OutOfTimeInfo : dbObject
    {
        public OutOfTimeInfo() { } //für SQL create on load
        public OutOfTimeInfo(int iYear, int iAfterMonth, int iLength)
        {
            if (iAfterMonth < 0 || iLength < 0)
                throw new Exception("invalid Month Number or Length on " + iYear + " " + iAfterMonth + " => " + iLength);
            Year = iYear;
            AfterMonth = iAfterMonth;
            Length = iLength;
            YearAfterMonth = DynamicCalendarModel.GetYearMonth(iYear, iAfterMonth);
        }

        [SQLite.Indexed(Unique = true)]
        public int YearAfterMonth { get; set; }
        public int Year { get; set; }
        public int AfterMonth { get; set; }
        public int Length { get; set; }
        //public DateTime UtcStart { get; set; }

        public override string ToString()
        {
            return AfterMonth + " " + Year + ": " + Length;
        }
    }

    public class YearInfo : dbObject
    {
        [SQLite.Indexed(Unique = true)]
        public int Year { get; set; }

        public int FirstMonthFirstDayNumber { get; set; } = 0;

        public DateTime UtcStart { get; set; }

        public int DayOfWeekStart { get; set; }

        public int DayCount { get; set; }
    }

    public enum DayNumberType
    {
        [XmlEnum("Z")]
        ZeroBased,
        [XmlEnum("1")]
        OneBased,
        [XmlEnum("D")]
        Dynamic
    }

    public enum MonthEndType
    {
        [XmlEnum("FL")]
        FixedLength = 1,
        [XmlEnum("MEO")]
        ManualEndOrOpen = 2,
        [XmlEnum("MEFL")]
        ManualEndOrFixedLength = 3
    }

    public class LeapModel
    {
        [XmlAttribute]
        public LeapConditionType ConditionType { get; set; } = LeapConditionType.None;
        [XmlElement]
        public TimeOffset LeapOffset { get; set; } = new TimeOffsetStatic(0);
        [XmlAttribute]
        public ExtraDaysAssignment DaysAssignment { get; set; } = ExtraDaysAssignment.None;
        [XmlAttribute]
        public int LeapBeforeMonth { get; set; }
        [XmlElement]
        public List<LeapYearAlgoCfg> LeapYearAlgo { get; set; } = new List<LeapYearAlgoCfg>();
        [XmlElement]
        public TimeOffsetAstronomic AstroConditionOffset { get; set; }
        [XmlAttribute]
        public CompareOperator AstroConditionOperator { get; set; }
        [XmlAttribute]
        public double AstroConditionYearDay { get; set; }

        public override int GetHashCode()
        {
            unchecked
            {
                // Choose large primes to avoid hashing collisions
                int HashingBase = GetType().GetHashCode();
                const int HashingMultiplier = 16777619;
                int hash = HashingBase;
                hash = (hash * HashingMultiplier) ^ ConditionType.GetHashCode();
                hash = (hash * HashingMultiplier) ^ (!Object.ReferenceEquals(null, LeapOffset) ? LeapOffset.GetHashCode() : 0);
                hash = (hash * HashingMultiplier) ^ DaysAssignment.GetHashCode();
                hash = (hash * HashingMultiplier) ^ LeapBeforeMonth.GetHashCode();
                if (LeapYearAlgo != null)
                {
                    foreach (var alg in LeapYearAlgo)
                        hash = (hash * HashingMultiplier) ^ alg.GetHashCode();
                }
                hash = (hash * HashingMultiplier) ^ AstroConditionYearDay.GetHashCode();
                hash = (hash * HashingMultiplier) ^ (!Object.ReferenceEquals(null, AstroConditionOffset) ? AstroConditionOffset.Count.GetHashCode() : 0);
                return hash;
            }
        }

        public override bool Equals(object obj)
            => obj is LeapModel && GetHashCode() == obj.GetHashCode();
    }

    public enum LeapConditionType
    {
        [XmlEnum("0")]
        None,
        [XmlEnum("AM")]
        Algorithm,
        [XmlEnum("AC")]
        AstronomicCondition
    }

    public enum ExtraDaysAssignment
    {
        [XmlEnum("0")]
        None = 0,
        [XmlEnum("ATM")]
        AttachToMonth = 1,
        [XmlEnum("CEM")]
        CreateExtraMonth = 2,
        [XmlEnum("OOT")]
        OutOfTime = 3
    }

    public enum CompareOperator
    {
        [XmlEnum("==")]
        Equal,
        [XmlEnum("!=")]
        NotEqual,
        [XmlEnum("<")]
        Smaller,
        [XmlEnum("<=")]
        SmallerOrEqual,
        [XmlEnum(">")]
        Greater,
        [XmlEnum(">=")]
        GreaterOrEqual
    }

    public class LeapYearAlgoCfg
    {
        [XmlAttribute("D")]
        public int Dividor { get; set; }
        [XmlAttribute("IL")]
        public bool IsLeap { get; set; }

        public override int GetHashCode()
        {
            unchecked
            {
                // Choose large primes to avoid hashing collisions
                int HashingBase = GetType().GetHashCode();
                const int HashingMultiplier = 16777619;
                int hash = HashingBase;
                hash = (hash * HashingMultiplier) ^ Dividor.GetHashCode();
                hash = (hash * HashingMultiplier) ^ IsLeap.GetHashCode();
                return hash;
            }
        }
    }

    public class GroupSection
    {
        public string Name { get; set; }

        public TimeOffset Length { get; set; }

    }
}
