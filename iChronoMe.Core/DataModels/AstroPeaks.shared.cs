using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using iChronoMe.Core.DynamicCalendar;

namespace iChronoMe.Core.Classes
{
    public enum Direction
    {
        [XmlEnum("-1")]
        Backward = -1,
        [XmlEnum("1")]
        Forward = 1
    }

    public enum AstroPointType
    {
        [XmlEnum("UPK")]
        UpperPeak = 1,
        [XmlEnum("LPK")]
        LowerPeak = -1,
        [XmlEnum("DP")]
        DestinationPoint = 0
    }

    [XmlType("string")]
    public enum OrbMember
    {
        DistanceCenter, //Distanz zum Erdmittelpunkt
        DistanceObserver, //Distanz zum Beobachter
        Lon, // = "Eklipt.Länge",
        Lat, //Eklipt. Breite des Mondes
        RA, // Text = "Rektaszension 
        Dec, //Deklination 
        Az, //"Azimut
        Alt, //Höhe der Sonne über Horizont
        Diameter, //Durchmesser 
        AstronomicalTwilightMorning, //" Text = "Astronomische Morgendämmerung
        NauticalTwilightMorning, //"Nautische Morgendämmerung
        CivilTwilightMorning, //"Bürgerliche Morgendämmerung
        Rise, //"Sonnenaufgang
        Transit, //"Sonnenkulmination
        Set, //"Sonnenuntergang
        CivilTwilightEvening, //"Bürgerliche Abenddämmerung
        NauticalTwilightEvening, //"Nautische Abenddämmerung
        AstronomicalTwilightEvening, //"Astronomische Abenddämmerung
        PhaseNumber, //Mondphase
        Age //Mondalter
    }

    public static class AstroPeaks
    {
        static mySQLiteConnection _dbAstroCache = null;

        public static mySQLiteConnection xDbAstroCache
        {
            get
            {
                if (_dbAstroCache == null)
                {
                    _dbAstroCache = db.dbAreaCache;
                    var data = _dbAstroCache.Query<AstroCyleLegthCache>("select * from AstroCyleLegthCache", new object[0]);
                    foreach (var l in data)
                    {
                        try
                        {
                            if (!_maxCycleLengthS.ContainsKey(l.OrbAndMember))
                                _maxCycleLengthS.Add(l.OrbAndMember, l.MaxLength);
                        }
                        catch { }
                    }
                }
                return _dbAstroCache;
            }
        }

        private static Dictionary<string, TimeSpan> _maxCycleLengthS = new Dictionary<string, TimeSpan>();

        public static TimeSpan GetMaxCycleLength(string OrbName, string OrbMember, int iTryCount = 4, DateTime? tFirstStart = null)
        {
            string cId = OrbName + OrbMember;
            if (!_maxCycleLengthS.ContainsKey(cId))
            {
                DateTime tStart = tFirstStart == null ? DateTime.Now : tFirstStart.Value;
                var xMax = FindPeakAfter(tStart, 1, cId).DateTime - tStart;
                tStart += xMax;
                for (int i = 0; i < 4; i++)
                {
                    var xNext = FindPeakAfter(tStart.AddDays(1), 1, cId).DateTime - tStart;
                    if (xNext > xMax)
                        xMax = xNext;
                    tStart += xNext;
                }
                _maxCycleLengthS.Add(cId, xMax);
                //DbAstroCache?.Insert(new AstroCyleLegthCache { OrbAndMember = cId, MaxLength = xMax });
            }
            return _maxCycleLengthS[cId];
        }

