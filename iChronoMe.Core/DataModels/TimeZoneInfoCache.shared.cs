using System;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml;

using Newtonsoft.Json.Linq;

using SQLite;

using Xamarin.Essentials;

namespace iChronoMe.Core.Classes
{
    public class TimeZoneInfoCache : dbObject
    {
        public static TimeZoneInfoCache FromLocation(double Latitude, double Longitude, bool bCacheOnly = false)
        {
            //check TimeZoneInfoCache
            var cache = db.dbAreaCache.Query<TimeZoneInfoCache>("select * from TimeZoneInfoCache where boxWest <= ? and boxNorth >= ? and boxEast >= ? and boxSouth <= ?", Longitude, Latitude, Longitude, Latitude);
            cache.ToString();
            if (cache.Count > 0)
            {
                xLog.Debug("TimeZoneInfoCache Volltreffer");
                if (cache.Count > 1)
                    cache.ToString();
                return cache[0];
            }
            if (bCacheOnly)
                return null;
            TimeZoneInfoCache tziNew = OnlineFromLocation(Latitude, Longitude);
            if (!string.IsNullOrEmpty(tziNew.timezoneId))
            {
                Task.Factory.StartNew(() =>
                {
                    int radius = 30000;

                    PointF center = new PointF((float)Latitude, (float)Longitude);
                    double mult = 1.1; // mult = 1.1; is more reliable

                        PointF pn, pe, ps, pw;
                    pn = pe = ps = pw = new PointF();
                    while (radius > 100)
                    {
                        pn = mySQLiteConnection.calculateDerivedPosition(center, mult * radius, 0);
                        pe = mySQLiteConnection.calculateDerivedPosition(center, mult * radius, 90);
                        ps = mySQLiteConnection.calculateDerivedPosition(center, mult * radius, 180);
                        pw = mySQLiteConnection.calculateDerivedPosition(center, mult * radius, 270);

                        if (CheckTimeZoneEnd(tziNew, pn, pe, ps, pw))
                        {
                            break;
                        }

                        radius = radius / 3;
                    }
                    tziNew.boxNorth = pn.X;
                    tziNew.boxEast = pe.Y;
                    tziNew.boxSouth = ps.X;
                    tziNew.boxWest = pw.Y;
                    db.dbAreaCache.Insert(tziNew);
                });
            }
            return tziNew;
        }

        private static bool CheckTimeZoneEnd(TimeZoneInfoCache info, PointF pn, PointF pe, PointF ps, PointF pw)
        {
            TimeZoneInfoCache check = OnlineFromLocation(pe.X, pe.Y);
            if (check == null) return true;
            if (info.timezoneId != check.timezoneId)
                return false;
            check = OnlineFromLocation(pw.X, pw.Y);
            if (check == null) return true;
            if (info.timezoneId != check.timezoneId)
                return false;
            check = OnlineFromLocation(pn.X, pn.Y);
            if (check == null) return true;
            if (info.timezoneId != check.timezoneId)
                return false;
            check = OnlineFromLocation(ps.X, ps.Y);
            if (check == null) return true;
            if (info.timezoneId != check.timezoneId)
                return false;
            return true;
        }

        public static TimeZoneInfoCache OnlineFromLocation(double Latitude, double Longitude)
        {
            //get TimezoneInfo online
            string cUri = "https://secure.geonames.org/timezoneJSON?style=full&lat=" + Latitude.ToString("0.######", CultureInfo.InvariantCulture) + "&lng=" + Longitude.ToString("0.######", CultureInfo.InvariantCulture) + "&username=" + Secrets.GeoNamesOrg_User;
            string cGeoInfo = sys.GetUrlContent(cUri).Result;

            try
            {
                JObject ts = JObject.Parse(cGeoInfo);
                if (ts["status"] != null)
                    return null;

                TimeZoneInfoCache ti = new TimeZoneInfoCache();
                ti.timezoneId = (string)ts["timezoneId"];
                ti.gmtOffset = (double)ts["gmtOffset"];
                ti.dstOffset = (double)ts["dstOffset"];

                ti.timezoneId = sys.ConvertTimeZoneToSystem(ti.timezoneId);
                return ti;
            }
            catch (Exception e) { e.ToString(); }
            return null;
        }

        public string timezoneId { get; set; }
        public double gmtOffset { get; set; }
        public double dstOffset { get; set; }

        [Ignore]
        public string Notes { get; set; }

        [Indexed]
        public double boxWest { get; set; }
        [Indexed]
        public double boxNorth { get; set; }
        [Indexed]
        public double boxEast { get; set; }
        [Indexed]
        public double boxSouth { get; set; }
        public int BoxWidth { get => (int)(Location.CalculateDistance(boxNorth, boxWest, boxNorth, boxEast, DistanceUnits.Kilometers) * 1000); set { } }
        public int BoxHeitgh { get => (int)(Location.CalculateDistance(boxNorth, boxWest, boxSouth, boxWest, DistanceUnits.Kilometers) * 1000); set { } }

        public override string ToString()
        {
            return string.Concat(timezoneId, " GMT ", gmtOffset, " DST ", dstOffset);
        }
    }
}