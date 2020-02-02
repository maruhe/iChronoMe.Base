using System;
using System.IO;

using iChronoMe.Core.Classes;
using iChronoMe.Core.Types;

using SkiaSharp;

namespace iChronoMe.Widgets
{
    public class WidgetView_ClockAnalog
    {
        private string cfgInstance = Guid.NewGuid().ToString();
        public string BackgroundImage { get; set; } = string.Empty;

        public xColor ColorBackground { get; set; } = xColor.Transparent;
        public xColor ColorTickMarks { get; set; } = xColor.Black;
        public xColor ColorHourHandStorke { get; set; } = xColor.Black;
        public xColor ColorHourHandFill { get; set; } = xColor.Blue;
        public xColor ColorMinuteHandStorke { get; set; } = xColor.Black;
        public xColor ColorMinuteHandFill { get; set; } = xColor.Blue;
        public xColor ColorSecondHandStorke { get; set; } = xColor.Black;
        public xColor ColorSecondHandFill { get; set; } = xColor.Blue;

        public bool ShowHourHand { get; set; } = true;
        public bool ShowMinuteHand { get; set; } = true;
        public bool ShowSecondHand { get; set; } = true;

        public bool FlowHourHand { set; get; } = true;
        public bool FlowMinuteHand { set; get; } = true;
        public bool FlowSecondHand { set; get; } = false;


        SKPaint fillPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.White
        };

        SKPaint strokePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black,
            StrokeWidth = 2,
            StrokeCap = SKStrokeCap.Round,
            IsAntialias = true
        };

        SKPath hourHandPath = SKPath.ParseSvgPathData(
            "M 0 -60 C 0 -30 20 -30 5 -20 L 5 0 C 5 7.5 -5 7.5 -5 0 L -5 -20 C -20 -30 0 -30 0 -60");
        SKPath minuteHandPath = SKPath.ParseSvgPathData(
            "M 0 -80 C 0 -75 0 -70 2.5 -60 L 2.5 0 C 2.5 5 -2.5 5 -2.5 0 L -2.5 -60 C 0 -70 0 -75 0 -80");

        TimeSpan tsMin = TimeSpan.FromHours(1);
        TimeSpan tsMax = TimeSpan.FromTicks(0);
        TimeSpan tsAllSum = TimeSpan.FromTicks(0);
        int iAllCount = 0;
        SKBitmap bitmap;

        public Stream GetBitmap(DateTime dateTime, int width = 512, int height = 512)
        {
            if (bitmap == null || bitmap.Width != width || bitmap.Height != height)
                bitmap = new SKBitmap(width, height);

            SKCanvas canvas = new SKCanvas(bitmap);

            DrawCanvas(canvas, dateTime, width, height, true);

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

        public void ReadConfig(WidgetCfg_ClockAnalog cfg)
        {
            cfgInstance = Guid.NewGuid().ToString();

            BackgroundImage = cfg.BackgroundImage;

            ColorBackground = cfg.ColorBackground;
            ColorTickMarks = cfg.ColorTickMarks;
            ColorHourHandStorke = cfg.ColorHourHandStorke;
            ColorHourHandFill = cfg.ColorHourHandFill;
            ColorMinuteHandStorke = cfg.ColorMinuteHandStorke;
            ColorMinuteHandFill = cfg.ColorMinuteHandFill;
            ColorSecondHandStorke = cfg.ColorSecondHandStorke;
            ColorSecondHandFill = cfg.ColorSecondHandFill;

            ShowHourHand = cfg.ShowHours;
            ShowMinuteHand = cfg.ShowMinutes;
            ShowSecondHand = cfg.ShowSeconds;

            FlowHourHand = cfg.FlowHourHand;
            FlowMinuteHand = cfg.FlowMinuteHand;
            FlowSecondHand = cfg.FlowSecondHand;
        }

        string cBackCacheInstance = "_";
        SKBitmap backCache = null;

        public void DrawCanvas(SKCanvas canvas, DateTime dateTime, int width = 512, int height = 512, bool bDrawBackImage = false)
        {
            DateTime swStart = DateTime.Now;

            canvas.Clear();

            //Background image
            if (bDrawBackImage && !string.IsNullOrEmpty(BackgroundImage))
            {
                try
                {
                    if (backCache != null && cfgInstance == cBackCacheInstance)
                        canvas.DrawBitmap(backCache, (width - backCache.Width) / 2, (height - backCache.Height) / 2);
                    else
                    {
                        using (var bitmap = SKBitmap.Decode(BackgroundImage))
                        {
                            int x = Math.Min(width, height);

                            var resizedBitmap = bitmap.Resize(new SKImageInfo(x, x), SKFilterQuality.High);

                            canvas.DrawBitmap(resizedBitmap, (width - resizedBitmap.Width) / 2, (height - resizedBitmap.Height) / 2);

                            backCache?.Dispose();
                            backCache = resizedBitmap;
                            cBackCacheInstance = cfgInstance;
                        }
                    }
                }
                catch { }
            }

            // Set transforms
            canvas.Translate(width / 2, height / 2);
            canvas.Scale(Math.Min(width / 200f, height / 200f));

            // Clock background
            fillPaint.Color = ColorBackground.ToSKColor();
            if (ColorBackground.A > 0)
                canvas.DrawCircle(0, 0, 100, fillPaint);

            // Hour and minute marks
            if (ColorTickMarks.A > 0)
            {
                fillPaint.Color = ColorTickMarks.ToSKColor();
                for (int angle = 0; angle < 360; angle += 6)
                {
                    canvas.DrawCircle(0, -90, angle % 30 == 0 ? 4 : 2, fillPaint);
                    canvas.RotateDegrees(6);
                }
            }

            // Hour hand
            if (ShowHourHand)
            {
                fillPaint.Color = ColorHourHandFill.ToSKColor();
                strokePaint.Color = ColorHourHandStorke.ToSKColor();
                canvas.Save();
                canvas.RotateDegrees(30 * dateTime.Hour + (FlowHourHand ? dateTime.Minute / 2f : 0));
                canvas.DrawPath(hourHandPath, fillPaint);
                canvas.DrawPath(hourHandPath, strokePaint);
                canvas.Restore();
            }

            // Minute hand
            if (ShowMinuteHand)
            {
                fillPaint.Color = ColorMinuteHandFill.ToSKColor();
                strokePaint.Color = ColorMinuteHandStorke.ToSKColor();
                canvas.Save();
                canvas.RotateDegrees(6 * dateTime.Minute + (FlowMinuteHand ? dateTime.Second / 10f : 0));
                canvas.DrawPath(minuteHandPath, fillPaint);
                canvas.DrawPath(minuteHandPath, strokePaint);
                canvas.Restore();
            }

            // Second hand
            if (ShowSecondHand)
            {
                fillPaint.Color = ColorSecondHandFill.ToSKColor();
                strokePaint.Color = ColorSecondHandStorke.ToSKColor();
                canvas.Save();
                float seconds = dateTime.Second + (FlowSecondHand ? dateTime.Millisecond / 1000f : 0);
                canvas.RotateDegrees(6 * seconds);
                canvas.DrawLine(0, 10, 0, -80, strokePaint);
                canvas.Restore();
            }

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
                if (iAllCount > 0)
                {
                    return "min: " + tsMin.TotalMilliseconds.ToString() + "\n" +
                           "max: " + tsMax.TotalMilliseconds.ToString() + "\n" +
                           "avg: " + TimeSpan.FromTicks(tsAllSum.Ticks / iAllCount).ToString();
                }
                return "init..";
            }
        }
    }
}
