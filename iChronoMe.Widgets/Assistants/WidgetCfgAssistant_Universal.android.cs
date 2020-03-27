using System;

using Android.App;
using Android.Content.PM;

using iChronoMe.Core.Interfaces;
using iChronoMe.Widgets.AndroidHelpers;

namespace iChronoMe.Widgets
{
    public partial class WidgetCfgAssistant_Universal_ClickAction<T>
    {
        public override void AfterSelect(IUserIO handler, WidgetCfgSample<T> sample)
        {
            base.AfterSelect(handler, sample);

            if (sample.WidgetConfig.ClickAction.Type == ClickActionType.OpenOtherApp)
            {
                try
                {
                    var appAdapter = new OtherAppAdapter(AndroidHelpers.Tools.HelperContext);
                    int iApp = AndroidHelpers.Tools.ShowSingleChoiseDlg(AndroidHelpers.Tools.HelperContext, "select", appAdapter).Result;
                    if (iApp < 0)
                        throw new Exception();
                    ApplicationInfo appInfo = appAdapter[iApp];

                    sample.WidgetConfig.ClickAction = new ClickAction(ClickActionType.OpenOtherApp);
                    sample.WidgetConfig.ClickAction.Params = new string[] { "PackageName=" + appInfo.PackageName };
                    if (sample.WidgetConfig is WidgetCfg_ActionButton)
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
