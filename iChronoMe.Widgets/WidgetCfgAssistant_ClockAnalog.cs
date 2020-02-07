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
            Title = "Standort-Typ";
            ShowPreviewImage = false;
            var cfg = baseSample == null ? new WidgetCfg_ClockAnalog() : baseSample.GetConfigClone();
            cfg.PositionType = WidgetCfgPositionType.LivePosition;
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>("Aktueller Standort", (WidgetCfg_ClockAnalog)cfg.Clone()));
            cfg.PositionType = WidgetCfgPositionType.StaticPosition;
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>("Festen Standort wählen", cfg));
            NextStepAssistantType = typeof(WidgetCfgAssistant_ClockAnalog_OptionsBase);
        }

        public override void AfterSelect(IUserIO handler, WidgetCfgSample<WidgetCfg_ClockAnalog> sample)
        {
            base.AfterSelect(handler, sample);

            if (sample.WidgetConfig.PositionType != WidgetCfgPositionType.LivePosition)
            {
                var pos = handler.TriggerSelectMapsLocation().Result;
                if (pos == null)
                {
                    handler.ShowError("solar time needs a location!");
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
        string cImageDir = "";

        public WidgetCfgAssistant_ClockAnalog_OptionsBase(WidgetCfgSample<WidgetCfg_ClockAnalog> baseSample)
        {
            Title = "anpassungen";
            BaseSample = baseSample;

            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>("speichern und beenden", BaseSample.GetConfigClone()));
            var cfg = BaseSample.GetConfigClone();
            cfg.ColorBackground = xColor.HotPink;
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>("Hintergrund", cfg, typeof(WidgetCfgAssistant_ClockAnalog_BackgroundImage)));
            cfg = BaseSample.GetConfigClone();
            cfg.ColorHourHandStorke = cfg.ColorMinuteHandFill = cfg.ColorMinuteHandStorke = cfg.ColorMinuteHandFill =
                cfg.ColorSecondHandStorke = cfg.ColorSecondHandFill = cfg.ColorTickMarks = xColor.HotPink;
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>("Zeigerfarben", cfg, typeof(WidgetCfgAssistant_ClockAnalog_HandColorType)));

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
            Title = "clock-faces";
            BaseSample = baseSample;

            cImageDir = ImageLoader.GetImagePathThumb("clockface");

            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>("einfärbig", EmptyBackSample));
            LoadSamples();

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
                ImageLoader.CheckImageThumbCache(handler, "clockface");

            Samples.Clear();
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>("ohne Hintergrund", EmptyBackSample));
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
                    WidgetCfg_ClockAnalog cfg = BaseSample.GetConfigClone();
                    cfg.BackgroundImage = cFile;
                    cfg.ColorBackground = xColor.Transparent;
                    cfg.ColorTickMarks = xColor.Transparent;
                    Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(Path.GetFileNameWithoutExtension(cFile).Replace("_", " "), cfg));
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
            Title = "background-color";
            BaseSample = baseSample;
            NextStepAssistantType = typeof(WidgetCfgAssistant_ClockAnalog_OptionsBase);

            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>("current", BaseSample.GetConfigClone()));
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>("transparent", BaseSample.GetConfigClone()));

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
    }

    public class WidgetCfgAssistant_ClockAnalog_HandColorType : WidgetConfigAssistant<WidgetCfg_ClockAnalog>
    {
        public WidgetCfgAssistant_ClockAnalog_HandColorType(WidgetCfgSample<WidgetCfg_ClockAnalog> baseSample)
        {
            Title = "color-types";
            BaseSample = baseSample;
            NextStepAssistantType = typeof(WidgetCfgAssistant_ClockAnalog_HandColors);

            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>("current", BaseSample.GetConfigClone()));
            LoadSamples();
        }

        void LoadSamples()
        {
            var clrs = DynamicColors.SampleColorSetS[5];

            AddSample("Schwarz/Transparent", xColor.Black, xColor.Transparent, xColor.Black, xColor.Transparent, xColor.Black, "SetDone");

            AddSample("Schwarz/Weiß", xColor.Black, xColor.White, xColor.Black, xColor.White, xColor.Black, "SetDone");

            AddSample("Schwarz", xColor.Black, xColor.Black, xColor.Black, xColor.Black, xColor.Black, "SetDone");

            AddSample("Schwarz/Custom", xColor.Black, clrs[1], xColor.Black, clrs[1], xColor.Black, new object[] { xColor.Black, 1, xColor.Black, 1, xColor.Black });

            AddSample("Weiß/Transparent", xColor.White, xColor.Transparent, xColor.White, xColor.Transparent, xColor.White, "SetDone");

            AddSample("Weiß/Schwarz", xColor.White, xColor.Black, xColor.White, xColor.Black, xColor.White, "SetDone");

            AddSample("Weiß", xColor.White, xColor.White, xColor.White, xColor.White, xColor.White, "SetDone");

            AddSample("Weiß/Custom", xColor.White, clrs[1], xColor.White, clrs[1], xColor.White, new object[] { xColor.White, 1, xColor.White, 1, xColor.White });

            AddSample("Einfärbig", clrs[0], xColor.Transparent, clrs[0], xColor.Transparent, clrs[0], new object[] { 0, xColor.Transparent, 0, xColor.Transparent, 0 });

            AddSample("Einfärbig gefüllt", clrs[0], clrs[1], clrs[0], clrs[1], clrs[0], new object[] { 0, 1, 0, 1, 0 });

            AddSample("Mehrfärbig", clrs[0], xColor.Transparent, clrs[2], xColor.Transparent, clrs[4], new object[] { 0, xColor.Transparent, 2, xColor.Transparent, 4 });

            AddSample("Mehrfärbig gefüllt", clrs[0], clrs[1], clrs[2], clrs[3], clrs[4], new object[] { 0, 1, 2, 3, 4 });

        }

        void AddSample(string cTitle, xColor clrHourStorke, xColor clrHourFill, xColor clrMinuteStorke, xColor clrMinuteFill, xColor clrSecond, object tag = null)
        {
            var cfg = BaseSample.GetConfigClone();
            cfg.ColorHourHandStorke = clrHourStorke;
            cfg.ColorHourHandFill = clrHourFill;
            cfg.ColorMinuteHandStorke = clrMinuteStorke;
            cfg.ColorMinuteHandFill = clrMinuteFill;
            cfg.ColorSecondHandStorke = clrSecond;

            List<xColor> clrS = new List<xColor>();
            clrS.Add(clrHourStorke);
            if (!clrS.Contains(clrHourFill))
                clrS.Add(clrHourFill);
            if (!clrS.Contains(clrMinuteStorke))
                clrS.Add(clrMinuteStorke);
            if (!clrS.Contains(clrMinuteFill))
                clrS.Add(clrMinuteFill);
            if (!clrS.Contains(clrSecond))
                clrS.Add(clrSecond);

            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(cTitle, clrS.ToArray(), cfg, tag));
        }

        public override void AfterSelect(IUserIO handler, WidgetCfgSample<WidgetCfg_ClockAnalog> sample)
        {
            base.AfterSelect(handler, sample);
            if (sample.Tag is string)
                NextStepAssistantType = null;
        }
    }

    public class WidgetCfgAssistant_ClockAnalog_HandColors : WidgetConfigAssistant<WidgetCfg_ClockAnalog>
    {
        public WidgetCfgAssistant_ClockAnalog_HandColors(WidgetCfgSample<WidgetCfg_ClockAnalog> baseSample)
        {
            Title = "hand-colors";
            BaseSample = baseSample;
            NextStepAssistantType = typeof(WidgetCfgAssistant_ClockAnalog_OptionsBase);

            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>("current", BaseSample.GetConfigClone()));
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

            xColor clrHourStorke = mapping[0] is xColor ? (xColor)mapping[0] : (xColor)sampleClrS[(int)mapping[0]];
            xColor clrHourFill = mapping[1] is xColor ? (xColor)mapping[1] : (xColor)sampleClrS[(int)mapping[1]];
            xColor clrMinuteStorke = mapping[2] is xColor ? (xColor)mapping[2] : (xColor)sampleClrS[(int)mapping[2]];
            xColor clrMinuteFill = mapping[3] is xColor ? (xColor)mapping[3] : (xColor)sampleClrS[(int)mapping[3]];
            xColor clrSecond = mapping[4] is xColor ? (xColor)mapping[4] : (xColor)sampleClrS[(int)mapping[4]];

            if (clrHourStorke == BaseSample.WidgetConfig.ColorBackground ||
                clrMinuteStorke == BaseSample.WidgetConfig.ColorBackground ||
                clrSecond == BaseSample.WidgetConfig.ColorBackground)
                return;

            var cfg = BaseSample.GetConfigClone();
            cfg.ColorHourHandStorke = clrHourStorke;
            cfg.ColorHourHandFill = clrHourFill;
            cfg.ColorMinuteHandStorke = clrMinuteStorke;
            cfg.ColorMinuteHandFill = clrMinuteFill;
            cfg.ColorSecondHandStorke = clrSecond;

            List<xColor> clrS = new List<xColor>();
            clrS.Add(clrHourStorke);
            if (!clrS.Contains(clrHourFill))
                clrS.Add(clrHourFill);
            if (!clrS.Contains(clrMinuteStorke))
                clrS.Add(clrMinuteStorke);
            if (!clrS.Contains(clrMinuteFill))
                clrS.Add(clrMinuteFill);
            if (!clrS.Contains(clrSecond))
                clrS.Add(clrSecond);

            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockAnalog>(cTitle, clrS.ToArray(), cfg));
        }
    }
}