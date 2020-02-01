using iChronoMe.Core.Classes;
using iChronoMe.Core.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace iChronoMe.Core.DynamicCalendar
{
    public struct DynamicDate : IComparable, IComparable<DynamicDate>, IEquatable<DynamicDate>, IFormattable, IConvertible
    {
        public static bool FormatLBMode = false;

        public static readonly DynamicDate EmptyDate = new DynamicDate();
        
        public string ModelID { get; }
        public int Year { get; }
        public int Month { get; }
        public int Day { get; }

        bool HasData { get; }

        int? _dayOfWeek;
        int? _utcYear;
        int? _utcDayOfYear;

        public DynamicDate(DynamicCalendarModel mModel, int iYear, int iMonth, int iDay, DateTime? tUtcDate = null)
        {
            HasData = true;
            ModelID = mModel.ID;
            Year = iYear;
            Month = iMonth;
            Day = iDay;

            while (Month < 0)
            {
                Month += mModel.GetMonthsOfYear(Year - 1); //Tage zählen!! auch bei +!!
                Year--;
            }
            while (Day < 0)
            {
                Day += Month > 0 
                    ? mModel.GetDaysOfMonth(Year, Month - 1) + mModel.GetOutOfTimeDays(Year, Month - 1) 
                    : mModel.GetDaysOfMonth(Year - 1, mModel.GetMonthsOfYear(Year - 1) - 1) + mModel.GetOutOfTimeDays(Year - 1, mModel.GetMonthsOfYear(Year - 1) -1) + mModel.GetOutOfTimeDays(Year - 1, mModel.GetMonthsOfYear(Year - 1));
                Month--;
                while (Month < 0)
                {
                    Month += mModel.GetMonthsOfYear(Year - 1);
                    Year--;
                }
            }
            while (Month > mModel.GetMonthsOfYear(Year) || (Month == mModel.GetMonthsOfYear(Year) && Day >= mModel.GetOutOfTimeDays(Year, Month)))
            {
                if (Month == mModel.GetMonthsOfYear(Year))
                    Day -= mModel.GetOutOfTimeDays(Year, Month);
                Month -= mModel.GetMonthsOfYear(Year);
                Year++;
            }
            while (Day >= mModel.GetDaysOfMonth(Year, Month) + mModel.GetOutOfTimeDays(Year, Month))
            {
                Day -= mModel.GetDaysOfMonth(Year, Month) + mModel.GetOutOfTimeDays(Year, Month);
                Month++;
                while (Month > mModel.GetMonthsOfYear(Year) || (Month == mModel.GetMonthsOfYear(Year) && Day >= mModel.GetOutOfTimeDays(Year, Month)))
                {
                    if (Month == mModel.GetMonthsOfYear(Year))
                        Day -= mModel.GetOutOfTimeDays(Year, Month);
                    Month -= mModel.GetMonthsOfYear(Year);
                    Year++;
                }
            }

            _dayOfWeek = null;
            if (tUtcDate != null)
            {
                _utcYear = tUtcDate.Value.Year;
                _utcDayOfYear = tUtcDate.Value.DayOfYear;
            }
            else
            {
                _utcYear = -1;
                _utcDayOfYear = -1;
            }
        }

        public int ID
        {
            get => Year * 100000 + Month * 1000 + Day;
        }

        public bool IsEmpty
        {
            get => !HasData;
        }
        
        public bool IsOutOfTime
        {
            get => HasData && (Day < Model.GetOutOfTimeDays(Year, Month));
        }
        
        public DynamicCalendarModel Model { get => DynamicCalendarModel.GetCachedModel(ModelID); }

        public DateTime UtcDate
        {
            get
            {
                if (IsEmpty)
                    return DateTime.Now;
                if (_utcYear < 0) {
                    DateTime t = Model.GetUtcDateFromDate(Year, Month, Day);
                    _utcYear = t.Year;
                    _utcDayOfYear = t.DayOfYear;
                }
                return new DateTime(_utcYear.Value, 1, 1).AddDays(_utcDayOfYear.Value - 1);
            }
        }
        
        public int DayNumber
        {
            get
            {
                if (IsEmpty)
                    return -1;

                int iDayNumber = Day;
                int iOotDays = Model.GetOutOfTimeDays(Year, Month);
                //int iMonthDays = Model.GetDaysOfMonth(Year, Month);

                if (Month == 0 && Model.FirstMonthFirstDayType == FirstMonthFirstDayType.ContinueLastYear)
                    iDayNumber += Model.GetYearInfo(Year).FirstMonthFirstDayNumber;

                DayNumberType nrType = DayNumberType.OneBased;
                if (Model.Months.Count > Month)
                    nrType = Model.Months[Month].DayNumberType;

                if (Day >= iOotDays) //OutOfTime
                    iDayNumber -= iOotDays;
                else if (Model.OutOfTimeSectionsDict.ContainsKey(Month))
                    nrType = Model.OutOfTimeSectionsDict[Month].DayNumberType;

                switch (nrType)
                {
                    case DayNumberType.ZeroBased:
                        return iDayNumber;
                    case DayNumberType.OneBased:
                        return iDayNumber + 1;
                    case DayNumberType.Dynamic:
                        try
                        {
                            if (Day >= iOotDays)
                                return Model.Months[Month].Days[iDayNumber].Number;
                            else
                                return Model.OutOfTimeSectionsDict[Month].Days[iDayNumber].Number;
                        }
                        catch
                        {
                            return -3;
                        }
                }

                return -2;
            }
        }

        public int MonthNumber
        {
            get
            {
                if (IsEmpty)
                    return -1;

                switch (Model.MonthNumberType)
                {
                    case MonthNumberType.ZeroBased:
                        return Month;
                    case MonthNumberType.OneBased:
                        return Month + 1;
                    case MonthNumberType.Dynamic:
                        try
                        {
                            return Model.Months[Month].Number;
                        }
                        catch
                        {
                            return -3;
                        }
                }

                return -2;
            }
        }

        public int YearNumber
        {
            get
            {
                if (IsEmpty)
                    return -1;

                return Model.GetYearNumber(Year);
            }
        }
        
        public int DayOfWeek
        {
            get
            {
                if (IsEmpty)
                    return -1;
                if (_dayOfWeek == null)
                    _dayOfWeek = Model.GetWeekDay(Year, Month, Day);
                return _dayOfWeek.Value;
            }
        }

        public int DayOfYear
        {
            get
            {
                return Model.GetMonthStartDayNumber(Year, Month) + Day;
            }
        }

        public int WeekNumber
        {
            get
            {
                return GetWeekOfYearFullDays(Model.FirstDayOfWeek, Model.FirstFullWeekMinLength);
            }
        }

        private int GetWeekOfYearFullDays(int firstDayOfWeek, int minFullDays)
        {
            //Cloned from System.Calendar
            if (UtcDate.Year <= 1900)
                return -1;

            var yi = Model.GetYearInfo(Year);
            int iWeekLength = Model.WeekLength;
            
            int dayOfYear = Model.GetMonthStartDayNumber(Year, Month) + Day ; //?? -1??
            if (!Model.ContinueWeeksOutOfTime)
            {
                for (int i = 0; i < Month; i++)
                    dayOfYear -= Model.GetOutOfTimeDays(Year, i);
            }
            int dayForYearDay1 = DayOfWeek - (dayOfYear % iWeekLength);

            int offset = (firstDayOfWeek - dayForYearDay1 + (iWeekLength*2)) % iWeekLength;
            if (offset != 0 && offset >= minFullDays)
            {
                offset -= iWeekLength;
            }
            int day = dayOfYear - offset;
            if (day >= 0)
            {
                return (day / iWeekLength + 1);
            }

            return AddDays(-(dayOfYear + 1)).GetWeekOfYearFullDays(firstDayOfWeek, minFullDays);
        }

        public string WeekDayNameFull
        {
            get => IsEmpty ? "-" : Model.GetWeekDay(DayOfWeek).FullName;
        }

        public string WeekDayNameShort
        {
            get => IsEmpty ? "-" : Model.GetWeekDay(DayOfWeek).ShortName;
        }

        public string MonthDayNameFull
        {
            get => IsEmpty || Model.Months.Count <= Month ? "-" : Model.GetMonthDay(Year, Month, Day)?.FullName ?? DayNumber.ToString();
        }

        public string MonthDayNameShort
        {
            get => IsEmpty || Model.Months.Count <= Month ? "-" : Model.GetMonthDay(Year, Month, Day)?.ShortName ?? DayNumber.ToString();
        }

        public string MonthNameShort
        { 
            get => IsEmpty || Model.Months.Count <= Month ? "-" : Model.Months[Month].ShortName;
        }
        
        public string MonthNameFull
        {
            get => IsEmpty || Model.Months.Count <= Month ? "-" : Model.Months[Month].FullName;
        }

        public DynamicDate AddDays(int iAdd)
        {
            return new DynamicDate(Model, Year, Month, Day + iAdd);

            if (Year - Model.ModelStartYearID > 200 || Model.ModelStartYearID - Year > 200)
                return Model.GetDateFromUtcDate(DateTime.Now);
            return Model.GetDateFromUtcDate(UtcDate.AddDays(iAdd));
        }

        public DynamicDate Add(TimeUnit unit, int count)
        {
            if (unit == TimeUnit.Day)
                return AddDays(count);
            else if (unit == TimeUnit.Week)
                return AddDays(count * Model.WeekLength);
            else {
                if (Year - Model.ModelStartYearID > 200 || Model.ModelStartYearID - Year > 200)
                    return Model.GetDateFromUtcDate(DateTime.Now);

                int iYear = Year;
                int iMonth = Month;
                if (unit == TimeUnit.Year)
                    iYear += count;
                if (unit == TimeUnit.Month)
                    iMonth += count;
                int iDay = Day;
                while (iMonth < 0)
                {
                    iYear--;
                    iMonth += Model.GetMonthsOfYear(iYear);
                } 
                while (iMonth > Model.GetMonthsOfYear(iYear)-1)
                {
                    iMonth -= Model.GetMonthsOfYear(iYear);
                    iYear++;
                }
                int iMaxDay = Model.GetDaysOfMonth(iYear, iMonth) - 1;
                if (iDay > iMaxDay)
                    iDay = iMaxDay;
                return new DynamicDate(Model, iYear, iMonth, iDay);
            }
        }

        public (xColor TextColor, xColor BackgroundColor) GetDayColors(xColor clTextDefault, xColor clBackDefault)
        {
            
            WeekDay wd = Model.GetWeekDay(DayOfWeek);
            if (wd.HasSpecialTextColor)
                clTextDefault = wd.SpecialTextColor;
            if (wd.HasSpecialBackgroundColor)
                clBackDefault = wd.SpecialBackgroundColor;

            if (Model.Months.Count > Month && Model.Months[Month].Days.Count > Day)
            {
                MonthDay md = Model.Months[Month].Days[Day];
                if (md.HasSpecialTextColor)
                    clTextDefault = md.SpecialTextColor;
                if (md.HasSpecialBackgroundColor)
                    clBackDefault = md.SpecialBackgroundColor;
            }

            return (clTextDefault, clBackDefault);
        }

        public DynamicDate BoW
        {
            get
            {
                int iFirstShownDay = DayOfWeek - Model.FirstDayOfWeek;
                if (iFirstShownDay < 0)
                    iFirstShownDay += Model.WeekLength;
                return AddDays(iFirstShownDay * -1);
            }
        }

        public DynamicDate BoM { get => new DynamicDate(Model, Year, Month, 0); }

        public DynamicDate BoY { get => new DynamicDate(Model, Year, 0, 0); }

        public DynamicDate EoW
        {
            get
            {
                int iFirstShownDay = DayOfWeek - Model.FirstDayOfWeek;
                if (iFirstShownDay < 0)
                    iFirstShownDay += Model.WeekLength;
                return AddDays(iFirstShownDay * -1 + Model.WeekLength - 1);
            }
        }

        public DynamicDate EoM { get => new DynamicDate(Model, Year, Month, Model.GetDaysOfMonth(Year, Month) - 1); }

        public DynamicDate EoY
        {
            get
            {
                int iLastMonth = Model.GetMonthsOfYear(Year) - 1;
                return new DynamicDate(Model, Year, iLastMonth, Model.GetDaysOfMonth(Year, iLastMonth) - 1);
            }
        }

        public (DynamicDate dMonthStart, DynamicDate dFirstMonthDay, DynamicDate dLastMonthDay, DynamicDate dOutOfTimeEnd, int iOotDaysBefore, int iOotDaysAfter) GetMonthCorners()
        {
            DynamicDate dMonthStart = BoM;
            DynamicDate dFirstMonthDay = dMonthStart;
            DynamicDate dLastMonthDay = EoM;
            DynamicDate dOutOfTimeEnd = dLastMonthDay;
            int iOotBefore = Model.GetOutOfTimeDays(Year, Month);
            int iOotAfter = Model.GetOutOfTimeDays(Year, Month + 1);
            if (iOotBefore > 0)
                dFirstMonthDay = dMonthStart.AddDays(iOotBefore);
            if (iOotAfter > 0)
                dOutOfTimeEnd = dLastMonthDay.AddDays(iOotAfter);

            return (dMonthStart, dFirstMonthDay, dLastMonthDay, dOutOfTimeEnd, iOotBefore, iOotAfter);
        }

        #region Operators, Converters and System-Stuff

        public override string ToString()
            => ToString(string.Empty, null);
        
        public string ToString(string format)
            => ToString(format, null);

        public string ToString(IFormatProvider provider)
            => ToString(string.Empty, provider);

        public string ToString(string format, IFormatProvider provider)
        {
            if (String.IsNullOrEmpty(format)) format = "G";
            if (provider == null) provider = CultureInfo.CurrentCulture;
            return DynamicDateFormat.Format(this, format, provider);
        }

        public string ToDynamicString(string format, DynamicDateFormatInfo ddfi)
        {
            if (String.IsNullOrEmpty(format)) format = "G";
            //if (ddfi == null) ddfi = DynamicDateFormatInfo.CurrentCulture;
            return DynamicDateFormat.Format(this, format, ddfi);
        }
        
        public override bool Equals(object other)
        {
            if (other is DynamicDate)
                return Equals((DynamicDate)other);
            if (other is DateTime)
                return UtcDate.Equals(((DateTime)other).ToUniversalTime());
            return false;
        }

        public bool Equals(DynamicDate other)
            => ID == other.ID && ModelID == other.ModelID;

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;
            if (obj is DynamicDate)
            {
                DynamicDate dd = (DynamicDate)obj;

                //Compary different Models via UtcDate (may be slower..)
                if (ModelID != dd.ModelID)
                    return (UtcDate.CompareTo(dd.UtcDate));

                if (ID < dd.ID)
                    return -1;
                if (ID > dd.ID)
                    return 1;
                return 0;
            }
            throw new ArgumentException();
        }

        public static bool operator == (DynamicDate d1, DynamicDate d2)
        {
            return d1.Equals(d2);
        }

        public static bool operator != (DynamicDate d1, DynamicDate d2)
        {
            return !d1.Equals(d2);
        }

        public static bool operator <= (DynamicDate d1, DynamicDate d2)
        {
            if (d1.ModelID != d2.ModelID)
                return (d1.UtcDate <= d2.UtcDate);

            return d1.ID <= d2.ID;
        }

        public static bool operator >= (DynamicDate d1, DynamicDate d2)
        {
            if (d1.ModelID != d2.ModelID)
                return (d1.UtcDate >= d2.UtcDate);

            return d1.ID >= d2.ID;
        }

        public static bool operator < (DynamicDate d1, DynamicDate d2)
        {
            if (d1.ModelID != d2.ModelID)
                return (d1.UtcDate < d2.UtcDate);

            return d1.ID < d2.ID;
        }

        public static bool operator > (DynamicDate d1, DynamicDate d2)
        {
            if (d1.ModelID != d2.ModelID)
                return (d1.UtcDate > d2.UtcDate);

            return d1.ID > d2.ID;
        }

        public static TimeSpan operator - (DynamicDate d1, DynamicDate d2)
        {
            return (d1.UtcDate - d2.UtcDate);
        }

        public override int GetHashCode()
        {
            return ID;
        }

        public int CompareTo(DynamicDate other)
        {
            if (ID < other.ID)
                return -1;
            if (ID > other.ID)
                return 1;
            return 0;
        }

        public TypeCode GetTypeCode()
            => TypeCode.String;

        public bool ToBoolean(IFormatProvider provider)
            => !IsEmpty;

        public byte ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            return UtcDate;
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public short ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public int ToInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public long ToInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public float ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public enum TimeUnit
    {
        Day,
        Week,
        Month,
        Year
    }
}
