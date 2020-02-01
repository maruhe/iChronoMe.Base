using iChronoMe.Core.Classes;
using iChronoMe.Core.Types;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace iChronoMe.Widgets
{
    public static class xColorExtention
    {
        public static xColor ToXColor(this SKColor color)
        {
            return new xColor(color.Red / 255.0, color.Green / 255.0, color.Blue / 255.0, color.Alpha / 255.0);
        }

        public static SKColor ToSKColor(this xColor color)
        {
            return new SKColor((byte)(color.R * 255), (byte)(color.G * 255), (byte)(color.B * 255), (byte)(color.A * 255));
        }
    }
}