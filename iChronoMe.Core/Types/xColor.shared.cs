using System;
using System.ComponentModel;
using System.Diagnostics;

/* Unmerged change from project 'iChronoMe.Core (uap10.0.16299)'
Before:
using System.Globalization;
using iChronoMe.Core.Classes;
After:
using System.Globalization;

using iChronoMe.Core.Classes;
*/

/* Unmerged change from project 'iChronoMe.Core (Xamarin.iOS10)'
Before:
using System.Globalization;
using iChronoMe.Core.Classes;
After:
using System.Globalization;

using iChronoMe.Core.Classes;
*/

/* Unmerged change from project 'iChronoMe.Core (MonoAndroid90)'
Before:
using System.Globalization;
using iChronoMe.Core.Classes;
After:
using System.Globalization;

using iChronoMe.Core.Classes;
*/
using System.Globalization;

//Xamarin Forms Color clone

namespace iChronoMe.Core.Types
{
    [DebuggerDisplay("R={R}, G={G}, B={B}, A={A}, Hue={Hue}, Saturation={Saturation}, Luminosity={Luminosity}")]
    //[TypeConverter(typeof(ColorTypeConverter))]
    public partial struct xColor
    {
        #region Extentions to Xmarin Class

        public static implicit operator string(xColor x)
        {
            return x.ToHex();
        }

        public static implicit operator xColor(string c)
        {
            return xColor.FromHex(c);
        }

        public xColor LuminosityDiff(double nDiffPercent)
        {
            try
            {
                return Luminosity < 0.5 ? AddLuminosity(nDiffPercent / 100) : AddLuminosity(nDiffPercent / -100);
            }
            catch (Exception ex)
            {
                try
                {
                    return Luminosity < 0.5 ? AddLuminosity(nDiffPercent / -100) : AddLuminosity(nDiffPercent / 100);
                }
                catch (Exception ex2)
                {
                    return this;
                }
            }
        }

        public xColor InvertLuminosityDiff()
        {
            return new xColor(1 - R, 1 - G, 1 - B, A);
        }

        public xColor GetBlackOrWhite()
        {
            if (Luminosity < 0.7)
                return xColor.White;
            return xColor.Black;
        }
        public xColor GetLightOrDark(xColor light, xColor dark)
        {
            if (Luminosity < 0.7)
                return light;
            return dark;
        }

        #endregion


        readonly Mode _mode;

        enum Mode
        {
            Default,
            Rgb,
            Hsl
        }

