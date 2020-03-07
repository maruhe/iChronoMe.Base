using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using iChronoMe.Core;
using iChronoMe.Core.Classes;
using iChronoMe.Core.DynamicCalendar;
using iChronoMe.Core.Extentions;
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
            cfg.SetDefaultColors();
            cfg.PositionType = WidgetCfgPositionType.LivePosition;
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(localize.CurrentLocation, (WidgetCfg_ClockAnalog)cfg.Clone()));
            cfg.PositionType = WidgetCfgPositionType.StaticPosition;
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(localize.StaticLocation, (WidgetCfg_ClockAnalog)cfg.Clone()));
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
            if (ClockHandConfig.Count > 1)
                NextStepAssistantType = typeof(WidgetCfgAssistant_ClockAnalog_StartStyle);
        }
    }

    public class WidgetCfgAssistant_ClockAnalog_StartStyle : WidgetConfigAssistant<WidgetCfg_ClockAnalog>
    {
        public WidgetCfgAssistant_ClockAnalog_StartStyle(WidgetCfgSample<WidgetCfg_ClockAnalog> baseSample)
        {
            Title = localize.Style;
            BaseSample = baseSample;

            var cfg = BaseSample.GetConfigClone();
            cfg.AllHandConfigID = "_";
            cfg.ColorBackground = xColor.Transparent;
            cfg.TickMarkStyle = TickMarkStyle.Circle;
            cfg.ColorHourHandStroke = cfg.ColorHourHandFill = cfg.ColorMinuteHandStroke = cfg.ColorMinuteHandFill =
                 cfg.ColorSecondHandStroke = cfg.ColorSecondHandFill = cfg.ColorTickMarks = xColor.White;

            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>("simple white", cfg));

            cfg = (WidgetCfg_ClockAnalog)cfg.Clone();
            cfg.ColorHourHandStroke = cfg.ColorHourHandFill = cfg.ColorMinuteHandStroke = cfg.ColorMinuteHandFill =
                 cfg.ColorSecondHandStroke = cfg.ColorSecondHandFill = cfg.ColorTickMarks = xColor.MaterialTextBlack;

            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>("simple black", cfg));

            cfg = BaseSample.GetConfigClone();
            cfg.AllHandConfigID = "cute";

            cfg.ColorBackground = xColor.MaterialTeal.WithAlpha(.7);
            cfg.ColorHourHandStroke = cfg.ColorHourHandFill = xColor.MaterialOrange;
            cfg.ColorMinuteHandStroke = cfg.ColorMinuteHandFill = xColor.MaterialIndigo;
            cfg.ColorSecondHandStroke = cfg.ColorSecondHandFill = xColor.MaterialGreen;
            cfg.ColorTickMarks = xColor.MaterialBlueGray;

            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(localize.MoreOptionsAndColors, null, BaseSample.GetConfigClone(), typeof(WidgetCfgAssistant_ClockAnalog_OptionsBase), cfg));

            NextStepAssistantType = null;
        }

        public override void AfterSelect(IUserIO handler, WidgetCfgSample<WidgetCfg_ClockAnalog> sample)
        {
            base.AfterSelect(handler, sample);

            NextStepAssistantType = (sample.Tag as Type);
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
            if (ClockHandConfig.Count > 1)
                Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(localize.HandTypes, null, cfg, typeof(WidgetCfgAssistant_ClockAnalog_HandType)));
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
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(localize.TimeType, null, BaseSample.GetConfigClone(), typeof(WidgetCfgAssistant_ClockAnalog_WidgetTimeType)));            

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

            NextStepAssistantType = typeof(WidgetCfgAssistant_ClockAnalog_BackgroundImageGroup);
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
                cfg.SetDefaultColors();
                return cfg;
            }
        }

        public override void PerformPreperation(IUserIO handler)
        {
            if (sys.Debugmode || AppConfigHolder.MainConfig.LastCheckClockFaces.AddDays(1) < DateTime.Now)
                ImageLoader.CheckImageThumbCache(handler, ImageLoader.filter_clockfaces);
            ClockHandConfig.CheckUpdateLocalData(handler);

            Samples.Clear();
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(localize.SingleColor, EmptyBackSample));
            LoadSamples();
            base.PerformPreperation(handler);
        }

        void LoadSamples()
        {
            try
            {
                var rnd = new Random(DateTime.Now.Millisecond);
                var cGroups = new List<string>(Directory.GetDirectories(cImageDir));
                cGroups.Sort();
                foreach (string cGroup in cGroups)
                {
                    string[] cFiles = Directory.GetFiles(cGroup, "*.png");
                    if (cFiles.Length > 0)
                    {
                        WidgetCfg_ClockAnalog cfg = BaseSample.GetConfigClone();
                        cfg.BackgroundImage = cFiles[rnd.Next(cFiles.Length - 1)];
                        cfg.ColorBackground = xColor.Transparent;
                        cfg.ColorTickMarks = xColor.Transparent;

                        cfg.SetDefaultColors();

                        string cDefaultHands = ClockHandConfig.GetDefaultID(Path.GetFileNameWithoutExtension(cfg.BackgroundImage));
                        if (!string.IsNullOrEmpty(cDefaultHands))
                        {
                            cfg.AllHandConfigID = cDefaultHands;
                            cfg.SetDefaultColors();
                        }
                        Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(Path.GetFileNameWithoutExtension(cGroup).Replace("_", " "), cfg));
                    }
                }
            }
            catch { }
        }

        public override void AfterSelect(IUserIO handler, WidgetCfgSample<WidgetCfg_ClockAnalog> sample)
        {
            base.AfterSelect(handler, sample);

            if (string.IsNullOrEmpty(sample.WidgetConfig.BackgroundImage))
                NextStepAssistantType = typeof(WidgetCfgAssistant_ClockAnalog_BackgroundColor);
        }
    }
    public class WidgetCfgAssistant_ClockAnalog_BackgroundImageGroup : WidgetConfigAssistant<WidgetCfg_ClockAnalog>
    {
        string cImageDir = "";

        public WidgetCfgAssistant_ClockAnalog_BackgroundImageGroup(WidgetCfgSample<WidgetCfg_ClockAnalog> baseSample)
        {
            Title = localize.Background;
            BaseSample = baseSample;

            cImageDir = Path.GetDirectoryName(BaseSample.WidgetConfig.BackgroundImage);

            NextStepAssistantType = typeof(WidgetCfgAssistant_ClockAnalog_OptionsBase);

            LoadSamples();
        }

        void LoadSamples()
        {
            try
            {
                string[] cFiles = Directory.GetFiles(cImageDir, "*.png");
                foreach (string cFile in cFiles)
                {
                    WidgetCfg_ClockAnalog cfg = BaseSample.GetConfigClone();
                    cfg.BackgroundImage = cFile;
                    cfg.ColorBackground = xColor.Transparent;
                    cfg.ColorTickMarks = xColor.Transparent;

                    cfg.SetDefaultColors();

                    string cDefaultHands = ClockHandConfig.GetDefaultID(Path.GetFileNameWithoutExtension(cfg.BackgroundImage));
                    if (!string.IsNullOrEmpty(cDefaultHands))
                    {
                        cfg.AllHandConfigID = cDefaultHands;
                        cfg.SetDefaultColors();
                    }
                    Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(sys.Debugmode ? Path.GetFileNameWithoutExtension(cFile) : " ", cfg));
                }
            }
            catch { }
        }

        public override void AfterSelect(IUserIO handler, WidgetCfgSample<WidgetCfg_ClockAnalog> sample)
        {
            base.AfterSelect(handler, sample);

            if (sample.WidgetConfig.BackImageAllowsBackColor)
                NextStepAssistantType = typeof(WidgetCfgAssistant_ClockAnalog_BackgroundColor);
            return;
            var cfg = sample.WidgetConfig;

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
                        webClient.DownloadFile("xxx" + Path.GetFileName(cfg.BackgroundImage), cFullSizeImg + "_");

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

            if (baseSample.WidgetConfig.ColorBackground.A > 0)
                Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(localize.current, BaseSample.GetConfigClone()));
            var cfg = BaseSample.GetConfigClone();
            cfg.ColorBackground = xColor.Transparent;
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(localize.transparent, cfg));

            int i = 0;
            List<string> clrS = new List<string>();
            foreach (var clrs in DynamicColors.SampleColorSetS)
            {
                i++;
                var clr = xColor.FromHex(clrs[0]);
                if (!clrS.Contains(clr.HexString))
                {
                    clrS.Add(clr.HexString);
                    cfg = BaseSample.GetConfigClone();
                    cfg.ColorBackground = clr;
                    cfg.SetDefaultColors();

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

    public class WidgetCfgAssistant_ClockAnalog_HandType : WidgetConfigAssistant<WidgetCfg_ClockAnalog>
    {
        public WidgetCfgAssistant_ClockAnalog_HandType(WidgetCfgSample<WidgetCfg_ClockAnalog> baseSample)
        {
            Title = localize.HandTypes;
            BaseSample = baseSample;
            NextStepAssistantType = typeof(WidgetCfgAssistant_ClockAnalog_HandColorType);
        }
        public override void PerformPreperation(IUserIO handler)
        {
            ClockHandConfig.CheckUpdateLocalData(handler);
            LoadSamples();
        }

        public void LoadSamples()
        {
            foreach (string id in ClockHandConfig.GetAllIds())
            {
                var cfg = BaseSample.GetConfigClone();
                cfg.AllHandConfigID = id;
                var prev = (WidgetCfg_ClockAnalog)cfg.Clone();
                prev.SetDefaultColors();
                Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(sys.Debugmode ? id
                    + " " + cfg.HourHandConfig.AllowCustomHourStroke.ToInt() + cfg.HourHandConfig.AllowCustomHourFill.ToInt()
                    + " " + cfg.MinuteHandConfig.AllowCustomMinuteStroke.ToInt() + cfg.MinuteHandConfig.AllowCustomMinuteFill.ToInt()
                    + " " + cfg.SecondHandConfig.AllowCustomSecondStroke.ToInt() + cfg.SecondHandConfig.AllowCustomSecondFill.ToInt()
                    : " ", null, cfg, null, prev));
            }
        }

        public override void AfterSelect(IUserIO handler, WidgetCfgSample<WidgetCfg_ClockAnalog> sample)
        {
            base.AfterSelect(handler, sample);

            if (sample.WidgetConfig.ShowSeconds && sample.WidgetConfig.SecondHandConfig.SecondPathList == null)
                NextStepAssistantType = typeof(WidgetCfgAssistant_ClockAnalog_SecondHandSelector);
        }
    }

    public class WidgetCfgAssistant_ClockAnalog_SecondHandSelector : WidgetConfigAssistant<WidgetCfg_ClockAnalog>
    {
        public WidgetCfgAssistant_ClockAnalog_SecondHandSelector(WidgetCfgSample<WidgetCfg_ClockAnalog> baseSample)
        {
            Title = localize.HandColorTypes;
            BaseSample = baseSample;
            NextStepAssistantType = typeof(WidgetCfgAssistant_ClockAnalog_HandColorType);

            var cfg = BaseSample.GetConfigClone();
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(localize.withoutSecondHand, cfg));

            foreach (string cId in ClockHandConfig.GetAllIds())
            {
                var chc = ClockHandConfig.Get(cId);
                if (chc.SecondPathList != null)
                {
                    cfg = BaseSample.GetConfigClone();
                    cfg.SecondHandConfigID = cId;
                    cfg.SetDefaultColors();
                    Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(" ", cfg));
                }
            }
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

            var cfg = BaseSample.GetConfigClone();
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(localize.current, cfg, "SetDone"));

            var def = (WidgetCfg_ClockAnalog)cfg.Clone();
            def.SetDefaultColors();
            if (cfg.ColorHourHandStroke != def.ColorHourHandStroke ||
                cfg.ColorMinuteHandStroke != def.ColorMinuteHandStroke ||
                cfg.ColorSecondHandStroke != def.ColorSecondHandStroke ||
                cfg.ColorHourHandFill != def.ColorHourHandFill ||
                cfg.ColorMinuteHandFill != def.ColorMinuteHandFill)
                Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(localize.Default, def, "SetDone"));

            LoadSamples();
        }

        void LoadSamples()
        {
            var clrs = DynamicColors.SampleColorSetS[5];

            var cfg = BaseSample.GetConfigClone();
            bool bHasFilled = cfg.HourHandConfig.AllowCustomHourFill || cfg.MinuteHandConfig.AllowCustomMinuteFill || cfg.SecondHandConfig.AllowCustomSecondFill;

            if (bHasFilled && cfg.HourHandConfig.AllowCustomHourStroke)
            {
                AddSample(localize.colorcfgBlackTransparent, xColor.Black, xColor.Transparent, xColor.Black, xColor.Transparent, xColor.Black, xColor.Transparent, "SetDone");

                AddSample(localize.colorcfgBlackWhite, xColor.Black, xColor.White, xColor.Black, xColor.White, xColor.Black, xColor.White, "SetDone");

                AddSample(localize.colorcfgBlack, xColor.Black, xColor.Black, xColor.Black, xColor.Black, xColor.Black, xColor.Black, "SetDone");

                AddSample(localize.colorcfgBlackCustom, xColor.Black, clrs[1], xColor.Black, clrs[1], xColor.Black, clrs[1], new object[] { xColor.Black, 1, xColor.Black, 1, xColor.Black, 1 });

            }
            else
                AddSample(localize.colorcfgBlack, xColor.Black, xColor.Black, xColor.Black, xColor.Black, xColor.Black, xColor.Black, "SetDone");

            if (bHasFilled && cfg.HourHandConfig.AllowCustomHourStroke)
            {
                AddSample(localize.colorcfgWhiteTransparent, xColor.White, xColor.Transparent, xColor.White, xColor.Transparent, xColor.White, xColor.Transparent, "SetDone");

                AddSample(localize.colorcfgWhiteBlack, xColor.White, xColor.Black, xColor.White, xColor.Black, xColor.White, xColor.Black, "SetDone");

                AddSample(localize.colorcfgWhite, xColor.White, xColor.White, xColor.White, xColor.White, xColor.White, xColor.White, "SetDone");

                AddSample(localize.colorcfgWhiteCurstom, xColor.White, clrs[1], xColor.White, clrs[1], xColor.White, clrs[1], new object[] { xColor.White, 1, xColor.White, 1, xColor.White, 1 });
            }
            else
                AddSample(localize.colorcfgWhite, xColor.White, xColor.White, xColor.White, xColor.White, xColor.White, xColor.White, "SetDone");

            if (cfg.HourHandConfig.AllowCustomHourStroke)
                AddSample(localize.colorcfgSingleColor, clrs[0], xColor.Transparent, clrs[0], xColor.Transparent, clrs[0], xColor.Transparent, new object[] { 0, xColor.Transparent, 0, xColor.Transparent, 0, xColor.Transparent });

            if (bHasFilled || !cfg.HourHandConfig.AllowCustomHourStroke)
                AddSample(localize.colorcfgSingleColorFilled, clrs[0], clrs[1], clrs[0], clrs[1], clrs[0], clrs[1], new object[] { 0, 1, 0, 1, 0, 1 });

            if (cfg.HourHandConfig.AllowCustomHourStroke)
                AddSample(localize.colorcfgMultiColor, clrs[0], xColor.Transparent, clrs[2], xColor.Transparent, clrs[4], xColor.Transparent, new object[] { 0, xColor.Transparent, 2, xColor.Transparent, 4, xColor.Transparent });

            if (bHasFilled || !cfg.HourHandConfig.AllowCustomHourStroke)
                AddSample(localize.colorcfgMultiColorFilled, clrs[0], clrs[1], clrs[2], clrs[3], clrs[4], clrs[0], new object[] { 0, 1, 2, 3, 4, 5 });
        }

        void AddSample(string cTitle, xColor clrHourStroke, xColor clrHourFill, xColor clrMinuteStroke, xColor clrMinuteFill, xColor clrSecond, xColor clrSecondFill, object tag = null)
        {
            var cfg = BaseSample.GetConfigClone();
            cfg.ColorHourHandStroke = clrHourStroke;
            cfg.ColorHourHandFill = clrHourFill;
            cfg.ColorMinuteHandStroke = clrMinuteStroke;
            cfg.ColorMinuteHandFill = clrMinuteFill;
            cfg.ColorSecondHandStroke = clrSecond;
            cfg.ColorSecondHandFill = clrSecondFill;

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
            var org = BaseSample.GetConfigClone();
            var cfg = BaseSample.GetConfigClone();
            try
            {
                xColor? clr = null;

                if (BaseSample.WidgetConfig.HourHandConfig.AllowCustomHourStroke)
                {
                    clr = await handler.UserSelectColor(localize.HourHandStroke, org.ColorHourHandStroke);
                    if (clr == null)
                        return;
                    cfg.ColorHourHandStroke = cfg.ColorMinuteHandStroke = cfg.ColorSecondHandStroke = clr.Value;
                    bIsValid = true;
                }

                if (BaseSample.WidgetConfig.HourHandConfig.AllowCustomHourFill)
                {
                    clr = await handler.UserSelectColor(localize.HourHandFill, org.ColorHourHandFill);
                    if (clr == null)
                        return;
                    cfg.ColorHourHandFill = cfg.ColorMinuteHandFill = cfg.ColorSecondHandFill = clr.Value;
                    bIsValid = true;
                }

                if (BaseSample.WidgetConfig.MinuteHandConfig.AllowCustomMinuteStroke)
                {
                    clr = await handler.UserSelectColor(localize.MinuteHandStroke, org.ColorMinuteHandStroke);
                    if (clr == null)
                        return;
                    cfg.ColorMinuteHandStroke = clr.Value;
                }

                if (BaseSample.WidgetConfig.MinuteHandConfig.AllowCustomMinuteFill)
                {
                    clr = await handler.UserSelectColor(localize.MinuteHandFill, org.ColorMinuteHandFill);
                    if (clr == null)
                        return;
                    cfg.ColorMinuteHandFill = clr.Value;
                }

                if (cfg.ShowSeconds && BaseSample.WidgetConfig.SecondHandConfig.AllowCustomSecondStroke)
                {
                    clr = await handler.UserSelectColor(localize.SecondHandStroke, org.ColorSecondHandStroke);
                    if (clr == null)
                        return;
                    cfg.ColorSecondHandStroke = clr.Value;
                }

                if (cfg.ShowSeconds && BaseSample.WidgetConfig.SecondHandConfig.AllowCustomSecondFill)
                {
                    clr = await handler.UserSelectColor(localize.SecondHandFill, org.ColorSecondHandFill);
                    if (clr == null)
                        return;
                    cfg.ColorSecondHandFill = clr.Value;
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
        bool bHourIsFilled = true;
        bool bMinuteIsFilled = true;
        bool bSecondIsFilled = false;

        public WidgetCfgAssistant_ClockAnalog_HandColors(WidgetCfgSample<WidgetCfg_ClockAnalog> baseSample)
        {
            Title = localize.HandColors;
            BaseSample = baseSample;
            ShowColors = true;
            AllowCustom = true;
            NextStepAssistantType = typeof(WidgetCfgAssistant_ClockAnalog_OptionsBase);

            bHourIsFilled = BaseSample.WidgetConfig.HourHandConfig.AllowCustomHourFill;
            bMinuteIsFilled = BaseSample.WidgetConfig.MinuteHandConfig.AllowCustomMinuteFill;
            bSecondIsFilled = BaseSample.WidgetConfig.SecondHandConfig.AllowCustomSecondFill;

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
            if (!(BaseSample.Tag is object[]) || (BaseSample.Tag as object[]).Length != 6)
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
            if (!clrS.Contains(clrHourFill) && bHourIsFilled)
                clrS.Add(clrHourFill);
            if (!clrS.Contains(clrMinuteStroke))
                clrS.Add(clrMinuteStroke);
            if (!clrS.Contains(clrMinuteFill) && bMinuteIsFilled)
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
            var org = BaseSample.GetConfigClone();
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
                    clr = await handler.UserSelectColor(localize.HourHandStroke, org.ColorHourHandStroke);
                    if (clr == null)
                        return;
                    cfg.ColorHourHandStroke = cfg.ColorMinuteHandStroke = cfg.ColorSecondHandStroke = clr.Value;
                    bIsValid = true;
                }

                if ((mapping[1] is xColor))
                    cfg.ColorHourHandFill = (xColor)mapping[1];
                else
                {
                    clr = await handler.UserSelectColor(localize.HourHandFill, org.ColorHourHandFill);
                    if (clr == null)
                        return;
                    cfg.ColorHourHandFill = cfg.ColorMinuteHandFill = cfg.ColorSecondHandFill = clr.Value;
                    bIsValid = true;
                }

                if (!(mapping[2] is xColor) && !Equals(mapping[2], mapping[0]))
                {
                    clr = await handler.UserSelectColor(localize.MinuteHandStroke, org.ColorMinuteHandStroke);
                    if (clr == null)
                        return;
                    cfg.ColorMinuteHandStroke = clr.Value;
                    bIsValid = true;
                }

                if (!(mapping[3] is xColor) && !Equals(mapping[3], mapping[1]))
                {
                    clr = await handler.UserSelectColor(localize.MinuteHandFill, org.ColorMinuteHandFill);
                    if (clr == null)
                        return;
                    cfg.ColorMinuteHandFill = clr.Value;
                    bIsValid = true;
                }

                if (cfg.ShowSeconds)
                {
                    if (!(mapping[4] is xColor) && !Equals(mapping[4], mapping[0]))
                    {
                        clr = await handler.UserSelectColor(localize.SecondHandStroke, org.ColorSecondHandStroke);
                        if (clr == null)
                            return;
                        cfg.ColorSecondHandStroke = clr.Value;
                        bIsValid = true;
                    }
                }

                if (cfg.ShowSeconds && bSecondIsFilled)
                {
                    if (!(mapping[5] is xColor) && !Equals(mapping[5], mapping[1]))
                    {
                        clr = await handler.UserSelectColor(localize.SecondHandFill, org.ColorSecondHandFill);
                        if (clr == null)
                            return;
                        cfg.ColorSecondHandFill = clr.Value;
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
                if (cfg.ColorTickMarks.A == 0)
                    cfg.ColorTickMarks = cfg.ColorHourHandStroke;
                if (cfg.ColorTickMarks.A == 0)
                    cfg.ColorTickMarks = cfg.ColorHourHandFill;
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

            if (BaseSample.WidgetConfig.ColorTickMarks.A > 0)
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

    public class WidgetCfgAssistant_ClockAnalog_WidgetTimeType : WidgetConfigAssistant<WidgetCfg_ClockAnalog>
    {
        public WidgetCfgAssistant_ClockAnalog_WidgetTimeType(WidgetCfgSample<WidgetCfg_ClockAnalog> baseSample)
        {
            Title = localize.HandColorTypes;
            BaseSample = baseSample;
            ShowPreviewImage = false;
            NextStepAssistantType = typeof(WidgetCfgAssistant_ClockAnalog_OptionsBase);
           
            var cfg = BaseSample.GetConfigClone();
            cfg.WidgetTimeType = cfg.CurrentTimeType = TimeType.RealSunTime;
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(localize.TimeType_RealSunTime, cfg));

            cfg = BaseSample.GetConfigClone();
            cfg.WidgetTimeType = cfg.CurrentTimeType = TimeType.MiddleSunTime;
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(localize.TimeType_MiddleSunTime, cfg));

            cfg = BaseSample.GetConfigClone();
            cfg.WidgetTimeType = cfg.CurrentTimeType = TimeType.TimeZoneTime;
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(localize.TimeType_TimeZoneTime, cfg));
        }
    }
}