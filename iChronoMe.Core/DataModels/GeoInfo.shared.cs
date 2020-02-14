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
                        if ("Unnamed Road".Equals(cAreaTitle))
                        {
                            if (!string.IsNullOrEmpty(ai.adminArea2))
                                cAreaTitle = ai.adminArea2;
                            if (!string.IsNullOrEmpty(ai.adminArea1))
                                cAreaTitle += ", " + ai.adminArea1;
                        }

                        ai.toponymName = cAreaTitle;
                        xLog.Debug("Final title: " + cAreaTitle);
                    }
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
                    if (Location.CalculateDistance(ai.boxSouth, ai.boxWest, ai.boxNorth, ai.boxEast, DistanceUnits.Kilometers) < 25)
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

                return ai;
            }
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

            }
        }
    }
}
