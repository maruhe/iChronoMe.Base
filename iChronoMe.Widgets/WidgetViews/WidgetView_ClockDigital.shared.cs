using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Xml.Serialization;

using iChronoMe.Core.Classes;
using iChronoMe.Core.Types;
using iChronoMe.Tools;

using SkiaSharp;

using static iChronoMe.Widgets.ClockHandConfig;
//using SKSvg = SkiaSharp.Extended.Svg.SKSvg;

namespace iChronoMe.Widgets
{
    public class WidgetView_ClockDigital : WidgetView_Clock
    {
        public SKPaint fillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.White
        };

        public SKPaint strokePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black,
            StrokeWidth = 10,
            StrokeCap = SKStrokeCap.Round,
            IsAntialias = true,
            StrokeJoin = SKStrokeJoin.Bevel
        };

        public SKPaint textPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            TextSize = sys.DpPx(24),
            Color = SKColors.Black,
            IsAntialias = true
        };

        TimeSpan tsMin = TimeSpan.FromHours(1);
        TimeSpan tsMax = TimeSpan.FromTicks(0);
        TimeSpan tsAllSum = TimeSpan.FromTicks(0);
        int iAllCount = 0;
        SKBitmap bitmap;

        public Stream GetBitmap(DateTime dateTime, int width = 512, int height = 512, bool bDrawBackImage = false)
        {
            if (bitmap == null || bitmap.Width != width || bitmap.Height != height)
            {
                bitmap?.Dispose();
                bitmap = new SKBitmap(width, height);
            }

            SKCanvas canvas = new SKCanvas(bitmap);

            DrawCanvas(canvas, dateTime, width, height, bDrawBackImage);

            // create an image COPY
            //SKImage image = SKImage.FromBitmap(bitmap);
            // OR
            // create an image WRAPPER
            SKImage image = SKImage.FromPixels(bitmap.PeekPixels());

            // encode the image (defaults to PNG)
            SKData encoded = image.Encode();

            // get a stream over the encoded data
            Stream stream = encoded.AsStream();

            return stream;
        }

        public void ReadConfig(WidgetCfg_ClockDigital cfg)
        {
            cfgInstance = Guid.NewGuid().ToString();

            ShowHours = cfg.ShowHours;
            ShowMinutes = cfg.ShowMinutes;
            ShowSeconds = cfg.ShowSeconds;
        }

        public void DrawCanvas(SKCanvas canvas, DateTime dateTime, int width = 512, int height = 512, bool bDrawBackImage = false)
        {
            var t = dateTime.TimeOfDay;
            DateTime swStart = DateTime.Now;

            canvas.Clear();
            int x = Math.Min(width, height);

            // Set scale to draw on a 1000*1000 points base
            canvas.Translate(width > height ? (width - height) / 2 : 0, height > width ? (height - width) / 2 : 0);
            //canvas.Scale(Math.Min(width / 1000f, height / 1000f));

            string cTime = dateTime.ToLongTimeString();

            canvas.DrawText(cTime, 100, 100, textPaint);

            TimeSpan tsDraw = DateTime.Now - swStart;
            iAllCount++;
            tsAllSum += tsDraw;
            if (tsMin > tsDraw)
                tsMin = tsDraw;
            if (tsMax < tsDraw)
                tsMax = tsDraw;
        }

        public string PerformanceInfo
        {
            get
            {
                if (DateTime.Now.Second < 2)
                {
                    tsMin = TimeSpan.FromHours(1);
                    tsMax = TimeSpan.FromTicks(0);
                    tsAllSum = TimeSpan.FromTicks(0);
                    iAllCount = 0;
                    return "reinit..";
                }
                if (iAllCount > 0)
                {
                    return "min: " + tsMin.TotalMilliseconds.ToString("#.00") + "\n" +
                           "max: " + tsMax.TotalMilliseconds.ToString("#.00") + "\n" +
                           "avg: " + TimeSpan.FromTicks(tsAllSum.Ticks / iAllCount).TotalMilliseconds.ToString("#.00");
                }
                return "init..";
            }
        }

    }
}