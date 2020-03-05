using System;
using System.Collections.Generic;
using System.Text;

namespace iChronoMe.Core.Extentions
{
    public static class BoolExtention
    {
        public static int ToInt(this bool value)
        {
            return value ? 1 : 0;
        }
    }
}
