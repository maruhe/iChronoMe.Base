using iChronoMe.Core.Classes;
using iChronoMe.Core.Types;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Xamarin.Essentials;

namespace iChronoMe.Core
{
    public class LocationTimeHolder : SelectPositionReceiver
    {
        bool bIsRunning = true;

        private DateTime _UtcNow = DateTime.MinValue;
        public DateTime UtcNow
        {
            get
            {
                if (_UtcNow == DateTime.MinValue)
                    return TimeHolder.GMTTime;
                return _UtcNow;
            }
            set => _UtcNow = value;
        }

        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public string CountryName { get; private set; }
        public string AreaName { get; private set; }
        public double TimeZoneOffset { get
            {
                if (timeZoneInfo != null)
                {
                    if (timeZoneInfo.IsDaylightSavingTime(UtcNow))
                        return TimeZoneOffsetDst;
                }
                return TimeZoneOffsetGmt;
            }
        }

        public double TimeZoneOffsetGmt { get; set; }
        public double TimeZoneOffsetDst { get; set; }
        public string TimeZoneName { get; private set; }
        public TimeZoneInfo timeZoneInfo { get; set; }

        public bool IsDst
        {
            get
            {
                if (timeZoneInfo != null)
                {
                    return timeZoneInfo.IsDaylightSavingTime(UtcNow);
                }
                return false;
            }
        }

        Thread mSecondThread;
        public delegate void ChangedEvent();
        public event ChangedEvent TimeChanged;
        public event ChangedEvent AreaChanged;

        public LocationTimeHolder(double nLatitude, double nLongitude, bool bAsync = true)
        {
            ChangePosition(nLatitude, nLongitude, true, bAsync);
        }

        public void ChangePosition(double nLatitude, double nLongitude, bool bClearAreaInfo = false, bool bAsync = true)
        {
            if ((Latitude == nLatitude) && (Longitude == nLongitude))
                return;

            double nDistance = Location.CalculateDistance(Latitude, Longitude, nLatitude, nLongitude, DistanceUnits.Kilometers);
            if (nDistance > 2.5)
                bClearAreaInfo = true;

            if (Latitude == 0 && Longitude == 0)
                TimeZoneOffsetGmt = (long)Math.Floor((nLongitude - 7.500000001) / 15) + 1; //mal so ein ungefär..
            else
            {
                if (bClearAreaInfo)
                {
                    AreaName = "";
                    if (nDistance > 25)
                    {
                        AreaName = "...";
                        CountryName = "";
                    }
                    /*if (ai != null)
                    {
                        AreaName = ai.toponymName;
                        CountryName = ai.countryName;
                    }*/
                }
            }

            Latitude = nLatitude;
            Longitude = nLongitude;

            if ((ai == null || !ai.CheckBoxIsUpToDate(nLatitude, nLongitude)) && bAsync)
            {
                Task.Factory.StartNew(() =>
                {
                    GetLocationInfo();
                });
            }
            if (!bAsync)
                GetLocationInfo();
            try { if (TimeChanged != null) TimeChanged(); } catch { }
        }

        public void Start(bool observeRealSunTime, bool observeMiddleSunTime, bool observeWorldTime)
        {
            if (!bIsRunning || mSecondThread == null)
            {
                bIsRunning = true;
                mSecondThread = new Thread(() =>
                {
                    while (bIsRunning)
                    {
                        int iSleep = 1000;
                        if (observeRealSunTime)
                            iSleep = Math.Min(iSleep, 1000 - RealSunTime.Millisecond);
                        if (observeMiddleSunTime)
                            iSleep = Math.Min(iSleep, 1000 - MidSunTime.Millisecond);
                        if (observeWorldTime)
                            iSleep = Math.Min(iSleep, 1000 - UtcNow.Millisecond);

                        Thread.Sleep(iSleep);
                        if (bIsRunning)
                            try { TimeChanged?.Invoke(); } catch { }
                    }
                    mSecondThread = null;
                });
                mSecondThread.IsBackground = true;
                mSecondThread.Start();
            }
        }

