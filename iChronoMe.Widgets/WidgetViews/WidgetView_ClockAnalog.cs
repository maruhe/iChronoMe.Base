using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using iChronoMe.Core.Classes;
using iChronoMe.Core.Types;

using SkiaSharp;
//using SKSvg = SkiaSharp.Extended.Svg.SKSvg;

namespace iChronoMe.Widgets
{
    public class WidgetView_ClockAnalog
    {
        private string cfgInstance = Guid.NewGuid().ToString();
        public string BackgroundImage { get; set; } = string.Empty;

        public xColor ColorBackground { get; set; } = xColor.Transparent;
        public xColor ColorTickMarks { get; set; } = xColor.Black;
        public TickMarkStyle TickMarkStyle { get; set; } = TickMarkStyle.Dotts;
        public xColor ColorHourHandStroke { get; set; } = xColor.Black;
        public xColor ColorHourHandFill { get; set; } = xColor.Blue;
        public xColor ColorMinuteHandStroke { get; set; } = xColor.Black;
        public xColor ColorMinuteHandFill { get; set; } = xColor.Blue;
        public xColor ColorSecondHandStroke { get; set; } = xColor.Black;
        public xColor ColorSecondHandFill { get; set; } = xColor.Blue;

        public bool ShowHourHand { get; set; } = true;
        public bool ShowMinuteHand { get; set; } = true;
        public bool ShowSecondHand { get; set; } = true;

        public bool FlowHourHand { set; get; } = true;
        public bool FlowMinuteHand { set; get; } = true;
        public bool FlowSecondHand { set; get; } = false;

        //public SKSvg svgHourHand { get; set; } = null;
        //public SKSvg svgMinuteHand { get; set; } = null;
        //public SKSvg svgSecondHand { get; set; } = null;
        //public SKSvg svgCenterDot { get; set; } = null;

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
        SKPath secondHandPath = SKPath.ParseSvgPathData(
            "M 0 10 0 -80");

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

            BackgroundImage = cfg.BackgroundImage;
            TickMarkStyle = cfg.TickMarkStyle;

            ColorBackground = cfg.ColorBackground;
            ColorTickMarks = cfg.ColorTickMarks;
            ColorHourHandStroke = cfg.ColorHourHandStroke;
            ColorHourHandFill = cfg.ColorHourHandFill;
            ColorMinuteHandStroke = cfg.ColorMinuteHandStroke;
            ColorMinuteHandFill = cfg.ColorMinuteHandFill;
            ColorSecondHandStroke = cfg.ColorSecondHandStroke;
            ColorSecondHandFill = cfg.ColorSecondHandFill;

            ShowHourHand = cfg.ShowHours;
            ShowMinuteHand = cfg.ShowMinutes;
            ShowSecondHand = cfg.ShowSeconds;

            FlowHourHand = cfg.FlowHourHand;
            FlowMinuteHand = cfg.FlowMinuteHand;
            FlowSecondHand = cfg.FlowSecondHand;

            //svgHourHand = null;
            //svgMinuteHand = null;
            //svgSecondHand = null;
            //svgCenterDot = null;
            if (!string.IsNullOrEmpty(BackgroundImage) && BackgroundImage.EndsWith(".svg"))
            {
                string cMask = BackgroundImage.Substring(0, BackgroundImage.Length - 4);
                if (File.Exists(cMask + "_hh_.svg")) {
                    //svgHourHand = new SKSvg();
                    //svgHourHand.Load(cMask + "_hh_.svg");
                }
                if (File.Exists(cMask + "_mh_.svg")) {
                    //svgMinuteHand= new SKSvg();
                    //svgMinuteHand.Load(cMask + "_mh_.svg");
                }
                if (File.Exists(cMask + "_sh_.svg")) {
                    //svgSecondHand = new SKSvg();
                    //svgSecondHand.Load(cMask + "_sh_.svg");
                }
                if (File.Exists(cMask + "_cd_.svg")) {
                    //svgCenterDot = new SKSvg();
                    //svgCenterDot.Load(cMask + "_cd_.svg");
                }
            }
        }

