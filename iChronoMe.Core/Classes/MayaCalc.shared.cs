using System;
using System.Collections.Generic;
using System.Text;

namespace iChronoMe.Core.Classes
{
    public static class MayaCalc
    {
        public const MayaCorrelation DefaultMayaCorrelation = MayaCorrelation.Lounsbury;

        public static DateTime Maya2Gregorian(int baktun, int katun, int tun, int uinal, int kin, MayaCorrelation MayaCorrelation = MayaCorrelation.Default)
            => Maya2Gregorian((baktun * 144000) + (katun * 7200) + (tun * 360) + (uinal * 20) + kin, MayaCorrelation);

        public static DateTime Maya2Gregorian(int MD, MayaCorrelation MayaCorrelation = MayaCorrelation.Default)
        {
            if (MayaCorrelation == MayaCorrelation.Default)
                MayaCorrelation = DefaultMayaCorrelation;
            // 1 step: Get MayaCorrelation to use
            //          MayaCorrelation is Julian Days date for Maya calender start 0.0.0.0.0
            // 2 step:  Add MayaCorrelation to maya_days
            int julian_day = MD + (int)MayaCorrelation;
            // 3 step:  Convert JUD to Gregorian date
            int year; int month; int day;
            Julian2Gregorian(julian_day, out year, out month, out day);
            return new DateTime(year, month, day);
        }


        public static void Julian2Gregorian(int JD, out int Y, out int M, out int D)
        {
            /*---------------------------------------------------------------------------------
             * Convert a Julian Day (JD) to Gregorian calendare
             * Information from Wikipedia http://en.wikipedia.org/wiki/Julian_day
             * algoritm converted to C#
             
             * Let J = JD + 0.5: (note: this shifts the epoch back by one half day, 
             *                      to start it at 00:00UTC, instead of 12:00 UTC);            
             * let j = J + 32044; (note: this shifts the epoch back to astronomical year -4800 
             *                      instead of the start of the Christian era in year AD 1 of 
             *                      the proleptic Gregorian calendar).
             * let g = j div 146097; let dg = j mod 146097;
             * let c = (dg div 36524 + 1) × 3 div 4; let dc = dg − c × 36524;
             * let b = dc div 1461; let db = dc mod 1461;
             * let a = (db div 365 + 1) × 3 div 4; let da = db − a × 365;
             * let y = g × 400 + c × 100 + b × 4 + a; (note: this is the integer number of full 
             *                      years elapsed since March 1, 4801 BC at 00:00 UTC);
             * let m = (da × 5 + 308) div 153 − 2; (note: this is the integer number of full 
             *                      months elapsed since the last March 1 at 00:00 UTC);
             * let d = da − (m + 4) × 153 div 5 + 122; (note: this is the number of days elapsed 
             *                      since day 1 of the month at 00:00 UTC, including fractions of one day);
        
             * let Y = y − 4800 + (m + 2) div 12; let M = (m + 2) mod 12 + 1; let D = d + 1;
             ------------------------------------------------------------------------------------------------------------------------------------------------------*/

            int j, g, c, b, a, y, m, d;
            int dg, dc, db, da;

            j = (int)JD + 32044;
            g = Math.DivRem(j, 146097, out dg);
            c = (dg / 36524 + 1) * 3 / 4;
            dc = dg - (c * 36524);
            b = Math.DivRem(dc, 1461, out db);
            a = (db / 365 + 1) * 3 / 4;
            da = db - (a * 365);
            y = g * 400 + c * 100 + b * 4 + a;
            m = (da * 5 + 308) / 153 - 2;
            d = da - (m + 4) * 153 / 5 + 122;
            Y = y - 4800 + Math.DivRem((m + 2), 12, out M);
            M += 1;
            D = d + 1;
        }

        public static void JulianDay2JuliaDate(int JD, out int Y, out int M, out int D)
        {
            // Own version to convert Julian Day to date in Julian calender.
            // Code is very similar to above version of Julian Day2Gregorian(..)
            // Main difference between Gregorian and Julian is that Julian doesn´t
            //   take care round of 400 years so calculation of variables "g" &  "gd"
            //   has been omited.
            int j, c, dc, b, db, a, da, y, m, d;
            j = JD - 59;  // shift to March 1 -4713;
            c = Math.DivRem(j, 36525, out dc);  // 365525 = 100 years
            b = Math.DivRem(dc, 1461, out db);  // 1461 = 4 year
            a = (db / 365 + 1) * 3 / 4;
            da = db - (a * 365);
            y = c * 100 + b * 4 + a;
            m = (da * 5 + 308) / 153 - 2;
            d = da - (m + 4) * 153 / 5 + 122;
            Y = y - 4712 + Math.DivRem((m + 2), 12, out M);
            M += 1;
            D = d;
        }

