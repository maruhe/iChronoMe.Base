using System;
using System.Collections.Generic;

using iChronoMe.Abstractions;
using iChronoMe.Core.Classes;

namespace iChronoMe.Core.DynamicCalendar
{
    public enum CalendarModelSample
    {
        Gregorian,
        InternationalFixed,
        NewEarth,
        MenstrualCycle,
        SolarSeasons,
        WorldSeasons,
        MoonCalendar,
        MayaHaab,
        SexagesimalCalendar
    }

    public static class DynamicCalendarModelAssistent
    {
        static object oLock = new object();

        public static DynamicCalendarModel CreateModel(CalendarModelSample sample)
        {
            var Model = new DynamicCalendarModel();
            Model.Name = "my" + sample.ToString();
            Model.ID = sample.ToString() + "_" + Guid.NewGuid().ToString();
            Model.BaseSample = sample.ToString();

            int iSameName = 0;
            if (CalendarModelCfgHolder.LastModelCfgList != null)
            {
                foreach (var s in CalendarModelCfgHolder.LastModelCfgList.Values)
                {
                    if (s.Name.StartsWith(Model.Name))
                        iSameName++;
                }
            }
            if (iSameName > 0)
                Model.Name += " " + (iSameName + 1).ToString();

            int iNowYear = DateTime.Now.Year;
            var to28 = new TimeOffsetStatic(28);

            switch (sample)
            {
                case CalendarModelSample.Gregorian:
                    Model.ModelStartPoint = new TimePoint(new DateTime(iNowYear, 01, 01));
                    Model.ModelStartYearNumber = iNowYear;
                    AddCommonMonthName(Model, 1, 1, 31);
                    AddCommonMonthName(Model, 2, 2, 28);
                    AddCommonMonthName(Model, 3, 3, 31);
                    AddCommonMonthName(Model, 4, 4, 30);
                    AddCommonMonthName(Model, 5, 5, 31);
                    AddCommonMonthName(Model, 6, 6, 30);
                    AddCommonMonthName(Model, 7, 7, 31);
                    AddCommonMonthName(Model, 8, 8, 31);
                    AddCommonMonthName(Model, 9, 9, 30);
                    AddCommonMonthName(Model, 10, 10, 31);
                    AddCommonMonthName(Model, 11, 11, 30);
                    AddCommonMonthName(Model, 12, 12, 31);
                    YearLengthSampleS[CalendarModelSample.Gregorian.ToString()].CopyToModel(Model);
                    break;

                case CalendarModelSample.InternationalFixed:
                    Model.ModelStartPoint = new TimePoint(new DateTime(iNowYear, 01, 01));
                    Model.ModelStartYearNumber = iNowYear;
                    Model.MonthWeekStartType = WeekStartType.FristDayIsFirstWeekDay;
                    AddCommonMonthName(Model, 1, 1, 28);
                    AddCommonMonthName(Model, 2, 2, 28);
                    AddCommonMonthName(Model, 3, 3, 28);
                    AddCommonMonthName(Model, 4, 4, 28);
                    AddCommonMonthName(Model, 5, 5, 28);
                    AddCommonMonthName(Model, 6, 6, 28);
                    Model.Months.Add(new Month() { Number = 7, Length = to28, FullName = "Sol", ShortName = "Sol" });
                    AddCommonMonthName(Model, 7, 8, 28);
                    AddCommonMonthName(Model, 8, 9, 28);
                    AddCommonMonthName(Model, 9, 10, 28);
                    AddCommonMonthName(Model, 10, 11, 28);
                    AddCommonMonthName(Model, 11, 12, 28);
                    AddCommonMonthName(Model, 12, 13, 28);
                    YearLengthSampleS[CalendarModelSample.Gregorian.ToString()].CopyToModel(Model);
                    Model.YearLengthAssignment = ExtraDaysAssignment.OutOfTime;
                    Model.ContinueWeeksOutOfTime = false;
                    Model.YearLengthAssigBeforeMonth = -1;
                    Model.LeapModel.LeapBeforeMonth = 6;
                    Model.LeapModel.DaysAssignment = ExtraDaysAssignment.OutOfTime;
                    break;

                case CalendarModelSample.NewEarth:
                    Model.ModelStartPoint = new TimePoint(new DateTime(iNowYear, 01, 01));
                    Model.ModelStartYearNumber = iNowYear;
                    Model.MonthWeekStartType = WeekStartType.FristDayIsFirstWeekDay;
                    AddCommonMonthName(Model, 1, 1, 28);
                    AddCommonMonthName(Model, 2, 2, 28);
                    AddCommonMonthName(Model, 3, 3, 28);
                    AddCommonMonthName(Model, 4, 4, 28);
                    AddCommonMonthName(Model, 5, 5, 28);
                    AddCommonMonthName(Model, 6, 6, 28);
                    Model.Months.Add(new Month() { Number = 7, Length = to28, FullName = "Luna", ShortName = "Lun" });
                    AddCommonMonthName(Model, 7, 8, 28);
                    AddCommonMonthName(Model, 8, 9, 28);
                    AddCommonMonthName(Model, 9, 10, 28);
                    AddCommonMonthName(Model, 10, 11, 28);
                    AddCommonMonthName(Model, 11, 12, 28);
                    AddCommonMonthName(Model, 12, 13, 28);
                    YearLengthSampleS[CalendarModelSample.NewEarth.ToString()].CopyToModel(Model);
                    break;

                case CalendarModelSample.MenstrualCycle:
                    Model.ModelStartPoint = new TimePoint(new DateTime(iNowYear, 01, 01));
                    Model.ModelStartYearNumber = iNowYear;
                    Model.MonthsTitle = "Zyklus";
                    Model.Months.Add(new Month() { Number = 1, Length = to28, FullName = Model.MonthsTitle + " 1", EndType = MonthEndType.ManualEndOrOpen });
                    Model.Months.Add(new Month() { Number = 2, Length = to28, FullName = Model.MonthsTitle + " 2", EndType = MonthEndType.ManualEndOrOpen });
                    Model.Months.Add(new Month() { Number = 3, Length = to28, FullName = Model.MonthsTitle + " 3", EndType = MonthEndType.ManualEndOrOpen });
                    Model.Months.Add(new Month() { Number = 4, Length = to28, FullName = Model.MonthsTitle + " 4", EndType = MonthEndType.ManualEndOrOpen });
                    Model.Months.Add(new Month() { Number = 5, Length = to28, FullName = Model.MonthsTitle + " 5", EndType = MonthEndType.ManualEndOrOpen });
                    Model.Months.Add(new Month() { Number = 6, Length = to28, FullName = Model.MonthsTitle + " 6", EndType = MonthEndType.ManualEndOrOpen });
                    Model.Months.Add(new Month() { Number = 7, Length = to28, FullName = Model.MonthsTitle + " 7", EndType = MonthEndType.ManualEndOrOpen });
                    Model.Months.Add(new Month() { Number = 8, Length = to28, FullName = Model.MonthsTitle + " 8", EndType = MonthEndType.ManualEndOrOpen });
                    Model.Months.Add(new Month() { Number = 9, Length = to28, FullName = Model.MonthsTitle + " 9", EndType = MonthEndType.ManualEndOrOpen });
                    Model.Months.Add(new Month() { Number = 10, Length = to28, FullName = Model.MonthsTitle + " 10", EndType = MonthEndType.ManualEndOrOpen });
                    Model.Months.Add(new Month() { Number = 11, Length = to28, FullName = Model.MonthsTitle + " 11", EndType = MonthEndType.ManualEndOrOpen });
                    Model.Months.Add(new Month() { Number = 12, Length = to28, FullName = Model.MonthsTitle + " 12", EndType = MonthEndType.ManualEndOrOpen });
                    Model.Months.Add(new Month() { Number = 13, Length = to28, FullName = Model.MonthsTitle + " 13", EndType = MonthEndType.ManualEndOrOpen });
                    Model.Months.Add(new Month() { Number = 14, Length = to28, FullName = Model.MonthsTitle + " 14", EndType = MonthEndType.ManualEndOrOpen });
                    Model.Months.Add(new Month() { Number = 15, Length = to28, FullName = Model.MonthsTitle + " 15", EndType = MonthEndType.ManualEndOrOpen });
                    YearLengthSampleS[CalendarModelSample.Gregorian.ToString()].CopyToModel(Model);
                    Model.FirstMonthFirstDayType = FirstMonthFirstDayType.ContinueLastYear;
                    Model.LeapModel.DaysAssignment = ExtraDaysAssignment.None;
                    break;
                case CalendarModelSample.MoonCalendar:
                    var toFullMoon = new TimeOffsetAstronomic { OrbName = "Moon", OrbMember = OrbMember.PhaseNumber, Direction = Direction.Forward, PointType = AstroPointType.UpperPeak };

                    Model.ModelStartPoint = new TimePoint(new DateTime(iNowYear, 01, 01));
                    Model.ModelStartYearNumber = iNowYear;
                    Model.MonthsTitle = "Moon";
                    Model.Months.Add(new Month() { Number = 1, Length = toFullMoon, FullName = Model.MonthsTitle + " 1", ShortName = "M1" });
                    Model.Months.Add(new Month() { Number = 2, Length = toFullMoon, FullName = Model.MonthsTitle + " 2", ShortName = "M2" });
                    Model.Months.Add(new Month() { Number = 3, Length = toFullMoon, FullName = Model.MonthsTitle + " 3", ShortName = "M3" });
                    Model.Months.Add(new Month() { Number = 4, Length = toFullMoon, FullName = Model.MonthsTitle + " 4", ShortName = "M4" });
                    Model.Months.Add(new Month() { Number = 5, Length = toFullMoon, FullName = Model.MonthsTitle + " 5", ShortName = "M5" });
                    Model.Months.Add(new Month() { Number = 6, Length = toFullMoon, FullName = Model.MonthsTitle + " 6", ShortName = "M6" });
                    Model.Months.Add(new Month() { Number = 7, Length = toFullMoon, FullName = Model.MonthsTitle + " 7", ShortName = "M7" });
                    Model.Months.Add(new Month() { Number = 8, Length = toFullMoon, FullName = Model.MonthsTitle + " 8", ShortName = "M8" });
                    Model.Months.Add(new Month() { Number = 9, Length = toFullMoon, FullName = Model.MonthsTitle + " 9", ShortName = "M9" });
                    Model.Months.Add(new Month() { Number = 10, Length = toFullMoon, FullName = Model.MonthsTitle + " 10", ShortName = "M10" });
                    Model.Months.Add(new Month() { Number = 11, Length = toFullMoon, FullName = Model.MonthsTitle + " 11", ShortName = "M11" });
                    Model.Months.Add(new Month() { Number = 12, Length = toFullMoon, FullName = Model.MonthsTitle + " 12", ShortName = "M12" });
                    Model.Months.Add(new Month() { Number = 13, Length = toFullMoon, FullName = Model.MonthsTitle + " 13", ShortName = "M13" });
                    Model.Months.Add(new Month() { Number = 14, Length = toFullMoon, FullName = Model.MonthsTitle + " 14", ShortName = "M14" });
                    YearLengthSampleS[CalendarModelSample.Gregorian.ToString()].CopyToModel(Model);
                    Model.FirstMonthFirstDayType = FirstMonthFirstDayType.ContinueLastYear;
                    Model.LeapModel.DaysAssignment = ExtraDaysAssignment.None;
                    break;

                case CalendarModelSample.SolarSeasons:
                    Model.ModelStartPoint = new TimePoint(new DateTime(iNowYear, 01, 01), new TimeOffsetAstronomic { OrbName = "Sun", OrbMember = OrbMember.RA, PointType = AstroPointType.LowerPeak });
                    Model.ModelStartYearNumber = iNowYear;
                    Model.Months.Add(new Month() { Number = 1, Length = new TimeOffsetAstronomic { OrbName = "Sun", OrbMember = OrbMember.RA, PointType = AstroPointType.DestinationPoint, DestinationValue = 6 }, FullName = "Frühling", ShortName = "Frü" });
                    Model.Months.Add(new Month() { Number = 2, Length = new TimeOffsetAstronomic { OrbName = "Sun", OrbMember = OrbMember.RA, PointType = AstroPointType.DestinationPoint, DestinationValue = 12 }, FullName = "Sommer", ShortName = "Som" });
                    Model.Months.Add(new Month() { Number = 3, Length = new TimeOffsetAstronomic { OrbName = "Sun", OrbMember = OrbMember.RA, PointType = AstroPointType.DestinationPoint, DestinationValue = 18 }, FullName = "Herbst", ShortName = "Her" });
                    Model.Months.Add(new Month() { Number = 4, Length = new TimeOffsetAstronomic { OrbName = "Sun", OrbMember = OrbMember.RA, PointType = AstroPointType.LowerPeak }, FullName = "Winter", ShortName = "Win" });
                    Model.LeapModel.DaysAssignment = ExtraDaysAssignment.None;
                    Model.CommonYearLength = new TimeOffsetAstronomic { OrbName = "Sun", OrbMember = OrbMember.RA, PointType = AstroPointType.LowerPeak };
                    Model.LeapModel = new LeapModel();
                    break;

                case CalendarModelSample.WorldSeasons:
                    Model.ModelStartPoint = new TimePoint(new DateTime(iNowYear - 1, 12, 21));
                    Model.ModelStartYearNumber = iNowYear;
                    Model.Months.Add(new Month() { Number = 1, Length = new TimeOffsetStatic(91), FullName = "Winter", ShortName = "Win" });
                    Model.Months.Add(new Month() { Number = 2, Length = new TimeOffsetStatic(91), FullName = "Frühling", ShortName = "Frü" });
                    Model.Months.Add(new Month() { Number = 3, Length = new TimeOffsetStatic(91), FullName = "Sommer", ShortName = "Som" });
                    Model.Months.Add(new Month() { Number = 4, Length = new TimeOffsetStatic(91), FullName = "Herbst", ShortName = "Her" });
                    Model.CommonYearLength = new TimeOffsetStatic(365);
                    Model.LeapModel = new LeapModel();
                    Model.LeapModel.ConditionType = LeapConditionType.Algorithm;
                    Model.LeapModel.LeapOffset = new TimeOffsetStatic(1);
                    Model.LeapModel.LeapBeforeMonth = 2;
                    Model.LeapModel.DaysAssignment = ExtraDaysAssignment.AttachToMonth;
                    Model.LeapModel.LeapYearAlgo.Add(new LeapYearAlgoCfg() { Dividor = 4, IsLeap = true });
                    Model.LeapModel.LeapYearAlgo.Add(new LeapYearAlgoCfg() { Dividor = 100, IsLeap = false });
                    Model.LeapModel.LeapYearAlgo.Add(new LeapYearAlgoCfg() { Dividor = 400, IsLeap = true });
                    break;

                case CalendarModelSample.MayaHaab:
                    var to20 = new TimeOffsetStatic(20);

                    Model.ModelStartPoint = new TimePoint(new DateTime(2012, 4, 4));
                    Model.ModelStartYearNumber = 1;
                    Model.MayaCorrelation = MayaCalc.DefaultMayaCorrelation;

                    Model.FormatInfo.ShortYearPattern = "&MayaDate&";
                    Model.FormatInfo.MiddleYearPattern = "&MayaDate&";
                    Model.FormatInfo.LongYearPattern = "&MayaDate&";

                    Model.FormatInfo.ShortMonthDayPattern = "&MayaDateHaab&";
                    Model.FormatInfo.MiddleMonthDayPattern = "&MayaDateHaab&";
                    Model.FormatInfo.LongMonthDayPattern = "&MayaDateHaab&";

                    Model.FormatInfo.ShortYearMonthPattern = "&MayaDateTzolkin& &MayaDateHaab&";
                    Model.FormatInfo.MiddleYearMonthPattern = "&MayaDateTzolkin& &MayaDateHaab&";
                    Model.FormatInfo.LongYearMonthPattern = "&MayaDateTzolkin& &MayaDateHaab&";

                    Model.FormatInfo.ShortDatePattern = "&MayaDateTzolkin& &MayaDateHaab&";
                    Model.FormatInfo.MiddleDatePattern = "&MayaDateTzolkin& &MayaDateHaab&";
                    Model.FormatInfo.LongDatePattern = "&MayaDate&, &MayaDateTzolkin& &MayaDateHaab&";

                    Model.FormatInfo.LongDayPattern = "'Kin' &MayaHaabKin&";
                    Model.FormatInfo.LongWeekDayDayPattern = "dddd, &MayaDateHaab&";

                    Model.FormatInfo.OverrideMaxYearDigits = 2;

                    Model.Months.Add(new Month() { Number = 1, Length = to20, FullName = "Pop", ShortName = "Pop", DayNumberType = DayNumberType.ZeroBased });
                    Model.Months.Add(new Month() { Number = 2, Length = to20, FullName = "Uo", ShortName = "Uo", DayNumberType = DayNumberType.ZeroBased });
                    Model.Months.Add(new Month() { Number = 3, Length = to20, FullName = "Zip", ShortName = "Zip", DayNumberType = DayNumberType.ZeroBased });
                    Model.Months.Add(new Month() { Number = 4, Length = to20, FullName = "Zotz", ShortName = "Zotz", DayNumberType = DayNumberType.ZeroBased });
                    Model.Months.Add(new Month() { Number = 5, Length = to20, FullName = "Zec", ShortName = "Zec", DayNumberType = DayNumberType.ZeroBased });
                    Model.Months.Add(new Month() { Number = 6, Length = to20, FullName = "Xul", ShortName = "Xul", DayNumberType = DayNumberType.ZeroBased });
                    Model.Months.Add(new Month() { Number = 7, Length = to20, FullName = "Yaxkin", ShortName = "Yaxkin", DayNumberType = DayNumberType.ZeroBased });
                    Model.Months.Add(new Month() { Number = 8, Length = to20, FullName = "Mol", ShortName = "Mol", DayNumberType = DayNumberType.ZeroBased });
                    Model.Months.Add(new Month() { Number = 9, Length = to20, FullName = "Chen", ShortName = "Chen", DayNumberType = DayNumberType.ZeroBased });
                    Model.Months.Add(new Month() { Number = 10, Length = to20, FullName = "Yax", ShortName = "Yax", DayNumberType = DayNumberType.ZeroBased });
                    Model.Months.Add(new Month() { Number = 11, Length = to20, FullName = "Zac", ShortName = "Zac", DayNumberType = DayNumberType.ZeroBased });
                    Model.Months.Add(new Month() { Number = 12, Length = to20, FullName = "Ceh", ShortName = "Ceh", DayNumberType = DayNumberType.ZeroBased });
                    Model.Months.Add(new Month() { Number = 13, Length = to20, FullName = "Mac", ShortName = "Mac", DayNumberType = DayNumberType.ZeroBased });
                    Model.Months.Add(new Month() { Number = 14, Length = to20, FullName = "Kankin", ShortName = "Kankin", DayNumberType = DayNumberType.ZeroBased });
                    Model.Months.Add(new Month() { Number = 15, Length = to20, FullName = "Muan", ShortName = "Muan", DayNumberType = DayNumberType.ZeroBased });
                    Model.Months.Add(new Month() { Number = 16, Length = to20, FullName = "Pax", ShortName = "Pax", DayNumberType = DayNumberType.ZeroBased });
                    Model.Months.Add(new Month() { Number = 17, Length = to20, FullName = "Kayab", ShortName = "Kayab", DayNumberType = DayNumberType.ZeroBased });
                    Model.Months.Add(new Month() { Number = 18, Length = to20, FullName = "Cumku", ShortName = "Cumku", DayNumberType = DayNumberType.ZeroBased });
                    Model.Months.Add(new Month() { Number = 19, Length = new TimeOffsetStatic(5), FullName = "Wayeb", ShortName = "Wayeb", DayNumberType = DayNumberType.ZeroBased });
                    Model.CommonYearLength = new TimeOffsetStatic(365);
                    Model.LeapModel = new LeapModel();
                    break;

                case CalendarModelSample.SexagesimalCalendar:
                    var to1 = new TimeOffsetStatic(1);
                    var to60 = new TimeOffsetStatic(60);

                    Model.ModelStartPoint = new TimePoint(new DateTime(2012, 12, 21));
                    Model.ModelStartYearNumber = 1;
                    Model.WeekLength = 6;
                    Model.MonthWeekStartType = WeekStartType.FristDayIsFirstWeekDay;
                    Model.Months.Add(new Month() { Number = 1, Length = to60, FullName = "Frigée", ShortName = "Fri" });
                    Model.OutOfTimeSections.Add(new OutOfTimeSection { BeforeMonth = 1, Length = to1, FullName = "Bacchanal" });
                    Model.Months.Add(new Month() { Number = 2, Length = to60, FullName = "Éclose", ShortName = "Écl" });
                    Model.OutOfTimeSections.Add(new OutOfTimeSection { BeforeMonth = 2, Length = to1, FullName = "Ceres" });
                    Model.Months.Add(new Month() { Number = 3, Length = to60, FullName = "Florée", ShortName = "Flo" });
                    Model.OutOfTimeSections.Add(new OutOfTimeSection { BeforeMonth = 3, Length = to1, FullName = "Musica" });
                    Model.Months.Add(new Month() { Number = 4, Length = to60, FullName = "Granée", ShortName = "Gra" });
                    Model.OutOfTimeSections.Add(new OutOfTimeSection { BeforeMonth = 4, Length = to1, FullName = "Liber" });
                    Model.Months.Add(new Month() { Number = 5, Length = to60, FullName = "Récole", ShortName = "Réc" });
                    Model.OutOfTimeSections.Add(new OutOfTimeSection { BeforeMonth = 5, Length = to1, FullName = "Memento mori" });
                    Model.Months.Add(new Month() { Number = 6, Length = to60, FullName = "Caduce", ShortName = "Cad" });
                    Model.OutOfTimeSections.Add(new OutOfTimeSection { BeforeMonth = 6, Length = to1, FullName = "Sext" });
                    Model.CommonYearLength = new TimeOffsetStatic(365);
                    Model.LeapModel = new LeapModel();
                    Model.LeapModel.ConditionType = LeapConditionType.Algorithm;
                    Model.LeapModel.LeapOffset = new TimeOffsetStatic(1);
                    Model.LeapModel.DaysAssignment = ExtraDaysAssignment.None;
                    Model.LeapModel.LeapYearAlgo.Add(new LeapYearAlgoCfg() { Dividor = 4, IsLeap = true });
                    Model.LeapModel.LeapYearAlgo.Add(new LeapYearAlgoCfg() { Dividor = 100, IsLeap = false });
                    Model.LeapModel.LeapYearAlgo.Add(new LeapYearAlgoCfg() { Dividor = 400, IsLeap = true });
                    Model.FormatInfo.OverrideMaxYearDigits = 3;
                    break;
            }

            DynamicCalendarModel.AddCachedModel(Model);

            return Model;
        }

