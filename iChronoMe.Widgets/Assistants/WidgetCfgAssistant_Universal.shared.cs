using System;
using System.Collections.Generic;

using iChronoMe.Core.Classes;
using iChronoMe.Core.DynamicCalendar;
using iChronoMe.Core.Interfaces;
using iChronoMe.Core.Types;

namespace iChronoMe.Widgets
{
    public partial class WidgetCfgAssistant_Universal_ClickAction<T> : WidgetConfigAssistant<T>
        where T : WidgetCfg
    {
        public WidgetCfgAssistant_Universal_ClickAction(WidgetCfgSample<T> baseSample)
        {
            var t = baseSample.WidgetConfig;
            Title = localize.ClickAction;
            if (t is WidgetCfg_Calendar)
                Title = localize.TitleClickAction;
            BaseSample = baseSample;
            ShowPreviewImage = false;

            foreach (ClickActionType ca in Enum.GetValues(typeof(ClickActionType)))
            {
                try
                {
                    if (ca == ClickActionType.CreateAlarm)
                        continue;
                    if (ca == ClickActionType.Animate && !(t is WidgetCfg_ActionButton))
                        continue;

                    string c = ca.ToString();
                    var res = typeof(localize).GetProperty("ClickActionType_" + c);
                    if (res != null)
                        c = (string)res.GetValue(null);

                    var cfg = baseSample.GetConfigClone();
                    cfg.ClickAction = new ClickAction(ca);
                    if (cfg is WidgetCfg_ActionButton)
                    {
                        if (ca == ClickActionType.Animate)
                        {
                            cfg.WidgetTitle = "";
                            (cfg as WidgetCfg_ActionButton).Style = ActionButton_Style.iChronEye;
                            (cfg as WidgetCfg_ActionButton).AnimateOnFirstClick = true;
                        }
                        else if (ca == ClickActionType.None)
                            cfg.WidgetTitle = "  ";
                        else if (ca != ClickActionType.OpenApp)
                            cfg.WidgetTitle = c;

                        if (!string.IsNullOrEmpty(cfg.WidgetTitle) && cfg.WidgetTitle.Contains("\'") && cfg.WidgetTitle.IndexOf("\'") != cfg.WidgetTitle.LastIndexOf("\'"))
                            cfg.WidgetTitle = cfg.WidgetTitle.Split('\'')[1];
                    }

                    Samples.Add(new WidgetCfgSample<T>(c, cfg));
                } 
                catch(Exception ex)
                {
                    xLog.Error(ex);
                }
            }
            
            if (t is WidgetCfg_ActionButton)
                NextStepAssistantType = typeof(WidgetCfgAssistant_ActionButton_Icon);
            else if (t is WidgetCfg_Clock)
                NextStepAssistantType = typeof(WidgetCfgAssistant_ClockAnalog_OptionsBase);
            else if (t is WidgetCfg_CalendarCircleWave)
                NextStepAssistantType = typeof(WidgetCfgAssistant_CalendarCircleWave_OptionsBase);
            else if (t is WidgetCfg_Calendar)
                NextStepAssistantType = typeof(WidgetCfgAssistant_Calendar_OptionsBase);
        }
    }
}
