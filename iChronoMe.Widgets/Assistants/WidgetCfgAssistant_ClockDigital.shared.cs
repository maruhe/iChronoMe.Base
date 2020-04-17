using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

using iChronoMe.Core;
using iChronoMe.Core.Classes;
using iChronoMe.Core.DynamicCalendar;
using iChronoMe.Core.Extentions;
using iChronoMe.Core.Interfaces;
using iChronoMe.Core.Types;

using Xamarin.Essentials;

namespace iChronoMe.Widgets
{
    public class WidgetCfgAssistant_ClockDigital_Start : WidgetConfigAssistant<WidgetCfg_ClockDigital>
    {
        public WidgetCfgAssistant_ClockDigital_Start(WidgetCfgSample<WidgetCfg_ClockDigital> baseSample)
        {
            Title = localize.LocationType;

            ShowPreviewImage = false;
            var cfg = baseSample == null ? new WidgetCfg_ClockDigital() : baseSample.GetConfigClone();
            cfg.PositionType = WidgetCfgPositionType.LivePosition;
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockDigital>(localize.CurrentLocation, (WidgetCfg_ClockDigital)cfg.Clone()));
            cfg.PositionType = WidgetCfgPositionType.StaticPosition;
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockDigital>(localize.text_StaticLocation, (WidgetCfg_ClockDigital)cfg.Clone()));
            NextStepAssistantType = typeof(WidgetCfgAssistant_ClockDigital_OptionsBase);
        }

        public override void AfterSelect(IUserIO handler, WidgetCfgSample<WidgetCfg_ClockDigital> sample)
        {
            base.AfterSelect(handler, sample);

            if (sample.WidgetConfig.PositionType != WidgetCfgPositionType.LivePosition)
            {
                var pos = handler.UserSelectMapsLocation(null, new Location(sample.WidgetConfig.Latitude, sample.WidgetConfig.Longitude)).Result;
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
            else
                sample.WidgetConfig.WidgetTitle = LocationTimeHolder.LocalInstance.AreaName;

            if (sample.WidgetConfig.WidgetId == 0 && ClockHandConfig.Count > 1)
                NextStepAssistantType = typeof(WidgetCfgAssistant_ClockDigital_StartStyle);
        }
    }

    public class WidgetCfgAssistant_ClockDigital_StartStyle : WidgetConfigAssistant<WidgetCfg_ClockDigital>
    {
        public WidgetCfgAssistant_ClockDigital_StartStyle(WidgetCfgSample<WidgetCfg_ClockDigital> baseSample)
        {
            Title = localize.text_Style;
            BaseSample = baseSample;

            var cfg = BaseSample.GetConfigClone();
            cfg.ClockStyle = DigitalClockStyle.Default;
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockDigital>(localize.text_Sample + " 1", cfg));

            cfg = BaseSample.GetConfigClone();
            cfg.ClockStyle = DigitalClockStyle.Detailed;
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockDigital>(localize.text_Sample + " 1", cfg));

            cfg = BaseSample.GetConfigClone();
            cfg.ClockStyle = DigitalClockStyle.JustTime;
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockDigital>(localize.text_Sample + " 1", cfg));

            NextStepAssistantType = null;
        }

        public override void AfterSelect(IUserIO handler, WidgetCfgSample<WidgetCfg_ClockDigital> sample)
        {
            base.AfterSelect(handler, sample);

            NextStepAssistantType = (sample.Tag as Type);
        }
    }


    public class WidgetCfgAssistant_ClockDigital_OptionsBase : WidgetConfigAssistant<WidgetCfg_ClockDigital>
    {
        public WidgetCfgAssistant_ClockDigital_OptionsBase(WidgetCfgSample<WidgetCfg_ClockDigital> baseSample)
        {
            Title = localize.title_EditWidget;
            ShowPreviewImage = false;
            ShowFirstPreviewImage = true;
            BaseSample = baseSample;

            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockDigital>(localize.action_SaveAndQuit, BaseSample.GetConfigClone()));
            var cfg = BaseSample.GetConfigClone();
            var cfgPrev = BaseSample.GetConfigClone();
            cfgPrev.ColorBackground = xColor.HotPink;
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockDigital>(localize.Background, null, cfg, typeof(WidgetCfgAssistant_ClockDigital_BackgroundColor), cfgPrev));
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockDigital>(localize.LocationType, null, BaseSample.GetConfigClone(), typeof(WidgetCfgAssistant_ClockDigital_Start)));
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockDigital>(localize.TimeType, null, BaseSample.GetConfigClone(), typeof(WidgetCfgAssistant_ClockDigital_WidgetTimeType)));

            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockDigital>(localize.TextColor, null, BaseSample.GetConfigClone(), typeof(WidgetCfgAssistant_ClockDigital_TextColor)));
            //Samples.Add(new WidgetCfgSample<WidgetCfg_ClockDigital>(localize.Theme, null, BaseSample.GetConfigClone(), typeof(WidgetCfgAssistant_ClockDigital_Theme)));

            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockDigital>(localize.ClickAction, null, BaseSample.GetConfigClone(), typeof(WidgetCfgAssistant_Universal_ClickAction<WidgetCfg_ClockDigital>)));

            NextStepAssistantType = null;
        }

        public override void AfterSelect(IUserIO handler, WidgetCfgSample<WidgetCfg_ClockDigital> sample)
        {
            base.AfterSelect(handler, sample);

            NextStepAssistantType = (sample.Tag as Type);
        }

    }

    public class WidgetCfgAssistant_ClockDigital_Theme : WidgetConfigAssistant<WidgetCfg_ClockDigital>
    {
        public WidgetCfgAssistant_ClockDigital_Theme(WidgetCfgSample<WidgetCfg_ClockDigital> baseSample)
        {
            Title = localize.Theme;

            foreach (var o in Enum.GetValues(typeof(WidgetTheme)))
            {
                var cfg = baseSample.GetConfigClone();
                cfg.SetTheme((WidgetTheme)o);
                Samples.Add(new WidgetCfgSample<WidgetCfg_ClockDigital>(o.ToString(), cfg));
            }

            NextStepAssistantType = typeof(WidgetCfgAssistant_ClockDigital_OptionsBase);
        }
    }

    public class WidgetCfgAssistant_ClockDigital_BackgroundColor : WidgetConfigAssistant<WidgetCfg_ClockDigital>
    {
        public WidgetCfgAssistant_ClockDigital_BackgroundColor(WidgetCfgSample<WidgetCfg_ClockDigital> baseSample)
        {
            Title = localize.BackgroundColor;
            BaseSample = baseSample;
            AllowCustom = true;
            NextStepAssistantType = typeof(WidgetCfgAssistant_ClockDigital_OptionsBase);

            var cfg = BaseSample.GetConfigClone();
            if (baseSample.WidgetConfig.ColorBackground.A > 0)
            {
                Samples.Add(new WidgetCfgSample<WidgetCfg_ClockDigital>(localize.current, cfg));
            }
            cfg = BaseSample.GetConfigClone();
            cfg.ColorBackground = xColor.Transparent;
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockDigital>(localize.text_transparent, cfg));

            int i = 0;
            List<string> clrS = new List<string>();
            foreach (var clrs in DynamicColors.SampleColorSetS)
            {
                i++;
                var clr = xColor.FromHex(clrs[0]);
                if (!clrS.Contains(clr.HexString))
                {
                    clrS.Add(clr.HexString);

                    int iSim = 15;
                    if (clr.IsSimilar(cfg.ColorTitleText, iSim))
                        cfg.ColorTitleText = cfg.ColorTitleText.Invert();

                    cfg = BaseSample.GetConfigClone();
                    cfg.ColorBackground = clr;

                    Samples.Add(new WidgetCfgSample<WidgetCfg_ClockDigital>(clr.HexString, cfg));
                }
            }
        }

        public override async void ExecCustom(IUserIO handler)
        {
            base.ExecCustom(handler);

            var cfg = BaseSample.GetConfigClone();

            var clr = await handler.UserSelectColor(localize.BackgroundColor, cfg.ColorBackground);
            if (clr == null)
            {
                handler.TriggerNegativeButtonClicked();
                return;
            }
            cfg.ColorBackground = clr.Value;
            BaseSample = new WidgetCfgSample<WidgetCfg_ClockDigital>("custom", cfg);
            handler.TriggerPositiveButtonClicked();
        }
    }
   
    public class WidgetCfgAssistant_ClockDigital_TextColor : WidgetConfigAssistant<WidgetCfg_ClockDigital>
    {
        public WidgetCfgAssistant_ClockDigital_TextColor(WidgetCfgSample<WidgetCfg_ClockDigital> baseSample)
        {
            Title = localize.TextColor;
            BaseSample = baseSample;
            AllowCustom = true;
            NextStepAssistantType = typeof(WidgetCfgAssistant_ClockDigital_OptionsBase);

            var cfg = BaseSample.GetConfigClone();
            cfg.ColorTitleText = xColor.Black;
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockDigital>(xColor.Black.HexString, cfg));

            cfg = BaseSample.GetConfigClone();
            cfg.ColorTitleText = xColor.White;
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockDigital>(xColor.White.HexString, cfg));

            int iSim = 15;
            int i = 0;
            List<string> clrS = new List<string>(new[] { xColor.Black.HexString, xColor.White.HexString });
            foreach (var clrs in DynamicColors.SampleColorSetS)
            {
                i++;
                var clr = xColor.FromHex(clrs[0]);
                if (!clrS.Contains(clr.HexString))
                {
                    clrS.Add(clr.HexString);
                    
                    if (cfg.ColorBackground.A >= .75 && clr.IsSimilar(cfg.ColorBackground, iSim))
                        continue;

                    cfg = BaseSample.GetConfigClone();
                    cfg.ColorTitleText = clr;

                    Samples.Add(new WidgetCfgSample<WidgetCfg_ClockDigital>(clr.HexString, cfg));
                }
            }
        }

        public override async void ExecCustom(IUserIO handler)
        {
            base.ExecCustom(handler);

            var cfg = BaseSample.GetConfigClone();

            var clr = await handler.UserSelectColor(localize.TextColor, cfg.ColorTitleText);
            if (clr == null)
            {
                handler.TriggerNegativeButtonClicked();
                return;
            }
            cfg.ColorTitleText = clr.Value;
            BaseSample = new WidgetCfgSample<WidgetCfg_ClockDigital>("custom", cfg);
            handler.TriggerPositiveButtonClicked();
        }
    }

    public class WidgetCfgAssistant_ClockDigital_WidgetTimeType : WidgetConfigAssistant<WidgetCfg_ClockDigital>
    {
        public WidgetCfgAssistant_ClockDigital_WidgetTimeType(WidgetCfgSample<WidgetCfg_ClockDigital> baseSample)
        {
            Title = localize.HandColorTypes;
            BaseSample = baseSample;
            ShowPreviewImage = false;
            NextStepAssistantType = typeof(WidgetCfgAssistant_ClockDigital_OptionsBase);

            var cfg = BaseSample.GetConfigClone();
            cfg.WidgetTimeType = cfg.CurrentTimeType = TimeType.RealSunTime;
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockDigital>(localize.TimeType_RealSunTime, cfg));

            cfg = BaseSample.GetConfigClone();
            cfg.WidgetTimeType = cfg.CurrentTimeType = TimeType.MiddleSunTime;
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockDigital>(localize.TimeType_MiddleSunTime, cfg));

            cfg = BaseSample.GetConfigClone();
            cfg.WidgetTimeType = cfg.CurrentTimeType = TimeType.TimeZoneTime;
            Samples.Add(new WidgetCfgSample<WidgetCfg_ClockDigital>(localize.TimeType_TimeZoneTime, cfg));
        }
    }
}