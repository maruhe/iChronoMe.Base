using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using iChronoMe.Core.Classes;

using Xamarin.Essentials;

using static iChronoMe.Core.AreaChangedEventArgs;
using static iChronoMe.Core.TimeChangedEventArgs;

namespace iChronoMe.Core
{
    public class LocationTimeHolder : IDisposable
    {
        bool bIsRunning = true;

        private string _id { get; } = Guid.NewGuid().ToString();

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
        public string CountryName { get; private set; } = String.Empty;
        public string AreaName { get; private set; } = string.Empty;
        public double TimeZoneOffset
        {
            get
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

        public delegate void ChangedEvent();
        public event EventHandler<AreaChangedEventArgs> AreaChanged;

        public static LocationTimeHolder NewInstance()
        {
            var lth = new LocationTimeHolder(FetchAreaInfoFlag.FullAreaInfo);

            return lth;
        }
        public static LocationTimeHolder NewInstanceDelay(double nLatitude, double nLongitude)
        {
            var lth = new LocationTimeHolder(FetchAreaInfoFlag.FullAreaInfo);
            lth.ChangePositionDelay(nLatitude, nLongitude);
            return lth;
        }
        public static LocationTimeHolder NewInstanceOffline(double nLatitude, double nLongitude, string cTimeZoneID, bool bDemeterAreaInfo = false)
        {
            var lth = new LocationTimeHolder(bDemeterAreaInfo ? FetchAreaInfoFlag.FullAreaInfo : FetchAreaInfoFlag.FullOffline);
            lth.ChangePositionDelay(nLatitude, nLongitude, cTimeZoneID, bDemeterAreaInfo);
            return lth;
        }

        public static async Task<LocationTimeHolder> NewInstanceAsync(double nLatitude, double nLongitude, bool bDisableAreaInfo = false)
        {
            var lth = new LocationTimeHolder(bDisableAreaInfo ? FetchAreaInfoFlag.EssentialOnly : FetchAreaInfoFlag.FullAreaInfo);
            await lth.ChangePositionAsync(nLatitude, nLongitude);
            return lth;
        }
        public static LocationTimeHolder NewInstanceDelay(double nLatitude, double nLongitude, bool bDisableAreaInfo = false)
        {
            var lth = new LocationTimeHolder(bDisableAreaInfo ? FetchAreaInfoFlag.EssentialOnly : FetchAreaInfoFlag.FullAreaInfo);
            lth.ChangePositionAsync(nLatitude, nLongitude).Start();
            return lth;
        }

        public static LocationTimeHolder NewInstanceOffline(double nLatitude, double nLongitude, string cTimeZoneID, string AreaName, string CountryName, FetchAreaInfoFlag faiType = FetchAreaInfoFlag.FullOffline)
        {
            var lth = new LocationTimeHolder(faiType);
            lth.ChangePositionParameters(nLatitude, nLongitude, false, false, cTimeZoneID, AreaName, CountryName);
            return lth;
        }

        private static LocationTimeHolder TryRestoreLocalInstance()
        {
            var lth = new LocationTimeHolder(LocalFetchAreaInfoFlag);
            var cfg = AppConfigHolder.LocationConfig;
            lth.ChangePositionParameters(cfg.Latitude, cfg.Longitude, false, false, cfg.TimeZoneID, cfg.AreaName, cfg.CountryName, cfg.TimeZoneOffsetGmt, cfg.TimeZoneOffsetDst);
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
                var res = LocationTimeHolder.NewInstanceOffline(LocalInstance.Latitude, LocalInstance.Longitude, LocalInstance.TimeZoneName, false);
                res.AreaInfoFlag = FetchAreaInfoFlag.FullAreaInfo;
                return res;
            }
        }

        protected FetchAreaInfoFlag AreaInfoFlag = FetchAreaInfoFlag.FullAreaInfo;
        public static FetchAreaInfoFlag LocalFetchAreaInfoFlag { get; set; } = FetchAreaInfoFlag.FullAreaInfo;

        protected LocationTimeHolder(FetchAreaInfoFlag faiType)
        {
            AreaInfoFlag = faiType;
        }

        static object oTZlock = new object();