        static void AddCommonMonthName(DynamicCalendarModel model, int commonMonthNr, int monthNr, int monthLength, List<MonthDay> days = null)
        {
            DateTime t = new DateTime(2000, commonMonthNr, 1);
            var m = new Month() { Number = monthNr, Length = new TimeOffsetStatic(monthLength), FullName = t.ToString("MMMM"), ShortName = t.ToString("MMM") };
            if (days != null && days.Count > 0)
                m.Days = days;
            model.Months.Add(m);
        }

        static Dictionary<TimeOffsetAstronomic, string> _astroSamples;
        public static Dictionary<TimeOffsetAstronomic, string> AstroSamples
        {
            get
            {
                if (_astroSamples == null)
                {
                    _astroSamples = new Dictionary<TimeOffsetAstronomic, string>();
                    foreach (var x in AstroSamplesMoon)
                        try { _astroSamples.Add(x.Key, x.Value); } catch { }
                    foreach (var x in AstroSamplesSun)
                        try { _astroSamples.Add(x.Key, x.Value); } catch { }
                }
                return _astroSamples;
            }
        }

        static Dictionary<TimeOffsetAstronomic, string> _astroSamplesMoon;
        public static Dictionary<TimeOffsetAstronomic, string> AstroSamplesMoon
        {
            get
            {
                lock (oLock)
                {
                    if (_astroSamplesMoon == null)
                    {
                        _astroSamplesMoon = new Dictionary<TimeOffsetAstronomic, string>();
                        foreach (var x in MonthLengthSampleS)
                        {
                            if (x.OffsetLength is TimeOffsetAstronomic)
                                try { _astroSamplesMoon.Add((TimeOffsetAstronomic)x.OffsetLength, x.Title); } catch { }
                        }
                    }
                    return _astroSamplesMoon;
                }
            }
        }

