﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Xml.Serialization;

using iChronoMe.Core.Classes;
using iChronoMe.Core.DataModels;
using iChronoMe.Core.Types;
using iChronoMe.Tools;

using SkiaSharp;
using SkiaSharp.Extended.Svg;

using static iChronoMe.Widgets.ClockHandConfig;
using SKSvg = SkiaSharp.Extended.Svg.SKSvg;
//using SKSvg = SkiaSharp.Extended.Svg.SKSvg;

namespace iChronoMe.Widgets
{
    public class WidgetView_ClockDigital : WidgetView_Clock
    {
        public DigitalClockStyle ClockStyle { get; set; } = DigitalClockStyle.Default;

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

        public Stream GetBitmap(DateTime dateTime, int width, int height, WidgetCfg_ClockDigital cfg, WeatherInfo wi, bool bDrawBackImage = false)
        {
            if (bitmap == null || bitmap.Width != width || bitmap.Height != height)
            {
                bitmap?.Dispose();
                bitmap = new SKBitmap(width, height);
            }

            SKCanvas canvas = new SKCanvas(bitmap);

            DrawCanvas(canvas, dateTime, width, height, cfg, wi, bDrawBackImage);

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

            ClockStyle = cfg.ClockStyle;

            ShowHours = cfg.ShowHours;
            ShowMinutes = cfg.ShowMinutes;
            ShowSeconds = cfg.ShowSeconds;
        }

        public void DrawCanvas(SKCanvas canvas, DateTime dateTime, float width, float height, WidgetCfg_ClockDigital cfg, WeatherInfo wi, bool bDrawBackImage = false)
        {
            var t = dateTime.TimeOfDay;
            DateTime swStart = DateTime.Now;

            canvas.Clear();

            if (bDrawBackImage && ColorBackground.A > 0)
            {
                fillPaint.Color = ColorBackground.ToSKColor();
                canvas.DrawRoundRect(0, 0, width, height, sys.DpPx(8), sys.DpPx(8), fillPaint);
            }

            float padding = sys.DpPx(5);
            canvas.Translate(padding, padding);
            width -= padding * 2;
            height -= padding * 2;

            string cTime = ShowSeconds ? dateTime.ToLongTimeString() : dateTime.ToShortTimeString();

            float x = 0;

            textPaint.TextSize = sys.DpPx(24);

            if (cfg != null)
            {
                textPaint.Color = cfg.ColorTitleText.ToSKColor();
            }

            switch (ClockStyle)
            {
                case DigitalClockStyle.Detailed:
                    GetMaxTextSize(textPaint, cTime, height * 2 / 3, width * .6f);
                    x += textPaint.TextSize * .8f;
                    canvas.DrawText(cTime, 0, x, textPaint);

                    float y = textPaint.MeasureText(cTime) + padding * 2;

                    textPaint.TextSize /= 2;
                    canvas.DrawText("lala", y, x, textPaint);

                    x += textPaint.TextSize + padding / 2;
                    canvas.DrawText(dateTime.ToString("ddd") + ". " + dateTime.ToShortDateString() + ", " + cfg.WidgetTitle, 0, x, textPaint);
                    break;

                case DigitalClockStyle.JustTime:
                    GetMaxTextSize(textPaint, cTime, height, width);
                    x += textPaint.TextSize * .8f;
                    textPaint.TextAlign = SKTextAlign.Center;
                    canvas.DrawText(cTime, width / 2, x + (height - x) / 2, textPaint);
                    break;

                case DigitalClockStyle.WeatherTime:
                    GetMaxTextSize(textPaint, cTime, height * 2 / 3, width * .6f);
                    float fMainText = textPaint.TextSize;
                    x += textPaint.TextSize * .8f;
                    canvas.DrawText(cTime, 0, x, textPaint);

                    float iconWidth = 0;
                    string icon = "42";
                    if (wi != null)
                        icon = wi.GetWeatherIcon().ToString("00");
                    try
                    {
                        var svg = new SKSvg();
                        svg.Load(typeof(WidgetView).Assembly.GetManifestResourceStream("iChronoMe.Widgets.Icons." + icon + ".svg"));

                        float pWidth = svg.Picture.CullRect.Width;
                        float pHeight = svg.Picture.CullRect.Height;

                        float scale = textPaint.TextSize / pHeight * 1.1f;
                        var matrix = new SKMatrix
                        {
                            ScaleX = scale,
                            ScaleY = scale,
                            TransX = width - pWidth * scale,
                            TransY = -(padding/2),
                            Persp2 = 1,
                        };

                        canvas.DrawPicture(svg.Picture, ref matrix);

                        iconWidth = pWidth * scale;
                    }
                    catch (Exception ex)
                    {
                        xLog.Error(ex);
                    }

                    textPaint.TextSize = fMainText / 3;
                    string cWeatherText1 = wi == null ? "" : (int)wi.Temp + "°" + xUnits.GetUnitStringClimacell(Core.Types.xUnit.Temp.Default);
                    y = width - iconWidth - textPaint.MeasureText(cWeatherText1) - padding;
                    canvas.DrawText(cWeatherText1, y, x, textPaint);

                    textPaint.TextSize = fMainText / 2;
                    x += textPaint.TextSize + padding / 2;
                    canvas.DrawText(dateTime.ToString("ddd") + ". " + dateTime.ToShortDateString() + ", " + cfg.WidgetTitle, 0, x, textPaint);

                    break;

                default:
                    GetMaxTextSize(textPaint, cTime, height * 2 / 3, width * .6f);
                    x += textPaint.TextSize * .8f;
                    canvas.DrawText(cTime, 0, x, textPaint);

                    textPaint.TextSize /= 2;
                    x += textPaint.TextSize + padding / 2;
                    canvas.DrawText(dateTime.ToString("ddd") + ". " + dateTime.ToShortDateString() + ", " + cfg.WidgetTitle, 0, x, textPaint);
                    break;
            }
            TimeSpan tsDraw = DateTime.Now - swStart;
            iAllCount++;
            tsAllSum += tsDraw;
            if (tsMin > tsDraw)
                tsMin = tsDraw;
            if (tsMax < tsDraw)
                tsMax = tsDraw;
        }

        public override bool NeedsWeatherInfo => ClockStyle == DigitalClockStyle.WeatherTime;
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

        public float GetMaxTextSize(SKPaint p, string text, float initSize, float maxWidth)
        {
            p.TextSize = initSize;
            while (p.MeasureText(text) > maxWidth)
                p.TextSize -= .5f;
            return p.TextSize;
        }
    }
}