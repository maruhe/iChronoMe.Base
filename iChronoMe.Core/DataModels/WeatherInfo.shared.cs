using System;
using System.Collections.Generic;
using System.Text;
using iChronoMe.Core.Classes;
using SQLite;

namespace iChronoMe.Core.DataModels
{
    public class WeatherInfo
    {
        public static WeatherInfo GetWeatherInfo(DateTime gmtNow, double Latitude, double Longitude)
        {
            DateTime tStart = DateTime.Now;

            try
            {
                var cache = db.dbAreaCache.Query<WeatherInfo>("select * from WeatherInfo where boxWest <= ? and boxNorth >= ? and boxEast >= ? and boxSouth <= ? and ObservationTime > ?", Longitude, Latitude, Longitude, Latitude, gmtNow);
                cache.ToString();
                if (cache.Count > 0)
                {
                    //xLog.Debug("AreaCacheCheck Volltreffer");
                    if (cache.Count > 1)
                        cache.ToString();
                    var ret = cache[0];
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
                xLog.Debug("GetWeatherInfo took " + tsCacheCheck.TotalMilliseconds.ToString() + "ms");
            }

            tStart = DateTime.Now;

            return null;
        }


        [PrimaryKey, AutoIncrement]
        [Indexed] public long RecNo { get; set; }
        [Indexed] public double boxWest { get; set; }
        [Indexed] public double boxNorth { get; set; }
        [Indexed] public double boxEast { get; set; }
        [Indexed] public double boxSouth { get; set; }

        [Indexed] public DateTime ObservationTime { get; set; }
        public double Temp { get; set; }
        public double FeelsLike { get; set; }
        public double Humidity { get; set; }
        public double WindSpeed { get; set; }
        public int WindDirection { get; set; }
        public double BaroPressure { get; set; }
        public double Precipitation { get; set; }
        public string PrecipitationType { get; set; }
        public DateTime Sunrise { get; set; }
        public DateTime Sunset { get; set; }
        public double Visibility { get; set; }
        public double CloudCover { get; set; }
        public string WeatherCode { get; set; }
        public int FireIndex { get; set; }

        public int GetWeatherIcon()
        {
            try
            {
                bool day = ObservationTime > Sunrise && ObservationTime < Sunset;
                switch (WeatherCode)
                {
                    case "freezing_rain_heavy":
                    case "freezing_rain":
                    case "freezing_rain_light":
                    case "freezing_drizzle":
                        return 31;
                    case "ice_pellets_heavy":
                        return 9;
                    case "ice_pellets":
                    case "ice_pellets_light":
                        return 8;
                    case "snow_heavy":
                    case "snow":
                    case "snow_light":
                        return 14;
                    case "flurries":
                        return 40;
                    case "tstorm":
                        return 41;
                    case "rain_heavy":
                    case "rain":
                        return 9;
                    case "rain_light":
                    case "drizzle":
                        return 8;
                    case "fog_light":
                    case "fog":
                    case "cloudy":
                        return day ? 6 : 16;
                    case "mostly_cloudy":
                        return 4;
                    case "partly_cloudy":
                        return day ? 6 : 16;
                    case "mostly_clear":
                        return day ? 6 : 7;
                    case "clear":
                        return day ? 2 : 1;
                    default:
                        return 42;
                }
            }
            catch
            {
                return 42;
            }
        }

        private static WeatherInfo _dummy;
        public static WeatherInfo Dummy
        {
            get {
                if (_dummy == null)
                    _dummy = new WeatherInfo
                    {
                        ObservationTime = DateTime.Now.ToUniversalTime(),
                        WeatherCode = "partly_cloudy",
                        Temp = 18
                    };
                else
                    _dummy.ObservationTime = DateTime.Now.ToUniversalTime();
                return _dummy;
            }
        }
    }
}