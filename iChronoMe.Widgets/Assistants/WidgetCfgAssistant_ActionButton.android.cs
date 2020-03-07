using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using iChronoMe.Core.Classes;
using iChronoMe.Core.DynamicCalendar;
using iChronoMe.Core.Interfaces;
using iChronoMe.Core.Types;
using iChronoMe.Widgets.AndroidHelpers;

namespace iChronoMe.Widgets
{
    public partial class WidgetCfgAssistant_ActionButton_ClickAction : WidgetConfigAssistant<WidgetCfg_ActionButton>
    {
        public override void AfterSelect(IUserIO handler, WidgetCfgSample<WidgetCfg_ActionButton> sample)
        {
            base.AfterSelect(handler, sample);

            if (sample.WidgetConfig.ClickAction.Type == ClickActionType.OpenOtherApp)
            {
                try
                {
                    var appAdapter = new OtherAppAdapter(Tools.HelperContext);
                    int iApp = Tools.ShowSingleChoiseDlg(Tools.HelperContext, "select", appAdapter).Result;
                    if (iApp < 0)
                        throw new Exception();
                    ApplicationInfo appInfo = appAdapter[iApp];

                    sample.WidgetConfig.ClickAction = new ClickAction(ClickActionType.OpenOtherApp);
                    sample.WidgetConfig.ClickAction.Params = new string[] { "PackageName=" + appInfo.PackageName };
                    sample.WidgetConfig.WidgetTitle = appInfo.LoadLabel(Application.Context.PackageManager);
                }
                catch
                {
                    sample.WidgetConfig.ClickAction = new ClickAction(ClickActionType.OpenSettings);
                }
            }
        }
    }
}
