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
    public class WidgetView_ClockAnalog
    {
        private string cfgInstance = Guid.NewGuid().ToString();
        public string BackgroundImage { get; set; } = string.Empty;
        public bool BackImageAllowsBackColor { get; set; } = false;

        public xColor ColorBackground { get; set; } = xColor.Transparent;
        public xColor ColorTickMarks { get; set; } = xColor.Black;
        public TickMarkStyle TickMarkStyle { get; set; } = TickMarkStyle.Dotts;
        public xColor ColorHourHandStroke { get; set; } = xColor.Black;
        public xColor ColorHourHandFill { get; set; } = xColor.Blue;
        public xColor ColorMinuteHandStroke { get; set; } = xColor.Black;
        public xColor ColorMinuteHandFill { get; set; } = xColor.Blue;
        public xColor ColorSecondHandStroke { get; set; } = xColor.Black;
        public xColor ColorSecondHandFill { get; set; } = xColor.Blue;
        public xColor ColorCenterCapStroke { get; set; } = xColor.Black;
        public xColor ColorCenterCapFill { get; set; } = xColor.Blue;

        public bool ShowHourHand { get; set; } = true;
        public bool ShowMinuteHand { get; set; } = true;
        public bool ShowSecondHand { get; set; } = true;

        public bool FlowHourHand { set; get; } = true;
        public bool FlowMinuteHand { set; get; } = true;
        public bool FlowSecondHand { set; get; } = false;

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
            IsAntialias = true
        };


        public List<ClockPath> HourPathList { get; set; } = ClockHandConfig.defaultHands.HourPathList;
        public List<ClockPath> MinutePathList { get; set; } = ClockHandConfig.defaultHands.MinutePathList;
        public List<ClockPath> SecondPathList { get; set; } = ClockHandConfig.defaultHands.SecondPathList;
        public List<ClockPath> CapPathList { get; set; }

        ClockfaceInfo _clockfaceInfo = null;
        [XmlIgnore]
        public ClockfaceInfo ClockfaceInfo
        {
            get
            {
                if (string.IsNullOrEmpty(BackgroundImage))
                    return null;
                if (_clockfaceInfo == null || BackgroundImage.Contains(_clockfaceInfo.Clockface))
                    _clockfaceInfo = ClockHandConfig.GetFaceInfo(Path.GetFileNameWithoutExtension(BackgroundImage));
                return _clockfaceInfo;
            }
        }

        TimeSpan tsMin = TimeSpan.FromHours(1);
        TimeSpan tsMax = TimeSpan.FromTicks(0);
        TimeSpan tsAllSum = TimeSpan.FromTicks(0);
        int iAllCount = 0;
        SKBitmap bitmap;

        public Stream GetBitmap(DateTime dateTime, int width = 512, int height = 512, bool bDrawBackImage = false)
        {
            return GetBitmap(dateTime.TimeOfDay.TotalHours % 12, dateTime.TimeOfDay.TotalMinutes % 60, dateTime.TimeOfDay.TotalSeconds % 60, width, height, bDrawBackImage);
        }

        public Stream GetBitmap(double nHour, double nMinute, double nSecond, int width = 512, int height = 512, bool bDrawBackImage = false)
        {
            if (bitmap == null || bitmap.Width != width || bitmap.Height != height)
                bitmap = new SKBitmap(width, height);

            SKCanvas canvas = new SKCanvas(bitmap);

            DrawCanvas(canvas, nHour, nMinute, nSecond, width, height, bDrawBackImage);

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
            bmpBackCache = null;

            BackgroundImage = cfg.BackgroundImage;
            BackImageAllowsBackColor = cfg.BackImageAllowsBackColor;
            TickMarkStyle = cfg.TickMarkStyle;

            ColorBackground = cfg.ColorBackground;
            ColorTickMarks = cfg.ColorTickMarks;
            ColorHourHandStroke = cfg.ColorHourHandStroke;
            ColorHourHandFill = cfg.ColorHourHandFill;
            ColorMinuteHandStroke = cfg.ColorMinuteHandStroke;
            ColorMinuteHandFill = cfg.ColorMinuteHandFill;
            ColorSecondHandStroke = cfg.ColorSecondHandStroke;
            ColorSecondHandFill = cfg.ColorSecondHandFill;
            ColorCenterCapStroke = cfg.ColorCenterCapStroke;
            ColorCenterCapFill = cfg.ColorCenterCapFill;

            ShowHourHand = cfg.ShowHours;
            ShowMinuteHand = cfg.ShowMinutes;
            ShowSecondHand = cfg.ShowSeconds;

            FlowHourHand = cfg.FlowHourHand;
            FlowMinuteHand = cfg.FlowMinuteHand;
            FlowSecondHand = cfg.FlowSecondHand;

            HourPathList = cfg.HourHandConfig.HourPathList;
            MinutePathList = cfg.MinuteHandConfig.MinutePathList;
            SecondPathList = cfg.SecondHandConfig.SecondPathList;
            CapPathList = cfg.CapConfig.CapPathList;
        }

        public void DrawCanvas(SKCanvas canvas, DateTime dateTime, int width = 512, int height = 512, bool bDrawBackImage = false)
        {
            var t = dateTime.TimeOfDay;
            DrawCanvas(canvas, t.TotalHours, t.TotalMinutes % 60, t.TotalSeconds % 60, width, height, bDrawBackImage);
        }

        SKBitmap bmpBackCache = null;

        public void DrawCanvas(SKCanvas canvas, double hour, double minute, double second, int width = 512, int height = 512, bool bDrawBackImage = false)
        {
            DateTime swStart = DateTime.Now;

            canvas.Clear();
            int x = Math.Min(width, height);

            // Clock background
            fillPaint.Color = ColorBackground.ToSKColor();
            if (bDrawBackImage && ColorBackground.A > 0)
                canvas.DrawCircle(width / 2, height / 2, x / 2, fillPaint);

            //Background image
            if (bDrawBackImage && !string.IsNullOrEmpty(BackgroundImage))
            {
                //this is only for widget-preview!!!
                try
                {
                    /*if (BackgroundImage.EndsWith(".svg"))
                    {

                        // load the SVG
                        var svg = new SKSvg();
                        svg.Load(BackgroundImage);

                        float scale = (float)x / Math.Max(svg.Picture.CullRect.Width, svg.Picture.CullRect.Height);
                        var matrix = new SKMatrix
                        {
                            ScaleX = scale,
                            ScaleY = scale,
                            TransX = (width - x) / 2,
                            TransY = (height - x) / 2,
                            Persp2 = 1,
                        };

                        canvas.DrawPicture(svg.Picture, ref matrix);
                    }
                    else*/
                    {
                        if (bmpBackCache == null)
                        {
                            using (var bitmap = SKBitmap.Decode(BackgroundImage))
                            {
                                bmpBackCache = bitmap.Resize(new SKImageInfo(x, x), SKFilterQuality.High);
                            }
                        }

                        if (bmpBackCache != null)
                            canvas.DrawBitmap(bmpBackCache, (width - bmpBackCache.Width) / 2, (height - bmpBackCache.Height) / 2);
                    }
                }
                catch { }
            }

            // Set scale to draw on a 1000*1000 points base
            canvas.Translate(width > height ? (width - height) / 2 : 0, height > width ? (height - width) / 2 : 0);
            canvas.Scale(Math.Min(width / 1000f, height / 1000f));

            // Hour and minute marks
            if (TickMarkStyle != TickMarkStyle.None && ColorTickMarks.A > 0)
            {
                var tickPaint = new SKPaint
                {
                    Color = ColorTickMarks.ToSKColor(),
                    StrokeCap = SKStrokeCap.Round,
                    IsAntialias = true
                };

                if (ColorTickMarks.IsEmpty)
                    tickPaint.Color = ClockPath.GetBestColor(ColorTickMarks, ClockfaceInfo != null && !BackImageAllowsBackColor ? ClockfaceInfo.MainColor + " " + ClockfaceInfo.HandColorsBanned : ColorBackground.HexString, ClockfaceInfo?.HandColorSuggestion).ToSKColor();

                switch (TickMarkStyle)
                {
                    case TickMarkStyle.Dotts:
                        tickPaint.Style = SKPaintStyle.Fill;
                        canvas.Translate(500, 500);
                        for (int angle = 0; angle < 360; angle += 6)
                        {
                            canvas.DrawCircle(0, -460, angle % 30 == 0 ? 20 : 10, tickPaint);
                            canvas.RotateDegrees(6);
                        }
                        canvas.Translate(-500, -500);
                        break;

                    case TickMarkStyle.Circle:
                        tickPaint.Style = SKPaintStyle.Stroke;
                        tickPaint.StrokeWidth = 20;
                        canvas.DrawCircle(500, 500, 470, tickPaint);

                        break;

                    case TickMarkStyle.LinesSquare:
                        tickPaint.StrokeCap = SKStrokeCap.Square;
                        goto case TickMarkStyle.LinesRound;
                    case TickMarkStyle.LinesButt:
                        tickPaint.StrokeCap = SKStrokeCap.Butt;
                        goto case TickMarkStyle.LinesRound;
                    case TickMarkStyle.LinesRound:
                        tickPaint.Style = SKPaintStyle.Stroke;
                        canvas.Translate(500, 500);
                        for (int angle = 0; angle < 360; angle += 6)
                        {
                            if (angle % 30 == 0)
                            {
                                tickPaint.StrokeWidth = 20;
                                canvas.DrawLine(0, -450, 0, -470, tickPaint);
                            }
                            else
                            {
                                tickPaint.StrokeWidth = 10;
                                canvas.DrawLine(0, -455, 0, -465, tickPaint);
                            }
                            canvas.RotateDegrees(6);
                        }
                        canvas.Translate(-500, -500);
                        break;
                }
            }


            if (!FlowHourHand)
                hour = Math.Truncate(hour);
            if (!FlowMinuteHand)
                minute = Math.Truncate(minute);
            if (!FlowSecondHand)
                second = Math.Truncate(second);

            // Hour hand
            if (ShowHourHand && HourPathList != null)
            {
                canvas.RotateDegrees((float)(30 * hour), 500, 500);

                drawClockPaths(canvas, HourPathList, ColorHourHandStroke, ColorHourHandFill);

                canvas.RotateDegrees((float)(-30 * hour), 500, 500);
            }

            // Minute hand
            if (ShowMinuteHand && MinutePathList != null)
            {
                canvas.RotateDegrees((float)(6 * minute), 500, 500);

                drawClockPaths(canvas, MinutePathList, ColorMinuteHandStroke, ColorMinuteHandFill);

                canvas.RotateDegrees((float)(-6 * minute), 500, 500);
            }

            // Second hand
            if (ShowSecondHand && SecondPathList != null)
            {
                canvas.RotateDegrees((float)(6 * second), 500, 500);

                drawClockPaths(canvas, SecondPathList, ColorSecondHandStroke, ColorSecondHandFill);

                canvas.RotateDegrees((float)(-6 * second), 500, 500);
            }

            if (CapPathList != null)
            {
                drawClockPaths(canvas, CapPathList, ColorCenterCapStroke, ColorCenterCapFill);
            }

            TimeSpan tsDraw = DateTime.Now - swStart;
            iAllCount++;
            tsAllSum += tsDraw;
            if (tsMin > tsDraw)
                tsMin = tsDraw;
            if (tsMax < tsDraw)
                tsMax = tsDraw;
        }

        internal (SKColor strokeClr, SKColor fillClr) drawClockPaths(SKCanvas canvas, List<ClockPath> pathList, xColor colorStroke, xColor colorFill)
        {
            strokePaint.Color = colorFill.ToSKColor();
            fillPaint.Color = colorStroke.ToSKColor();
            if (pathList != null)
            {
                foreach (var path in pathList)
                {
                    canvas?.Translate(path.OffsetX, path.OffsetY);

                    if (!string.IsNullOrEmpty(path.StrokeColor) && !"-".Equals(path.StrokeColor))
                    {
                        strokePaint.Color = colorStroke.IsDefault || colorStroke.IsEmpty ? path.GetStrokeColor(ClockfaceInfo != null && !BackImageAllowsBackColor ? ClockfaceInfo.MainColor + " " + ClockfaceInfo.HandColorsBanned : ColorBackground.HexString, ClockfaceInfo?.HandColorSuggestion).ToSKColor() : colorStroke.ToSKColor();
                        strokePaint.StrokeWidth = path.StrokeWidth;
                        canvas?.DrawPath(path.SkPath, strokePaint);
                    }

                    if (!string.IsNullOrEmpty(path.FillColor) && !"-".Equals(path.FillColor))
                    {
                        fillPaint.Color = colorFill.IsDefault || colorFill.IsEmpty ? path.GetFillColor(ClockfaceInfo != null && !BackImageAllowsBackColor ? ClockfaceInfo.MainColor + " " + ClockfaceInfo.HandColorsBanned : ColorBackground.HexString, ClockfaceInfo?.HandColorSuggestion).ToSKColor() : colorFill.ToSKColor();
                        canvas?.DrawPath(path.SkPath, fillPaint);
                    }

                    canvas?.Translate(-(path.OffsetX), -(path.OffsetY));
                }
            }
            return (strokePaint.Color, fillPaint.Color);
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
                    return "min: " + tsMin.TotalMilliseconds.ToString() + "\n" +
                           "max: " + tsMax.TotalMilliseconds.ToString() + "\n" +
                           "avg: " + TimeSpan.FromTicks(tsAllSum.Ticks / iAllCount).ToString();
                }
                return "init..";
            }
        }

        public event EventHandler ClockFaceLoaded;

        static List<(string filter, string group, string file, int size, string destFile)> imgsToLoad = new List<(string, string, string, int, string)>();
        static Thread imgLoader = null;

        public string GetClockFacePng(string backgroundImage, int size)
        {
            if (backgroundImage.EndsWith(".png"))
            {
                string cFilter = ImageLoader.filter_clockfaces;
                string cGroup = Path.GetFileName(Path.GetDirectoryName(backgroundImage));
                string cFile = Path.GetFileName(backgroundImage);

                string cThumbPath = Path.Combine(Path.Combine(Path.Combine(Path.Combine(sys.PathShare, "imgCache_" + cFilter), cGroup), "thumb_" + size, cFile));

                if (File.Exists(cThumbPath) && (!File.Exists(backgroundImage) || File.GetLastWriteTime(cThumbPath).AddMinutes(1) >= File.GetLastWriteTime(backgroundImage)))
                    return cThumbPath;

                string maxFile = Path.Combine(Path.Combine(Path.Combine(Path.Combine(sys.PathShare, "imgCache_" + cFilter), cGroup), "thumb_max", cFile));

                if (!File.Exists(maxFile))
                {
                    lock (imgsToLoad)
                        imgsToLoad.Add((cFilter, cGroup, cFile, sys.DisplayShortSite, maxFile));

                    if (imgLoader == null)
                        StartImageLoader();
                }

                if (File.Exists(backgroundImage) && File.Exists(maxFile))
                {
                    if (File.GetLastWriteTime(backgroundImage) > File.GetLastWriteTime(maxFile).AddMinutes(1))
                        try { File.Delete(maxFile); } catch { }
                }

                if (File.Exists(maxFile))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(cThumbPath));
                    if (DrawableHelper.ResizeImage(maxFile, cThumbPath, size))
                        return cThumbPath;

                    return maxFile;
                }

            }
            return backgroundImage;
            /*
            if (backgroundImage.EndsWith(".svg"))
            {
                string cPrevPng = Path.Combine(Path.Combine(Path.GetDirectoryName(backgroundImage), "thumb_" + size), Path.GetFileNameWithoutExtension(backgroundImage) + ".png");

                if (File.Exists(cPrevPng))
                    return cPrevPng;
                try
                {
                    // create the bitmap
                    var bmpCack = new SKBitmap(size, size);
                    var mCvs = new SKCanvas(bmpCack);

                    // load the SVG
                    var svg = new SKSvg();
                    svg.Load(backgroundImage);

                    float scale = (float)size / Math.Max(svg.Picture.CullRect.Width, svg.Picture.CullRect.Height);
                    var matrix = SKMatrix.MakeScale(scale, scale);
                    mCvs.DrawPicture(svg.Picture, ref matrix);

                    Directory.CreateDirectory(Path.GetDirectoryName(cPrevPng));
                    using (Stream s = new FileStream(cPrevPng, FileMode.CreateNew))
                    {
                        SKData d = SKImage.FromBitmap(bmpCack).Encode(SKEncodedImageFormat.Png, 100);
                        d.SaveTo(s);
                    }
                    return cPrevPng;
                } 
                catch (Exception ex)
                {
                    sys.LogException(ex);
                }
            }
            return backgroundImage;
            */
        }

        private void StartImageLoader()
        {
            imgLoader = new Thread(() =>
            {
                try
                {
                    WebClient webClient = new WebClient();

                    List<string> doneS = new List<string>();

                    int iCount = imgsToLoad.Count;
                    while (iCount > 0)
                    {
                        try
                        {
                            var item = imgsToLoad[iCount - 1];
                            if (!doneS.Contains(item.destFile))
                            {
                                doneS.Add(item.destFile);
                                Directory.CreateDirectory(Path.GetDirectoryName(item.destFile));
                                webClient.DownloadFile(Secrets.zAppDataUrl + "imageprev.php?filter=" + item.filter + "&group=" + item.group + "&image=" + item.file + "&max=" + item.size, item.destFile + "_");

                                if (File.Exists(item.destFile))
                                    File.Delete(item.destFile);
                                File.Move(item.destFile + "_", item.destFile);

                                ClockFaceLoaded?.Invoke(null, null);
                            }
                        }
                        catch (Exception ex)
                        {
                            xLog.Error(ex);
                        }
                        lock (imgsToLoad)
                        {
                            imgsToLoad.RemoveAt(iCount - 1);
                            iCount = imgsToLoad.Count;
                        }
                    }
                }
                finally
                {
                    imgLoader = null;
                }
            });
            imgLoader.Start();
        }
    }
}
