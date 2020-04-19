using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using iChronoMe.Core.Types.xUnit;

namespace iChronoMe.Core.Types
{
    public static class xUnits
    {
        public static string GetUnitStringClimacell(Enum val)
        {
            switch (val)
            {
                case UnitSystem.Default:
                    return GetUnitStringClimacell(DefaultUnitSystem);
                case UnitSystem.SI:
                    return "si";
                case UnitSystem.US:
                    return "us";

                case Distance.Default:
                    return GetUnitStringClimacell(DefaultDistance);
                case Distance.km:
                    return "km";
                case Distance.mi:
                    return "mi";

                case Temp.Default:
                    return GetUnitStringClimacell(DefaultTemp);
                case Temp.C:
                    return "C";
                case Temp.F:
                    return "F";

                case WindSpeed.Default:
                    return GetUnitStringClimacell(DefaultWindSpeed);
                case WindSpeed.m_s:
                    return "m/s";
                case WindSpeed.km_h:
                    return "kph";
                case WindSpeed.ml_h:
                    return "mph";
                case WindSpeed.knots:
                    return "knots";
                case WindSpeed.beaufort:
                    return "beaufort";

                case BarumPressure.Default:
                    return GetUnitStringClimacell(DefaultBarumPressure);
                case BarumPressure.mmHg:
                    return "mmHg";
                case BarumPressure.hPa:
                    return "hPa";
                case BarumPressure.inHg:
                    return "inHg";

                case Precipitation.Default:
                    return GetUnitStringClimacell(DefaultPrecipitation);
                case Precipitation.mm_hr:
                    return "mm/hr";
                case Precipitation.in_hr:
                    return "in/hr";

                case CloudHeight.Default:
                    return GetUnitStringClimacell(DefaultCloudHeight);
                case CloudHeight.m:
                    return "m";
                case CloudHeight.ft:
                    return "ft";
            }

            if (!val.GetType().Namespace.StartsWith("iChronoMe.Core.Types.xUnit"))
                throw new Exception("bad enum type");

            var x = Enum.GetValues(val.GetType());
            return GetUnitStringClimacell((Enum)x.GetValue(0));
        }

        static xUnits()
        {
            UpdateDefaultUnitSystem();
        }

        public static void UpdateDefaultUnitSystem()
        {
            if (RegionInfo.CurrentRegion.IsMetric)
            {
                DefaultUnitSystem = UnitSystem.SI;
                DefaultDistance = Distance.km;
                DefaultTemp = Temp.C;
                DefaultWindSpeed = WindSpeed.km_h;
                DefaultBarumPressure = BarumPressure.mmHg;
                DefaultPrecipitation = Precipitation.mm_hr;
                DefaultCloudHeight = CloudHeight.m;
            }
            else
            {
                DefaultUnitSystem = UnitSystem.US;
                DefaultDistance = Distance.mi;
                DefaultTemp = Temp.F;
                DefaultWindSpeed = WindSpeed.ml_h;
                DefaultBarumPressure = BarumPressure.inHg;
                DefaultPrecipitation = Precipitation.in_hr;
                DefaultCloudHeight = CloudHeight.ft;
            }
        }

        public static UnitSystem DefaultUnitSystem { get; private set; } = UnitSystem.SI;
        public static Distance DefaultDistance { get; private set; } = Distance.km;
        public static Temp DefaultTemp { get; private set; } = Temp.C;
        public static WindSpeed DefaultWindSpeed { get; private set; } = WindSpeed.km_h;
        public static BarumPressure DefaultBarumPressure { get; private set; } = BarumPressure.mmHg;
        public static Precipitation DefaultPrecipitation { get; private set; } = Precipitation.mm_hr;
        public static CloudHeight DefaultCloudHeight { get; private set; } = CloudHeight.m;
    }
}


namespace iChronoMe.Core.Types.xUnit
{
    public enum UnitSystem
    {
        Default,
        SI,
        US
    }
    public enum Text
    {
        test1,
        test2,
    }
    public enum Distance
    {
        Default, 
        km,
        mi,
        test
    }
    public enum Temp
    {
        Default, 
        C,
        F
    }
    public enum WindSpeed
    {
        Default, 
        m_s,
        km_h,
        ml_h,
        knots,
        beaufort
    }
    public enum BarumPressure
    {
        Default, 
        hPa,
        mmHg,
        inHg
    }
    public enum Precipitation
    {
        Default, 
        mm_hr,
        in_hr
    }
    public enum CloudHeight
    {
        Default, 
        m,
        ft
    }
}
