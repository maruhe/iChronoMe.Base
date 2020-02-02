using System;
using System.Xml.Serialization;

using iChronoMe.Core.Classes;

namespace iChronoMe.Core.DynamicCalendar
{
    public class TimePoint
    {
        public TimePoint() { }

        public TimePoint(DateTime startDate, TimeOffset offset = null)
        {
            StartDate = startDate;
            Offset = offset;
        }

        [XmlAttribute]
        public DateTime StartDate { get; set; }

        [XmlElement]
        public TimeOffset Offset { get; set; }

        public DateTime GetTimePoint(DynamicCalendarModel model)
        {
            return (StartDate + (Offset == null ? TimeSpan.FromTicks(0) : TimeSpan.FromDays(Offset.GetOffsetDays(StartDate, model))));
        }

        public string Info
        {
            get
            {
                return Offset == null ? StartDate.ToShortDateString() : Offset.Info;
            }
        }
    }


    [XmlInclude(typeof(TimeOffsetStatic))]
    [XmlInclude(typeof(TimeOffsetWeekDay))]
    [XmlInclude(typeof(TimeOffsetGregorianDate))]
    [XmlInclude(typeof(TimeOffsetAstronomic))]
    public abstract class TimeOffset
    {
        protected TimeSpan? _value = null;
        protected TimeSpan? _maxValue = null;
        public TimeSpan GetOffset(DateTime tStart, DynamicCalendarModel model, int nOffsetDays = 0)
        {
            //if (!_value.HasValue)
            {
                Resync(tStart, model, nOffsetDays);
                if (AddOnOffset != null)
                    _value += AddOnOffset.GetOffset(tStart + _value.Value, model);
            }
            return _value.Value;
        }

        public int GetOffsetDays(DateTime tStart, DynamicCalendarModel model, int nOffsetDays = 0)
        {
            double nDays = GetOffset(tStart, model, nOffsetDays).TotalDays;
            return (int)Math.Ceiling(nDays);
        }

        public TimeSpan GetMaxOffset(DynamicCalendarModel model)
        {
            if (!_maxValue.HasValue)
            {
                ResyncMax(model);
                if (AddOnOffset != null)
                    _maxValue += AddOnOffset.GetMaxOffset(model);
            }
            return _maxValue.Value;
        }

        public int GetMaxOffsetDays(DynamicCalendarModel model)
        {
            double nDays = GetMaxOffset(model).TotalDays;
            return (int)Math.Ceiling(nDays);
        }

        protected abstract void Resync(DateTime tStart, DynamicCalendarModel model, int nOffsetDays = 0);
        protected abstract void ResyncMax(DynamicCalendarModel model);

        public abstract string Info { get; }

        [XmlElement]
        public TimeOffset AddOnOffset { get; set; }

        public override bool Equals(object obj)
            => obj is TimeOffset && GetHashCode() == obj.GetHashCode();

        public override int GetHashCode()
            => base.GetHashCode();

        public bool IsFullStatic
        {
            get
            {
                bool bRes = this is TimeOffsetStatic;
                if (bRes && AddOnOffset != null)
                    bRes = bRes && AddOnOffset.IsFullStatic;
                return bRes;
            }
        }

        public bool HasAstronomics
        {
            get
            {
                if (this is TimeOffsetAstronomic)
                    return true;
                if (AddOnOffset != null)
                    return AddOnOffset.HasAstronomics;
                return false;
            }
        }
    }

    public class TimeOffsetStatic : TimeOffset
    {
        public TimeOffsetStatic() { }

        public TimeOffsetStatic(double nDays) { StaticOffsetDays = nDays; }

        [XmlAttribute]
        public double StaticOffsetDays { get; set; } = 1;

        public override string Info { get => sys.EzMzText((int)StaticOffsetDays, "Ein Tag", "{0} Tage") + (AddOnOffset == null ? "" : "++"); }

        protected override void Resync(DateTime tStart, DynamicCalendarModel model, int nOffsetDays = 0) { _value = TimeSpan.FromDays(StaticOffsetDays); }

        protected override void ResyncMax(DynamicCalendarModel model) { _maxValue = TimeSpan.FromDays(StaticOffsetDays); }

