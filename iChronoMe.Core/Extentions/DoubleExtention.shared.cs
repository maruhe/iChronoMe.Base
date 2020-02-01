using System;
using System.Collections.Generic;
using System.Text;

namespace iChronoMe.Core.Extentions
{
    public static class DoubleExtensions
    {
        public static double ToRadians(this double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        static public double ToDegrees(this double radians)
        {
            return radians * (180 / Math.PI);
        }
    }
}