        public static PeakPoint FindPeakAfter(DateTime tStartPoint, int iDirection, string cMember, int? zone = null, double iJumpSize = 360000, LocationTimeHolder lth = null)
        {
            if (zone == null)
                zone = (DateTime.Now - DateTime.UtcNow).Hours;

            DateTime mNow = tStartPoint;
            var fld = typeof(SunMoon).GetProperty(cMember);
            if (fld != null)
            {

                SunMoon sm = new SunMoon(sys.lastUserLocation.Latitude, sys.lastUserLocation.Longitude, mNow, zone, lth);
                double nMaxVal = getValue(fld.GetValue(sm), cMember) * iDirection;

                var xPoints = new Dictionary<DateTime, double>();
                xPoints.Add(sm.Datum, nMaxVal);

                TimeSpan tsJump = TimeSpan.FromSeconds(iJumpSize);

                //prüfen, ob nicht ein halber Zyklus übersprungen werden sollt..
                var nPrev = getValue(fld.GetValue(new SunMoon(sys.lastUserLocation.Latitude, sys.lastUserLocation.Longitude, mNow - TimeSpan.FromHours(6), zone, lth)), cMember) * iDirection;
                var nNext = getValue(fld.GetValue(new SunMoon(sys.lastUserLocation.Latitude, sys.lastUserLocation.Longitude, mNow + TimeSpan.FromHours(6), zone, lth)), cMember) * iDirection;
                if (nPrev > nMaxVal && nNext < nMaxVal)
                {
                    var next = FindPeakAfter(tStartPoint.AddHours(12), iDirection * -1, cMember, zone, iJumpSize, lth);
                    tsJump = next.DateTime-tStartPoint;
                    nMaxVal = getValue(fld.GetValue(new SunMoon(sys.lastUserLocation.Latitude, sys.lastUserLocation.Longitude, mNow + tsJump, zone, lth)), cMember) * iDirection;
                    xPoints.Clear();
                    xPoints.Add(mNow + tsJump, nMaxVal);
                }

                int iTry = 0;
                while (iTry < 1000)
                {
                    iTry++;
                    sm = new SunMoon(sys.lastUserLocation.Latitude, sys.lastUserLocation.Longitude, mNow + tsJump, zone, lth);
                    double nVal = getValue(fld.GetValue(sm), cMember) * iDirection;

                    if (nVal > nMaxVal)
                        nMaxVal = nVal;

                    if (nVal < nMaxVal)
                    {
                        tsJump -= TimeSpan.FromSeconds(iJumpSize);
                        if (iJumpSize > 0.1)
                        {
                            //two step back and continue smaler
                            tsJump -= TimeSpan.FromSeconds(iJumpSize);

                            xPoints.Add(sm.Datum.AddMilliseconds(iTry), nVal);

                            nMaxVal = getValue(fld.GetValue(new SunMoon(sys.lastUserLocation.Latitude, sys.lastUserLocation.Longitude, mNow + tsJump, zone, lth)), cMember) * iDirection;
                            xPoints.Add((mNow + tsJump).AddMilliseconds(iTry + 1), getValue(fld.GetValue(new SunMoon(sys.lastUserLocation.Latitude, sys.lastUserLocation.Longitude, mNow + tsJump, zone, lth)), cMember) * iDirection);

                            iJumpSize = iJumpSize / 10;

                        }
                        else
                        {
                            sm = new SunMoon(sys.lastUserLocation.Latitude, sys.lastUserLocation.Longitude, mNow + tsJump, zone, lth);
                            break;
                        }
                    }
                    tsJump += TimeSpan.FromSeconds(iJumpSize);
                }

                string c = "";
                foreach (var x in xPoints)
                    c += x.Key + "    " + x.Value + "\n";

                c.ToString();

                double nPeakVal = getValue(fld.GetValue(sm), cMember);
                DateTime tPeakTime = sm.Datum;

                var peak = FindPeakCenter(tPeakTime, nPeakVal, cMember, zone, lth);
                tPeakTime = peak.Item1;

                string cEinheit = GetEinheit(cMember);
                string cPeakVal = nPeakVal.ToString();
                if ("h".Equals(cEinheit))
                    cPeakVal = GetTime(nPeakVal);
                else if ("°".Equals(cEinheit))
                    cPeakVal = GetGrad(nPeakVal);
                else if ("km".Equals(cEinheit))
                    cPeakVal = GetKm(nPeakVal);

                return new PeakPoint() { DateTime = tPeakTime, Value = nPeakVal, JumpCount = iTry, JumpSize = TimeSpan.FromSeconds(iJumpSize), PeakStart = peak.Item2, PeakEnd = peak.Item3 };
            }
            return null;
        }

