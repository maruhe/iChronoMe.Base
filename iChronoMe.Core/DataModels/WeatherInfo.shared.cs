using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using iChronoMe.Core.Classes;
using SQLite;

namespace iChronoMe.Core.DataModels
{
    [DebuggerDisplay("{xString}")]
    public class WeatherInfo
    {
        public static WeatherInfo GetWeatherInfo(DateTime gmtNow, double Latitude, double Longitude)
        {
            DateTime tStart = DateTime.Now;

            try
            {
                var cache = db.dbAreaCache.Query<WeatherInfo>("select * from WeatherInfo where ((boxWest <= ? and boxNorth >= ? and boxEast >= ? and boxSouth <= ?) or (boxWest == 0 and boxNorth == 0)) and ObservationTime > ? limit 1", Longitude, Latitude, Longitude, Latitude, gmtNow.AddMinutes(-20));
                cache.ToString();
                if (cache.Count > 0)
                {
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

        public WeatherInfo() { } //just for sql

        public WeatherInfo(double lat, double lng, DateTime observationTime)
        {
            ObservationTime = observationTime;

            if (lat == 0 && lng == 0)
            {
                boxNorth = 0;
                boxEast = 0;
                boxSouth = 0;
                boxWest = 0;
            }
            else
            {
                int radius = 3000;
                System.Drawing.PointF center = new System.Drawing.PointF((float)lat, (float)lng);
                double mult = 1.1; // mult = 1.1; is more reliable
                System.Drawing.PointF pn, pe, ps, pw;
                pn = pe = ps = pw = new System.Drawing.PointF();
                pn = mySQLiteConnection.calculateDerivedPosition(center, mult * radius, 0);
                pe = mySQLiteConnection.calculateDerivedPosition(center, mult * radius, 90);
                ps = mySQLiteConnection.calculateDerivedPosition(center, mult * radius, 180);
                pw = mySQLiteConnection.calculateDerivedPosition(center, mult * radius, 270);

                boxNorth = pn.X;
                boxEast = pe.Y;
                boxSouth = ps.X;
                boxWest = pw.Y;
            }
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
        public string Error { get; set; }

        public bool IsValid()
            => string.IsNullOrEmpty(Error);

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

        public override string ToString()
        {
            return ObservationTime.ToShortDateString() + " " + ObservationTime.ToLongTimeString() + " " + (WeatherCode ?? Error);
        }

        [Ignore]
        public string xString { get => ToString(); }
    }
}