        public override int GetHashCode()
        {
            unchecked
            {
                // Choose large primes to avoid hashing collisions
                int HashingBase = GetType().GetHashCode();
                const int HashingMultiplier = 16777619;
                int hash = HashingBase;
                hash = (hash * HashingMultiplier) ^ StaticOffsetDays.GetHashCode();
                hash = (hash * HashingMultiplier) ^ (!Object.ReferenceEquals(null, AddOnOffset) ? AddOnOffset.GetHashCode() : 0);
                return hash;
            }
        }
    }

    public class TimeOffsetWeekDay : TimeOffset
    {
        [XmlAttribute]
        public int DayOfWeek { get; set; }
        [XmlAttribute]
        public Direction Direction { get; set; } = Direction.Forward;
        [XmlAttribute]
        public int Count { get; set; }

        public override string Info { get => "Tag " + DayOfWeek + (AddOnOffset == null ? "" : "++"); }

        protected override void Resync(DateTime tStart, DynamicCalendarModel model, int nOffsetDays = 0)
        {
            DynamicDate dDtate = model.GetDateFromUtcDate(tStart);
            int iCurrent = dDtate.DayOfWeek;
            int iTry = 0;
            while (iCurrent != DayOfWeek && iTry++ < 100)
            {
                dDtate = dDtate.AddDays((int)Direction);
                iCurrent = dDtate.DayOfWeek;
            }
            if (Count > 1)
                dDtate = dDtate.Add(TimeUnit.Week, (Count - 1) * (int)Direction);
            _value = dDtate.UtcDate - tStart;
        }

        protected override void ResyncMax(DynamicCalendarModel model) { _maxValue = TimeSpan.FromDays(model.WeekLength - 1); }

        public override int GetHashCode()
        {
            unchecked
            {
                // Choose large primes to avoid hashing collisions
                int HashingBase = GetType().GetHashCode();
                const int HashingMultiplier = 16777619;
                int hash = HashingBase;
                hash = (hash * HashingMultiplier) ^ DayOfWeek.GetHashCode();
                hash = (hash * HashingMultiplier) ^ Direction.GetHashCode();
                hash = (hash * HashingMultiplier) ^ Count.GetHashCode();
                hash = (hash * HashingMultiplier) ^ (!Object.ReferenceEquals(null, AddOnOffset) ? AddOnOffset.GetHashCode() : 0);
                return hash;
            }
        }
    }

    public class TimeOffsetGregorianDate : TimeOffset
    {
        int _day = 1;
        int _month = 1;
        [XmlAttribute]
        public int Month { get => _month; set { _month = value; checkme(); } }
        [XmlAttribute]
        public int Day { get => _day; set { _day = value; checkme(); } }
        [XmlAttribute]
        public Direction Direction { get; set; } = Direction.Forward;
        [XmlAttribute]
        public int Count { get; set; }

        public override string Info { get => "greg. " + Day + "." + Month + "." + (AddOnOffset == null ? "" : "++"); }

        void checkme()
        {
            if (_month < 1)
                _month = 1;
            if (_month > 12)
                _month = 12;

            if (_day > DateTime.DaysInMonth(1999, _month))
                _day = DateTime.DaysInMonth(1999, _month);
        }

        protected override void Resync(DateTime tStart, DynamicCalendarModel model, int nOffsetDays = 0)
        {
            DateTime tNext = new DateTime(tStart.Year, Month, Day);

            if (tNext <= tStart && Direction == Direction.Forward)
                tNext = new DateTime(tStart.Year + 1, Month, Day);
            else if (tNext >= tStart && Direction == Direction.Backward)
                tNext = new DateTime(tStart.Year - 1, Month, Day);

            if (Count > 1)
                tNext = new DateTime(tNext.Year + ((Count - 1) * (int)Direction), Month, Day);

            _value = tNext - tStart.Date;
        }

        protected override void ResyncMax(DynamicCalendarModel model) { _maxValue = TimeSpan.FromDays(365); }