        static Dictionary<TimeOffsetAstronomic, string> _astroSamplesSun;
        public static Dictionary<TimeOffsetAstronomic, string> AstroSamplesSun
        {
            get
            {
                lock (oLock)
                {
                    if (_astroSamplesSun == null)
                    {
                        _astroSamplesSun = new Dictionary<TimeOffsetAstronomic, string>();
                        foreach (var x in YearLengthSampleS.Values)
                        {
                            if (x.CommonYearLength is TimeOffsetAstronomic)
                                try { _astroSamplesSun.Add((TimeOffsetAstronomic)x.CommonYearLength, x.Title); } catch { }
                        }
                    }
                    return _astroSamplesSun;
                }
            }
        }

        static List<MonthLengthSample> _monthLengthSampleS;
        public static List<MonthLengthSample> MonthLengthSampleS
        {
            get
            {
                lock (oLock)
                {
                    if (_monthLengthSampleS == null)
                    {
                        _monthLengthSampleS = new List<MonthLengthSample>();
                        _monthLengthSampleS.Add(new MonthLengthSample { Title = "28 days", LengthS = new[] { 28 } });
                        _monthLengthSampleS.Add(new MonthLengthSample { Title = "gregorian", LengthS = new[] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 } });
                        _monthLengthSampleS.Add(new MonthLengthSample { Title = "Vollmond", OffsetLength = new TimeOffsetAstronomic { OrbName = "Moon", OrbMember = OrbMember.PhaseNumber, PointType = AstroPointType.UpperPeak } });
                        _monthLengthSampleS.Add(new MonthLengthSample { Title = "Neumond", OffsetLength = new TimeOffsetAstronomic { OrbName = "Moon", OrbMember = OrbMember.PhaseNumber, PointType = AstroPointType.LowerPeak } });
                    }
                    return _monthLengthSampleS;
                }
            }
        }