        public void Stop()
        {
            bIsRunning = false;
            mSecondThread = null;
        }

        GeoInfo.AreaInfo ai = null;
        private void GetLocationInfo(bool bInternal = false)
        {
            double nLatitude = Latitude;
            double nLongitude = Longitude;
            ai = GeoInfo.GetAreaInfo(Latitude, Longitude);
            if (nLatitude != Latitude || nLongitude != Longitude)
                return; //wenn's z'lang dauert..
            if (ai == null)
            {
                AreaName = "";
            }
            else
            {
                AreaName = ai.toponymName;
                CountryName = ai.countryName;
                TimeZoneName = ai.timezoneId;
                TimeZoneOffsetGmt = ai.gmtOffset;
                TimeZoneOffsetDst = ai.dstOffset;
                if (ai.timeZoneInfo != null)
                {
                    timeZoneInfo = ai.timeZoneInfo;
                }
            }
            if (!bInternal)
            {
                try { AreaChanged?.Invoke(); } catch { }
                try { TimeChanged?.Invoke(); } catch { }
            }
        }

        public void ReceiveSelectedPosition(SelectPositionResult pos)
        {
            ChangePosition(pos.Latitude, pos.Longitude);
            AreaName = pos.Title;
        }

        public TimeSpan UTCGeoLngDiff { get => GetUTCGeoLngDiff(Longitude); }

        public static TimeSpan GetUTCGeoLngDiff(double lng)
        {
            long iLngGrad = (long)lng;
            double nLngMin = ((lng - iLngGrad) * 60);
            long iLngMin = (long)nLngMin;
            double nLngSec = (nLngMin - iLngMin) * 60;

            return new TimeSpan().Add(TimeSpan.FromMinutes(iLngGrad * 4)).Add(TimeSpan.FromSeconds(nLngMin * 4)); //Ein Grad ist 4 Minuten // Eine Minute (') ist 4 Sekunden
        }

        public DateTime TimeZoneTime { get { return UtcNow.Add(TimeSpan.FromHours(TimeZoneOffset)); } }
        public DateTime MidSunTime { get { return UtcNow.Add(UTCGeoLngDiff); } }

        public TimeSpan SunMidRealDiff { get => GetSunMidRealDiff(UtcNow); }
        public static TimeSpan GetSunMidRealDiff(DateTime now)
        {
            double iSecOfYear = ((now.DayOfYear - 1) * 86400) + ((now.Hour - 12) * 3600) + (now.Minute * 60) + now.Second;
            double nYearDone = iSecOfYear / sys.OneYear / 86400;
            double dTimeEquation = 229.18 * (0.000075 + 0.001868 * Math.Cos(2 * Math.PI * nYearDone) - 0.032077 * Math.Sin(2 * Math.PI * nYearDone) - 0.014615 * Math.Cos(4 * Math.PI * nYearDone) - 0.040849 * Math.Sin(4 * Math.PI * nYearDone));

            //double iDayOfYear = now.DayOfYear + ((double)now.Hour / 24) + ((double)now.Minute / 1440) + ((double)now.Second / 86400);
            //double dTimeEquation = 60 * (-0.171 * Math.Sin(0.0337 * iDayOfYear + 0.465) - 0.1299 * Math.Sin(0.01787 * iDayOfYear - 0.168));

            return TimeSpan.FromMinutes(dTimeEquation);
        }

        public TimeSpan OldSunMidRealDiff
        {
            get
            {
                DateTime now = TimeZoneTime;
                double iDayOfYear = now.DayOfYear + ((double)now.Hour / 24) + ((double)now.Minute / 1440) + ((double)now.Second / 86400);
                double dTimeEquation = 60 * (-0.171 * Math.Sin(0.0337 * iDayOfYear + 0.465) - 0.1299 * Math.Sin(0.01787 * iDayOfYear - 0.168));

                return TimeSpan.FromMinutes(dTimeEquation);
            }
        }