        public override int GetHashCode()
        {
            unchecked
            {
                // Choose large primes to avoid hashing collisions
                int HashingBase = GetType().GetHashCode();
                const int HashingMultiplier = 16777619;
                int hash = HashingBase;
                hash = (hash * HashingMultiplier) ^ Month.GetHashCode();
                hash = (hash * HashingMultiplier) ^ Day.GetHashCode();
                hash = (hash * HashingMultiplier) ^ Direction.GetHashCode();
                hash = (hash * HashingMultiplier) ^ Count.GetHashCode();
                hash = (hash * HashingMultiplier) ^ (!Object.ReferenceEquals(null, AddOnOffset) ? AddOnOffset.GetHashCode() : 0);
                return hash;
            }
        }
    }

    public class TimeOffsetAstronomic : TimeOffset
    {
        [XmlAttribute]
        public string OrbName { get; set; } = "Sun";
        [XmlAttribute]
        public OrbMember OrbMember { get; set; }
        [XmlAttribute]
        public AstroPointType PointType { get; set; } = AstroPointType.UpperPeak;
        [XmlAttribute]
        public double DestinationValue { get; set; }
        [XmlAttribute]
        public Direction Direction { get; set; } = Direction.Forward;
        [XmlAttribute]
        public int Count { get; set; } = 1;

        public override string Info
        {
            get
            {
                if (DynamicCalendarModelAssistent.AstroSamples.ContainsKey(this))
                    return DynamicCalendarModelAssistent.AstroSamples[this];
                return OrbName + ":" + OrbMember + " " + PointType + (AddOnOffset == null ? "" : "++");
            }
        }

        protected override void Resync(DateTime tStart, DynamicCalendarModel model, int nOffsetDays = 0)
        {
            string cMember = OrbName + OrbMember;
            PeakPoint peak = GetNextPeak(tStart.AddDays(nOffsetDays * (int)Direction), model);
            if (peak == null)
            {
                _value = TimeSpan.FromTicks(0);
            }
            else
            {
                if (Direction == Direction.Forward && peak.DateTime <= tStart)
                    peak = GetNextPeak(peak.PeakEnd.AddDays(10), model);

                if (Direction == Direction.Backward && peak.DateTime >= tStart)
                    peak = GetNextPeak(peak.PeakStart.AddDays(-10), model);


                for (int i = 1; i < Count; i++)
                {
                    peak = GetNextPeak(peak.PeakEnd.AddDays(3), model);
                }

                _value = peak.DateTime - tStart;
            }
        }

        protected override void ResyncMax(DynamicCalendarModel model)
        {
            _maxValue = AstroPeaks.GetMaxCycleLength(OrbName, OrbMember.ToString());
        }

        protected PeakPoint GetNextPeak(DateTime tStart, DynamicCalendarModel model)
        {
            string cMember = OrbName + OrbMember;

            if (tStart == DateTime.MinValue || tStart.Year < 1900 || tStart.Year > 2100)
                tStart.ToString();

            switch (Direction)
            {

                case Direction.Forward:

                    switch (PointType)
                    {
                        case AstroPointType.DestinationPoint:
                            return AstroPeaks.FindPeakAt(DestinationValue, tStart, cMember);

                        case AstroPointType.UpperPeak:
                        case AstroPointType.LowerPeak:
                            return AstroPeaks.FindPeakAfter(tStart, (int)PointType, cMember);
                    }
                    break;

                case Direction.Backward:
                    return AstroPeaks.FindPeakBefore(tStart, (int)Direction, cMember);
            }
            throw new Exception("Invalid TimeOffsetAstronomic-Config");
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // Choose large primes to avoid hashing collisions
                int HashingBase = GetType().GetHashCode();
                const int HashingMultiplier = 16777619;
                int hash = HashingBase;
                hash = (hash * HashingMultiplier) ^ (!Object.ReferenceEquals(null, OrbName) ? OrbName.GetHashCode() : 0);
                hash = (hash * HashingMultiplier) ^ (!Object.ReferenceEquals(null, OrbMember) ? OrbMember.GetHashCode() : 0);
                hash = (hash * HashingMultiplier) ^ PointType.GetHashCode();
                hash = (hash * HashingMultiplier) ^ DestinationValue.GetHashCode();
                hash = (hash * HashingMultiplier) ^ Direction.GetHashCode();
                hash = (hash * HashingMultiplier) ^ Count.GetHashCode();
                hash = (hash * HashingMultiplier) ^ (!Object.ReferenceEquals(null, AddOnOffset) ? AddOnOffset.GetHashCode() : 0);
                return hash;
            }
        }
    }
}