        static List<MonthNameSample> _monthNamesSampleS;
        public static List<MonthNameSample> MonthNamesSampleS
        {
            get
            {
                lock (oLock)
                {
                    if (_monthNamesSampleS == null)
                    {
                        _monthNamesSampleS = new List<MonthNameSample>();
                        List<string> fullNameS = new List<string>();
                        List<string> shortNameS = new List<string>();
                        for (int i = 1; i <= 12; i++)
                        {
                            DateTime t = new DateTime(2000, i, 1);
                            fullNameS.Add(t.ToString("MMMM"));
                            shortNameS.Add(t.ToString("MMM"));
                        }
                        _monthNamesSampleS.Add(new MonthNameSample("gregorian", fullNameS.ToArray(), shortNameS.ToArray()));

                        fullNameS.Insert(6, "Sol");
                        shortNameS.Insert(6, "Sol");
                        _monthNamesSampleS.Add(new MonthNameSample("InternationalFixed", fullNameS.ToArray(), shortNameS.ToArray()));

                        fullNameS[6] = "Luna";
                        shortNameS[6] = "Lun";
                        _monthNamesSampleS.Add(new MonthNameSample("NewEarth", fullNameS.ToArray(), shortNameS.ToArray()));

                        _monthNamesSampleS.Add(new MonthNameSample("latin", new[] { "primus", "secundus", "tertius", "quartus", "quintus", "sextus", "septimus", "octavus", "nonus", "decimus", "undecimus", "duodecimus", "tertius decimus", "quartus decimus", "quintus decimus", "sextus decimus", "septimus decimus", "duodevicesimus", "undevicesimus", "vocesimus", "vicesimus primus", "vicesimus alter", "vicesimus tertius", "vicesimus quartus", "vicesimus quintus", "vicesimus sextus", "vicesimus septimus", "duodetricesimus", "undetricesimus", "tricesimus", "tricesimus primus", "tricesimus alter", "tricesimus tertius" }));

                        _monthNamesSampleS.Add(new MonthNameSample("spain", new[] { "primero", "segundo", "tercero", "cuarto", "quinto", "sexto", "séptimo", "octavo", "noveno", "décimo", "undécimo", "duodécimo", "decimotercero", "decimocuarto", "decimoquinto", "decimosexto", "decimoséptimo", "decimoctavo", "decimonoveno", "vigésimo", "vigésimo primero", "vigésimo segundo", "vigésimo tercero", "vigésimo cuarto", "vigésimo quinto", "vigésimo sexto", "vigésimo séptimo", "vigésimo octavo", "vigésimo noveno", "trigésimo", "trigésimo primero", "trigésimo segundo", "trigésimo tercero" }));

                        _monthNamesSampleS.Add(new MonthNameSample("cylces", new[] { "Zyklus %M" }, new[] { "Z %M" }));
                    }
                    return _monthNamesSampleS;
                }
            }
        }


