/*
 based on sample code from www.astronomie.info
 changed to compute only necessary information on property-access to be faster if you only need one or some values for multiple time-points
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace iChronoMe.Core.Classes
{
    public class SunMoon
    {
        #region Variablen/Konstanten
        public object oLock = new object();
        private double _Lat = 50.0;
        private double _Lon = 10.0;
        private DateTime _Datum = DateTime.Now;
        private LocationTimeHolder _lth = null;
        private double _DeltaT = 65.0;
        private int _Zone = (DateTime.Now - DateTime.UtcNow).Hours;

        private const double DEG = Math.PI / 180.0;
        private const double RAD = 180.0 / Math.PI;
        #endregion

        #region Enumeratoren
        public enum SIGN
        {
            SIGN_ARIES,         //!< Widder
            SIGN_TAURUS,        //!< Stier
            SIGN_GEMINI,        //!< Zwillinge
            SIGN_CANCER,        //!< Krebs
            SIGN_LEO,           //!< Löwe
            SIGN_VIRGO,         //!< Jungfrau
            SIGN_LIBRA,         //!< Waage
            SIGN_SCORPIO,       //!< Skorpion
            SIGN_SAGITTARIUS,   //!< Schütze
            SIGN_CAPRICORNUS,   //!< Steinbock
            SIGN_AQUARIUS,      //!< Wassermann
            SIGN_PISCES         //!< Fische
        };

        public enum LUNARPHASE
        {
            LP_NEW_MOON,                //!< Neumond
            LP_WAXING_CRESCENT_MOON,    //!< Zunehmende Sichel
            LP_FIRST_QUARTER_MOON,      //!< Erstes Viertel
            LP_WAXING_GIBBOUS_MOON,     //!< Zunehmender Mond
            LP_FULL_MOON,               //!< Vollmond
            LP_WANING_GIBBOUS_MOON,     //!< Abnehmender Mond
            LP_LAST_QUARTER_MOON,       //!< Letztes Viertel
            LP_WANING_CRESCENT_MOON,    //!< Abnehmende Sichel
        };
        #endregion

        #region RealSunTime
        public double LocWoz12
        {
            get
            {
                if (_lth == null)
                    _lth = new LocationTimeHolder(_Lat, _Lon, false);
                var bak = _lth.timeZoneInfo;
                _lth.timeZoneInfo = null;
                _lth.SetTime(_Datum.Date.AddHours(12), TimeType.RealSunTime);
                var val =_lth.GetTime(TimeType.TimeZoneTime).TimeOfDay.TotalHours;
                _lth.timeZoneInfo = bak;
                return val;
            }
        }

        public double LocWoz12Old
        {
            get
            {
#if DEBUG
                if (_lth == null)
                    _lth = new LocationTimeHolder(_Lat, _Lon, false);
                var bak = _lth.timeZoneInfo;
                _lth.timeZoneInfo = null;
                _lth.SetTime(_Datum.Date.AddHours(12), TimeType.RealSunTimeOld);
                var val = _lth.GetTime(TimeType.TimeZoneTime).TimeOfDay.TotalHours;
                _lth.timeZoneInfo = bak;
                return val;
#else
                return 0;
#endif
            }
        }        

        public double LocWoz12DST
        {
            get
            {
                if (_lth == null)
                    _lth = new LocationTimeHolder(_Lat, _Lon, false);
                _lth.SetTime(_Datum.Date.AddHours(12), TimeType.RealSunTime);
                return _lth.GetTime(TimeType.TimeZoneTime).TimeOfDay.TotalHours;
            }
        }
#endregion

#region Eigenschaften
        /// <summary>
        /// Östl. geografische Länge
        /// </summary>
        public double Lat
        {
            get { return this._Lat; }
        }

        /// <summary>
        /// Geografische Breite
        /// </summary>
        public double Lon
        {
            get { return this._Lon; }
        }

        /// <summary>
        /// Datum und Zeit (lokal)
        /// </summary>
        public DateTime Datum
        {
            get { return this._Datum; }
        }

        /// <summary>
        /// Zeitdifferenz zu Weltzeit
        /// </summary>
        public int Zone
        {
            get { return this._Zone; }
        }

        /// <summary>
        /// deltaT
        /// </summary>
        public double DeltaT
        {
            get { return this._DeltaT; }
        }

        /// <summary>
        /// Julianisches Datum
        /// </summary>
        public double JD { get => _JD; }

        /// <summary>
        /// Greenwich Sternzeit GMST
        /// </summary>
        public TimeSpan GMST { get => _GMST; }

        /// <summary>
        /// Lokale Sternzeit LMST
        /// </summary>
        public TimeSpan LMST { get => _LMST; }

        /// <summary>
        /// Entfernung der Sonne (Erdmittelpunkt)
        /// </summary>
        public double SunDistanceCenter
        {
            get
            {
                if (_SunDistanceCenter == null)
                    ComputeSunCore();
                return _SunDistanceCenter.Value;
            }
        }

        /// <summary>
        /// Entfernung der Sonne (vom Beobachter)
        /// </summary>
        public double SunDistanceObserver
        {
            get
            {
                if (_SunDistanceObserver == null)
                    ComputeSunDistanceObserver();
                return _SunDistanceObserver.Value;
            }
        }

        /// <summary>
        /// Eklipt. Länge der Sonne
        /// </summary>
        public double SunLon
        {
            get
            {
                if (_SunLon == null)
                    ComputeSunCore();
                return _SunLon.Value;
            }
        }

        /// <summary>
        /// Rektaszension der Sonne
        /// </summary>
        public TimeSpan SunRA
        {
            get
            {
                if (_SunRA == null)
                    ComputeSunCore();
                return _SunRA.Value;
            }
        }

        /// <summary>
        /// Deklination der Sonne
        /// </summary>
        public double SunDec
        {
            get
            {
                if (_SunDec == null)
                    ComputeSunCore();
                return _SunDec.Value;
            }
        }

        /// <summary>
        /// Azimut der Sonne
        /// </summary>
        public double SunAz
        {
            get
            {
                if (_SunAz == null)
                    ComputeSunCore();
                return _SunAz.Value;
            }
        }

        /// <summary>
        /// Höhe der Sonne über Horizont
        /// </summary>
        public double SunAlt
        {
            get
            {
                if (_SunAlt == null)
                    ComputeSunCore();
                return _SunAlt.Value;
            }
        }

        /// <summary>
        /// Durchmesser der Sonne
        /// </summary>
        public double SunDiameter
        {
            get
            {
                if (_SunDiameter == null)
                    ComputeSunCore();
                return _SunDiameter.Value;
            }
        }

        /// <summary>
        /// Astronomische Morgendämmerung
        /// </summary>
        public TimeSpan SunAstronomicalTwilightMorning
        {
            get
            {
                if (_SunAstronomicalTwilightMorning == null)
                    ComputeSunRise();
                return _SunAstronomicalTwilightMorning.Value;
            }
        }

        /// <summary>
        /// Nautische Morgendämmerung
        /// </summary>
        public TimeSpan SunNauticalTwilightMorning
        {
            get
            {
                if (_SunNauticalTwilightMorning == null)
                    ComputeSunRise();
                return _SunNauticalTwilightMorning.Value;
            }
        }

        /// <summary>
        /// Bürgerliche Morgendämmerung
        /// </summary>
        public TimeSpan SunCivilTwilightMorning
        {
            get
            {
                if (_SunCivilTwilightMorning == null)
                    ComputeSunRise();
                return _SunCivilTwilightMorning.Value;
            }
        }

        /// <summary>
        /// Sonnenaufgang
        /// </summary>
        public TimeSpan SunRise
        {
            get
            {
                if (_SunRise == null)
                    ComputeSunRise();
                return _SunRise.Value;
            }
        }

        /// <summary>
        /// Sonnenkulmination
        /// </summary>
        public TimeSpan SunTransit
        {
            get
            {
                if (_SunTransit == null)
                    ComputeSunRise();
                return _SunTransit.Value;
            }
        }

        /// <summary>
        /// Sonnenuntergang
        /// </summary>
        public TimeSpan SunSet
        {
            get
            {
                if (_SunSet == null)
                    ComputeSunRise();
                return _SunSet.Value;
            }
        }

        /// <summary>
        /// Bürgerliche Abenddämmerung
        /// </summary>
        public TimeSpan SunCivilTwilightEvening
        {
            get
            {
                if (_SunCivilTwilightEvening == null)
                    ComputeSunRise();
                return _SunCivilTwilightEvening.Value;
            }
        }

        /// <summary>
        /// Nautische Abenddämmerung
        /// </summary>
        public TimeSpan SunNauticalTwilightEvening
        {
            get
            {
                if (_SunNauticalTwilightEvening == null)
                    ComputeSunRise();
                return _SunNauticalTwilightEvening.Value;
            }
        }

        /// <summary>
        /// Astronomische Abenddämmerung
        /// </summary>
        public TimeSpan SunAstronomicalTwilightEvening
        {
            get
            {
                if (_SunAstronomicalTwilightEvening == null)
                    ComputeSunRise();
                return _SunAstronomicalTwilightEvening.Value;
            }
        }

        /// <summary>
        /// Tierkreiszeichen
        /// </summary>
        public SIGN SunSign
        {
            get
            {
                if (_SunSign == null)
                    ComputeSunCore();
                return _SunSign.Value;
            }
        }

        /// <summary>
        /// Entfernung des Mondes (Erdmittelpunkt)
        /// </summary>
        public double MoonDistanceCenter
        {
            get
            {
                if (_MoonDistanceCenter == null)
                    ComputeMoonCore();
                return _MoonDistanceCenter.Value;
            }
        }

        /// <summary>
        /// Entfernung des Mondes (vom Beobachter)
        /// </summary>
        public double MoonDistanceObserver
        {
            get
            {
                if (_MoonDistanceObserver == null)
                    ComputeMoonCore();
                return _MoonDistanceObserver.Value;
            }
        }

        /// <summary>
        /// Eklipt. Länge des Mondes
        /// </summary>
        public double MoonLon
        {
            get
            {
                if (_MoonLon == null)
                    ComputeMoonCore();
                return _MoonLon.Value;
            }
        }

        /// <summary>
        /// Eklipt. Breite des Mondes
        /// </summary>
        public double MoonLat
        {
            get
            {
                if (_MoonLat == null)
                    ComputeMoonCore();
                return _MoonLat.Value;
            }
        }

        /// <summary>
        /// Rektaszension des Mondes
        /// </summary>
        public TimeSpan MoonRA
        {
            get
            {
                if (_MoonRA == null)
                    ComputeMoonCore();
                return _MoonRA.Value;
            }
        }

        /// <summary>
        /// Deklination des Mondes
        /// </summary>
        public double MoonDec
        {
            get
            {
                if (_MoonDec == null)
                    ComputeMoonCore();
                return _MoonDec.Value;
            }
        }

        /// <summary>
        /// Azimut des Mondes
        /// </summary>
        public double MoonAz
        {
            get
            {
                if (_MoonAz == null)
                    ComputeMoonCore();
                return _MoonAz.Value;
            }
        }

        /// <summary>
        /// Höhe des Mondes über Horizont
        /// </summary>
        public double MoonAlt
        {
            get
            {
                if (_MoonAlt == null)
                    ComputeMoonCore();
                return _MoonAlt.Value;
            }
        }

        /// <summary>
        /// Durchmesser des Mondes
        /// </summary>
        public double MoonDiameter
        {
            get
            {
                if (_MoonDiameter == null)
                    ComputeMoonCore();
                return _MoonDiameter.Value;
            }
        }

        /// <summary>
        /// Mondaufgang
        /// </summary>
        public TimeSpan MoonRise
        {
            get
            {
                if (_MoonRise == null)
                    ComputeMoonRise();
                return _MoonRise.Value;
            }
        }

        /// <summary>
        /// Mondkulmination
        /// </summary>
        public TimeSpan MoonTransit
        {
            get
            {
                if (_MoonTransit == null)
                    ComputeMoonRise();
                return _MoonTransit.Value;
            }
        }

        /// <summary>
        /// Monduntergang
        /// </summary>
        public TimeSpan MoonSet
        {
            get
            {
                if (_MoonSet == null)
                    ComputeMoonRise();
                return _MoonSet.Value;
            }
        }

        /// <summary>
        /// Mondphase
        /// </summary>
        public double MoonPhaseNumber
        {
            get
            {
                if (_MoonPhaseNumber == null)
                    ComputeMoonCore();
                return _MoonPhaseNumber.Value;
            }
        }

        /// <summary>
        /// Mondalter
        /// </summary>
        public double MoonAge
        {
            get
            {
                if (_MoonAge == null)
                    ComputeMoonCore();
                return _MoonAge.Value;
            }
        }

        /// <summary>
        /// Mondphase
        /// </summary>
        public LUNARPHASE MoonPhase
        {
            get
            {
                if (_MoonPhase == null)
                    ComputeMoonCore();
                return _MoonPhase.Value;
            }
        }

        /// <summary>
        /// Mondzeichen
        /// </summary>
        public SIGN MoonSign
        {
            get
            {
                if (_MoonSign == null)
                    ComputeMoonCore();
                return _MoonSign.Value;
            }
        }
#endregion

#region internal Fields

        //die internen Felder werden in den Berechnungsgruppen gesetzt sobald auf eine entsprechende Eigenschaft zugegriffen wird
        double _JD;

        TimeSpan _GMST;

        TimeSpan _LMST;

        double? _SunDistanceCenter;

        double? _SunDistanceObserver;

        double? _SunLon;

        TimeSpan? _SunRA;

        double? _SunDec;

        double? _SunAz;

        double? _SunAlt;

        double? _SunDiameter;

        TimeSpan? _SunAstronomicalTwilightMorning;

        TimeSpan? _SunNauticalTwilightMorning;

        TimeSpan? _SunCivilTwilightMorning;

        TimeSpan? _SunRise;

        TimeSpan? _SunTransit;

        TimeSpan? _SunSet;

        TimeSpan? _SunCivilTwilightEvening;

        TimeSpan? _SunNauticalTwilightEvening;

        TimeSpan? _SunAstronomicalTwilightEvening;

        SIGN? _SunSign;

        double? _MoonDistanceCenter;

        double? _MoonDistanceObserver;

        double? _MoonLon;

        double? _MoonLat;

        TimeSpan? _MoonRA;

        double? _MoonDec;

        double? _MoonAz;

        double? _MoonAlt;

        double? _MoonDiameter;

        TimeSpan? _MoonRise;

        TimeSpan? _MoonTransit;

        TimeSpan? _MoonSet;

        double? _MoonPhaseNumber;

        double? _MoonAge;

        LUNARPHASE? _MoonPhase;
        SIGN? _MoonSign;


#endregion

#region Berechnungsfunktionen
        private double sqr(double x) { return x * x; }
        private int Int(double x) { return (x < 0) ? (int)Math.Ceiling(x) : (int)Math.Floor(x); }
        private double frac(double x) { return (x - Math.Floor(x)); }
        private double Mod(double a, double b) { return (a - Math.Floor(a / b) * b); }
        private double Mod2Pi(double x) { return Mod(x, 2.0 * Math.PI); }
        private double round100000(double x) { return (Math.Round(100000.0 * x) / 100000.0); }
        private double round10000(double x) { return (Math.Round(10000.0 * x) / 10000.0); }
        private double round1000(double x) { return (Math.Round(1000.0 * x) / 1000.0); }
        private double round100(double x) { return (Math.Round(100.0 * x) / 100.0); }
        private double round10(double x) { return (Math.Round(10.0 * x) / 10.0); }

        private TimeSpan HHMM(double hh)
        {
            return HHMMSS(hh);
        }
        private string HHMMFormat(double hh)
        {
            if (hh == 0) return null;
            return HHMM(hh).ToString("hh:mm");
        }
        private TimeSpan HHMMSS(double hh)
        {
            if (hh == 0 || double.IsNaN(hh)) return new TimeSpan();
            double m = frac(hh) * 60.0;
            int h = Int(hh);
            double s = frac(m) * 60.0;
            m = Int(m);
            if (s >= 59.5) { m++; s -= 60.0; }
            if (m >= 60.0) { h++; m -= 60.0; }
            s = Math.Round(s);
            return new TimeSpan(h, (int)m, (int)s);
        }
        private string HHMMSSFormat(double hh)
        {
            if (hh == 0) return null;
            return HHMMSS(hh).ToString("hh:mm:ss");
        }

        private SIGN Sign(double lon)
        {
            int ix = (int)Math.Floor(lon * RAD / 30.0);
            return (SIGN)ix;
        }

        // Calculate Julian date: valid only from 1.3.1901 to 28.2.2100
        private double CalcJD(int day, int month, int year)
        {
            double jd = 2415020.5 - 64; // 1.1.1900 - correction of algorithm
            if (month <= 2) { year--; month += 12; }
            jd += Int(((year - 1900)) * 365.25);
            jd += Int(30.6001 * ((1 + month)));
            return jd + day;
        }

        // Julian Date to Greenwich Mean Sidereal Time
        private double CalcGMST(double JD)
        {
            double UT = frac(JD - 0.5) * 24.0; // UT in hours
            JD = Math.Floor(JD - 0.5) + 0.5;   // JD at 0 hours UT
            double T = (JD - 2451545.0) / 36525.0;
            double T0 = 6.697374558 + T * (2400.051336 + T * 0.000025862);
            return (Mod(T0 + UT * 1.002737909, 24.0));
        }

        // Convert Greenweek mean sidereal time to UT
        private double GMST2UT(double JD, double gmst)
        {
            JD = Math.Floor(JD - 0.5) + 0.5;   // JD at 0 hours UT
            double T = (JD - 2451545.0) / 36525.0;
            double T0 = Mod(6.697374558 + T * (2400.051336 + T * 0.000025862), 24.0);
            return 0.9972695663 * ((gmst - T0));
        }

        // Local Mean Sidereal Time, geographical longitude in radians, East is positive
        private double GMST2LMST(double gmst, double lon) { return Mod(gmst + RAD * lon / 15, 24.0); }

        // Transform ecliptical coordinates (lon/lat) to equatorial coordinates (RA/dec)
        private Dictionary<string, double> Ecl2Equ(Dictionary<string, double> coor, double TDT)
        {
            double T = (TDT - 2451545.0) / 36525.0; // Epoch 2000 January 1.5
            double eps = (23.0 + (26 + 21.45 / 60.0) / 60.0 + T * (-46.815 + T * (-0.0006 + T * 0.00181)) / 3600.0) * DEG;
            double coseps = Math.Cos(eps);
            double sineps = Math.Sin(eps);

            double sinlon = Math.Sin(coor["lon"]);
            coor["ra"] = Mod2Pi(Math.Atan2((sinlon * coseps - Math.Tan(coor["lat"]) * sineps), Math.Cos(coor["lon"])));
            coor["dec"] = Math.Asin(Math.Sin(coor["lat"]) * coseps + Math.Cos(coor["lat"]) * sineps * sinlon);

            return coor;
        }

        // Transform equatorial coordinates (RA/Dec) to horizonal coordinates (azimuth/altitude)
        // Refraction is ignored
        private Dictionary<string, double> Equ2Altaz(Dictionary<string, double> coor, double TDT, double geolat, double lmst)
        {
            double cosdec = Math.Cos(coor["dec"]);
            double sindec = Math.Sin(coor["dec"]);
            double lha = lmst - coor["ra"];
            double coslha = Math.Cos(lha);
            double sinlha = Math.Sin(lha);
            double coslat = Math.Cos(geolat);
            double sinlat = Math.Sin(geolat);

            double N = -cosdec * sinlha;
            double D = sindec * coslat - cosdec * coslha * sinlat;
            coor["az"] = Mod2Pi(Math.Atan2(N, D));
            coor["alt"] = Math.Asin(sindec * sinlat + cosdec * coslha * coslat);

            return coor;
        }

        // Transform geocentric equatorial coordinates (RA/Dec) to topocentric equatorial coordinates
        private Dictionary<string, double> GeoEqu2TopoEqu(Dictionary<string, double> coor, Dictionary<string, double> observer, double lmst)
        {
            double cosdec = Math.Cos(coor["dec"]);
            double sindec = Math.Sin(coor["dec"]);
            double coslst = Math.Cos(lmst);
            double sinlst = Math.Sin(lmst);
            double coslat = Math.Cos(observer["lat"]); // we should use geocentric latitude, not geodetic latitude
            double sinlat = Math.Sin(observer["lat"]);
            double rho = observer["radius"]; // observer-geocenter in Kilometer

            double x = coor["distance"] * cosdec * Math.Cos(coor["ra"]) - rho * coslat * coslst;
            double y = coor["distance"] * cosdec * Math.Sin(coor["ra"]) - rho * coslat * sinlst;
            double z = coor["distance"] * sindec - rho * sinlat;

            coor["distanceTopocentric"] = Math.Sqrt(x * x + y * y + z * z);
            coor["decTopocentric"] = Math.Asin(z / coor["distanceTopocentric"]);
            coor["raTopocentric"] = Mod2Pi(Math.Atan2(y, x));

            return coor;
        }

        // Calculate cartesian from polar coordinates
        private Dictionary<string, double> EquPolar2Cart(double lon, double lat, double distance)
        {
            Dictionary<string, double> cart = new Dictionary<string, double>();
            double rcd = Math.Cos(lat) * distance;
            cart["x"] = rcd * Math.Cos(lon);
            cart["y"] = rcd * Math.Sin(lon);
            cart["z"] = distance * Math.Sin(lat);
            return cart;
        }

        // Calculate observers cartesian equatorial coordinates (x,y,z in celestial frame) 
        // from geodetic coordinates (longitude, latitude, height above WGS84 ellipsoid)
        // Currently only used to calculate distance of a body from the observer
        private Dictionary<string, double> Observer2EquCart(double lon, double lat, double height, double gmst)
        {
            double flat = 298.257223563;        // WGS84 flatening of earth
            double aearth = 6378.137;           // GRS80/WGS84 semi major axis of earth ellipsoid
            Dictionary<string, double> cart = new Dictionary<string, double>();
            // Calculate geocentric latitude from geodetic latitude
            double co = Math.Cos(lat);
            double si = Math.Sin(lat);
            double fl = 1.0 - 1.0 / flat;
            fl = fl * fl;
            si = si * si;
            double u = 1.0 / Math.Sqrt(co * co + fl * si);
            double a = aearth * u + height;
            double b = aearth * fl * u + height;
            double radius = Math.Sqrt(a * a * co * co + b * b * si); // geocentric distance from earth center
            cart["y"] = Math.Acos(a * co / radius); // geocentric latitude, rad
            cart["x"] = lon; // longitude stays the same
            if (lat < 0.0) { cart["y"] = -cart["y"]; } // adjust sign
            cart = EquPolar2Cart(cart["x"], cart["y"], radius); // convert from geocentric polar to geocentric cartesian, with regard to Greenwich
                                                                // rotate around earth's polar axis to align coordinate system from Greenwich to vernal equinox
            double x = cart["x"];
            double y = cart["y"];
            double rotangle = gmst / 24.0 * 2.0 * Math.PI; // sideral time gmst given in hours. Convert to radians
            cart["x"] = x * Math.Cos(rotangle) - y * Math.Sin(rotangle);
            cart["y"] = x * Math.Sin(rotangle) + y * Math.Cos(rotangle);
            cart["radius"] = radius;
            cart["lon"] = lon;
            cart["lat"] = lat;
            return cart;
        }

        // Calculate coordinates for Sun
        // Coordinates are accurate to about 10s (right ascension) 
        // and a few minutes of arc (declination)
        private Dictionary<string, double> SunPosition(double TDT, double? geolat = null, double? lmst = null)
        {
            double D = TDT - 2447891.5;

            double eg = 279.403303 * DEG;
            double wg = 282.768422 * DEG;
            double e = 0.016713;
            double a = 149598500; // km
            double diameter0 = 0.533128 * DEG; // angular diameter of Moon at a distance

            double MSun = 360 * DEG / 365.242191 * D + eg - wg;
            double nu = MSun + 360.0 * DEG / Math.PI * e * Math.Sin(MSun);

            Dictionary<string, double> sunCoor = new Dictionary<string, double>();
            sunCoor["lon"] = Mod2Pi(nu + wg);
            sunCoor["lat"] = 0;
            sunCoor["anomalyMean"] = MSun;

            sunCoor["distance"] = (1 - sqr(e)) / (1 + e * Math.Cos(nu)); // distance in astronomical units
            sunCoor["diameter"] = diameter0 / sunCoor["distance"]; // angular diameter in radians
            sunCoor["distance"] = sunCoor["distance"] * a;                         // distance in km
            sunCoor["parallax"] = 6378.137 / sunCoor["distance"];  // horizonal parallax

            sunCoor = Ecl2Equ(sunCoor, TDT);

            // Calculate horizonal coordinates of sun, if geographic positions is given
            if (geolat.HasValue && lmst.HasValue)
            {
                sunCoor = Equ2Altaz(sunCoor, TDT, geolat.Value, lmst.Value);
            }
            return sunCoor;
        }

        // Calculate data and coordinates for the Moon
        // Coordinates are accurate to about 1/5 degree (in ecliptic coordinates)
        private Dictionary<string, double> MoonPosition(Dictionary<string, double> sunCoor, double TDT, Dictionary<string, double> observer = null, double? lmst = null)
        {
            double D = TDT - 2447891.5;

            // Mean Moon orbit elements as of 1990.0
            double l0 = 318.351648 * DEG;
            double P0 = 36.340410 * DEG;
            double N0 = 318.510107 * DEG;
            double i = 5.145396 * DEG;
            double e = 0.054900;
            double a = 384401; // km
            double diameter0 = 0.5181 * DEG; // angular diameter of Moon at a distance
            double parallax0 = 0.9507 * DEG; // parallax at distance a

            double l = 13.1763966 * DEG * D + l0;
            double MMoon = l - 0.1114041 * DEG * D - P0; // Moon's mean anomaly M
            double N = N0 - 0.0529539 * DEG * D;       // Moon's mean ascending node longitude
            double C = l - sunCoor["lon"];
            double Ev = 1.2739 * DEG * Math.Sin(2 * C - MMoon);
            double Ae = 0.1858 * DEG * Math.Sin(sunCoor["anomalyMean"]);
            double A3 = 0.37 * DEG * Math.Sin(sunCoor["anomalyMean"]);
            double MMoon2 = MMoon + Ev - Ae - A3;  // corrected Moon anomaly
            double Ec = 6.2886 * DEG * Math.Sin(MMoon2);  // equation of centre
            double A4 = 0.214 * DEG * Math.Sin(2 * MMoon2);
            double l2 = l + Ev + Ec - Ae + A4; // corrected Moon's longitude
            double V = 0.6583 * DEG * Math.Sin(2 * (l2 - sunCoor["lon"]));
            double l3 = l2 + V; // true orbital longitude;

            double N2 = N - 0.16 * DEG * Math.Sin(sunCoor["anomalyMean"]);

            Dictionary<string, double> moonCoor = new Dictionary<string, double>();
            moonCoor["lon"] = Mod2Pi(N2 + Math.Atan2(Math.Sin(l3 - N2) * Math.Cos(i), Math.Cos(l3 - N2)));
            moonCoor["lat"] = Math.Asin(Math.Sin(l3 - N2) * Math.Sin(i));
            moonCoor["orbitLon"] = l3;

            moonCoor = Ecl2Equ(moonCoor, TDT);
            // relative distance to semi mayor axis of lunar oribt
            moonCoor["distance"] = (1 - sqr(e)) / (1 + e * Math.Cos(MMoon2 + Ec));
            moonCoor["diameter"] = diameter0 / moonCoor["distance"]; // angular diameter in radians
            moonCoor["parallax"] = parallax0 / moonCoor["distance"]; // horizontal parallax in radians
            moonCoor["distance"] = moonCoor["distance"] * a; // distance in km

            // Calculate horizonal coordinates of sun, if geographic positions is given
            if (observer != null && lmst.HasValue)
            {
                // transform geocentric coordinates into topocentric (==observer based) coordinates
                moonCoor = GeoEqu2TopoEqu(moonCoor, observer, lmst.Value);
                moonCoor["raGeocentric"] = moonCoor["ra"]; // backup geocentric coordinates
                moonCoor["decGeocentric"] = moonCoor["dec"];
                moonCoor["ra"] = moonCoor["raTopocentric"];
                moonCoor["dec"] = moonCoor["decTopocentric"];
                moonCoor = Equ2Altaz(moonCoor, TDT, observer["lat"], lmst.Value); // now ra and dec are topocentric
            }

            // Age of Moon in radians since New Moon (0) - Full Moon (pi)
            moonCoor["moonAge"] = Mod2Pi(l3 - sunCoor["lon"]);
            moonCoor["phase"] = 0.5 * (1 - Math.Cos(moonCoor["moonAge"])); // Moon phase, 0-1

            double mainPhase = 1.0 / 29.53 * 360 * DEG; // show 'Newmoon, 'Quarter' for +/-1 day arond the actual event
            double p = Mod(moonCoor["moonAge"], 90.0 * DEG);
            if (p < mainPhase || p > 90 * DEG - mainPhase) p = 2 * Math.Round(moonCoor["moonAge"] / (90.0 * DEG));
            else p = 2 * Math.Floor(moonCoor["moonAge"] / (90.0 * DEG)) + 1;
            moonCoor["moonPhase"] = (int)p;

            return moonCoor;
        }

        // Rough refraction formula using standard atmosphere: 1015 mbar and 10°C
        // Input true altitude in radians, Output: increase in altitude in degrees
        private double Refraction(double alt)
        {
            double altdeg = alt * RAD;
            if (altdeg < -2 || altdeg >= 90) return 0.0;

            double pressure = 1015;
            double temperature = 10;
            if (altdeg > 15) return (0.00452 * pressure / ((273 + temperature) * Math.Tan(alt)));

            double y = alt;
            double D = 0.0;
            double P = (pressure - 80.0) / 930.0;
            double Q = 0.0048 * (temperature - 10.0);
            double y0 = y;
            double D0 = D;

            for (int i = 0; i < 3; i++)
            {
                double N = y + (7.31 / (y + 4.4));
                N = 1.0 / Math.Tan(N * DEG);
                D = N * P / (60.0 + Q * (N + 39.0));
                N = y - y0;
                y0 = D - D0 - N;
                if ((N != 0.0) && (y0 != 0.0)) { N = y - N * (alt + D - y) / y0; }
                else { N = alt + D; }
                y0 = y;
                D0 = D;
                y = N;
            }
            return D; // Hebung durch Refraktion in radians
        }

        // returns Greenwich sidereal time (hours) of time of rise 
        // and set of object with coordinates coor.ra/coor.dec
        // at geographic position lon/lat (all values in radians)
        // Correction for refraction and semi-diameter/parallax of body is taken care of in function RiseSet
        // h is used to calculate the twilights. It gives the required elevation of the disk center of the sun
        private Dictionary<string, double> GMSTRiseSet(Dictionary<string, double> coor, double lon, double lat, double? hn)
        {
            double h = hn.HasValue ? hn.Value : 0.0; // set default value
            Dictionary<string, double> riseset = new Dictionary<string, double>();
            //  double tagbogen = Math.Acos(-Math.Tan(lat)*Math.Tan(coor["dec"])); // simple formula if twilight is not required
            double tagbogen = Math.Acos((Math.Sin(h) - Math.Sin(lat) * Math.Sin(coor["dec"])) / (Math.Cos(lat) * Math.Cos(coor["dec"])));

            riseset["transit"] = RAD / 15 * (+coor["ra"] - lon);
            riseset["rise"] = 24.0 + RAD / 15 * (-tagbogen + coor["ra"] - lon); // calculate GMST of rise of object
            riseset["set"] = RAD / 15 * (+tagbogen + coor["ra"] - lon); // calculate GMST of set of object

            // using the modulo function Mod, the day number goes missing. This may get a problem for the moon
            riseset["transit"] = Mod(riseset["transit"], 24);
            riseset["rise"] = Mod(riseset["rise"], 24);
            riseset["set"] = Mod(riseset["set"], 24);

            return riseset;
        }

        // Find GMST of rise/set of object from the two calculates 
        // (start)points (day 1 and 2) and at midnight UT(0)
        private double InterpolateGMST(double gmst0, double gmst1, double gmst2, double timefactor)
        {
            return ((timefactor * 24.07 * gmst1 - gmst0 * (gmst2 - gmst1)) / (timefactor * 24.07 + gmst1 - gmst2));
        }

        // JD is the Julian Date of 0h UTC time (midnight)
        private Dictionary<string, double> RiseSet(double jd0UT, Dictionary<string, double> coor1, Dictionary<string, double> coor2, double lon, double lat, double timeinterval, double? naltitude = null)
        {
            // altitude of sun center: semi-diameter, horizontal parallax and (standard) refraction of 34'
            double alt = 0.0; // calculate 
            double altitude = (naltitude.HasValue) ? naltitude.Value : 0.0; // set default value

            // true height of sun center for sunrise and set calculation. Is kept 0 for twilight (ie. altitude given):
            if (altitude == 0.0) alt = 0.5 * coor1["diameter"] - coor1["parallax"] + 34.0 / 60 * DEG;

            Dictionary<string, double> rise1 = GMSTRiseSet(coor1, lon, lat, altitude);
            Dictionary<string, double> rise2 = GMSTRiseSet(coor2, lon, lat, altitude);

            Dictionary<string, double> rise = new Dictionary<string, double>();

            // unwrap GMST in case we move across 24h -> 0h
            if (rise1["transit"] > rise2["transit"] && Math.Abs(rise1["transit"] - rise2["transit"]) > 18) rise2["transit"] = rise2["transit"] + 24;
            if (rise1["rise"] > rise2["rise"] && Math.Abs(rise1["rise"] - rise2["rise"]) > 18) rise2["rise"] = rise2["rise"] + 24;
            if (rise1["set"] > rise2["set"] && Math.Abs(rise1["set"] - rise2["set"]) > 18) rise2["set"] = rise2["set"] + 24;
            double T0 = CalcGMST(jd0UT);
            //  double T02 = T0-zone*1.002738; // Greenwich sidereal time at 0h time zone (zone: hours)

            // Greenwich sidereal time for 0h at selected longitude
            double T02 = T0 - lon * RAD / 15 * 1.002738; if (T02 < 0) T02 += 24;

            if (rise1["transit"] < T02) { rise1["transit"] = rise1["transit"] + 24; rise2["transit"] = rise2["transit"] + 24; }
            if (rise1["rise"] < T02) { rise1["rise"] = rise1["rise"] + 24; rise2["rise"] = rise2["rise"] + 24; }
            if (rise1["set"] < T02) { rise1["set"] = rise1["set"] + 24; rise2["set"] = rise2["set"] + 24; }

            // Refraction and Parallax correction
            double decMean = 0.5 * (coor1["dec"] + coor2["dec"]);
            double psi = Math.Acos(Math.Sin(lat) / Math.Cos(decMean));
            double y = Math.Asin(Math.Sin(alt) / Math.Sin(psi));
            double dt = 240 * RAD * y / Math.Cos(decMean) / 3600; // time correction due to refraction, parallax

            rise["transit"] = GMST2UT(jd0UT, InterpolateGMST(T0, rise1["transit"], rise2["transit"], timeinterval));
            rise["rise"] = GMST2UT(jd0UT, InterpolateGMST(T0, rise1["rise"], rise2["rise"], timeinterval) - dt);
            rise["set"] = GMST2UT(jd0UT, InterpolateGMST(T0, rise1["set"], rise2["set"], timeinterval) + dt);

            return (rise);
        }

        // Find (local) time of sunrise and sunset, and twilights
        // JD is the Julian Date of 0h local time (midnight)
        // Accurate to about 1-2 minutes
        // recursive: 1 - calculate rise/set in UTC in a second run
        // recursive: 0 - find rise/set on the current local day. This is set when doing the first call to this function
        private Dictionary<string, double> CalcSunRise(double JD, double deltaT, double lon, double lat, int zone, bool recursive)
        {
            double jd0UT = Math.Floor(JD - 0.5) + 0.5;   // JD at 0 hours UT
            Dictionary<string, double> coor1 = SunPosition(jd0UT + deltaT / 24.0 / 3600.0);
            Dictionary<string, double> coor2 = SunPosition(jd0UT + 1.0 + deltaT / 24.0 / 3600.0); // calculations for next day's UTC midnight

            Dictionary<string, double> risetemp;
            // rise/set time in UTC. 
            Dictionary<string, double> rise = RiseSet(jd0UT, coor1, coor2, lon, lat, 1);
            if (!recursive)
            { // check and adjust to have rise/set time on local calendar day
                if (zone > 0)
                {
                    // rise time was yesterday local time -> calculate rise time for next UTC day
                    if (rise["rise"] >= 24 - zone || rise["transit"] >= 24 - zone || rise["set"] >= 24 - zone)
                    {
                        risetemp = CalcSunRise(JD + 1, deltaT, lon, lat, zone, true);
                        if (rise["rise"] >= 24 - zone) rise["rise"] = risetemp["rise"];
                        if (rise["transit"] >= 24 - zone) rise["transit"] = risetemp["transit"];
                        if (rise["set"] >= 24 - zone) rise["set"] = risetemp["set"];
                    }
                }
                else if (zone < 0)
                {
                    // rise time was yesterday local time -> calculate rise time for next UTC day
                    if (rise["rise"] < -zone || rise["transit"] < -zone || rise["set"] < -zone)
                    {
                        risetemp = CalcSunRise(JD - 1, deltaT, lon, lat, zone, true);
                        if (rise["rise"] < -zone) rise["rise"] = risetemp["rise"];
                        if (rise["transit"] < -zone) rise["transit"] = risetemp["transit"];
                        if (rise["set"] < -zone) rise["set"] = risetemp["set"];
                    }
                }

                rise["transit"] = Mod(rise["transit"] + zone, 24.0);
                rise["rise"] = Mod(rise["rise"] + zone, 24.0);
                rise["set"] = Mod(rise["set"] + zone, 24.0);

                // Twilight calculation
                // civil twilight time in UTC. 
                risetemp = RiseSet(jd0UT, coor1, coor2, lon, lat, 1, -6.0 * DEG);
                rise["cicilTwilightMorning"] = Mod(risetemp["rise"] + zone, 24.0);
                rise["cicilTwilightEvening"] = Mod(risetemp["set"] + zone, 24.0);

                // nautical twilight time in UTC. 
                risetemp = RiseSet(jd0UT, coor1, coor2, lon, lat, 1, -12.0 * DEG);
                rise["nauticalTwilightMorning"] = Mod(risetemp["rise"] + zone, 24.0);
                rise["nauticalTwilightEvening"] = Mod(risetemp["set"] + zone, 24.0);

                // astronomical twilight time in UTC. 
                risetemp = RiseSet(jd0UT, coor1, coor2, lon, lat, 1, -18.0 * DEG);
                rise["astronomicalTwilightMorning"] = Mod(risetemp["rise"] + zone, 24.0);
                rise["astronomicalTwilightEvening"] = Mod(risetemp["set"] + zone, 24.0);
            }
            return rise;
        }

        // Find local time of moonrise and moonset
        // JD is the Julian Date of 0h local time (midnight)
        // Accurate to about 5 minutes or better
        // recursive: 1 - calculate rise/set in UTC
        // recursive: 0 - find rise/set on the current local day (set could also be first)
        // returns '' for moonrise/set does not occur on selected day
        private Dictionary<string, double> CalcMoonRise(double JD, double deltaT, double lon, double lat, int zone, bool recursive)
        {
            double timeinterval = 0.5;
            double jd0UT = Math.Floor(JD - 0.5) + 0.5;   // JD at 0 hours UT
            Dictionary<string, double> suncoor1 = SunPosition(jd0UT + deltaT / 24.0 / 3600.0);
            Dictionary<string, double> coor1 = MoonPosition(suncoor1, jd0UT + deltaT / 24.0 / 3600.0);

            Dictionary<string, double> suncoor2 = SunPosition(jd0UT + timeinterval + deltaT / 24.0 / 3600.0); // calculations for noon
                                                                                                              // calculations for next day's midnight
            Dictionary<string, double> coor2 = MoonPosition(suncoor2, jd0UT + timeinterval + deltaT / 24.0 / 3600.0);

            Dictionary<string, double> risetemp;

            // rise/set time in UTC, time zone corrected later.
            // Taking into account refraction, semi-diameter and parallax
            Dictionary<string, double> rise = RiseSet(jd0UT, coor1, coor2, lon, lat, timeinterval);

            if (!recursive)
            { // check and adjust to have rise/set time on local calendar day
                if (zone > 0)
                {
                    // recursive call to MoonRise returns events in UTC
                    risetemp = CalcMoonRise(JD - 1.0, deltaT, lon, lat, zone, true);
                    if (rise["transit"] >= 24.0 - zone || rise["transit"] < -zone)
                    { // transit time is tomorrow local time
                        if (risetemp["transit"] < 24.0 - zone) rise["transit"] = double.NaN; // there is no moontransit today
                        else rise["transit"] = risetemp["transit"];
                    }

                    if (rise["rise"] >= 24.0 - zone || rise["rise"] < -zone)
                    { // rise time is tomorrow local time
                        if (risetemp["rise"] < 24.0 - zone) rise["rise"] = double.NaN; // there is no moontransit today
                        else rise["rise"] = risetemp["rise"];
                    }

                    if (rise["set"] >= 24.0 - zone || rise["set"] < -zone)
                    { // set time is tomorrow local time
                        if (risetemp["set"] < 24.0 - zone) rise["set"] = double.NaN; // there is no moontransit today
                        else rise["set"] = risetemp["set"];
                    }

                }
                else if (zone < 0)
                {
                    // rise/set time was tomorrow local time -> calculate rise time for former UTC day
                    if (rise["rise"] < -zone || rise["set"] < -zone || rise["transit"] < -zone)
                    {
                        risetemp = CalcMoonRise(JD + 1.0, deltaT, lon, lat, zone, true);

                        if (rise["rise"] < -zone)
                        {
                            if (risetemp["rise"] > -zone) rise["rise"] = double.NaN; // there is no moonrise today
                            else rise["rise"] = risetemp["rise"];
                        }

                        if (rise["transit"] < -zone)
                        {
                            if (risetemp["transit"] > -zone) rise["transit"] = double.NaN; // there is no moonset today
                            else rise["transit"] = risetemp["transit"];
                        }

                        if (rise["set"] < -zone)
                        {
                            if (risetemp["set"] > -zone) rise["set"] = double.NaN; // there is no moonset today
                            else rise["set"] = risetemp["set"];
                        }

                    }
                }

                if (rise["rise"] != double.NaN) rise["rise"] = Mod(rise["rise"] + zone, 24.0);    // correct for time zone, if time is valid
                if (rise["transit"] != double.NaN) rise["transit"] = Mod(rise["transit"] + zone, 24.0); // correct for time zone, if time is valid
                if (rise["set"] != double.NaN) rise["set"] = Mod(rise["set"] + zone, 24.0);    // correct for time zone, if time is valid
            }
            return rise;
        }
#endregion

#region Initiale Berechnungen und Berechnungsgruppen
        double __JD0;
        double __jd;
        double __TDT;
        double __lat;
        double __lon;
        double __height;

        double __gmst;
        double __lmst;

        private void Init()
        {
            __JD0 = CalcJD(this._Datum.Day, this._Datum.Month, this._Datum.Year);
            __jd = __JD0 + (this._Datum.Hour - this._Zone + this._Datum.Minute / 60.0 + this._Datum.Second / 3600.0) / 24.0;
            __TDT = __jd + this._DeltaT / 24.0 / 3600.0;
            __lat = this._Lat * DEG; // geodetic latitude of observer on WGS84
            __lon = this._Lon * DEG; // latitude of observer
            __height = 0 * 0.001; // altiude of observer in meters above WGS84 ellipsoid (and converted to kilometers)

            __gmst = CalcGMST(__jd);
            __lmst = GMST2LMST(__gmst, __lon);

            this._JD = round100000(__jd);
            this._GMST = HHMMSS(__gmst);
            this._LMST = HHMMSS(__lmst);
        }


        Dictionary<string, double> sunCoor;
        private void ComputeSunCore()
        {
            lock (oLock)
            {
                if (sunCoor != null)
                    return;
                sunCoor = SunPosition(__TDT, __lat, __lmst * 15.0 * DEG);   // Calculate data for the Sun at given time

                this._SunLon = round1000(sunCoor["lon"] * RAD);
                this._SunRA = HHMM(sunCoor["ra"] * RAD / 15);
                this._SunDec = round1000(sunCoor["dec"] * RAD);
                this._SunAz = round100(sunCoor["az"] * RAD);
                this._SunAlt = round10(sunCoor["alt"] * RAD + Refraction(sunCoor["alt"]));  // including refraction

                this._SunSign = Sign(sunCoor["lon"]);
                this._SunDiameter = round100(sunCoor["diameter"] * RAD * 60.0); // angular diameter in arc seconds
                this._SunDistanceCenter = round10(sunCoor["distance"]);
            }
        }

        Dictionary<string, double> observerCart;
        private void ComputeSunDistanceObserver()
        {
            if (sunCoor == null)
                ComputeSunCore();

            // Calculate distance from the observer (on the surface of earth) to the center of the sun
            Dictionary<string, double> sunCart = EquPolar2Cart(sunCoor["ra"], sunCoor["dec"], sunCoor["distance"]);
            observerCart = Observer2EquCart(__lon, __lat, __height, __gmst); // geocentric cartesian coordinates of observer

            this._SunDistanceObserver = round10(Math.Sqrt(sqr(sunCart["x"] - observerCart["x"]) + sqr(sunCart["y"] - observerCart["y"]) + sqr(sunCart["z"] - observerCart["z"])));
        }

        private void ComputeSunRise() {
            // JD0: JD of 0h UTC time
            lock (oLock)
            {
                if (_SunTransit != null)
                    return;
                Dictionary<string, double> sunRise = CalcSunRise(__JD0, this._DeltaT, __lon, __lat, this._Zone, false);

                this._SunTransit = HHMM(sunRise["transit"]);
                this._SunRise = HHMM(sunRise["rise"]);
                this._SunSet = HHMM(sunRise["set"]);

                this._SunCivilTwilightMorning = HHMM(sunRise["cicilTwilightMorning"]);
                this._SunCivilTwilightEvening = HHMM(sunRise["cicilTwilightEvening"]);
                this._SunNauticalTwilightMorning = HHMM(sunRise["nauticalTwilightMorning"]);
                this._SunNauticalTwilightEvening = HHMM(sunRise["nauticalTwilightEvening"]);
                this._SunAstronomicalTwilightMorning = HHMM(sunRise["astronomicalTwilightMorning"]);
                this._SunAstronomicalTwilightEvening = HHMM(sunRise["astronomicalTwilightEvening"]);
            }
        }

        private void ComputeMoonCore()
        {
            if (sunCoor == null)
                ComputeSunCore();
            if (observerCart == null)
                observerCart = Observer2EquCart(__lon, __lat, __height, __gmst); // geocentric cartesian coordinates of observer
            lock (oLock)
            {
                if (_MoonLon != null)
                    return;
                Dictionary<string, double> moonCoor = MoonPosition(sunCoor, __TDT, observerCart, __lmst * 15.0 * DEG);    // Calculate data for the Moon at given time

                this._MoonLon = round1000(moonCoor["lon"] * RAD);
                this._MoonLat = round1000(moonCoor["lat"] * RAD);
                this._MoonRA = HHMM(moonCoor["ra"] * RAD / 15.0);
                this._MoonDec = round1000(moonCoor["dec"] * RAD);
                this._MoonAz = round100(moonCoor["az"] * RAD);
                this._MoonAlt = round10(moonCoor["alt"] * RAD + Refraction(moonCoor["alt"]));  // including refraction
                this._MoonAge = round1000(moonCoor["moonAge"] * RAD);
                this._MoonPhaseNumber = round1000(moonCoor["phase"]);
                int phase = (int)moonCoor["moonPhase"];
                if (phase == 8) phase = 0;
                this._MoonPhase = (LUNARPHASE)phase;

                this._MoonSign = Sign(moonCoor["lon"]);
                this._MoonDistanceCenter = round10(moonCoor["distance"]);
                this._MoonDiameter = round100(moonCoor["diameter"] * RAD * 60.0); // angular diameter in arc seconds

                // Calculate distance from the observer (on the surface of earth) to the center of the moon
                Dictionary<string, double> moonCart = EquPolar2Cart(moonCoor["raGeocentric"], moonCoor["decGeocentric"], moonCoor["distance"]);
                this._MoonDistanceObserver = round10(Math.Sqrt(sqr(moonCart["x"] - observerCart["x"]) + sqr(moonCart["y"] - observerCart["y"]) + sqr(moonCart["z"] - observerCart["z"])));
            }
        }

        private void ComputeMoonRise()
        {
            lock (oLock)
            {
                if (_MoonTransit != null)
                    return;
                Dictionary<string, double> moonRise = CalcMoonRise(__JD0, this._DeltaT, __lon, __lat, this._Zone, false);

                this._MoonTransit = HHMM(moonRise["transit"]);
                this._MoonRise = HHMM(moonRise["rise"]);
                this._MoonSet = HHMM(moonRise["set"]);
            }
        }
#endregion

#region Konstruktoren
        public SunMoon()
        {
            this._Lat = 50.0;
            this._Lon = 10.0;
            this._Datum = DateTime.Now;
            this._DeltaT = 65.0;
            this._Zone = (DateTime.Now - DateTime.UtcNow).Hours;
            Init();
        }

        public SunMoon(double latitude, double longitude, DateTime datetime, int? zone = null, LocationTimeHolder lth = null, double deltaT = 65)
        {
            this._Lat = latitude;
            this._Lon = longitude;
            this._Datum = datetime;
            this._lth = lth;
            this._DeltaT = deltaT;
            if (zone.HasValue)
                this._Zone = zone.Value;
            else
                this._Zone = (datetime - datetime.ToUniversalTime()).Hours;
            Init();
        }
#endregion
    }
}