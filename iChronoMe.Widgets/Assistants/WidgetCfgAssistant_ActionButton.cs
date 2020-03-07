using System;
using System.Collections.Generic;

using iChronoMe.Core.Classes;
using iChronoMe.Core.DynamicCalendar;
using iChronoMe.Core.Interfaces;
using iChronoMe.Core.Types;

namespace iChronoMe.Widgets
{
    public class WidgetCfgAssistant_ActionButton_OptionBase : WidgetConfigAssistant<WidgetCfg_ActionButton>
    {
        public WidgetCfgAssistant_ActionButton_OptionBase(WidgetCfgSample<WidgetCfg_ActionButton> baseSample)
        {
            Title = localize.title_EditWidget;
            BaseSample = baseSample;
            ShowPreviewImage = false;
            ShowFirstPreviewImage = true;

            Samples.Add(new WidgetCfgSample<WidgetCfg_ActionButton>(localize.action_SaveAndQuit, BaseSample.GetConfigClone()));

            var cfg = BaseSample.GetConfigClone();
            var cfgPrev = BaseSample.GetConfigClone();
            Samples.Add(new WidgetCfgSample<WidgetCfg_ActionButton>(localize.ActionButtonStyle, null, cfg, typeof(WidgetCfgAssistant_ActionButton_Style), cfgPrev));

            cfg = BaseSample.GetConfigClone();
            cfgPrev = BaseSample.GetConfigClone();
            cfgPrev.ColorTitleText = xColor.Pink;
            Samples.Add(new WidgetCfgSample<WidgetCfg_ActionButton>(localize.WidgetTitle, null, cfg, typeof(WidgetCfgAssistant_ActionButton_Title), cfgPrev));

            cfg = BaseSample.GetConfigClone();
            cfgPrev = BaseSample.GetConfigClone();
            Samples.Add(new WidgetCfgSample<WidgetCfg_ActionButton>(localize.ClickAction, null, cfg, typeof(WidgetCfgAssistant_ActionButton_ClickAction), cfgPrev));


            cfg = BaseSample.GetConfigClone();
            cfgPrev = BaseSample.GetConfigClone();
            Samples.Add(new WidgetCfgSample<WidgetCfg_ActionButton>(localize.Theme, null, cfg, typeof(WidgetCfgAssistant_ActionButton_Theme), cfgPrev));

            NextStepAssistantType = null;
        }

        public override void AfterSelect(IUserIO handler, WidgetCfgSample<WidgetCfg_ActionButton> sample)
        {
            base.AfterSelect(handler, sample);

            NextStepAssistantType = (sample.Tag as Type);
        }
    }

    public class WidgetCfgAssistant_ActionButton_ClickAction : WidgetConfigAssistant<WidgetCfg_ActionButton>
    {
        public WidgetCfgAssistant_ActionButton_ClickAction(WidgetCfgSample<WidgetCfg_ActionButton> baseSample)
        {
            Title = localize.ClickAction;
            BaseSample = baseSample;
            ShowPreviewImage = false;

            foreach (ClickActionType ca in Enum.GetValues(typeof(ClickActionType)))
            {
                var cfg = new WidgetCfg_ActionButton();
                cfg.ClickAction = new ClickAction(ca);
                if (ca == ClickActionType.Animate)
                {
                    cfg.WidgetTitle = "";
                    cfg.Style = ActionButton_Style.iChronEye;
                    cfg.AnimateOnFirstClick = true;
                }
                else if (ca != ClickActionType.OpenApp)
                    cfg.WidgetTitle = ca.ToString();

                Samples.Add(new WidgetCfgSample<WidgetCfg_ActionButton>(ca.ToString(), cfg));
            }

            NextStepAssistantType = typeof(WidgetCfgAssistant_ActionButton_Style);
        }
    }