        static Dictionary<string, YearLengthSample> _yearLengthSampleS;
        public static Dictionary<string, YearLengthSample> YearLengthSampleS
        {
            get
            {
                lock (oLock)
                {
                    if (_yearLengthSampleS == null)
                    {
                        _yearLengthSampleS = new Dictionary<string, YearLengthSample>();

                        _yearLengthSampleS.Add(CalendarModelSample.Gregorian.ToString(), new YearLengthSample
                        {
                            Title = "3 mal 365, 1 mal 366",
                            CommonYearLength = new TimeOffsetStatic(365),
                            LeapModel = new LeapModel
                            {
                                ConditionType = LeapConditionType.Algorithm,
                                LeapOffset = new TimeOffsetStatic(1),
                                LeapBeforeMonth = 2,
                                DaysAssignment = ExtraDaysAssignment.AttachToMonth,
                                LeapYearAlgo = new List<LeapYearAlgoCfg>(new[]
                                      { new LeapYearAlgoCfg() { Dividor = 4, IsLeap = true },
                                    new LeapYearAlgoCfg() { Dividor = 100, IsLeap = false },
                                    new LeapYearAlgoCfg() { Dividor = 400, IsLeap = true } })
                            }
                        });

                        _yearLengthSampleS.Add(CalendarModelSample.NewEarth.ToString(), new YearLengthSample
                        {
                            Title = "4 mal 364, 1 mal 371",
                            CommonYearLength = new TimeOffsetStatic(364),
                            YearLengthAssignment = ExtraDaysAssignment.AttachToMonth,
                            YearLengthAssigBeforeMonth = -1,
                            LeapModel = new LeapModel
                            {
                                ConditionType = LeapConditionType.Algorithm,
                                LeapOffset = new TimeOffsetStatic(7),
                                LeapBeforeMonth = -1,
                                DaysAssignment = ExtraDaysAssignment.AttachToMonth,
                                LeapYearAlgo = new List<LeapYearAlgoCfg>(new[]
                                      { new LeapYearAlgoCfg() { Dividor = 5, IsLeap = true },
                                    new LeapYearAlgoCfg() { Dividor = 40, IsLeap = false },
                                    new LeapYearAlgoCfg() { Dividor = 400, IsLeap = true } })
                            }
                        });

                        _yearLengthSampleS.Add("gregJan1", new YearLengthSample
                        {
                            Title = "gerg. Neujahr",
                            CommonYearLength = new TimeOffsetGregorianDate { Month = 1, Day = 1 },
                            LeapModel = new LeapModel()
                        });

                        _yearLengthSampleS.Add("spring", new YearLengthSample
                        {
                            Title = "Frühlingsanfang",
                            FirstMonthFirstDayType = FirstMonthFirstDayType.ContinueLastYear,
                            CommonYearLength = new TimeOffsetAstronomic { OrbName = "Sun", OrbMember = OrbMember.RA, PointType = AstroPointType.LowerPeak },
                            LeapModel = new LeapModel()
                        });

                        _yearLengthSampleS.Add("birthday", new YearLengthSample
                        {
                            Title = "Geburtstag / Jubiläum",
                            OnlyUserAction = true,
                            UserAction = (m) =>
                            {
                                var tStartDay = xUserInput.Instance.GetDate("Start-Datum", new DateTime(1985, 5, 10)).Result;
                                if (tStartDay == null)
                                    return;
                                m.ModelStartPoint = new TimePoint { StartDate = tStartDay.Value };
                                m.ModelStartYearNumber = 0;
                                m.CommonYearLength = new TimeOffsetGregorianDate { Month = tStartDay.Value.Month, Day = tStartDay.Value.Day };
                                m.FirstMonthFirstDayType = FirstMonthFirstDayType.ContinueLastYear;
                                m.YearLengthAssignment = ExtraDaysAssignment.None;
                                m.LeapModel = sys.CloneObject(new LeapModel());
                                m.FormatInfo.OverrideMaxYearDigits = 3;
                            }
                        });

                        _yearLengthSampleS.Add("custom", new YearLengthSample
                        {
                            Title = "custom",
                            OnlyUserAction = true
                        });
                    }

                    return _yearLengthSampleS;
                }
            }
        }
    }

    public class MonthLengthSample
    {
        public string Title { get; set; }
        public int[] LengthS { get; set; }
        public TimeOffset OffsetLength { get; set; }
    }

    public class MonthNameSample
    {
        public MonthNameSample(string title, string[] fullNames)
        {
            Title = title;
            FullNames = fullNames;
            Numbers = new int[fullNames.Length];
            ShortNames = new string[fullNames.Length];

            int i = 0;
            foreach (string fn in fullNames)
            {
                Numbers[i] = i + 1;
                ShortNames[i] = fn.Substring(0, 3);
                i++;
            }
        }
        public MonthNameSample(string title, string[] fullNames, string[] shortNames)
        {
            Title = title;
            FullNames = fullNames;
            ShortNames = shortNames;
            Numbers = new int[fullNames.Length];

            int i = 0;
            foreach (string fn in fullNames)
            {
                Numbers[i] = i;
                i++;
            }
        }

        public string Title { get; }
        int[] Numbers { get; }
        public string[] FullNames { get; }
        public string[] ShortNames { get; }
        public Action<DynamicCalendarModel> UserAction { get; set; } = null;
        public bool OnlyUserAction { get; set; } = false;

        public void CopyToModel(DynamicCalendarModel model)
        {
            if (!OnlyUserAction)
            {
                int i = 0;
                int iMonth = 1;
                foreach (var m in model.Months)
                {
                    if (i >= FullNames.Length)
                        i = 0;

                    try
                    {
                        m.FullName = FullNames[i].Replace("%M", iMonth.ToString());
                        m.ShortName = ShortNames[i].Replace("%M", iMonth.ToString());
                    }
                    catch { }

                    i++;
                    iMonth++;
                }
            }
            UserAction?.Invoke(model);
        }

        public bool EqualsModel(DynamicCalendarModel model)
        {
            int i = 0;
            int iMonth = 1;
            foreach (var m in model.Months)
            {
                if (i >= FullNames.Length)
                    i = 0;

                try
                {
                    if (m.FullName != FullNames[i].Replace("%M", iMonth.ToString()) ||
                        m.ShortName != ShortNames[i].Replace("%M", iMonth.ToString()))
                        return false;
                }
                catch { }

                i++;
                iMonth++;
            }
            return true;
        }
    }

    public class YearLengthSample
    {
        public string Title { get; set; }
        public TimeOffset CommonYearLength { get; set; }
        public FirstMonthFirstDayType FirstMonthFirstDayType { get; set; } = FirstMonthFirstDayType.NumberOne;
        public ExtraDaysAssignment YearLengthAssignment { get; set; } = ExtraDaysAssignment.None;
        public int YearLengthAssigBeforeMonth { get; set; } = -1;
        public LeapModel LeapModel { get; set; }
        public Action<DynamicCalendarModel> UserAction { get; set; } = null;
        public bool OnlyUserAction { get; set; } = false;

        public void CopyToModel(DynamicCalendarModel model)
        {
            if (!OnlyUserAction)
            {
                int iStartYearNumberDiff = 0;
                if (model.ModelStartPoint != null)
                    iStartYearNumberDiff = model.ModelStartPoint.GetTimePoint(model).Year - model.ModelStartYearNumber;

                if (CommonYearLength.HasAstronomics)
                    model.ModelStartPoint = new TimePoint { StartDate = new DateTime(DateTime.Now.Year - 1, 3, 1), Offset = sys.CloneObject(CommonYearLength) };
                else if (CommonYearLength is TimeOffsetGregorianDate)
                    model.ModelStartPoint = new TimePoint { StartDate = new DateTime(DateTime.Now.Year - 1, 3, 1), Offset = sys.CloneObject(CommonYearLength) };
                else
                    model.ModelStartPoint = new TimePoint { StartDate = new DateTime(DateTime.Now.Year, 1, 1), Offset = null };

                model.ModelStartYearNumber = model.ModelStartPoint.GetTimePoint(model).Year - iStartYearNumberDiff;

                model.CommonYearLength = sys.CloneObject(CommonYearLength);
                model.FirstMonthFirstDayType = FirstMonthFirstDayType;
                model.YearLengthAssignment = YearLengthAssignment;
                model.YearLengthAssigBeforeMonth = YearLengthAssigBeforeMonth;
                model.LeapModel = sys.CloneObject(LeapModel);
            }
            UserAction?.Invoke(model);

            model.ClearCache();
        }

        public bool EqualsModel(DynamicCalendarModel model)
        {
            return model.CommonYearLength.Equals(CommonYearLength) &&
                model.FirstMonthFirstDayType == FirstMonthFirstDayType &&
                model.YearLengthAssignment == YearLengthAssignment &&
                model.YearLengthAssigBeforeMonth == YearLengthAssigBeforeMonth &&
                model.LeapModel.Equals(LeapModel);
        }
    }
}
