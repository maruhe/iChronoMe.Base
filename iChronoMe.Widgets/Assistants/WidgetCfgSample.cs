
using iChronoMe.Core.Classes;
using iChronoMe.Core.Types;

namespace iChronoMe.Widgets
{
    public class WidgetCfgSample<T>
        where T : WidgetCfg
    {
        public WidgetCfgSample(string title, T cfg, object tag = null)
        {
            Title = title;
            WidgetConfig = cfg;
            Tag = tag;
        }

        public WidgetCfgSample(string title, xColor[] colors, T cfg, object tag = null, T previewOverride = null) : this(title, cfg, tag)
        {
            Colors = colors;
            PreviewConfig = previewOverride;
        }
        public WidgetCfgSample(string title, xColor[] colors, T cfg, int previewImage, object tag = null) : this(title, cfg, tag)
        {
            Colors = colors;
            PreviewImage = previewImage;
        }

        public int Icon { get; set; }

        public string Title { get; set; }

        public T WidgetConfig { get; set; }

        public T PreviewConfig { get; set; }

        public int PreviewImage { get; set; }

        public T GetConfigClone()
        {
            return (T)WidgetConfig?.Clone();
        }

        public xColor[] Colors { get; set; }

        public object Tag { get; set; }
    }
}