    public class WidgetCfgAssistant_ActionButton_Style : WidgetConfigAssistant<WidgetCfg_ActionButton>
    {
        public WidgetCfgAssistant_ActionButton_Style(WidgetCfgSample<WidgetCfg_ActionButton> baseSample)
        {
            Title = localize.ActionButtonStyle;
            BaseSample = baseSample;

            var cfg = baseSample.GetConfigClone();
            cfg.Style = ActionButton_Style.iChronEye;
            cfg.AnimateOnFirstClick = false;
            Samples.Add(new WidgetCfgSample<WidgetCfg_ActionButton>(localize.ActionButtonStyle_eye_static, cfg));

            cfg = baseSample.GetConfigClone();
            cfg.Style = ActionButton_Style.iChronEye;
            cfg.AnimateOnFirstClick = true;
            Samples.Add(new WidgetCfgSample<WidgetCfg_ActionButton>(localize.ActionButtonStyle_eye_animated, cfg));

            string cIconPrefix = "";
            switch (cfg.ClickAction.Type)
            {
                case ClickActionType.OpenApp:
                    //cIconPrefix = "icons8_calendar_plus";
                    break;
                case ClickActionType.OpenCalendar:
                    cIconPrefix = "icons8_calendar";
                    break;
                case ClickActionType.CreateEvent:
                    cIconPrefix = "icons8_calendar_plus_";
                    break;
                case ClickActionType.CreateAlarm:
                    cIconPrefix = "icons8_alarm";
                    break;
                    //case ClickActionType.TimeToTimeDialog:
                    //  cIconPrefix = "icons8_map_marker";
                    //break;
            }
            if (!string.IsNullOrEmpty(cIconPrefix))
            {
                foreach (var prop in sys.AllDrawables)
                {
                    if (prop.ToLower().StartsWith(cIconPrefix) && prop.Replace("_clrd", "").Length - 3 < cIconPrefix.Length)
                    {
                        cfg = baseSample.GetConfigClone();
                        cfg.Style = ActionButton_Style.Icon;
                        cfg.IconName = prop;
                        Samples.Add(new WidgetCfgSample<WidgetCfg_ActionButton>(prop, cfg));
                    }
                }
            }

            NextStepAssistantType = typeof(WidgetCfgAssistant_ActionButton_OptionBase);
        }

        public override void AfterSelect(IUserIO handler, WidgetCfgSample<WidgetCfg_ActionButton> sample)
        {
            base.AfterSelect(handler, sample);
            if (sample.WidgetConfig.Style == ActionButton_Style.iChronEye && sample.WidgetConfig.AnimateOnFirstClick)
                NextStepAssistantType = typeof(WidgetCfgAssistant_ActionButton_AnimationDuriation);

            if (sample.WidgetConfig.Style == ActionButton_Style.Icon)
            {
                if (!sample.WidgetConfig.IconName.Contains("_clrd"))
                    NextStepAssistantType = typeof(WidgetCfgAssistant_ActionButton_IconColor);
            }
        }
    }

    public class WidgetCfgAssistant_ActionButton_AnimationDuriation : WidgetConfigAssistant<WidgetCfg_ActionButton>
    {
        public WidgetCfgAssistant_ActionButton_AnimationDuriation(WidgetCfgSample<WidgetCfg_ActionButton> baseSample)
        {
            Title = localize.AnimationDuriation;
            BaseSample = baseSample;
            ShowPreviewImage = false;

            for (int i = 1; i <= 5; i++)
            {
                var cfg = baseSample.GetConfigClone();
                cfg.AnimationDuriation = i;
                Samples.Add(new WidgetCfgSample<WidgetCfg_ActionButton>(string.Format(localize.nSeconds, i), cfg));
            }

            NextStepAssistantType = typeof(WidgetCfgAssistant_ActionButton_AnimationRounds);
        }
    }

    public class WidgetCfgAssistant_ActionButton_AnimationRounds : WidgetConfigAssistant<WidgetCfg_ActionButton>
    {
        public WidgetCfgAssistant_ActionButton_AnimationRounds(WidgetCfgSample<WidgetCfg_ActionButton> baseSample)
        {
            Title = localize.AnimationRounds;
            BaseSample = baseSample;
            ShowPreviewImage = false;

            for (int i = 1; i <= (int)(baseSample.WidgetConfig.AnimationDuriation * 3); i++)
            {
                var cfg = baseSample.GetConfigClone();
                cfg.AnimationRounds = i;
                Samples.Add(new WidgetCfgSample<WidgetCfg_ActionButton>(string.Format(localize.nAnimationRounds, i), cfg));
            }

            NextStepAssistantType = typeof(WidgetCfgAssistant_ActionButton_OptionBase);
        }
    }

    public class WidgetCfgAssistant_ActionButton_Title : WidgetConfigAssistant<WidgetCfg_ActionButton>
    {
        public WidgetCfgAssistant_ActionButton_Title(WidgetCfgSample<WidgetCfg_ActionButton> baseSample)
        {
            Title = localize.WidgetTitle;
            BaseSample = baseSample;

            var cfg = baseSample.GetConfigClone();

            NextStepAssistantType = typeof(WidgetCfgAssistant_ActionButton_OptionBase);
        }

