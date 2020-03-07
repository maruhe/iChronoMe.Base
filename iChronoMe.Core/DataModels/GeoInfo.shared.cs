using System;
/* Nicht gemergte Änderung aus Projekt "iChronoMe.Core (uap10.0.16299)"
Vor:
using System.Xml;
using GeoJSON.Net.Geometry;
using iChronoMe.Core.Tools;
using Newtonsoft.Json;
using SQLite;
Nach:
using System.Xml;

using GeoJSON.Net.Geometry;

using iChronoMe.Core.Tools;

using Newtonsoft.Json;

using SQLite;
*/

/* Nicht gemergte Änderung aus Projekt "iChronoMe.Core (Xamarin.iOS10)"
Vor:
using System.Xml;
using GeoJSON.Net.Geometry;
using iChronoMe.Core.Tools;
using Newtonsoft.Json;
using SQLite;
Nach:
using System.Xml;

using GeoJSON.Net.Geometry;

using iChronoMe.Core.Tools;

using Newtonsoft.Json;

using SQLite;
*/

/* Nicht gemergte Änderung aus Projekt "iChronoMe.Core (MonoAndroid90)"
Vor:
using System.Xml;
using GeoJSON.Net.Geometry;
using iChronoMe.Core.Tools;
using Newtonsoft.Json;
using SQLite;
Nach:
using System.Xml;

using GeoJSON.Net.Geometry;

using iChronoMe.Core.Tools;

using Newtonsoft.Json;

using SQLite;
*/


using iChronoMe.Core.Tools;

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

            try
            {

                var ai = GeoCoder.GetAreaInfo(Latitude, Longitude);

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

            public double pointLat { get; set; }
            public double pointLng { get; set; }

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

            public DateTime BoxTimeStamp = DateTime.MinValue;

            [Ignore]
            public int BoxWidth { get => (int)(Location.CalculateDistance(boxNorth, boxWest, boxNorth, boxEast, DistanceUnits.Kilometers) * 1000); set { } }
            [Ignore]
            public int BoxHeitgh { get => (int)(Location.CalculateDistance(boxNorth, boxWest, boxSouth, boxWest, DistanceUnits.Kilometers) * 1000); set { } }

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