        private bool ChangePositionxx(double nLatitude, double nLongitude, string cTimeZoneID, string cAreaName, string cCountryName, double? nTimeZoneOffsetGmt = null, double? nTimeZoneOffsetDst = null)
        {
            if (string.IsNullOrEmpty(cTimeZoneID))
                return false;
            var timezone = TimeZoneInfo.FindSystemTimeZoneById(cTimeZoneID);
            if (timezone == null)
                return false;

            Latitude = nLatitude;
            Longitude = nLongitude;

            AreaName = cAreaName;
            CountryName = cCountryName;
            TimeZoneName = timezone.Id;
            TimeZoneOffsetGmt = nTimeZoneOffsetGmt.HasValue ? nTimeZoneOffsetGmt.Value : timezone.BaseUtcOffset.TotalHours;
            TimeZoneOffsetDst = nTimeZoneOffsetDst.HasValue ? nTimeZoneOffsetDst.Value : timezone.BaseUtcOffset.TotalHours + (timezone.SupportsDaylightSavingTime ? 1 : 0);
            timeZoneInfo = timezone;

            try { AreaChanged?.Invoke(this, new AreaChangedEventArgs(AreaChangedFlag.LocationUpdate)); } catch { }

            if (AreaInfoFlag != FetchAreaInfoFlag.FullOffline)
                StartRefreshLocationInfo();
            return true;
        }

        private bool ChangePositionParameters(double nLatitude, double nLongitude, bool bClearAreaInfo = false, bool bForceRefreshAreaInfo = false, string cTimeZoneID = null,
                                              string cAreaName = null, string cCountryName = null, double? nTimeZoneOffsetGmt = null, double? nTimeZoneOffsetDst = null)
        {
            if (!string.IsNullOrEmpty(AreaName) && !bClearAreaInfo && !bForceRefreshAreaInfo)
            {
                if ((Latitude == nLatitude) && (Longitude == nLongitude))
                    return false;

                /* Nicht gemergte Änderung aus Projekt "iChronoMe.Core (MonoAndroid90)"
                Vor:
                            }

                            double nDistance = Location.CalculateDistance(Latitude, Longitude, nLatitude, nLongitude, DistanceUnits.Kilometers);
                Nach:
                            }

                            double nDistance = Location.CalculateDistance(Latitude, Longitude, nLatitude, nLongitude, DistanceUnits.Kilometers);
                */

                /* Nicht gemergte Änderung aus Projekt "iChronoMe.Core (uap10.0.16299)"
                Vor:
                            }

                            double nDistance = Location.CalculateDistance(Latitude, Longitude, nLatitude, nLongitude, DistanceUnits.Kilometers);
                Nach:
                            }

                            double nDistance = Location.CalculateDistance(Latitude, Longitude, nLatitude, nLongitude, DistanceUnits.Kilometers);
                */

                /* Nicht gemergte Änderung aus Projekt "iChronoMe.Core (Xamarin.iOS10)"
                Vor:
                            }

                            double nDistance = Location.CalculateDistance(Latitude, Longitude, nLatitude, nLongitude, DistanceUnits.Kilometers);
                Nach:
                            }

                            double nDistance = Location.CalculateDistance(Latitude, Longitude, nLatitude, nLongitude, DistanceUnits.Kilometers);
                */
            }

            double nDistance = Location.CalculateDistance(Latitude, Longitude, nLatitude, nLongitude, DistanceUnits.Kilometers);
            if (string.IsNullOrEmpty(cAreaName) && string.IsNullOrEmpty(cCountryName) && nDistance > 2.5)
                bClearAreaInfo = true;

            TimeZoneInfoJson tzj = null;
            if (string.IsNullOrEmpty(cTimeZoneID) || nTimeZoneOffsetGmt == null || nTimeZoneOffsetDst == null)
                tzj = FindTimeZoneByLocation(nLatitude, nLongitude);

            if (string.IsNullOrEmpty(cTimeZoneID) && tzj != null && !string.IsNullOrEmpty(tzj.timezoneId))
                cTimeZoneID = tzj.timezoneId;

            if (string.IsNullOrEmpty(cTimeZoneID))
            {
                timeZoneInfo = null; //to make clear, timezone is unclear
                if ((Latitude == 0 && Longitude == 0) || nDistance > 100)
                    TimeZoneOffsetGmt = TimeZoneOffsetDst = (long)Math.Floor((nLongitude - 7.500000001) / 15) + 1; //mal so ein ungefär..
            }
            else
            {
                try
                {
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(cTimeZoneID);
                    TimeZoneName = tz.Id;
                    TimeZoneOffsetGmt = nTimeZoneOffsetGmt.HasValue ? nTimeZoneOffsetGmt.Value : (tzj == null ? tz.BaseUtcOffset.TotalHours : tzj.gmtOffset);
                    TimeZoneOffsetDst = nTimeZoneOffsetDst.HasValue ? nTimeZoneOffsetDst.Value : (tzj == null ? tz.BaseUtcOffset.TotalHours + (tz.SupportsDaylightSavingTime ? 1 : 0) : tzj.dstOffset);
                    timeZoneInfo = tz;
                }
                catch (Exception ex)
                {
                    xLog.Error(ex);
                    return false;
                }
            }

            Latitude = nLatitude;
            Longitude = nLongitude;

            if (Latitude != 0 || Longitude != 0)
            {
                if (bClearAreaInfo)
                {
                    AreaName = string.Empty;
                    if (bForceRefreshAreaInfo)
                        CountryName = string.Empty;
                    else if (nDistance > 25)
                        CountryName += "?";
                }
            }

            if (!string.IsNullOrEmpty(cAreaName))
                AreaName = cAreaName;
            if (!string.IsNullOrEmpty(cCountryName))
                CountryName = cCountryName;

            try { AreaChanged?.Invoke(this, new AreaChangedEventArgs(AreaChangedFlag.LocationUpdate)); } catch { }

            return true;
        }