        public static xColor Default
        {
            get { return new xColor(-1d, -1d, -1d, -1d, Mode.Default); }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IsDefault
        {
            get { return _mode == Mode.Default; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetAccent(xColor value) => Accent = value;
        public static xColor Accent { get; internal set; }

        readonly float _a;

        public double A
        {
            get { return _a; }
        }

        readonly float _r;

        public double R
        {
            get { return _r; }
        }

        readonly float _g;

        public double G
        {
            get { return _g; }
        }

        readonly float _b;

        public double B
        {
            get { return _b; }
        }

        readonly float _hue;

        public double Hue
        {
            get { return _hue; }
        }

        readonly float _saturation;

        public double Saturation
        {
            get { return _saturation; }
        }

        readonly float _luminosity;

        public double Luminosity
        {
            get { return _luminosity; }
        }

        public xColor(double r, double g, double b, double a) : this(r, g, b, a, Mode.Rgb)
        {
        }

        xColor(double w, double x, double y, double z, Mode mode)
        {
            _mode = mode;
            switch (mode)
            {
                default:
                case Mode.Default:
                    _r = _g = _b = _a = -1;
                    _hue = _saturation = _luminosity = -1;
                    break;
                case Mode.Rgb:
                    _r = (float)w.Clamp(0, 1);
                    _g = (float)x.Clamp(0, 1);
                    _b = (float)y.Clamp(0, 1);
                    _a = (float)z.Clamp(0, 1);
                    ConvertToHsl(_r, _g, _b, mode, out _hue, out _saturation, out _luminosity);
                    break;
                case Mode.Hsl:
                    _hue = (float)w.Clamp(0, 1);
                    _saturation = (float)x.Clamp(0, 1);
                    _luminosity = (float)y.Clamp(0, 1);
                    _a = (float)z.Clamp(0, 1);
                    ConvertToRgb(_hue, _saturation, _luminosity, mode, out _r, out _g, out _b);
                    break;
            }
        }

        public xColor(double r, double g, double b) : this(r, g, b, 1)
        {
        }

        public xColor(double value) : this(value, value, value, 1)
        {
        }

        public xColor MultiplyAlpha(double alpha)
        {
            switch (_mode)
            {
                default:
                case Mode.Default:
                    throw new InvalidOperationException("Invalid on Color.Default");
                case Mode.Rgb:
                    return new xColor(_r, _g, _b, _a * alpha, Mode.Rgb);
                case Mode.Hsl:
                    return new xColor(_hue, _saturation, _luminosity, _a * alpha, Mode.Hsl);
            }
        }

        public xColor AddLuminosity(double delta)
        {
            if (_mode == Mode.Default)
                throw new InvalidOperationException("Invalid on Color.Default");

            return new xColor(_hue, _saturation, _luminosity + delta, _a, Mode.Hsl);
        }

        public xColor WithHue(double hue)
        {
            if (_mode == Mode.Default)
                throw new InvalidOperationException("Invalid on Color.Default");
            return new xColor(hue, _saturation, _luminosity, _a, Mode.Hsl);
        }

        public xColor WithSaturation(double saturation)
        {
            if (_mode == Mode.Default)
                throw new InvalidOperationException("Invalid on Color.Default");
            return new xColor(_hue, saturation, _luminosity, _a, Mode.Hsl);
        }

        public xColor WithLuminosity(double luminosity)
        {
            if (_mode == Mode.Default)
                throw new InvalidOperationException("Invalid on Color.Default");
            return new xColor(_hue, _saturation, luminosity, _a, Mode.Hsl);
        }

        public xColor WithAlpha(int iA)
        {
            return new xColor(_r, _g, _b, (double)iA / 255);
        }
        public xColor WithAlpha(double nA)
        {
            return new xColor(_r, _g, _b, nA);
        }

        static void ConvertToRgb(float hue, float saturation, float luminosity, Mode mode, out float r, out float g, out float b)
        {
            if (mode != Mode.Hsl)
                throw new InvalidOperationException();

            if (luminosity == 0)
            {
                r = g = b = 0;
                return;
            }

            if (saturation == 0)
            {
                r = g = b = luminosity;
                return;
            }
            float temp2 = luminosity <= 0.5f ? luminosity * (1.0f + saturation) : luminosity + saturation - luminosity * saturation;
            float temp1 = 2.0f * luminosity - temp2;

            var t3 = new[] { hue + 1.0f / 3.0f, hue, hue - 1.0f / 3.0f };
            var clr = new float[] { 0, 0, 0 };
            for (var i = 0; i < 3; i++)
            {
                if (t3[i] < 0)
                    t3[i] += 1.0f;
                if (t3[i] > 1)
                    t3[i] -= 1.0f;
                if (6.0 * t3[i] < 1.0)
                    clr[i] = temp1 + (temp2 - temp1) * t3[i] * 6.0f;
                else if (2.0 * t3[i] < 1.0)
                    clr[i] = temp2;
                else if (3.0 * t3[i] < 2.0)
                    clr[i] = temp1 + (temp2 - temp1) * (2.0f / 3.0f - t3[i]) * 6.0f;
                else
                    clr[i] = temp1;
            }

            r = clr[0];
            g = clr[1];
            b = clr[2];
        }

        static void ConvertToHsl(float r, float g, float b, Mode mode, out float h, out float s, out float l)
        {
            float v = Math.Max(r, g);
            v = Math.Max(v, b);

            float m = Math.Min(r, g);
            m = Math.Min(m, b);

            l = (m + v) / 2.0f;
            if (l <= 0.0)
            {
                h = s = l = 0;
                return;
            }
            float vm = v - m;
            s = vm;

            if (s > 0.0)
            {
                s /= l <= 0.5f ? v + m : 2.0f - v - m;
            }
            else
            {
                h = 0;
                s = 0;
                return;
            }

            float r2 = (v - r) / vm;
            float g2 = (v - g) / vm;
            float b2 = (v - b) / vm;

            if (r == v)
            {
                h = g == m ? 5.0f + b2 : 1.0f - g2;
            }
            else if (g == v)
            {
                h = b == m ? 1.0f + r2 : 3.0f - b2;
            }
            else
            {
                h = r == m ? 3.0f + g2 : 5.0f - r2;
            }
            h /= 6.0f;
        }

        public static bool operator ==(xColor color1, xColor color2)
        {
            return EqualsInner(color1, color2);
        }

        public static bool operator !=(xColor color1, xColor color2)
        {
            return !EqualsInner(color1, color2);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashcode = _r.GetHashCode();
                hashcode = (hashcode * 397) ^ _g.GetHashCode();
                hashcode = (hashcode * 397) ^ _b.GetHashCode();
                hashcode = (hashcode * 397) ^ _a.GetHashCode();
                return hashcode;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is xColor)
            {
                return EqualsInner(this, (xColor)obj);
            }
            return base.Equals(obj);
        }

        static bool EqualsInner(xColor color1, xColor color2)
        {
            if (color1._mode == Mode.Default && color2._mode == Mode.Default)
                return true;
            if (color1._mode == Mode.Default || color2._mode == Mode.Default)
                return false;
            if (color1._mode == Mode.Hsl && color2._mode == Mode.Hsl)
                return color1._hue == color2._hue && color1._saturation == color2._saturation && color1._luminosity == color2._luminosity && color1._a == color2._a;
            return color1._r == color2._r && color1._g == color2._g && color1._b == color2._b && color1._a == color2._a;
        }

        public override string ToString()
        {
            return ToHex() + " => " + string.Format(CultureInfo.InvariantCulture, "[Color: A={0}, R={1}, G={2}, B={3}, Hue={4}, Saturation={5}, Luminosity={6}]", A * 255, R * 255, G * 255, B * 255, Hue, Saturation, Luminosity);
        }

        public string ToHex()
        {
            var red = (uint)(R * 255);
            var green = (uint)(G * 255);
            var blue = (uint)(B * 255);
            var alpha = (uint)(A * 255);
            return $"#{alpha:X2}{red:X2}{green:X2}{blue:X2}";
        }

        public string HexString { get => ToHex(); }

        static uint ToHex(char c)
        {
            ushort x = (ushort)c;
            if (x >= '0' && x <= '9')
                return (uint)(x - '0');

            x |= 0x20;
            if (x >= 'a' && x <= 'f')
                return (uint)(x - 'a' + 10);
            return 0;
        }

        static uint ToHexD(char c)
        {
            var j = ToHex(c);
            return (j << 4) | j;
        }

        public static xColor FromHex(string hex, xColor? defaultColor = null)
        {
            // Undefined
            if (string.IsNullOrEmpty(hex) || hex.Length < 3)
                return defaultColor ?? Default;

            int idx = (hex[0] == '#') ? 1 : 0;
            try {
                switch (hex.Length - idx)
                {
                    case 3: //#rgb => ffrrggbb
                        var t1 = ToHexD(hex[idx++]);
                        var t2 = ToHexD(hex[idx++]);
                        var t3 = ToHexD(hex[idx]);

                        return FromRgb((int)t1, (int)t2, (int)t3);

                    case 4: //#argb => aarrggbb
                        var f1 = ToHexD(hex[idx++]);
                        var f2 = ToHexD(hex[idx++]);
                        var f3 = ToHexD(hex[idx++]);
                        var f4 = ToHexD(hex[idx]);
                        return FromRgba((int)f2, (int)f3, (int)f4, (int)f1);

                    case 6: //#rrggbb => ffrrggbb
                        return FromRgb((int)(ToHex(hex[idx++]) << 4 | ToHex(hex[idx++])),
                                (int)(ToHex(hex[idx++]) << 4 | ToHex(hex[idx++])),
                                (int)(ToHex(hex[idx++]) << 4 | ToHex(hex[idx])));

                    case 8: //#aarrggbb
                        var a1 = ToHex(hex[idx++]) << 4 | ToHex(hex[idx++]);
                        return FromRgba((int)(ToHex(hex[idx++]) << 4 | ToHex(hex[idx++])),
                                (int)(ToHex(hex[idx++]) << 4 | ToHex(hex[idx++])),
                                (int)(ToHex(hex[idx++]) << 4 | ToHex(hex[idx])),
                                (int)a1);

                    default: //everything else will result in unexpected results
                        return defaultColor ?? Default;
                }
            }
            catch
            {
                return defaultColor ?? Default;
            }
        }

        public static xColor FromUint(uint argb)
        {
            return FromRgba((byte)((argb & 0x00ff0000) >> 0x10), (byte)((argb & 0x0000ff00) >> 0x8), (byte)(argb & 0x000000ff), (byte)((argb & 0xff000000) >> 0x18));
        }

        public static xColor FromRgba(int r, int g, int b, int a)
        {
            double red = (double)r / 255;
            double green = (double)g / 255;
            double blue = (double)b / 255;
            double alpha = (double)a / 255;
            return new xColor(red, green, blue, alpha, Mode.Rgb);
        }

        public static xColor FromRgb(int r, int g, int b)
        {
            return FromRgba(r, g, b, 255);
        }

        public static xColor FromRgba(double r, double g, double b, double a)
        {
            return new xColor(r, g, b, a);
        }

        public static xColor FromRgb(double r, double g, double b)
        {
            return new xColor(r, g, b, 1d, Mode.Rgb);
        }

        public static xColor FromHsla(double h, double s, double l, double a = 1d)
        {
            return new xColor(h, s, l, a, Mode.Hsl);
        }
        #region Color Definitions

        //maretial colors
        //https://cdn.dribbble.com/users/634131/screenshots/2012608/material-design-colors.png
        public static readonly xColor MaterialLightGreen = FromHex("#FF8bc34a");
        public static readonly xColor MaterialGreen = FromHex("#ff259b24");
        public static readonly xColor MaterialIndigo = FromHex("#ffddeb3b");
        public static readonly xColor MaterialDeepPurple = FromHex("#ff673ab7");
        public static readonly xColor MaterialYellow = FromHex("#ffffeb3b");
        public static readonly xColor MaterialLime = FromHex("#ffcddc39");
        public static readonly xColor MaterialLightBlue = FromHex("#ff03a9f4");
        public static readonly xColor MaterialBlue = FromHex("#ff5677fc");
        public static readonly xColor MaterialAmber = FromHex("#ffffc107");
        public static readonly xColor MaterialOrange = FromHex("#ffff9800");
        public static readonly xColor MaterialCyan = FromHex("#ff00bcd4");
        public static readonly xColor MaterialTeal = FromHex("#ff009688");
        public static readonly xColor MaterialDeepOrange = FromHex("#FFff5722");
        public static readonly xColor MaterialRed = FromHex("#ffe51c23");
        public static readonly xColor MaterialGrey = FromHex("#ff9e9e9e");
        public static readonly xColor MaterialBrown = FromHex("#ff795548");
        public static readonly xColor MaterialPink = FromHex("#ffe91e63");
        public static readonly xColor MaterialPurple = FromHex("#ff9c27b0");
        public static readonly xColor MaterialBlueGray = FromHex("#ff607d8b");
        public static readonly xColor MaterialTextBlack = FromHex("#ff212121");

        // matches xColors in WPF's System.Windows.Media.xColors
        public static readonly xColor AliceBlue = FromRgb(240, 248, 255);
        public static readonly xColor AntiqueWhite = FromRgb(250, 235, 215);
        public static readonly xColor Aqua = FromRgb(0, 255, 255);
        public static readonly xColor Aquamarine = FromRgb(127, 255, 212);
        public static readonly xColor Azure = FromRgb(240, 255, 255);
        public static readonly xColor Beige = FromRgb(245, 245, 220);
        public static readonly xColor Bisque = FromRgb(255, 228, 196);
        public static readonly xColor Black = FromRgb(0, 0, 0);
        public static readonly xColor BlanchedAlmond = FromRgb(255, 235, 205);
        public static readonly xColor Blue = FromRgb(0, 0, 255);
        public static readonly xColor BlueViolet = FromRgb(138, 43, 226);
        public static readonly xColor Brown = FromRgb(165, 42, 42);
        public static readonly xColor BurlyWood = FromRgb(222, 184, 135);
        public static readonly xColor CadetBlue = FromRgb(95, 158, 160);
        public static readonly xColor Chartreuse = FromRgb(127, 255, 0);
        public static readonly xColor Chocolate = FromRgb(210, 105, 30);
        public static readonly xColor Coral = FromRgb(255, 127, 80);
        public static readonly xColor CornflowerBlue = FromRgb(100, 149, 237);
        public static readonly xColor Cornsilk = FromRgb(255, 248, 220);
        public static readonly xColor Crimson = FromRgb(220, 20, 60);
        public static readonly xColor Cyan = FromRgb(0, 255, 255);
        public static readonly xColor DarkBlue = FromRgb(0, 0, 139);
        public static readonly xColor DarkCyan = FromRgb(0, 139, 139);
        public static readonly xColor DarkGoldenrod = FromRgb(184, 134, 11);
        public static readonly xColor DarkGray = FromRgb(169, 169, 169);
        public static readonly xColor DarkGreen = FromRgb(0, 100, 0);
        public static readonly xColor DarkKhaki = FromRgb(189, 183, 107);
        public static readonly xColor DarkMagenta = FromRgb(139, 0, 139);
        public static readonly xColor DarkOliveGreen = FromRgb(85, 107, 47);
        public static readonly xColor DarkOrange = FromRgb(255, 140, 0);
        public static readonly xColor DarkOrchid = FromRgb(153, 50, 204);
        public static readonly xColor DarkRed = FromRgb(139, 0, 0);
        public static readonly xColor DarkSalmon = FromRgb(233, 150, 122);
        public static readonly xColor DarkSeaGreen = FromRgb(143, 188, 143);
        public static readonly xColor DarkSlateBlue = FromRgb(72, 61, 139);
        public static readonly xColor DarkSlateGray = FromRgb(47, 79, 79);
        public static readonly xColor DarkTurquoise = FromRgb(0, 206, 209);
        public static readonly xColor DarkViolet = FromRgb(148, 0, 211);
        public static readonly xColor DeepPink = FromRgb(255, 20, 147);
        public static readonly xColor DeepSkyBlue = FromRgb(0, 191, 255);
        public static readonly xColor DimGray = FromRgb(105, 105, 105);
        public static readonly xColor DodgerBlue = FromRgb(30, 144, 255);
        public static readonly xColor Firebrick = FromRgb(178, 34, 34);
        public static readonly xColor FloralWhite = FromRgb(255, 250, 240);
        public static readonly xColor ForestGreen = FromRgb(34, 139, 34);
        public static readonly xColor Fuchsia = FromRgb(255, 0, 255);
        public static readonly xColor Gainsboro = FromRgb(220, 220, 220);
        public static readonly xColor GhostWhite = FromRgb(248, 248, 255);
        public static readonly xColor Gold = FromRgb(255, 215, 0);
        public static readonly xColor Goldenrod = FromRgb(218, 165, 32);
        public static readonly xColor Gray = FromRgb(128, 128, 128);
        public static readonly xColor Green = FromRgb(0, 128, 0);
        public static readonly xColor GreenYellow = FromRgb(173, 255, 47);
        public static readonly xColor Honeydew = FromRgb(240, 255, 240);
        public static readonly xColor HotPink = FromRgb(255, 105, 180);
        public static readonly xColor IndianRed = FromRgb(205, 92, 92);
        public static readonly xColor Indigo = FromRgb(75, 0, 130);
        public static readonly xColor Ivory = FromRgb(255, 255, 240);
        public static readonly xColor Khaki = FromRgb(240, 230, 140);
        public static readonly xColor Lavender = FromRgb(230, 230, 250);
        public static readonly xColor LavenderBlush = FromRgb(255, 240, 245);
        public static readonly xColor LawnGreen = FromRgb(124, 252, 0);
        public static readonly xColor LemonChiffon = FromRgb(255, 250, 205);
        public static readonly xColor LightBlue = FromRgb(173, 216, 230);
        public static readonly xColor LightCoral = FromRgb(240, 128, 128);
        public static readonly xColor LightCyan = FromRgb(224, 255, 255);
        public static readonly xColor LightGoldenrodYellow = FromRgb(250, 250, 210);
        public static readonly xColor LightGray = FromRgb(211, 211, 211);
        public static readonly xColor LightGreen = FromRgb(144, 238, 144);
        public static readonly xColor LightPink = FromRgb(255, 182, 193);
        public static readonly xColor LightSalmon = FromRgb(255, 160, 122);
        public static readonly xColor LightSeaGreen = FromRgb(32, 178, 170);
        public static readonly xColor LightSkyBlue = FromRgb(135, 206, 250);
        public static readonly xColor LightSlateGray = FromRgb(119, 136, 153);
        public static readonly xColor LightSteelBlue = FromRgb(176, 196, 222);
        public static readonly xColor LightYellow = FromRgb(255, 255, 224);
        public static readonly xColor Lime = FromRgb(0, 255, 0);
        public static readonly xColor LimeGreen = FromRgb(50, 205, 50);
        public static readonly xColor Linen = FromRgb(250, 240, 230);
        public static readonly xColor Magenta = FromRgb(255, 0, 255);
        public static readonly xColor Maroon = FromRgb(128, 0, 0);
        public static readonly xColor MediumAquamarine = FromRgb(102, 205, 170);
        public static readonly xColor MediumBlue = FromRgb(0, 0, 205);
        public static readonly xColor MediumOrchid = FromRgb(186, 85, 211);
        public static readonly xColor MediumPurple = FromRgb(147, 112, 219);
        public static readonly xColor MediumSeaGreen = FromRgb(60, 179, 113);
        public static readonly xColor MediumSlateBlue = FromRgb(123, 104, 238);
        public static readonly xColor MediumSpringGreen = FromRgb(0, 250, 154);
        public static readonly xColor MediumTurquoise = FromRgb(72, 209, 204);
        public static readonly xColor MediumVioletRed = FromRgb(199, 21, 133);
        public static readonly xColor MidnightBlue = FromRgb(25, 25, 112);
        public static readonly xColor MintCream = FromRgb(245, 255, 250);
        public static readonly xColor MistyRose = FromRgb(255, 228, 225);
        public static readonly xColor Moccasin = FromRgb(255, 228, 181);
        public static readonly xColor NavajoWhite = FromRgb(255, 222, 173);
        public static readonly xColor Navy = FromRgb(0, 0, 128);
        public static readonly xColor OldLace = FromRgb(253, 245, 230);
        public static readonly xColor Olive = FromRgb(128, 128, 0);
        public static readonly xColor OliveDrab = FromRgb(107, 142, 35);
        public static readonly xColor Orange = FromRgb(255, 165, 0);
        public static readonly xColor OrangeRed = FromRgb(255, 69, 0);
        public static readonly xColor Orchid = FromRgb(218, 112, 214);
        public static readonly xColor PaleGoldenrod = FromRgb(238, 232, 170);
        public static readonly xColor PaleGreen = FromRgb(152, 251, 152);
        public static readonly xColor PaleTurquoise = FromRgb(175, 238, 238);
        public static readonly xColor PaleVioletRed = FromRgb(219, 112, 147);
        public static readonly xColor PapayaWhip = FromRgb(255, 239, 213);
        public static readonly xColor PeachPuff = FromRgb(255, 218, 185);
        public static readonly xColor Peru = FromRgb(205, 133, 63);
        public static readonly xColor Pink = FromRgb(255, 192, 203);
        public static readonly xColor Plum = FromRgb(221, 160, 221);
        public static readonly xColor PowderBlue = FromRgb(176, 224, 230);
        public static readonly xColor Purple = FromRgb(128, 0, 128);
        public static readonly xColor Red = FromRgb(255, 0, 0);
        public static readonly xColor RosyBrown = FromRgb(188, 143, 143);
        public static readonly xColor RoyalBlue = FromRgb(65, 105, 225);
        public static readonly xColor SaddleBrown = FromRgb(139, 69, 19);
        public static readonly xColor Salmon = FromRgb(250, 128, 114);
        public static readonly xColor SandyBrown = FromRgb(244, 164, 96);
        public static readonly xColor SeaGreen = FromRgb(46, 139, 87);
        public static readonly xColor SeaShell = FromRgb(255, 245, 238);
        public static readonly xColor Sienna = FromRgb(160, 82, 45);
        public static readonly xColor Silver = FromRgb(192, 192, 192);
        public static readonly xColor SkyBlue = FromRgb(135, 206, 235);
        public static readonly xColor SlateBlue = FromRgb(106, 90, 205);
        public static readonly xColor SlateGray = FromRgb(112, 128, 144);
        public static readonly xColor Snow = FromRgb(255, 250, 250);
        public static readonly xColor SpringGreen = FromRgb(0, 255, 127);
        public static readonly xColor SteelBlue = FromRgb(70, 130, 180);
        public static readonly xColor Tan = FromRgb(210, 180, 140);
        public static readonly xColor Teal = FromRgb(0, 128, 128);
        public static readonly xColor Thistle = FromRgb(216, 191, 216);
        public static readonly xColor Tomato = FromRgb(255, 99, 71);
        public static readonly xColor Transparent = FromRgba(255, 255, 255, 0);
        public static readonly xColor Turquoise = FromRgb(64, 224, 208);
        public static readonly xColor Violet = FromRgb(238, 130, 238);
        public static readonly xColor Wheat = FromRgb(245, 222, 179);
        public static readonly xColor White = FromRgb(255, 255, 255);
        public static readonly xColor WhiteSmoke = FromRgb(245, 245, 245);
        public static readonly xColor Yellow = FromRgb(255, 255, 0);
        public static readonly xColor YellowGreen = FromRgb(154, 205, 50);

        #endregion
    }
}