        public DateTime RealSunTime { get { return MidSunTime.Add(SunMidRealDiff); } }
        public DateTime OldRealSunTime { get { return MidSunTime.Add(OldSunMidRealDiff); } }

        public DateTime GetTime(TimeType type = TimeType.RealSunTime)
        {
            switch (type)
            {
                case TimeType.RealSunTime:
                    return RealSunTime;
                case TimeType.MiddleSunTime:
                    return MidSunTime;
                case TimeType.TimeZoneTime:
                    return TimeZoneTime;
                case TimeType.UtcTime:
                    return UtcNow;
#if DEBUG
                case TimeType.RealSunTimeOld:
                    return OldRealSunTime;
#endif
                default:
                    return DateTime.MinValue;

            }
        }

            public DateTime GetTime(TimeType type = TimeType.RealSunTime, DateTime? tUtcNow = null)
        {
            DateTime oldNow = _UtcNow;
            if (tUtcNow != null)
                UtcNow = (DateTime)tUtcNow;
            try
            {
                return GetTime(type);
            }
            finally
            {
                if (tUtcNow != null)
                    UtcNow = oldNow;
            }
        }

        public void SetTime(DateTime tSet, TimeType type = TimeType.TimeZoneTime)
        {
            SetTime(tSet, type, false);
        }

        private void SetTime(DateTime tSet, TimeType type, bool bInternal)
        {
            switch (type) {
                case TimeType.UtcTime:
                    UtcNow = tSet;
                    break;

                case TimeType.TimeZoneTime:
                    UtcNow = tSet - TimeSpan.FromHours(TimeZoneOffset);
                    break;

                case TimeType.MiddleSunTime:
                    UtcNow = tSet - UTCGeoLngDiff;
                    break;

                case TimeType.RealSunTime:

                    UtcNow = tSet; //zum Datum vorbelegen, damit SunMidRealDiff halbwegs passt
                    UtcNow = tSet - UTCGeoLngDiff - SunMidRealDiff; // damit sollt's passen
                    UtcNow = tSet - UTCGeoLngDiff - SunMidRealDiff; // für den Fall der Fälle

                    TimeSpan tsDiff = tSet - RealSunTime;
                    if (tsDiff.TotalMilliseconds != 0)
                        tsDiff.ToString();

                    break;

#if DEBUG
                case TimeType.RealSunTimeOld:

                    UtcNow = tSet; //zum Datum vorbelegen, damit SunMidRealDiff halbwegs passt
                    UtcNow = tSet - UTCGeoLngDiff - OldSunMidRealDiff; // damit sollt's passen
                    UtcNow = tSet - UTCGeoLngDiff - OldSunMidRealDiff; // für den Fall der Fälle

                    tsDiff = tSet - RealSunTime;
                    if (tsDiff.TotalMilliseconds != 0)
                        tsDiff.ToString();

                    break;
#endif
            }
            if (!bInternal)
                try { TimeChanged?.Invoke(); } catch { }
        }

        public DateTime GetLocationTime(double locLatitude, double locLongitude, TimeType locType, DateTime? baseTime = null, TimeType baseType = TimeType.UtcTime)
        {
            DateTime oldNow = _UtcNow;
            Double oldLatitude = Latitude;
            Double oldLongitude = Longitude;
            Latitude = locLatitude;
            Longitude = locLongitude;
            if (baseTime != null)
                SetTime((DateTime)baseTime, baseType, true);
            try
            {
                //if (locType == TimeType.TimeZoneTime)
                  //  GetLocationInfo(true);
                return GetTime(locType);
            }
            finally
            {
                if (baseTime != null)
                    UtcNow = oldNow;
                Latitude = oldLatitude;
                Longitude = oldLongitude;
            }
            return DateTime.MinValue;
        }
    }

    public enum TimeType
    {
        RealSunTime = 11,
#if DEBUG
        RealSunTimeOld = 17,
#endif
        MiddleSunTime = 12,
        TimeZoneTime = 21,
        UtcTime = 22,
    }
}