        private TimeZoneInfoJson FindTimeZoneByLocation(double nLatitude, double nLongitude)
        {
            if (TimeZoneMap.MapIsReady)
            {
                var xxTZ = TimeZoneMap.GetTimeZone((float)nLatitude, (float)nLongitude);
                if (xxTZ != null)
                {
                    return xxTZ;
                }
            }
            var cache = TimeZoneInfoCache.FromLocation(nLatitude, nLongitude, true);
            if (cache != null)
                return new TimeZoneInfoJson { timezoneId = cache.timezoneId, gmtOffset = cache.gmtOffset, dstOffset = cache.dstOffset };
            return null;
        }

        public async Task<bool> ChangePositionAsync(double nLatitude, double nLongitude, string cTimeZoneID, bool bDemeterAreaInfo = false)
        {
            if (!ChangePositionParameters(nLatitude, nLongitude, false, false, cTimeZoneID))
                return false;

            if (bDemeterAreaInfo || timeZoneInfo == null)
            {
                await Task.Factory.StartNew(() =>
                {
                    GetLocationInfo();
                });
                return true;
            }
            return true;
        }

        public async Task<bool> ChangePositionAsync(double nLatitude, double nLongitude, bool bClearAreaInfo = false, bool bForceRefreshAreaInfo = false)
        {
            if (!ChangePositionParameters(nLatitude, nLongitude, bClearAreaInfo))
                return false;

            if ((ai == null || timeZoneInfo == null || !ai.CheckBoxIsUpToDate(nLatitude, nLongitude) || bClearAreaInfo || bForceRefreshAreaInfo))
            {
                return await Task.Factory.StartNew<bool>(() =>
                {
                    return GetLocationInfo();
                });
            }
            return false;
        }

        public void ChangePositionDelay(double nLatitude, double nLongitude, bool bClearAreaInfo = false, bool bForceRefreshAreaInfo = false)
        {
            if (!ChangePositionParameters(nLatitude, nLongitude, bClearAreaInfo, bForceRefreshAreaInfo) && !bClearAreaInfo && !bForceRefreshAreaInfo)
                return;

            if ((ai == null || timeZoneInfo == null || !ai.CheckBoxIsUpToDate(nLatitude, nLongitude) || bClearAreaInfo || bForceRefreshAreaInfo))
                StartRefreshLocationInfo();
        }

