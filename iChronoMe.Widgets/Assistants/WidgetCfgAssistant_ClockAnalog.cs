using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

using iChronoMe.Core.Classes;
using iChronoMe.Core.DynamicCalendar;
using iChronoMe.Core.Interfaces;
using iChronoMe.Core.Types;

namespace iChronoMe.Widgets
{
    public class WidgetCfgAssistant_ClockAnalog_Start : WidgetConfigAssistant<WidgetCfg_ClockAnalog>
    {
        public WidgetCfgAssistant_ClockAnalog_Start(WidgetCfgSample<WidgetCfg_ClockAnalog> baseSample)
        {
            Title = localize.LocationType;

            ShowPreviewImage = false;
            var cfg = baseSample == null ? new WidgetCfg_ClockAnalog() : baseSample.GetConfigClone();
            cfg.PositionType = WidgetCfgPositionType.LivePosition;
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(localize.CurrentLocation, (WidgetCfg_ClockAnalog)cfg.Clone()));
            cfg.PositionType = WidgetCfgPositionType.StaticPosition;
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(localize.StaticLocation, cfg));
            NextStepAssistantType = typeof(WidgetCfgAssistant_ClockAnalog_OptionsBase);
        }

        public override void AfterSelect(IUserIO handler, WidgetCfgSample<WidgetCfg_ClockAnalog> sample)
        {
            base.AfterSelect(handler, sample);

            if (sample.WidgetConfig.PositionType != WidgetCfgPositionType.LivePosition)
            {
                var pos = handler.UserSelectMapsLocation().Result;
                if (pos == null)
                {
                    handler.ShowError(localize.error_SloarTimeNeedLocation);
                    handler.TriggerAbortProzess();
                }
                sample.WidgetConfig.PositionType = WidgetCfgPositionType.StaticPosition;
                sample.WidgetConfig.WidgetTitle = pos.Title;
                sample.WidgetConfig.Latitude = pos.Latitude;
                sample.WidgetConfig.Longitude = pos.Longitude;
            }
        }
    }

    public class WidgetCfgAssistant_ClockAnalog_OptionsBase : WidgetConfigAssistant<WidgetCfg_ClockAnalog>
    {
        public WidgetCfgAssistant_ClockAnalog_OptionsBase(WidgetCfgSample<WidgetCfg_ClockAnalog> baseSample)
        {
            Title = localize.title_EditWidget;
            ShowPreviewImage = false;
            ShowFirstPreviewImage = true;
            BaseSample = baseSample;

            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(localize.action_SaveAndQuit, BaseSample.GetConfigClone()));
            var cfg = BaseSample.GetConfigClone();
            var cfgPrev = BaseSample.GetConfigClone();
            cfgPrev.ColorBackground = xColor.HotPink;
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(localize.Background, null, cfg, typeof(WidgetCfgAssistant_ClockAnalog_BackgroundImage), cfgPrev));
            cfg = BaseSample.GetConfigClone();
            cfgPrev = BaseSample.GetConfigClone();
            cfgPrev.ColorHourHandStroke = cfgPrev.ColorMinuteHandFill = cfgPrev.ColorMinuteHandStroke = cfgPrev.ColorMinuteHandFill =
                cfgPrev.ColorSecondHandStroke = cfgPrev.ColorSecondHandFill = xColor.HotPink;
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(localize.ClockHandColors, null, cfg, typeof(WidgetCfgAssistant_ClockAnalog_HandColorType), cfgPrev));
            //if (string.IsNullOrEmpty(cfg.BackgroundImage))
            {
                cfg = BaseSample.GetConfigClone();
                cfgPrev = BaseSample.GetConfigClone();
                cfgPrev.ColorTickMarks = xColor.HotPink;
                Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(localize.ClockFace, null, cfg, typeof(WidgetCfgAssistant_ClockAnalog_TickMarks), cfgPrev));
            }

            NextStepAssistantType = null;
        }

        public override void AfterSelect(IUserIO handler, WidgetCfgSample<WidgetCfg_ClockAnalog> sample)
        {
            base.AfterSelect(handler, sample);

            NextStepAssistantType = (sample.Tag as Type);
        }

    }

    public class WidgetCfgAssistant_ClockAnalog_BackgroundImage : WidgetConfigAssistant<WidgetCfg_ClockAnalog>
    {
        string cImageDir = "";

        public WidgetCfgAssistant_ClockAnalog_BackgroundImage(WidgetCfgSample<WidgetCfg_ClockAnalog> baseSample)
        {
            Title = localize.Background;
            BaseSample = baseSample;

            cImageDir = ImageLoader.GetImagePathThumb(ImageLoader.filter_clockfaces);

            //create samples on PerformPreperation()

            NextStepAssistantType = typeof(WidgetCfgAssistant_ClockAnalog_OptionsBase);
        }

        private WidgetCfg_ClockAnalog EmptyBackSample
        {
            get
            {
                var cfg = (WidgetCfg_ClockAnalog)(BaseSample.WidgetConfig.Clone());
                cfg.BackgroundImage = string.Empty;
                if (cfg.ColorBackground == xColor.Transparent && cfg.ColorTickMarks == xColor.Transparent)
                {
                    var x = new WidgetCfg_ClockAnalog();
                    cfg.ColorBackground = x.ColorBackground;
                    cfg.ColorTickMarks = x.ColorTickMarks;
                }
                return cfg;
            }
        }

        public override bool NeedsPreperation()
        {
            return AppConfigHolder.MainConfig.LastCheckClockFaces.AddDays(1) < DateTime.Now;
        }

        public override void PerformPreperation(IUserIO handler)
        {
            if (NeedsPreperation())
                ImageLoader.CheckImageThumbCache(handler, ImageLoader.filter_clockfaces);

            Samples.Clear();
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(localize.SingleColor, EmptyBackSample));
            LoadSamples();
            base.PerformPreperation(handler);
        }

        void LoadSamples()
        {
            try
            {
                string[] cFiles = Directory.GetFiles(cImageDir, "*.png");
                foreach (string cFile in cFiles)
                {
                    if (!Path.GetFileNameWithoutExtension(cFile).EndsWith("_"))
                    {
                        WidgetCfg_ClockAnalog cfg = BaseSample.GetConfigClone();
                        cfg.BackgroundImage = cFile;
                        cfg.ColorBackground = xColor.Transparent;
                        cfg.ColorTickMarks = xColor.Transparent;
                        Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(Path.GetFileNameWithoutExtension(cFile).Replace("_", " "), cfg));
                    }
                }
            }
            catch { }
        }

        public override void AfterSelect(IUserIO handler, WidgetCfgSample<WidgetCfg_ClockAnalog> sample)
        {
            base.AfterSelect(handler, sample);

            var cfg = sample.WidgetConfig;

            if (string.IsNullOrEmpty(cfg.BackgroundImage))
                NextStepAssistantType = typeof(WidgetCfgAssistant_ClockAnalog_BackgroundColor);

            if (!string.IsNullOrEmpty(cfg.BackgroundImage) && cfg.BackgroundImage.Contains("/thumb_"))
            {
                string cFullSizeImg = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(cfg.BackgroundImage)), Path.GetFileName(cfg.BackgroundImage));
                if (File.Exists(cFullSizeImg))
                    cfg.BackgroundImage = cFullSizeImg;
                else
                {
                    try
                    {
                        handler.StartProgress("download full size image..");

                        WebClient webClient = new WebClient();
                        webClient.DownloadFile(ImageLoader.cUrlDir + Path.GetFileName(cfg.BackgroundImage), cFullSizeImg + "_");

                        if (File.Exists(cFullSizeImg))
                            File.Delete(cFullSizeImg);
                        File.Move(cFullSizeImg + "_", cFullSizeImg);

                        cfg.BackgroundImage = cFullSizeImg;
                    }
                    catch (Exception ex)
                    {
                        handler.ShowError(ex.Message);
                    }
                    finally
                    {
                        handler.SetProgressDone();
                    }
                }
            }
        }
    }

    public class WidgetCfgAssistant_ClockAnalog_BackgroundColor : WidgetConfigAssistant<WidgetCfg_ClockAnalog>
    {
        public WidgetCfgAssistant_ClockAnalog_BackgroundColor(WidgetCfgSample<WidgetCfg_ClockAnalog> baseSample)
        {
            Title = localize.BackgroundColor;
            BaseSample = baseSample;
            AllowCustom = true;
            NextStepAssistantType = typeof(WidgetCfgAssistant_ClockAnalog_OptionsBase);

            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(localize.current, BaseSample.GetConfigClone()));
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(localize.transparent, BaseSample.GetConfigClone()));

            int i = 0;
            List<string> clrS = new List<string>();
            foreach (var clrs in DynamicColors.SampleColorSetS)
            {
                i++;
                var clr = xColor.FromHex(clrs[0]);
                if (!clrS.Contains(clr.HexString))
                {
                    clrS.Add(clr.HexString);
                    var cfg = BaseSample.GetConfigClone();
                    cfg.ColorBackground = clr;

                    Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(clr.HexString, cfg));
                }
            }
        }

        public override async void ExecCustom(IUserIO handler)
        {
            base.ExecCustom(handler);
            NextStepAssistantType = typeof(WidgetCfgAssistant_ClockAnalog_OptionsBase);

            var cfg = BaseSample.GetConfigClone();

            var clr = await handler.UserSelectColor(localize.BackgroundColor, cfg.ColorBackground);
            if (clr == null)
            {
                handler.TriggerNegativeButtonClicked();
                return;
            }
            cfg.ColorBackground = clr.Value;
            BaseSample = new WidgetCfgSample<WidgetCfg_ClockAnalog>("custom", cfg);
            handler.TriggerPositiveButtonClicked();
        }
    }

    public class WidgetCfgAssistant_ClockAnalog_HandColorType : WidgetConfigAssistant<WidgetCfg_ClockAnalog>
    {
        public WidgetCfgAssistant_ClockAnalog_HandColorType(WidgetCfgSample<WidgetCfg_ClockAnalog> baseSample)
        {
            Title = localize.HandColorTypes;
            BaseSample = baseSample;
            AllowCustom = true;
            NextStepAssistantType = typeof(WidgetCfgAssistant_ClockAnalog_HandColors);

            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(localize.current, BaseSample.GetConfigClone()));
            LoadSamples();
        }

        void LoadSamples()
        {
            var clrs = DynamicColors.SampleColorSetS[5];

            AddSample(localize.colorcfgBlackTransparent, xColor.Black, xColor.Transparent, xColor.Black, xColor.Transparent, xColor.Black, "SetDone");

            AddSample(localize.colorcfgBlackWhite, xColor.Black, xColor.White, xColor.Black, xColor.White, xColor.Black, "SetDone");

            AddSample(localize.colorcfgBlack, xColor.Black, xColor.Black, xColor.Black, xColor.Black, xColor.Black, "SetDone");

            AddSample(localize.colorcfgBlackCustom, xColor.Black, clrs[1], xColor.Black, clrs[1], xColor.Black, new object[] { xColor.Black, 1, xColor.Black, 1, xColor.Black });

            AddSample(localize.colorcfgWhiteTransparent, xColor.White, xColor.Transparent, xColor.White, xColor.Transparent, xColor.White, "SetDone");

            AddSample(localize.colorcfgWhiteBlack, xColor.White, xColor.Black, xColor.White, xColor.Black, xColor.White, "SetDone");

            AddSample(localize.colorcfgWhite, xColor.White, xColor.White, xColor.White, xColor.White, xColor.White, "SetDone");

            AddSample(localize.colorcfgWhiteCurstom, xColor.White, clrs[1], xColor.White, clrs[1], xColor.White, new object[] { xColor.White, 1, xColor.White, 1, xColor.White });

            AddSample(localize.colorcfgSingleColor, clrs[0], xColor.Transparent, clrs[0], xColor.Transparent, clrs[0], new object[] { 0, xColor.Transparent, 0, xColor.Transparent, 0 });

            AddSample(localize.colorcfgSingleColorFilled, clrs[0], clrs[1], clrs[0], clrs[1], clrs[0], new object[] { 0, 1, 0, 1, 0 });

            AddSample(localize.colorcfgMultiColor, clrs[0], xColor.Transparent, clrs[2], xColor.Transparent, clrs[4], new object[] { 0, xColor.Transparent, 2, xColor.Transparent, 4 });

            AddSample(localize.colorcfgMultiColorFilled, clrs[0], clrs[1], clrs[2], clrs[3], clrs[4], new object[] { 0, 1, 2, 3, 4 });

        }

        void AddSample(string cTitle, xColor clrHourStroke, xColor clrHourFill, xColor clrMinuteStroke, xColor clrMinuteFill, xColor clrSecond, object tag = null)
        {
            var cfg = BaseSample.GetConfigClone();
            cfg.ColorHourHandStroke = clrHourStroke;
            cfg.ColorHourHandFill = clrHourFill;
            cfg.ColorMinuteHandStroke = clrMinuteStroke;
            cfg.ColorMinuteHandFill = clrMinuteFill;
            cfg.ColorSecondHandStroke = clrSecond;

            List<xColor> clrS = new List<xColor>();
            clrS.Add(clrHourStroke);
            if (!clrS.Contains(clrHourFill))
                clrS.Add(clrHourFill);
            if (!clrS.Contains(clrMinuteStroke))
                clrS.Add(clrMinuteStroke);
            if (!clrS.Contains(clrMinuteFill))
                clrS.Add(clrMinuteFill);
            if (cfg.ShowSeconds && !clrS.Contains(clrSecond))
                clrS.Add(clrSecond);

            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(cTitle, clrS.ToArray(), cfg, tag));
        }

        public override void AfterSelect(IUserIO handler, WidgetCfgSample<WidgetCfg_ClockAnalog> sample)
        {
            base.AfterSelect(handler, sample);
            if ("SetDone".Equals(sample.Tag))
                NextStepAssistantType = typeof(WidgetCfgAssistant_ClockAnalog_OptionsBase);
        }

        public override async void ExecCustom(IUserIO handler)
        {
            base.ExecCustom(handler);
            NextStepAssistantType = typeof(WidgetCfgAssistant_ClockAnalog_OptionsBase);

            bool bIsValid = false;
            var cfg = BaseSample.GetConfigClone();
            try
            {
                var clr = await handler.UserSelectColor(localize.HourHandStroke, cfg.ColorHourHandStroke);
                if (clr == null)
                    return;
                cfg.ColorHourHandStroke = cfg.ColorMinuteHandStroke = cfg.ColorSecondHandStroke = clr.Value;
                bIsValid = true;

                clr = await handler.UserSelectColor(localize.HourHandFill, cfg.ColorHourHandFill);
                if (clr == null)
                    return;
                cfg.ColorHourHandFill = cfg.ColorMinuteHandFill = cfg.ColorSecondHandFill = clr.Value;

                clr = await handler.UserSelectColor(localize.MinuteHandStroke, cfg.ColorMinuteHandStroke);
                if (clr == null)
                    return;
                cfg.ColorMinuteHandStroke = clr.Value;

                clr = await handler.UserSelectColor(localize.MinuteHandFill, cfg.ColorMinuteHandFill);
                if (clr == null)
                    return;
                cfg.ColorMinuteHandFill = clr.Value;

                if (cfg.ShowSeconds)
                {
                    clr = await handler.UserSelectColor(localize.SecondHandStroke, cfg.ColorSecondHandStroke);
                    if (clr == null)
                        return;
                    cfg.ColorSecondHandStroke = clr.Value;
                }
            }
            catch
            {
                bIsValid = false;
            }
            finally
            {
                if (bIsValid)
                {
                    BaseSample = new WidgetCfgSample<WidgetCfg_ClockAnalog>("custom", cfg);
                    handler.TriggerPositiveButtonClicked();
                }
                else
                    handler.TriggerNegativeButtonClicked();
            }
        }
    }

    public class WidgetCfgAssistant_ClockAnalog_HandColors : WidgetConfigAssistant<WidgetCfg_ClockAnalog>
    {
        public WidgetCfgAssistant_ClockAnalog_HandColors(WidgetCfgSample<WidgetCfg_ClockAnalog> baseSample)
        {
            Title = localize.HandColors;
            BaseSample = baseSample;
            ShowColors = true;
            AllowCustom = true;
            NextStepAssistantType = typeof(WidgetCfgAssistant_ClockAnalog_OptionsBase);

            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(localize.current, BaseSample.GetConfigClone()));
            LoadSamples();
        }

        void LoadSamples()
        {
            int i = 0;
            foreach (var clrS in DynamicColors.SampleColorSetS)
            {
                i++;
                AddSample("sample " + i, clrS);
            }
        }

        void AddSample(string cTitle, string[] sampleClrS)
        {
            if (!(BaseSample.Tag is object[]) || (BaseSample.Tag as object[]).Length != 5)
                return;

            object[] mapping = BaseSample.Tag as object[];

            xColor clrHourStroke = mapping[0] is xColor ? (xColor)mapping[0] : (xColor)sampleClrS[(int)mapping[0]];
            xColor clrHourFill = mapping[1] is xColor ? (xColor)mapping[1] : (xColor)sampleClrS[(int)mapping[1]];
            xColor clrMinuteStroke = mapping[2] is xColor ? (xColor)mapping[2] : (xColor)sampleClrS[(int)mapping[2]];
            xColor clrMinuteFill = mapping[3] is xColor ? (xColor)mapping[3] : (xColor)sampleClrS[(int)mapping[3]];
            xColor clrSecond = mapping[4] is xColor ? (xColor)mapping[4] : (xColor)sampleClrS[(int)mapping[4]];

            if (clrHourStroke == BaseSample.WidgetConfig.ColorBackground ||
                clrMinuteStroke == BaseSample.WidgetConfig.ColorBackground ||
                clrSecond == BaseSample.WidgetConfig.ColorBackground)
                return;

            var cfg = BaseSample.GetConfigClone();
            cfg.ColorHourHandStroke = clrHourStroke;
            cfg.ColorHourHandFill = clrHourFill;
            cfg.ColorMinuteHandStroke = clrMinuteStroke;
            cfg.ColorMinuteHandFill = clrMinuteFill;
            cfg.ColorSecondHandStroke = clrSecond;

            List<xColor> clrS = new List<xColor>();
            clrS.Add(clrHourStroke);
            if (!clrS.Contains(clrHourFill))
                clrS.Add(clrHourFill);
            if (!clrS.Contains(clrMinuteStroke))
                clrS.Add(clrMinuteStroke);
            if (!clrS.Contains(clrMinuteFill))
                clrS.Add(clrMinuteFill);
            if (cfg.ShowSeconds && !clrS.Contains(clrSecond))
                clrS.Add(clrSecond);

            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(cTitle, clrS.ToArray(), cfg));
        }

        public override async void ExecCustom(IUserIO handler)
        {
            base.ExecCustom(handler);
            NextStepAssistantType = typeof(WidgetCfgAssistant_ClockAnalog_OptionsBase);

            bool bIsValid = false;
            var cfg = BaseSample.GetConfigClone();
            try
            {
                xColor? clr = null;

                object[] mapping = BaseSample.Tag as object[];

                //here only ask for undefined Colors !! 
                if ((mapping[0] is xColor))
                    cfg.ColorHourHandStroke = (xColor)mapping[0];
                else
                {
                    clr = await handler.UserSelectColor(localize.HourHandStroke, cfg.ColorHourHandStroke);
                    if (clr == null)
                        return;
                    cfg.ColorHourHandStroke = cfg.ColorMinuteHandStroke = cfg.ColorSecondHandStroke = clr.Value;
                    bIsValid = true;
                }

                if ((mapping[1] is xColor))
                    cfg.ColorHourHandFill = (xColor)mapping[1];
                else
                {
                    clr = await handler.UserSelectColor(localize.HourHandFill, cfg.ColorHourHandFill);
                    if (clr == null)
                        return;
                    cfg.ColorHourHandFill = cfg.ColorMinuteHandFill = cfg.ColorSecondHandFill = clr.Value;
                    bIsValid = true;
                }

                if (!(mapping[2] is xColor) && !Equals(mapping[2], mapping[0]))
                {
                    clr = await handler.UserSelectColor(localize.MinuteHandStroke, cfg.ColorMinuteHandStroke);
                    if (clr == null)
                        return;
                    cfg.ColorMinuteHandStroke = clr.Value;
                    bIsValid = true;
                }

                if (!(mapping[3] is xColor) && !Equals(mapping[3], mapping[1]))
                {
                    clr = await handler.UserSelectColor(localize.MinuteHandFill, cfg.ColorMinuteHandStroke);
                    if (clr == null)
                        return;
                    cfg.ColorMinuteHandStroke = clr.Value;
                    bIsValid = true;
                }

                if (cfg.ShowSeconds)
                {
                    if (!(mapping[4] is xColor) && !Equals(mapping[4], mapping[0]))
                    {
                        clr = await handler.UserSelectColor(localize.SecondHandStroke, cfg.ColorSecondHandStroke);
                        if (clr == null)
                            return;
                        cfg.ColorSecondHandStroke = clr.Value;
                        bIsValid = true;
                    }
                }
            }
            catch
            {
                bIsValid = false;
            }
            finally
            {
                if (bIsValid)
                {
                    BaseSample = new WidgetCfgSample<WidgetCfg_ClockAnalog>("custom", cfg);
                    handler.TriggerPositiveButtonClicked();
                }
                else
                    handler.TriggerNegativeButtonClicked();
            }
        }
    }


    public class WidgetCfgAssistant_ClockAnalog_TickMarks : WidgetConfigAssistant<WidgetCfg_ClockAnalog>
    {
        public WidgetCfgAssistant_ClockAnalog_TickMarks(WidgetCfgSample<WidgetCfg_ClockAnalog> baseSample)
        {
            Title = localize.TickMarks;
            BaseSample = baseSample;
            NextStepAssistantType = typeof(WidgetCfgAssistant_ClockAnalog_TickMarkColors);

            LoadSamples();
        }

        void LoadSamples()
        {
            foreach (var o in Enum.GetValues(typeof(TickMarkStyle)))
            {
                var cfg = BaseSample.GetConfigClone();
                cfg.TickMarkStyle = (TickMarkStyle)o;
                Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(o.ToString(), cfg));
            }
        }

        public override void AfterSelect(IUserIO handler, WidgetCfgSample<WidgetCfg_ClockAnalog> sample)
        {
            base.AfterSelect(handler, sample);

            if (sample.WidgetConfig.TickMarkStyle == TickMarkStyle.None)
                NextStepAssistantType = typeof(WidgetCfgAssistant_ClockAnalog_OptionsBase);
        }
    }

    public class WidgetCfgAssistant_ClockAnalog_TickMarkColors : WidgetConfigAssistant<WidgetCfg_ClockAnalog>
    {
        public WidgetCfgAssistant_ClockAnalog_TickMarkColors(WidgetCfgSample<WidgetCfg_ClockAnalog> baseSample)
        {
            Title = localize.TickMarksColor;
            BaseSample = baseSample;
            AllowCustom = true;
            NextStepAssistantType = typeof(WidgetCfgAssistant_ClockAnalog_OptionsBase);

            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(localize.current, BaseSample.GetConfigClone()));
            LoadSamples();
        }

        void LoadSamples()
        {
            int i = 0;
            List<string> cDone = new List<string>();
            foreach (var clrS in DynamicColors.SampleColorSetS)
            {
                i++;
                var cfg = BaseSample.GetConfigClone();
                cfg.ColorTickMarks = (xColor)clrS[0];
                string cTitle = cfg.ColorTickMarks.HexString;
                if (cDone.Contains(cTitle))
                    continue;
                cDone.Add(cTitle);
                Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(cTitle, cfg));
            }
        }

        public override async void ExecCustom(IUserIO handler)
        {
            base.ExecCustom(handler);
            NextStepAssistantType = typeof(WidgetCfgAssistant_ClockAnalog_OptionsBase);

            var cfg = BaseSample.GetConfigClone();

            var clr = await handler.UserSelectColor(localize.TickMarksColor, cfg.ColorTickMarks);
            if (clr == null)
            {
                handler.TriggerNegativeButtonClicked();
                return;
            }
            cfg.ColorTickMarks = clr.Value;
            BaseSample = new WidgetCfgSample<WidgetCfg_ClockAnalog>("custom", cfg);
            handler.TriggerPositiveButtonClicked();
        }
    }
}