        public static PeakPoint FindPeakBefore(DateTime tStartPoint, int iDirection, string cMember, int? zone = null, double iJumpSize = 360000, LocationTimeHolder lth = null)
        {
            if (zone == null)
                zone = (DateTime.Now - DateTime.UtcNow).Hours;
            DateTime mNow = tStartPoint;
            var fld = typeof(SunMoon).GetProperty(cMember);
            if (fld != null)
            {

                SunMoon sm = new SunMoon(sys.lastUserLocation.Latitude, sys.lastUserLocation.Longitude, mNow, zone, lth);
                double nMaxVal = getValue(fld.GetValue(sm), cMember) * iDirection;

                var xPoints = new Dictionary<DateTime, double>();
                xPoints.Add(sm.Datum, nMaxVal);

                TimeSpan tsJump = TimeSpan.FromSeconds(iJumpSize);

                //prüfen, ob nicht ein halber Zyklus übersprungen werden sollt..

                var nPrev = getValue(fld.GetValue(new SunMoon(sys.lastUserLocation.Latitude, sys.lastUserLocation.Longitude, mNow - TimeSpan.FromHours(6), zone, lth)), cMember) * iDirection;
                var nNext = getValue(fld.GetValue(new SunMoon(sys.lastUserLocation.Latitude, sys.lastUserLocation.Longitude, mNow + TimeSpan.FromHours(6), zone, lth)), cMember) * iDirection;
                if (nPrev > nMaxVal && nNext < nMaxVal)
                {
                    var next = FindPeakBefore(tStartPoint.AddHours(-12), iDirection * -1, cMember, zone, iJumpSize, lth);
                    tsJump = next.DateTime - tStartPoint;
                    nMaxVal = getValue(fld.GetValue(new SunMoon(sys.lastUserLocation.Latitude, sys.lastUserLocation.Longitude, mNow + tsJump, zone, lth)), cMember) * iDirection;
                    xPoints.Clear();
                    xPoints.Add(mNow + tsJump, nMaxVal);
                }

                int iTry = 0;
                while (iTry < 1000)
                {
                    iTry++;
                    sm = new SunMoon(sys.lastUserLocation.Latitude, sys.lastUserLocation.Longitude, mNow - tsJump, zone, lth);
                    double nVal = getValue(fld.GetValue(sm), cMember) * iDirection;

                    if (nVal > nMaxVal)
                        nMaxVal = nVal;

                    if (nVal < nMaxVal)
                    {
                        tsJump -= TimeSpan.FromSeconds(iJumpSize);
                        if (iJumpSize > 0.1)
                        {
                            //two step back and continue smaler
                            tsJump -= TimeSpan.FromSeconds(iJumpSize);

                            xPoints.Add(sm.Datum.AddMilliseconds(iTry), nVal);

                            nMaxVal = getValue(fld.GetValue(new SunMoon(sys.lastUserLocation.Latitude, sys.lastUserLocation.Longitude, mNow - tsJump, zone, lth)), cMember) * iDirection;
                            xPoints.Add((mNow + tsJump).AddMilliseconds(iTry + 1), getValue(fld.GetValue(new SunMoon(sys.lastUserLocation.Latitude, sys.lastUserLocation.Longitude, mNow - tsJump, zone, lth)), cMember) * iDirection);

                            iJumpSize = iJumpSize / 10;

                        }
                        else
                        {
                            sm = new SunMoon(sys.lastUserLocation.Latitude, sys.lastUserLocation.Longitude, mNow - tsJump, zone, lth);
                            break;
                        }
                    }
                    tsJump += TimeSpan.FromSeconds(iJumpSize);
                }

                string c = "";
                foreach (var x in xPoints)
                    c += x.Key + "    " + x.Value + "\n";

                c.ToString();

                double nPeakVal = getValue(fld.GetValue(sm), cMember);
                DateTime tPeakTime = sm.Datum;

                var peak = FindPeakCenter(tPeakTime, nPeakVal, cMember, zone, lth);
                tPeakTime = peak.Item1;

                string cEinheit = GetEinheit(cMember);
                string cPeakVal = nPeakVal.ToString();
                if ("h".Equals(cEinheit))
                    cPeakVal = GetTime(nPeakVal);
                else if ("°".Equals(cEinheit))
                    cPeakVal = GetGrad(nPeakVal);
                else if ("km".Equals(cEinheit))
                    cPeakVal = GetKm(nPeakVal);

                return new PeakPoint() { DateTime = tPeakTime, Value = nPeakVal, JumpCount = iTry, JumpSize = TimeSpan.FromSeconds(iJumpSize), PeakStart = peak.Item2, PeakEnd = peak.Item3 };

            }
            return null;
        }

