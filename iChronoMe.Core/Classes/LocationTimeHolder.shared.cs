using iChronoMe.Core.Classes;
using iChronoMe.Core.Types;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Xamarin.Essentials;

namespace iChronoMe.Core
{
    public class LocationTimeHolder// : SelectPositionReceiver
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

        public double Latitude { get; private set; } = 0;
        public double Longitude { get; private set; } = 0;
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

        public double TimeZoneOffsetGmt { get; set; } = 0;
        public double TimeZoneOffsetDst { get; set; } = 0;
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
        public EventHandler<TimeChangedEventArgs> TimeChanged;
        public EventHandler<AreaChangedEventArgs> AreaChanged;

        public static LocationTimeHolder NewInstance()
        {
            var lth = new LocationTimeHolder(FetchAreaInfoType.FullAreaInfo);

            return lth;
        }
        public static LocationTimeHolder NewInstanceDelay(double nLatitude, double nLongitude)
        {
            var lth = new LocationTimeHolder(FetchAreaInfoType.FullAreaInfo);
            lth.ChangePositionDelay(nLatitude, nLongitude);
            return lth;
        }
        public static LocationTimeHolder NewInstanceOffline(double nLatitude, double nLongitude, string cTimeZoneID, bool bDemeterAreaInfo = false)
        {
            var lth = new LocationTimeHolder(bDemeterAreaInfo ? FetchAreaInfoType.FullAreaInfo : FetchAreaInfoType.FullOffline);
            lth.ChangePositionDelay(nLatitude, nLongitude, cTimeZoneID, bDemeterAreaInfo);
            return lth;
        }

        public static async Task<LocationTimeHolder> NewInstanceAsync(double nLatitude, double nLongitude, bool bDisableAreaInfo = false)
        {
            var lth = new LocationTimeHolder(bDisableAreaInfo ? FetchAreaInfoType.EssentialOnly : FetchAreaInfoType.FullAreaInfo);
            await lth.ChangePositionAsync(nLatitude, nLongitude);
            return lth;
        }
        public static LocationTimeHolder NewInstanceDelay(double nLatitude, double nLongitude, bool bDisableAreaInfo = false)
        {
            var lth = new LocationTimeHolder(bDisableAreaInfo ? FetchAreaInfoType.EssentialOnly : FetchAreaInfoType.FullAreaInfo);
            lth.ChangePositionAsync(nLatitude, nLongitude).Start();
            return lth;
        }

        public static LocationTimeHolder NewInstanceOffline(double nLatitude, double nLongitude, string cTimeZoneID, string AreaName, string CountryName, FetchAreaInfoType faiType = FetchAreaInfoType.FullOffline)
        {
            var lth = new LocationTimeHolder(faiType);
            lth.ChangePosition(nLatitude, nLongitude, cTimeZoneID, AreaName, CountryName);
            return lth;
        }

        private static LocationTimeHolder TryRestoreLocalInstance()
        {
            var lth = new LocationTimeHolder(LocalFetchAreaInfoType);
            var cfg = AppConfigHolder.LocationConfig;
            lth.ChangePosition(cfg.Latitude, cfg.Longitude, cfg.TimeZoneID, cfg.AreaName, cfg.CountryName, cfg.TimeZoneOffsetGmt, cfg.TimeZoneOffsetDst);
            if (sys.lastUserLocation.Latitude == 0 && sys.lastUserLocation.Longitude == 0)
            {
                sys.lastUserLocation.Latitude = cfg.Latitude;
                sys.lastUserLocation.Longitude = cfg.Longitude;
            }
            return lth;
        }

        private static LocationTimeHolder _localInstance;
        public static LocationTimeHolder LocalInstance
        {
            get
            {
                if (_localInstance == null)
                    _localInstance = TryRestoreLocalInstance();
                return _localInstance;
            }
        }

        public static LocationTimeHolder LocalInstanceClone
        {
            get
            {
                return (LocationTimeHolder)LocalInstance.MemberwiseClone();
            }
        }

        protected FetchAreaInfoType FetchAreaInfoType = FetchAreaInfoType.FullAreaInfo;
        public static FetchAreaInfoType LocalFetchAreaInfoType { get; set; } = FetchAreaInfoType.FullAreaInfo;

        protected LocationTimeHolder(FetchAreaInfoType faiType)
        {
            FetchAreaInfoType = faiType;
        }