        public bool ChangePositionDelay(double nLatitude, double nLongitude, string cTimeZoneID, bool bDemeterAreaInfo = false)
        {
            if (!ChangePositionParameters(nLatitude, nLongitude, false, false, cTimeZoneID))
                return false;

            if (bDemeterAreaInfo || timeZoneInfo == null)
                StartRefreshLocationInfo();
            return true;
        }

        Task locationTask = null;
        bool bStartLocationTaskAgain = false;
        DateTime tLastLocationTaskStart = DateTime.MinValue;
        private void StartRefreshLocationInfo()
        {
            xLog.Debug("StartRefreshLocationInfo: " + (locationTask != null).ToString());
            bStartLocationTaskAgain = true;
            if (locationTask != null && tLastLocationTaskStart.AddSeconds(30) < DateTime.Now)
                return;

            tLastLocationTaskStart = DateTime.Now;
            locationTask = Task.Factory.StartNew(() =>
            {
                try
                {
                    tLastLocationTaskStart = DateTime.Now;
                    bStartLocationTaskAgain = false;
                    GetLocationInfo();
                }
                catch (ThreadAbortException)
                { locationTask = null; return; }
                catch { }
                finally
                {
                    locationTask = null;
                }
                if (bStartLocationTaskAgain)
                    StartRefreshLocationInfo();
            });
        }

        GeoInfo.AreaInfo ai = null;
        private bool GetLocationInfo(bool bForceDemeterAreaInfo = false)
        {
            if (AreaInfoFlag == FetchAreaInfoFlag.FullOffline && !bForceDemeterAreaInfo)
                return false;
            if (Latitude == 0 && Longitude == 0)
                return false;
            if (this == _localInstance)
            {
                sys.lastUserLocation.Latitude = Latitude;
                sys.lastUserLocation.Longitude = Longitude;
            }
            double nLatitude = Latitude;
            double nLongitude = Longitude;

            if (AreaInfoFlag == FetchAreaInfoFlag.FullAreaInfo || bForceDemeterAreaInfo)
            {
                ai = GeoInfo.GetAreaInfo(Latitude, Longitude);
                if (nLatitude != Latitude || nLongitude != Longitude)
                    return false; //wenn's z'lang dauert..
                if (ai == null)
                {
                    AreaName = string.Empty;
                    CountryName = string.Empty;
                }
                else
                {
                    AreaName = ai.toponymName;
                    CountryName = ai.countryName;
                }
            }
            //if (this.timeZoneInfo == null)
            {
                var tz = TimeZoneInfoCache.FromLocation(Latitude, Longitude);
                if (tz != null)
                {
                    TimeZoneName = tz.timezoneId;
                    TimeZoneOffsetGmt = tz.gmtOffset;
                    TimeZoneOffsetDst = tz.dstOffset;
                    timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(tz.timezoneId);
                }
            }

            try { AreaChanged?.Invoke(this, new AreaChangedEventArgs(AreaChangedFlag.LocationUpdate)); } catch { }

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
            return true;
        }

        private SortedDictionary<string, Thread> timeHandlers = new SortedDictionary<string, Thread>();

        public int StopTimeChangedHandler(object senderTag)
        {
            if (senderTag == null)
                return 0;
            string id = senderTag.GetType().Name + "_" + senderTag.GetHashCode().ToString() + "_";
            List<string> idS = new List<string>(timeHandlers.Keys);

            int iRes = 0;
            foreach (string c in idS)
            {
                if (c.StartsWith(id))
                    iRes += StopTimeChangedHandlerById(c);
            }

            if (iRes < 1)
                iRes.ToString();
            return iRes;
        }

        public int StopTimeChangedHandler(object senderTag, TimeType timeType)
        {
            if (senderTag == null)
                return 0;
            string id = senderTag.GetType().Name + "_" + senderTag.GetHashCode().ToString() + "_" + timeType.ToString();
            if (!timeHandlers.ContainsKey(id))
                return 0;
            return StopTimeChangedHandlerById(id);
        }

        private int StopTimeChangedHandlerById(string id)
        {
            try
            {
                lock (timeHandlers)
                {
                    var tr = timeHandlers[id];
                    try { timeHandlers.Remove(id); } catch { }
                    tr.Abort();
                }
                return 1;
            }
            catch (Exception ex)
            {
                xLog.Error(ex);
                return 0;
            }
        }