        public static PeakPoint FindPeakAt(double nPeakDest, DateTime tStartPoint, string cMember, int? zone = null, double iJumpSize = 360000, LocationTimeHolder lth = null)
        {
            if (zone == null)
                zone = (DateTime.Now - DateTime.UtcNow).Hours;

            DateTime mNow = tStartPoint;
            var fld = typeof(SunMoon).GetProperty(cMember);
            if (fld != null)
            {
                SunMoon sm = new SunMoon(sys.lastUserLocation.Latitude, sys.lastUserLocation.Longitude, mNow, zone, lth);
                double nLastDist = double.MaxValue;
                TimeSpan tsJump = TimeSpan.FromSeconds(0);

                var xPoints = new Dictionary<DateTime, double>();
                xPoints.Add(sm.Datum, getValue(fld.GetValue(sm), cMember));

                int iTry = 0;
                while (iTry < 1000)
                {
                    iTry++;

                    if (iTry > 100)
                        iTry.ToString();

                    sm = new SunMoon(sys.lastUserLocation.Latitude, sys.lastUserLocation.Longitude, mNow + tsJump, zone, lth);
                    double nVal = getValue(fld.GetValue(sm), cMember);
                    double nDist = nPeakDest - nVal;
                    if (nDist < 0)
                        nDist = nDist * -1;
                    if (nDist > nLastDist) //wenn am PeakVorbei
                    {
                        xPoints.Add(sm.Datum.AddMilliseconds(iTry), nVal);
                        nLastDist = double.MaxValue;

                        //two step back and continue smaler
                        tsJump -= TimeSpan.FromSeconds(iJumpSize * 2);
                        iJumpSize = iJumpSize / 10;

                    } else
                        nLastDist = nDist;
                    tsJump += TimeSpan.FromSeconds(iJumpSize);
                    if (iJumpSize < 0.01)
                    {
                        sm = new SunMoon(sys.lastUserLocation.Latitude, sys.lastUserLocation.Longitude, mNow + tsJump, zone, lth);
                        iJumpSize = iJumpSize * 10; //für die Anzeige
                        break;
                    }
                }

                string c = "";
                foreach (var x in xPoints)
                    c += x.Key + "    " + x.Value + "\n";

                c.ToString();

                double nPeakVal = getValue(fld.GetValue(sm), cMember);

                var peak = FindPeakCenter(sm.Datum, nPeakVal, cMember, zone, lth);
                var tPeakTime = peak.Item1;

                string cEinheit = GetEinheit(cMember);
                string cPeakVal = nPeakVal.ToString();
                if ("h".Equals(cEinheit))
                    cPeakVal = GetTime(nPeakVal);
                else if ("°".Equals(cEinheit))
                    cPeakVal = GetGrad(nPeakVal);
                else if ("km".Equals(cEinheit))
                    cPeakVal = GetKm(nPeakVal);

                return new PeakPoint() { DateTime = tPeakTime, Value = nPeakVal, JumpCount = iTry, JumpSize = TimeSpan.FromSeconds(iJumpSize), PeakStart = peak.Item2, PeakEnd = peak.Item3 };

            }
            return null;
        }