        private bool ChangePositionParameters(double nLatitude, double nLongitude, bool bClearAreaInfo = false)
        {
            if ((Latitude == nLatitude) && (Longitude == nLongitude))
                return false;

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
                }
            }

            Latitude = nLatitude;
            Longitude = nLongitude;

            return true;
        }

        public async Task ChangePositionAsync(double nLatitude, double nLongitude, bool bClearAreaInfo = false, bool bForceRefreshAreaInfo = false)
        {
            if (!ChangePositionParameters(nLatitude, nLongitude, bClearAreaInfo))
                return;

            if ((ai == null || !ai.CheckBoxIsUpToDate(nLatitude, nLongitude) || bClearAreaInfo || bForceRefreshAreaInfo))
            {
                await Task.Factory.StartNew(() =>
                {
                    GetLocationInfo();
                });
            }
        }

        public void ChangePositionDelay(double nLatitude, double nLongitude, bool bClearAreaInfo = false, bool bForceRefreshAreaInfo = false)
        {
            if (!ChangePositionParameters(nLatitude, nLongitude, bClearAreaInfo))
                return;

            if ((ai == null || !ai.CheckBoxIsUpToDate(nLatitude, nLongitude) || bClearAreaInfo || bForceRefreshAreaInfo))
            {
                Task.Factory.StartNew(() =>
                {
                    GetLocationInfo();
                });
            }
        }

        public bool ChangePosition(double nLatitude, double nLongitude, string cTimeZoneID, string cAreaName, string cCountryName, double? nTimeZoneOffsetGmt = null, double? nTimeZoneOffsetDst = null)
        {
            if (string.IsNullOrEmpty(cTimeZoneID))
                return false;
            var timezone = TimeZoneInfo.FindSystemTimeZoneById(cTimeZoneID);
            if (timezone == null)
                return false;

            Latitude = nLatitude;
            Longitude = nLongitude;

            AreaName = cAreaName;
            CountryName = cCountryName;// timezone.DisplayName + " & " + timezone.StandardName + " & " + timezone.DaylightName;
            TimeZoneName = timezone.Id;
            TimeZoneOffsetGmt = nTimeZoneOffsetGmt.HasValue ? nTimeZoneOffsetGmt.Value : timezone.BaseUtcOffset.TotalHours;
            TimeZoneOffsetDst = nTimeZoneOffsetDst.HasValue ? nTimeZoneOffsetDst.Value : timezone.BaseUtcOffset.TotalHours + (timezone.SupportsDaylightSavingTime ? 1 : 0);
            timeZoneInfo = timezone;

            try { AreaChanged?.Invoke(this, new AreaChangedEventArgs(AreaChangedFlag.LocationUpdate)); } catch { }
            try { TimeChanged?.Invoke(this, new TimeChangedEventArgs(TimeChangedFlag.LocationUpdate)); } catch { }

            if (FetchAreaInfoType != FetchAreaInfoType.FullOffline)
            {
                Task.Factory.StartNew(() =>
                {
                    GetLocationInfo();
                });
            }
            return true;
        }

        private bool ChangePositionParameters(double nLatitude, double nLongitude, string cTimeZoneID)
        {
            if (string.IsNullOrEmpty(cTimeZoneID))
                return false;
            var timezone = TimeZoneInfo.FindSystemTimeZoneById(cTimeZoneID);
            if (timezone == null)
                return false;

            Latitude = nLatitude;
            Longitude = nLongitude;

            CountryName = timezone.DisplayName + " & " + timezone.StandardName + " & " + timezone.DaylightName;
            TimeZoneName = timezone.Id;
            TimeZoneOffsetGmt = timezone.BaseUtcOffset.TotalHours;
            TimeZoneOffsetDst = timezone.BaseUtcOffset.TotalHours + (timezone.SupportsDaylightSavingTime ? 1 : 0);
            timeZoneInfo = timezone;

            try { AreaChanged?.Invoke(this, new AreaChangedEventArgs(AreaChangedFlag.LocationUpdate)); } catch { }
            try { TimeChanged?.Invoke(this, new TimeChangedEventArgs(TimeChangedFlag.LocationUpdate)); } catch { }

            return true;
        }

        public bool ChangePositionDelay(double nLatitude, double nLongitude, string cTimeZoneID, bool bDemeterAreaInfo = false)
        {
            if (!ChangePositionParameters(nLatitude, nLongitude, cTimeZoneID))
                return false;

            if (bDemeterAreaInfo)
            {
                Task.Factory.StartNew(() =>
                {
                    GetLocationInfo();
                });
            }
            return true;
        }

        public async Task<bool> ChangePositionAsync(double nLatitude, double nLongitude, string cTimeZoneID, bool bDemeterAreaInfo = false)
        {
            if (!ChangePositionParameters(nLatitude, nLongitude, cTimeZoneID))
                return false;

            if (bDemeterAreaInfo)
            {
                await Task.Factory.StartNew(() =>
                {
                    GetLocationInfo();
                });
                return true;
            }
            return true;
        }

        GeoInfo.AreaInfo ai = null;
        private void GetLocationInfo(bool bForceDemeterAreaInfo = false)
        {
            if (FetchAreaInfoType == FetchAreaInfoType.FullOffline && !bForceDemeterAreaInfo)
                return;
            if (this == _localInstance)
            {
                sys.lastUserLocation.Latitude = Latitude;
                sys.lastUserLocation.Longitude = Longitude;
            }
            double nLatitude = Latitude;
            double nLongitude = Longitude;

            if (FetchAreaInfoType == FetchAreaInfoType.FullAreaInfo || bForceDemeterAreaInfo)
            {
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
            }
            else if (FetchAreaInfoType == FetchAreaInfoType.EssentialOnly)
            {
                ai = new GeoInfo.AreaInfo();
                GeoInfo.FillTimeZoneID(ai, Latitude, Longitude);
            }
            else
                xLog.Wtf("LocationTimeHolder", "Should not happen: GetLocationInfo: " + FetchAreaInfoType.ToString());

            try { AreaChanged?.Invoke(this, new AreaChangedEventArgs(AreaChangedFlag.LocationUpdate)); } catch { }
            try { TimeChanged?.Invoke(this, new TimeChangedEventArgs(TimeChangedFlag.LocationUpdate)); } catch { }

            if (this == _localInstance)
            {
                var cfg = AppConfigHolder.LocationConfig;
                cfg.Latitude = Latitude;
                cfg.Longitude = Longitude;
                cfg.TimeZoneID = TimeZoneName;
                cfg.AreaName = AreaName;
                cfg.CountryName = CountryName;
                cfg.TimeZoneOffsetGmt = TimeZoneOffsetGmt;
                cfg.TimeZoneOffsetDst = TimeZoneOffsetDst;
                AppConfigHolder.SaveLocationConfig();
            }
        }

        #region Obsolete
        /*
        [Obsolete("This construrctor is obsoleted use static NewInstance or LocalInstance")]
        public LocationTimeHolder(double nLatitude, double nLongitude, bool bAsync = true)
        {
            FetchAreaInfoType = FetchAreaInfoType.FullAreaInfo;
            ChangePosition(nLatitude, nLongitude, true, bAsync);
        }

        [Obsolete("This construrctor is obsoleted use Async or Delay")]
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
            try { TimeChanged?.Invoke(this, new TimeChangedEventArgs(TimeChangedFlag.LocationUpdate)); } catch { }
        }

 *         public void ReceiveSelectedPosition(SelectPositionResult pos)
        {
            ChangePosition(pos.Latitude, pos.Longitude);
            AreaName = pos.Title;
        }

        */
        public void Start(bool observeRealSunTime, bool observeMiddleSunTime, bool observeWorldTime)
        {
            if (!bIsRunning || mSecondThread == null)
            {
                bIsRunning = true;

                try { AreaChanged?.Invoke(this, new AreaChangedEventArgs(AreaChangedFlag.Initial)); } catch { }
                try { TimeChanged?.Invoke(this, new TimeChangedEventArgs(TimeChangedFlag.Initial)); } catch { }

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
                            try { TimeChanged?.Invoke(this, new TimeChangedEventArgs(TimeChangedFlag.SecondChanged)); } catch { }

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


        #endregion

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
                try { TimeChanged?.Invoke(this, new TimeChangedEventArgs(TimeChangedFlag.TimeSourceUpdate)); } catch { }
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

    public enum FetchAreaInfoType
    {
        FullAreaInfo = 0,
        EssentialOnly = 10,
        FullOffline = -1
    }

    public class TimeChangedEventArgs : EventArgs
    {
        public TimeChangedFlag Flag { get; }
        public TimeChangedEventArgs(TimeChangedFlag flag)
        {
            Flag = flag;
        }
    }

    public class AreaChangedEventArgs : EventArgs
    {
        public AreaChangedFlag Flag { get; }
        public AreaChangedEventArgs(AreaChangedFlag flag)
        {
            Flag = flag;
        }
    }

    public enum TimeChangedFlag
    {
        SecondChanged,
        LocationUpdate,
        TimeSourceUpdate,
        Initial,
        Unspecific
    }

    public enum AreaChangedFlag
    {
        LocationUpdate,
        Initial,
        Unspecific
    }
}