        public bool StartTimeChangedHandler(object senderTag, TimeType timeType, EventHandler<TimeChangedEventArgs> timeChanged)
        {
            if (senderTag == null || timeChanged == null)
                return false;
            string id = senderTag.GetType().Name + "_" + senderTag.GetHashCode().ToString() + "_" + timeType.ToString();

            lock (timeHandlers)
            {
                try
                {
                    if (timeHandlers.ContainsKey(id))
                    {
                        StopTimeChangedHandlerById(id);
                        if (timeHandlers.ContainsKey(id))
                        {
                            this.ToString();
                        }
                    }

                    var thread = new Thread(() =>
                    {
                        try
                        {
                            timeChanged?.Invoke(this, new TimeChangedEventArgs(TimeChangedFlag.Initial));
                            while (timeHandlers.ContainsKey(id))
                            {
                                Thread.Sleep(1000 - this.GetTime(timeType).Millisecond);
                                if (bIsRunning)
                                    timeChanged?.Invoke(this, new TimeChangedEventArgs(TimeChangedFlag.SecondChanged));
                            }
                        }
                        catch (ThreadAbortException)
                        {
                            //all fine, handler has been removed
                        }
                        catch (Exception ex)
                        {
                            xLog.Error(ex, "StartTimeChangedHandler: " + id);
                        }
                        finally
                        {
                            lock (timeHandlers)
                            {
                                if (timeHandlers.ContainsKey(id))
                                    timeHandlers.Remove(id);
                            }
                        }
                    });
                    timeHandlers.Add(id, thread);
                    thread.Start();
                }
                catch (Exception ex)
                {
                    xLog.Error(ex, "StartTimeChangedHandler: " + id);
                    return false;
                }
            }
            return true;
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

        public DateTime GetTime(TimeType locType, DateTime baseTime, TimeType baseType)
        {
            DateTime oldNow = _UtcNow;
            SetTime((DateTime)baseTime, baseType);
            try
            {
                return GetTime(locType);
            }
            finally
            {
                UtcNow = oldNow;
            }
        }

        public void SetTime(DateTime tSet, TimeType type = TimeType.TimeZoneTime)
        {
            switch (type)
            {
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
        }

        public DateTime GetLocationTime(double locLatitude, double locLongitude, TimeType locType, DateTime? baseTime = null, TimeType baseType = TimeType.UtcTime)
        {
            DateTime oldNow = _UtcNow;
            Double oldLatitude = Latitude;
            Double oldLongitude = Longitude;
            Latitude = locLatitude;
            Longitude = locLongitude;
            if (baseTime != null)
                SetTime((DateTime)baseTime, baseType);
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

        public override int GetHashCode()
        {
            unchecked
            {
                // Choose large primes to avoid hashing collisions
                int HashingBase = GetType().GetHashCode();
                const int HashingMultiplier = 16777619;
                int hash = HashingBase;
                hash = (hash * HashingMultiplier) ^ _id.GetHashCode();
                return hash;
            }
        }

        public void Dispose()
        {
            bIsRunning = false;
            //end all TimeChanged-Threads within the next second
            timeHandlers.Clear();
        }

        public enum FetchAreaInfoFlag
        {
            FullAreaInfo = 0,
            EssentialOnly = 10,
            FullOffline = -1
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

    public class TimeChangedEventArgs : EventArgs
    {
        public TimeChangedFlag Flag { get; }
        public TimeChangedEventArgs(TimeChangedFlag flag)
        {
            Flag = flag;
        }

        public enum TimeChangedFlag
        {
            SecondChanged,
            LocationUpdate,
            TimeSourceUpdate,
            Initial,
            Unspecific
        }
    }

    public class AreaChangedEventArgs : EventArgs
    {
        public AreaChangedFlag Flag { get; }
        public AreaChangedEventArgs(AreaChangedFlag flag)
        {
            Flag = flag;
        }

        public enum AreaChangedFlag
        {
            LocationUpdate,
            Initial,
            Unspecific
        }
    }
}
