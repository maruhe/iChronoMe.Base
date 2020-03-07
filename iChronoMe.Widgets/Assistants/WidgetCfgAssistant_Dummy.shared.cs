using System;
using System.Collections.Generic;
using System.Text;

namespace iChronoMe.Widgets.Assistants
{
    public class WidgetCfgAssistant_Dummy : WidgetConfigAssistant<WidgetCfg>
    {
        public WidgetCfgAssistant_Dummy(string title, List<WidgetCfgSample<WidgetCfg>> samples)
        {
            Title = title;

            foreach (var sample in samples)
                Samples.Add(sample);

            NextStepAssistantType = null;
        }
    }
}