        public static int Gregorian2JulianDay(DateTime date)
            => Gregorian2JulianDay(date.Year, date.Month, date.Day);

        public static int Gregorian2JulianDay(int year, int month, int day)
        {
            /*-------------------------------------------------------------------------
            
             * wWikipedia http://en.wikipedia.org/wiki/Julian_day
             * Converting Gregorian calendar date to Julian Day Number
             * The algorithm is valid for all Gregorian calendar dates after 4800 BC.[14]
             * You must compute first:
             *    a = (14-mont)/12
             *    y = year + 4800 - a
             *    m = month + 12a -3
             * then compute:
             *              153m + 2          y    y     y
             * JDN = day + --------- + 365y + - - --- + --- - 32045
             *                 5              4   100   400
             * J\!D\!N = \text{day} + \frac{153m+2}{5}+ 365y+ \frac{y}{4} - \frac{y}{100} + \frac{y}{400} - 32045

            -------------------------------------------------------------------------------*/

            int a = (14 - month) / 12;
            int y = year + 4800 - a;
            int m = month + (12 * a) - 3;
            int JDN = day + (((153 * m) + 2) / 5) + (365 * y) + (y / 4) - (y / 100) + (y / 400) - 32045;
            return JDN;

        }

        public static string Gregorian2Maya(DateTime date, MayaCorrelation MayaCorrelation = MayaCorrelation.Default)
            => Gregorian2Maya(Gregorian2JulianDay(date), MayaCorrelation);
        public static string Gregorian2Maya(int julian_day, MayaCorrelation MayaCorrelation = MayaCorrelation.Default)
        {
            if (MayaCorrelation == MayaCorrelation.Default)
                MayaCorrelation = DefaultMayaCorrelation;
            // step 1:  julian_day - MayaCorrelation
            if (julian_day < (int)MayaCorrelation)
            {
                return "Julian-Day lest of MayaCorrelation";
            }
            var md = Gregorian2MayaDate(julian_day, MayaCorrelation) ;

            return string.Format("{0:d}.{1:D}.{2:D}.{3:D}.{4:D}", md.baktun, md.katun, md.tun, md.uinal, md.kin);
        }
        public static (int baktun, int katun, int tun, int uinal, int kin) Gregorian2MayaDate(DateTime date, MayaCorrelation MayaCorrelation = MayaCorrelation.Default)
            => Gregorian2MayaDate(Gregorian2JulianDay(date), MayaCorrelation);

        public static (int baktun, int katun, int tun, int uinal, int kin) Gregorian2MayaDate(int julian_day, MayaCorrelation MayaCorrelation = MayaCorrelation.Default)
        {
            if (MayaCorrelation == MayaCorrelation.Default)
                MayaCorrelation = DefaultMayaCorrelation;
            // step 1:  julian_day - MayaCorrelation
            if (julian_day < (int)MayaCorrelation)
            {
                return (-1, -1, -1, -1, -1);
            }
            int maya_day = julian_day - (int)MayaCorrelation;
            // step 2:  Find number of baktun
            int baktun, baktun_remain;
            baktun = Math.DivRem(maya_day, 144000, out baktun_remain);
            // step 3: Find katun
            int katun, katun_remain;
            katun = Math.DivRem(baktun_remain, 7200, out katun_remain);
            // step 4: Find tun
            int tun, tun_remain;
            tun = Math.DivRem(katun_remain, 360, out tun_remain);
            // step 5: Find uinal & kin
            int uinal, kin;
            uinal = Math.DivRem(tun_remain, 20, out kin);

            return (baktun, katun, tun, uinal, kin);
        }

        static string[] Uinal_name = {"Pop", "Uo", "Zip", "Zotz", "Zec",
                                      "Xul", "Yaxkin", "Mol", "Chen", "Yax",
                                      "Zac", "Ceh", "Mac", "Kankin", "Muan",
                                      "Pax", "Kayab", "Cumku", "Wayeb"};

        static string[] Uinal_namex = {"Pop", "Wo", "Sip", "Sotz", "Sek",
                                       "Xul", "Yaxkin", "Mol", "Chen", "Yax",
                                       "Sak", "Keh", "Mak", "Kankin", "Muwan",
                                       "Pax", "Kayab", "Kunku", "Wayev"};