        public override void PerformPreperation(IUserIO handler)
        {
            base.PerformPreperation(handler);
            string cTitle = handler.UserInputText(localize.WidgetTitle, null, BaseSample.WidgetConfig.WidgetTitle).Result;
            if (cTitle != null)
                BaseSample.WidgetConfig.WidgetTitle = cTitle;
            handler.TriggerPositiveButtonClicked();
        }
    }

    public class WidgetCfgAssistant_ActionButton_Theme : WidgetConfigAssistant<WidgetCfg_ActionButton>
    {
        public WidgetCfgAssistant_ActionButton_Theme(WidgetCfgSample<WidgetCfg_ActionButton> baseSample)
        {
            Title = localize.Theme;
            BaseSample = baseSample;
            AllowCustom = true;

            foreach (var o in Enum.GetValues(typeof(WidgetTheme)))
            {
                var cfg = baseSample.GetConfigClone();
                cfg.SetTheme((WidgetTheme)o);
                Samples.Add(new WidgetCfgSample<WidgetCfg_ActionButton>(o.ToString(), cfg));
            }

            NextStepAssistantType = typeof(WidgetCfgAssistant_ActionButton_OptionBase);
        }

        public override async void ExecCustom(IUserIO handler)
        {
            base.ExecCustom(handler);
            var clr = await handler.UserSelectColor(localize.BackgroundColor);

            if (clr != null)
            {
                var cfg = BaseSample.GetConfigClone();
                cfg.ColorBackground = clr.Value;
                BaseSample = new WidgetCfgSample<WidgetCfg_ActionButton>("custom", cfg);
            }

            handler.TriggerPositiveButtonClicked();
        }
    }

    public class WidgetCfgAssistant_ActionButton_IconColor : WidgetConfigAssistant<WidgetCfg_ActionButton>
    {
        public WidgetCfgAssistant_ActionButton_IconColor(WidgetCfgSample<WidgetCfg_ActionButton> baseSample)
        {
            Title = localize.IconColor;
            BaseSample = baseSample;
            AllowCustom = true;

            NextStepAssistantType = typeof(WidgetCfgAssistant_ActionButton_OptionBase);
        }

        public override void PerformPreperation(IUserIO handler)
        {
            base.PerformPreperation(handler);

            var cfg = BaseSample.GetConfigClone();

            if (cfg.ColorBackground != WidgetCfg.tcBlack)
            {
                cfg.IconColor = WidgetCfg.tcBlack;
                Samples.Add(new WidgetCfgSample<WidgetCfg_ActionButton>(localize.colorcfgBlack, cfg));
            }

            if (cfg.ColorBackground != WidgetCfg.tcWhite)
            {
                cfg = BaseSample.GetConfigClone();
                cfg.IconColor = WidgetCfg.tcWhite;
                Samples.Add(new WidgetCfgSample<WidgetCfg_ActionButton>("White", cfg));
            }

            if (cfg.ColorBackground != WidgetCfg.tcDark)
            {
                cfg = BaseSample.GetConfigClone();
                cfg.IconColor = WidgetCfg.tcDark;
                Samples.Add(new WidgetCfgSample<WidgetCfg_ActionButton>("Dark", cfg));
            }

            if (cfg.ColorBackground != WidgetCfg.tcLight)
            {
                cfg = BaseSample.GetConfigClone();
                cfg.IconColor = WidgetCfg.tcLight;
                Samples.Add(new WidgetCfgSample<WidgetCfg_ActionButton>("Light", cfg));
            }

            int i = 0;
            var cDone = new List<string>();
            foreach (var clrs in DynamicColors.SampleColorSetS)
            {
                i++;
                var clr = xColor.FromHex(clrs[2]);
                if (!cDone.Contains(clr.HexString) && cfg.ColorBackground != clr)
                {
                    cDone.Add(clr.HexString);
                    cfg = BaseSample.GetConfigClone();
                    cfg.IconColor = clr;
                    Samples.Add(new WidgetCfgSample<WidgetCfg_ActionButton>(clr.HexString, cfg));
                }
            }

        }

        public override async void ExecCustom(IUserIO handler)
        {
            base.ExecCustom(handler);
            var clr = await handler.UserSelectColor(localize.IconColor);

            if (clr != null)
            {
                var cfg = BaseSample.GetConfigClone();
                cfg.IconColor = clr.Value;
                BaseSample = new WidgetCfgSample<WidgetCfg_ActionButton>("custom", cfg);
            }

            handler.TriggerPositiveButtonClicked();
        }
    }
}