        private static (DateTime peak, DateTime min, DateTime max) FindPeakCenter(DateTime tPeakTime, double nPeakVal, string cMember, int? zone, LocationTimeHolder lth = null)
        {
            var fld = typeof(SunMoon).GetProperty(cMember);

            DateTime tStartPeakTime = DateTime.MaxValue;
            int xTry = 0;
            int iJumpSize = 3600;
            TimeSpan tsJump = TimeSpan.FromSeconds(iJumpSize);
            while (xTry < 1000)
            {
                xTry++;
                var sm = new SunMoon(sys.lastUserLocation.Latitude, sys.lastUserLocation.Longitude, tPeakTime - tsJump, zone, lth);
                double nVal = getValue(fld.GetValue(sm), cMember);
                if (nVal == nPeakVal)
                {
                    tsJump += TimeSpan.FromSeconds(iJumpSize);
                    if (sm.Datum < tStartPeakTime)
                        tStartPeakTime = sm.Datum;
                }
                else
                {
                    tsJump -= TimeSpan.FromSeconds(iJumpSize);
                    iJumpSize = iJumpSize / 10;
                    tsJump += TimeSpan.FromSeconds(iJumpSize);
                }
                if (iJumpSize < 0.01)
                    break;
            }

            DateTime tEndPeakTime = DateTime.MinValue;
            xTry = 0;
            iJumpSize = 3600;
            tsJump = TimeSpan.FromSeconds(iJumpSize);
            while (xTry < 1000)
            {
                xTry++;
                var sm = new SunMoon(sys.lastUserLocation.Latitude, sys.lastUserLocation.Longitude, tPeakTime + tsJump, zone, lth);
                double nVal = getValue(fld.GetValue(sm), cMember);
                if (nVal == nPeakVal)
                {
                    tsJump += TimeSpan.FromSeconds(iJumpSize);
                    if (sm.Datum > tStartPeakTime)
                        tEndPeakTime = sm.Datum;
                }
                else
                {
                    tsJump -= TimeSpan.FromSeconds(iJumpSize);
                    iJumpSize = iJumpSize / 10;
                    tsJump += TimeSpan.FromSeconds(iJumpSize);
                }
                if (iJumpSize < 0.01)
                    break;
            }

            if (tStartPeakTime == DateTime.MaxValue)
                tStartPeakTime = tPeakTime;
            if (tEndPeakTime == DateTime.MinValue)
                tEndPeakTime = tPeakTime;

            if (tStartPeakTime != tPeakTime || tEndPeakTime != tPeakTime)
            {
                tPeakTime = tStartPeakTime.AddMilliseconds((tEndPeakTime - tStartPeakTime).TotalMilliseconds / 2);
            }

            return (tPeakTime, tStartPeakTime, tEndPeakTime);
        }

        public static double getValue(object oVal, string cMember)
        {
            try
            {
                double nVal = 0;
                if (oVal is double)
                    nVal = (double)oVal;
                else if (oVal is TimeSpan)
                {
                    nVal = ((TimeSpan)oVal).TotalHours;
                    if (cMember.EndsWith("TwilightEvening") && nVal < 4)
                        nVal += 24;
                    if (cMember.EndsWith("TwilightMorning") && nVal > 20)
                        nVal -= 24;
                }
                return nVal;
            }
            catch (Exception ex)
            {
                ex.ToString();
                return 0;
            }
        }

        public static string GetTime(double hour)
        {
            return GetTime(hour, false);
        }
        public static string GetTime(double hour, bool bShowSeconds)
        {
            var ts = TimeSpan.FromHours(hour);
            return ts.TotalHours.ToString("N0") + ":" + ts.Minutes.ToString("00") + (bShowSeconds ? ":" + ts.Seconds.ToString("00") : "");
        }
        public static string GetKm(double km)
        {
            return GetKm(km, true);
        }
        public static string GetKm(double km, bool bX)
        {
            string cEh = "km";
            if (bX)
            {
                if (km > 1000)
                {
                    km = km / 1000;
                    cEh = " t.km";
                }
                if (km > 1000)
                {
                    km = km / 1000;
                    cEh = " mio.km";
                }
            }
            return km.ToString("F0") + cEh;
        }
        public static string GetGrad(double grad)
        {
            return grad.ToString() + "°";
        }

        public static string GetEinheit(string cMember)
        {
            switch (cMember)
            {
                case "SunDistanceCenter":
                case "SunDistanceObserver":
                case "MoonDistanceCenter":
                case "MoonDistanceObserver":
                    return "km";

                case "SunLon":
                case "SunDec":
                case "SunAz":
                case "SunAlt":
                case "MoonLon":
                case "MoonLat":
                case "MoonDec":
                case "MoonAz":
                case "MoonAlt":
                    return "°";

                case "SunDiameter":
                case "MoonDiameter":
                    return "'";

                case "SunRA":
                case "SunAstronomicalTwilightMorning":
                case "SunNauticalTwilightMorning":
                case "SunCivilTwilightMorning":
                case "SunRise":
                case "SunTransit":
                case "LocWoz12":
                case "LocWoz12Old":
                case "LocWoz12DST":
                case "SunSet":
                case "SunCivilTwilightEvening":
                case "SunNauticalTwilightEvening":
                case "SunAstronomicalTwilightEvening":
                case "MoonRA":
                case "MoonRise":
                case "MoonTransit":
                case "MoonSet":
                    return "h";

                case "MoonPhaseNumber":
                    break;

                case "MoonAge":
                    break;
            }
            return "";
        }