        public static string Gregorian2MayaHaab(DateTime date, MayaCorrelation MayaCorrelation = MayaCorrelation.Default)
            => Gregorian2MayaHaab(Gregorian2JulianDay(date), MayaCorrelation);
        public static string Gregorian2MayaHaab(int julian_day, MayaCorrelation MayaCorrelation = MayaCorrelation.Default)
        {
            if (MayaCorrelation == MayaCorrelation.Default)
                MayaCorrelation = DefaultMayaCorrelation;
            if (julian_day < (int)MayaCorrelation)
            {
                return "Julian-Day lest of MayaCorrelation";
            }
            int myhaab = Gregorian2MayaHaabKin(julian_day, MayaCorrelation);
            int myuinal, mykin;
            myuinal = Math.DivRem(myhaab, 20, out mykin);
            return string.Format("{0:D} {1}", mykin, Uinal_name[myuinal]);
        }

        public static int Gregorian2MayaHaabKin(DateTime date, MayaCorrelation MayaCorrelation = MayaCorrelation.Default)
            => Gregorian2MayaHaabKin(Gregorian2JulianDay(date), MayaCorrelation);
        public static int Gregorian2MayaHaabKin(int julian_day, MayaCorrelation MayaCorrelation = MayaCorrelation.Default)
        {
            if (MayaCorrelation == MayaCorrelation.Default)
                MayaCorrelation = DefaultMayaCorrelation;
            int maya_day = julian_day - (int)MayaCorrelation;
            
            int x, myhaab;
            x = maya_day + 348;     // 0.0.0.0.0 is (8 Cunku = 348 days)
            Math.DivRem(x, 365, out myhaab);   // haab calender of 365 days
            return myhaab;
        }

        static string[] kin_name = { "Imix", "Ik", "Akbal", "Kan", "Chicchán",
                                     "Cimí", "Manik", "Lamat", "Muluk", "Ok",
                                     "Chuwen", "Eb", "Ben", "Ix", "Men",
                                     "Kib", "Kaban", "Etznab", "Kawak", "Ahau" };

        static string[] kin_namex = {"Imix", "Ik", "Akbal", "Kan", "Chikchan",
                                    "Kimi", "Manik", "Lamat", "Maluk", "Ok",
                                    "Chuwen", "Eb", "Ben", "Ix", "Men",
                                    "Kib", "Kaban", "Etxnab", "Kawak", "Ajaw"};

        public static string Gregorian2MayaTzolkin(DateTime date, MayaCorrelation MayaCorrelation = MayaCorrelation.Default)
            => Gregorian2MayaTzolkin(Gregorian2JulianDay(date), MayaCorrelation);
        public static string Gregorian2MayaTzolkin(int julian_day, MayaCorrelation MayaCorrelation = MayaCorrelation.Default)
        {
            if (MayaCorrelation == MayaCorrelation.Default)
                MayaCorrelation = DefaultMayaCorrelation;
            // step 1:  julian_day - MayaCorrelation
            if (julian_day < (int)MayaCorrelation)
            {
                return "Julian-Day lest of MayaCorrelation";
            }
            int maya_day = julian_day - (int)MayaCorrelation;


            int i13, i20, x;
            x = maya_day + 159;  //  0.0.0.0.0 is ( 4 Ahau = 159 days)
            Math.DivRem(x, 13, out i13);
            Math.DivRem(x, 20, out i20);
            i13++;                         // 1 to 13
            return string.Format("{0:D} {1}", i13, kin_name[i20]);
        }
    }
    public class MayaDateInfo
    {
        public MayaDateInfo(DateTime date, MayaCorrelation MayaCorrelation = MayaCorrelation.Default)
        {
            if (MayaCorrelation == MayaCorrelation.Default)
                MayaCorrelation = MayaCalc.DefaultMayaCorrelation;
            int iJd = MayaCalc.Gregorian2JulianDay(date);
            MayaDate = MayaCalc.Gregorian2Maya(iJd, MayaCorrelation);
            HaabDate = MayaCalc.Gregorian2MayaHaab(iJd, MayaCorrelation);
            TzolkinDate = MayaCalc.Gregorian2MayaTzolkin(iJd, MayaCorrelation);
        }

        public string MayaDate { get; }
        public string HaabDate { get; }
        public string TzolkinDate { get; }
    }

    public enum MayaCorrelation
    {
        Default       = -1,
        Thompson      = 584283,
        Lounsbury     = 584285,
        Martin        = 563334,
        Kelley        = 553279,
        Spinden       = 489384,
        Makenson      = 489138,
        Owen          = 487410,
        Smiley        = 482699,
        Bowditch      = 394483,
        Böhm          = 622261,
        Kreichgauer   = 626927,
        EscalonaRamos = 679108,
        Weitzel       = 774078,
        Vaillant      = 774083
    }
}