        public void DrawCanvas(SKCanvas canvas, DateTime dateTime, int width = 512, int height = 512, bool bDrawBackImage = false, float? nSecondHandOverrideSecond = null)
        {
            var t = dateTime.TimeOfDay;
            DrawCanvas(canvas, t.TotalHours, t.TotalMinutes % 60, t.TotalSeconds % 60, width, height, bDrawBackImage, nSecondHandOverrideSecond);
        }

        public void DrawCanvas(SKCanvas canvas, double hour, double minute, double second, int width = 512, int height = 512, bool bDrawBackImage = false, float? nSecondHandOverrideSecond = null)
        {
            DateTime swStart = DateTime.Now;

            canvas.Clear();
            int x = Math.Min(width, height);

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
                        using (var bitmap = SKBitmap.Decode(BackgroundImage))
                        {
                            var resizedBitmap = bitmap.Resize(new SKImageInfo(x, x), SKFilterQuality.High);
                            canvas.DrawBitmap(resizedBitmap, (width - resizedBitmap.Width) / 2, (height - resizedBitmap.Height) / 2);
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
            if (TickMarkStyle != TickMarkStyle.None && ColorTickMarks.A > 0)
            {
                var tickPaint = new SKPaint{ 
                    Color = ColorTickMarks.ToSKColor(),
                    StrokeCap = SKStrokeCap.Round,
                    IsAntialias = true
                };

                switch (TickMarkStyle)
                {
                    case TickMarkStyle.Dotts:
                        tickPaint.Style = SKPaintStyle.Fill;
                        for (int angle = 0; angle < 360; angle += 6)
                        {
                            canvas.DrawCircle(0, -92, angle % 30 == 0 ? 4 : 2, tickPaint);
                            canvas.RotateDegrees(6);
                        }
                        break;

                    case TickMarkStyle.LinesSquare:
                        tickPaint.StrokeCap = SKStrokeCap.Square;
                        goto case TickMarkStyle.LinesRound;
                    case TickMarkStyle.LinesButt:
                        tickPaint.StrokeCap = SKStrokeCap.Butt;
                        goto case TickMarkStyle.LinesRound;
                    case TickMarkStyle.LinesRound:
                        tickPaint.Style = SKPaintStyle.Stroke;
                        for (int angle = 0; angle < 360; angle += 6)
                        {
                            if (angle % 30 == 0)
                            {
                                tickPaint.StrokeWidth = 4;
                                canvas.DrawLine(0, -90, 0, -94, tickPaint);
                            }
                            else
                            {
                                tickPaint.StrokeWidth = 2;
                                canvas.DrawLine(0, -91, 0, -93, tickPaint);
                            }
                            canvas.RotateDegrees(6);
                        }
                        break;
                }
            }

            if (!FlowHourHand)
                hour = Math.Truncate(hour);
            if (!FlowHourHand)
                hour = Math.Truncate(hour);
            if (!FlowHourHand)
                hour = Math.Truncate(hour);

            // Hour hand
            if (ShowHourHand)
            {
                canvas.Save();
                canvas.RotateDegrees((float)(30 * hour));
                //if (svgHourHand == null)
                {
                    fillPaint.Color = ColorHourHandFill.ToSKColor();
                    strokePaint.Color = ColorHourHandStroke.ToSKColor();
                    canvas.DrawPath(hourHandPath, fillPaint);
                    canvas.DrawPath(hourHandPath, strokePaint);
                } 
                /*else
                {
                    float scale = (float)200F / Math.Max(svgHourHand.Picture.CullRect.Width, svgHourHand.Picture.CullRect.Height);
                    var matrix = new SKMatrix
                    {
                        ScaleX = scale,
                        ScaleY = scale,
                        TransX = -100,
                        TransY = -100,
                        Persp2 = 1,
                    };

                    canvas.DrawPicture(svgHourHand.Picture, ref matrix);
                }*/
                canvas.Restore();
            }

            // Minute hand
            if (ShowMinuteHand)
            {
                canvas.Save();
                canvas.RotateDegrees((float)(6 * minute));
                //if (svgMinuteHand == null)
                {
                    fillPaint.Color = ColorMinuteHandFill.ToSKColor();
                    strokePaint.Color = ColorMinuteHandStroke.ToSKColor();
                    canvas.DrawPath(minuteHandPath, fillPaint);
                    canvas.DrawPath(minuteHandPath, strokePaint);
                }
                /*else
                {
                    float scale = (float)200F / Math.Max(svgMinuteHand.Picture.CullRect.Width, svgMinuteHand.Picture.CullRect.Height);
                    var matrix = new SKMatrix
                    {
                        ScaleX = scale,
                        ScaleY = scale,
                        TransX = -100,
                        TransY = -100,
                        Persp2 = 1,
                    };

                    canvas.DrawPicture(svgMinuteHand.Picture, ref matrix);
                }*/
                canvas.Restore();
            }

            // Second hand
            if (ShowSecondHand)
            {
                canvas.Save();
                canvas.RotateDegrees(6 * (float)second);
                //if (svgSecondHand == null)
                {
                    fillPaint.Color = ColorSecondHandFill.ToSKColor();
                    strokePaint.Color = ColorSecondHandStroke.ToSKColor();
                    canvas.DrawPath(secondHandPath, fillPaint);
                    canvas.DrawPath(secondHandPath, strokePaint);
                }
                /*else
                {
                    float scale = (float)200F / Math.Max(svgSecondHand.Picture.CullRect.Width, svgSecondHand.Picture.CullRect.Height);
                    var matrix = new SKMatrix
                    {
                        ScaleX = scale,
                        ScaleY = scale,
                        TransX = -100,
                        TransY = -100,
                        Persp2 = 1,
                    };

                    canvas.DrawPicture(svgSecondHand.Picture, ref matrix);
                }*/
                canvas.Restore();
            }

            /*if (svgCenterDot != null)
            {
                float scale = (float)200F / Math.Max(svgCenterDot.Picture.CullRect.Width, svgCenterDot.Picture.CullRect.Height);
                var matrix = new SKMatrix
                {
                    ScaleX = scale,
                    ScaleY = scale,
                    TransX = -100,
                    TransY = -100,
                    Persp2 = 1,
                };

                canvas.DrawPicture(svgCenterDot.Picture, ref matrix);
            }*/

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

        public event EventHandler ClockFaceLoaded;

        static List<(string filter, string group, string file, int size, string destFile, string maxFile)> imgsToLoad = new List<(string, string, string, int, string, string)>();
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

                lock (imgsToLoad)
                    imgsToLoad.Add((cFilter, cGroup, cFile, size, cThumbPath, maxFile));

                if (imgLoader == null)
                    StartImageLoader();

                if (File.Exists(backgroundImage) && File.Exists(maxFile))
                {
                    if (File.GetLastWriteTime(backgroundImage).AddMinutes(1) > File.GetLastWriteTime(maxFile))
                        try { File.Delete(maxFile); } catch { }
                }
                
                if (File.Exists(maxFile))
                    return maxFile;
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
                                webClient.DownloadFile(Secrets.zAppImageUrl + "imageprev.php?filter=" + item.filter + "&group=" + item.group + "&image=" + item.file + "&max=" + item.size, item.destFile + "_");

                                if (!File.Exists(item.maxFile) || new FileInfo(item.maxFile).Length < new FileInfo(item.destFile + "_").Length)
                                {
                                    Directory.CreateDirectory(Path.GetDirectoryName(item.maxFile));
                                    File.Copy(item.destFile + "_", item.maxFile, true);
                                    Thread.Sleep(500);
                                }
                                if (File.Exists(item.destFile))
                                    File.Delete(item.destFile);
                                File.Move(item.destFile + "_", item.destFile);
                                Thread.Sleep(500);

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
