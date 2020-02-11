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
    public static class GeoInfo
    {
        public static AreaInfo GetAreaInfo(double Latitude, double Longitude, bool bCheckDbCache = true)
        {
            DateTime tStart = DateTime.Now;

            if (bCheckDbCache)
            {
                try
                {
                    var cache = db.dbAreaCache.Query<AreaInfo>("select * from AreaInfo where boxWest <= ? and boxNorth >= ? and boxEast >= ? and boxSouth <= ?", Longitude, Latitude, Longitude, Latitude);
                    cache.ToString();
                    if (cache.Count > 0)
                    {
                        //xLog.Debug("AreaCacheCheck Volltreffer");
                        if (cache.Count > 1)
                            cache.ToString();
                        var ret = cache[0];
                        ret.OnInstanceCreatedDB();
                        return ret;
                    }
                }
                catch (Exception e)
                {
                    xLog.Error(e);
                }
                finally
                {
                    TimeSpan tsCacheCheck = DateTime.Now - tStart;
                    xLog.Debug("AreaCacheCheck took " + tsCacheCheck.TotalMilliseconds.ToString() + "ms");
                }
            }

            tStart = DateTime.Now;

            string cUri = "https://maps.googleapis.com/maps/api/geocode/xml?key=" + Secrets.GApiKey + "&latlng=" + Latitude.ToString("0.######", CultureInfo.InvariantCulture) + "," + Longitude.ToString("0.######", CultureInfo.InvariantCulture) + "&sensor=true";
            string cGeoInfo = sys.GetUrlContent(cUri).Result;

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(cGeoInfo);
                XmlElement eStatus = (XmlElement)doc.DocumentElement.FirstChild;
                if (eStatus.InnerText != "OK")
                    return null;
                XmlElement eArea = (XmlElement)eStatus.NextSibling;

                eArea.ToString();

                AreaInfo ai = new AreaInfo();
                if ("result".Equals(eArea.Name))
                {
                    string cType = eArea.SelectSingleNode("type").InnerText;
                    ai.toponymName = eArea.SelectSingleNode("formatted_address").InnerText;

                    xLog.Debug("Found Address:" + ai.toponymName);
                    string cAreaTitle = "";

                    foreach (XmlElement elResultPart in eArea.ChildNodes)
                    {
                        if ("address_component".Equals(elResultPart.Name))
                        {
                            xLog.Debug(elResultPart.SelectSingleNode("type").InnerText + " => " + elResultPart.SelectSingleNode("long_name").InnerText + " => " + elResultPart.SelectSingleNode("short_name").InnerText);

                            string cPartType = elResultPart.SelectSingleNode("type").InnerText;
                            if (!("street_number".Equals(cPartType)))
                            {
                                string cPartNameLong = elResultPart.SelectSingleNode("long_name").InnerText;
                                string cPartNameShort = elResultPart.SelectSingleNode("short_name").InnerText;
                                if (string.IsNullOrEmpty(cAreaTitle))
                                    cAreaTitle = elResultPart.SelectSingleNode("long_name").InnerText;

                                switch (cPartType)
                                {
                                    case "route":
                                        ai.route = cPartNameLong;
                                        break;
                                    case "locality":
                                        ai.locality = cPartNameLong;
                                        if ("Unnamed Road".Equals(cAreaTitle))
                                            cAreaTitle += ", " + cPartNameLong;

                                        cAreaTitle = cPartNameLong;

                                        break;
                                    case "administrative_area_level_2":
                                        ai.adminArea2 = cPartNameLong;
                                        break;
                                    case "administrative_area_level_1":
                                        ai.adminArea1 = cPartNameLong;
                                        break;
                                    case "country":
                                        ai.countryName = cPartNameLong;
                                        ai.countryCode = cPartNameShort;
                                        break;
                                    case "postal_code":
                                        ai.postalCode = cPartNameLong;
                                        break;
                                }
                            }
                        }
                        if ("geometry".Equals(elResultPart.Name))
                        {
                            XmlNode elVp = elResultPart.SelectSingleNode("bounds");
                            if (elVp == null)
                                elVp = elResultPart.SelectSingleNode("viewport");
                            if (elVp != null)
                            {
                                XmlNode elVpSW = elVp.SelectSingleNode("southwest");
                                XmlNode elVpNE = elVp.SelectSingleNode("northeast");
                                ai.boxSouth = sys.parseDouble(elVpSW.SelectSingleNode("lat").InnerText);
                                ai.boxWest = sys.parseDouble(elVpSW.SelectSingleNode("lng").InnerText);
                                ai.boxNorth = sys.parseDouble(elVpNE.SelectSingleNode("lat").InnerText);
                                ai.boxEast = sys.parseDouble(elVpNE.SelectSingleNode("lng").InnerText);
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(cAreaTitle))
                    {
                        ai.toponymName = cAreaTitle;
                        xLog.Debug("Final title: " + cAreaTitle);
                    }
                }

                if (string.IsNullOrEmpty(ai.timezoneId))
                {
                    FillTimeZoneID(ai, Latitude, Longitude);
                }
                try
                {
                    var delS = db.dbAreaCache.Query<AreaInfo>("select * from AreaInfo where boxWest = ? and boxNorth = ? and boxEast = ? and boxSouth = ?", ai.boxWest, ai.boxNorth, ai.boxEast, ai.boxSouth);
                    if (delS.Count > 0)
                        db.dbAreaCache.Delete<AreaInfo>(delS);
                }
                catch (Exception e) { e.ToString(); }

                try
                {
                    db.dbAreaCache.Insert(ai);
                }
                catch (Exception e) { e.ToString(); }

                TimeSpan tsOnlineCheck = DateTime.Now - tStart;
                xLog.Debug("AreaCheckOnline took " + tsOnlineCheck.TotalMilliseconds.ToString() + "ms");

                if (ai.boxWest <= Longitude && ai.boxNorth >= Latitude && ai.boxEast >= Longitude && ai.boxSouth <= Latitude)
                    ai.ToString(); //in the box
                else
                    ai.ToString(); //outside the box

                return ai;
            }
            catch (Exception e)
            {
                e.ToString();

                AreaInfo ai = new AreaInfo();
                FillTimeZoneID(ai, Latitude, Longitude);
                return ai;
            }
        }
        public static bool FillTimeZoneID(AreaInfo ai, double Latitude, double Longitude)
        {
            if (ai == null)
                return false;

            try
            {
                TimeZoneInfoCache inf = TimeZoneInfoCache.FromLocation(Latitude, Longitude);

                ai.timezoneId = inf.timezoneId;
                ai.gmtOffset = inf.gmtOffset;
                ai.dstOffset = inf.dstOffset;

                ai.timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(ai.timezoneId);
                return ai.timeZoneInfo != null;
            }
            catch (Exception e) { e.ToString(); ai.timeZoneInfo = null; }

            return false;
        }

        public class AreaInfo : dbObject
        {
            public string countryCode { get; set; }
            public string countryName { get; set; }

            public string adminArea1 { get; set; }
            public string adminArea2 { get; set; }
            public string locality { get; set; }
            public string route { get; set; }
            public string postalCode { get; set; }

            public string toponymName { get; set; }
            [Ignore]
            public TimeZoneInfo timeZoneInfo { get; set; }
            public string timezoneId { get; set; }
            public double gmtOffset { get; set; }
            public double dstOffset { get; set; }

            [Indexed]
            public double boxWest { get; set; }
            [Indexed]
            public double boxNorth { get; set; }
            [Indexed]
            public double boxEast { get; set; }
            [Indexed]
            public double boxSouth { get; set; }

            public double centerLat { get => (boxNorth + boxSouth) / 2; set { } }
            public double centerLong { get => (boxEast + boxWest) / 2; set { } }

            public int BoxWidth { get => (int)(Location.CalculateDistance(boxNorth, boxWest, boxNorth, boxEast, DistanceUnits.Kilometers) * 1000); set { } }
            public int BoxHeitgh { get => (int)(Location.CalculateDistance(boxNorth, boxWest, boxSouth, boxWest, DistanceUnits.Kilometers) * 1000); set { } }

            public DateTime BoxTimeStamp = DateTime.MinValue;

            public bool CheckBoxIsUpToDate(double nLatitude, double nLongitude)
            {

                if (BoxTimeStamp.Equals(DateTime.MinValue))
                    return false;

                if (boxWest == 0 && boxNorth == 0 && boxEast == 0 && boxSouth == 0)
                    return false;

                if (nLatitude > boxNorth || nLatitude < boxSouth)
                    return false;

                if (nLongitude > boxEast || nLongitude < boxWest)
                    return false;

                return true;
            }

            public override void OnInstanceCreatedDB()
            {
                try
                {
                    if (!string.IsNullOrEmpty(timezoneId))
                        timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
                }
                catch
                {
                    timeZoneInfo = null;
                }
            }
        }

        public class TimeZoneInfoCache : dbObject
        {
            public static TimeZoneInfoCache FromLocation(double Latitude, double Longitude)
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
                if (info.timezoneId != check.timezoneId)
                    return false;
                check = OnlineFromLocation(pw.X, pw.Y);
                if (info.timezoneId != check.timezoneId)
                    return false;
                check = OnlineFromLocation(pn.X, pn.Y);
                if (info.timezoneId != check.timezoneId)
                    return false;
                check = OnlineFromLocation(ps.X, ps.Y);
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
        }
    }
}
