using System;

using iChronoMe.Core.Classes;
using iChronoMe.Core.Types;

namespace iChronoMe.Widgets
{
    public class WidgetCfgSample<T> where T : WidgetCfg
    {
        public WidgetCfgSample(string title, T cfg, object tag = null)
        {
            Title = title;
            WidgetConfig = cfg;
            Tag = tag;
        }

        public WidgetCfgSample(string title, xColor[] colors, T cfg, object tag = null) : this(title, cfg, tag)
        {
            Colors = colors;
        }

        public string Title { get; set; }

        public T WidgetConfig { get; set; }

        public T GetConfigClone()
        {
            return (T)WidgetConfig?.Clone();
        }

        public xColor[] Colors { get; set; }

        public object Tag { get; set; }

        public Action RunAfterSelect { get; set; }
    }
}