        public static string FormatMemberValue(string cMember, double nValue)
        {
            string cEinheit = GetEinheit(cMember);
            if ("h".Equals(cEinheit))
                return GetTime(nValue, true);
            else if ("°".Equals(cEinheit))
                return GetGrad(nValue);
            else if ("km".Equals(cEinheit))
                return GetKm(nValue, false);
            return nValue.ToString();
        }

        static Dictionary<string, TimeOffset> _timeOffsetSamplesYearCycle;
        public static Dictionary<string, TimeOffset> xTimeOffsetSamplesYearCycle
        {
            get
            {
                if (_timeOffsetSamplesYearCycle == null)
                {
                    _timeOffsetSamplesYearCycle = new Dictionary<string, TimeOffset>();
                    _timeOffsetSamplesYearCycle.Add("gerg. Neujahr", new TimeOffsetGregorianDate { Month = 1, Day = 1 });
                    _timeOffsetSamplesYearCycle.Add("Frühlingsanfang", new TimeOffsetAstronomic { OrbName = "Sun", OrbMember = OrbMember.RA, PointType = AstroPointType.LowerPeak });
                }
                return _timeOffsetSamplesYearCycle;
            }
        }

        static Dictionary<string, TimeOffset> _timeOffsetSamplesMoonCycle;
        public static Dictionary<string, TimeOffset> TimeOffsetSamplesMoonCycle
        {
            get
            {
                if (_timeOffsetSamplesMoonCycle == null)
                {
                    _timeOffsetSamplesMoonCycle = new Dictionary<string, TimeOffset>();
                    _timeOffsetSamplesMoonCycle.Add("Vollmond", new TimeOffsetAstronomic { OrbName = "Moon", OrbMember = OrbMember.PhaseNumber, PointType = AstroPointType.UpperPeak });
                    _timeOffsetSamplesMoonCycle.Add("Neumond", new TimeOffsetAstronomic { OrbName = "Moon", OrbMember = OrbMember.PhaseNumber, PointType = AstroPointType.LowerPeak });
                }
                return _timeOffsetSamplesMoonCycle;
            }
        }


        static Dictionary<string, TimeOffsetAstronomic> _timeOffsetSamples;
        public static Dictionary<string, TimeOffsetAstronomic> TimeOffsetSamples
        {
            get
            {
                if (_timeOffsetSamples == null)
                {
                    _timeOffsetSamples = new Dictionary<string, TimeOffsetAstronomic>();
                    _timeOffsetSamples.Add("Vollmond", new TimeOffsetAstronomic { OrbName = "Moon", OrbMember = OrbMember.PhaseNumber, PointType = AstroPointType.UpperPeak });
                    _timeOffsetSamples.Add("Frühlingsanfang", new TimeOffsetAstronomic { OrbName = "Sun", OrbMember = OrbMember.RA, PointType = AstroPointType.LowerPeak });
                }
                return _timeOffsetSamples;
            }
        }
    }

    public class PeakPoint
    {
        public DateTime DateTime { get; set; }
        public DateTime PeakStart { get; set; }
        public DateTime PeakEnd { get; set; }
        public double Value { get; set; }
        public int JumpCount { get; set; }
        public TimeSpan JumpSize { get; set; }
        public int JumpDirection { get; set; }
    }

    public class AstroCyleLegthCache : dbObject
    {
        [SQLite.Indexed]
        public string OrbAndMember { get; set; }

        public TimeSpan MaxLength { get; set; }
    }

    public class AstroPeakPointCache : dbObject
    {
        [SQLite.Indexed]
        public string PeakID { get; set; }
        public string Orb { get; set; }
        public string Member { get; set; }
        public int PeakType { get; set; }
        public double PeakDest { get; set; }
        [SQLite.Indexed]
        public DateTime DateTime { get; set; }
        public DateTime PeakStart { get; set; }
        public DateTime PeakEnd { get; set; }
        public double PeakValue { get; set; }
    }
}
