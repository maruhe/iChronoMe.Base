using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Xml.Serialization;

using iChronoMe.Core.Classes;
using iChronoMe.Core.Types;
using iChronoMe.Tools;

namespace iChronoMe.Widgets
{
    public abstract class WidgetView_Clock : WidgetView
    {
        public string BackgroundImage { get; set; } = string.Empty;
        public bool BackImageAllowsBackColor { get; set; } = false;

        public xColor ColorBackground { get; set; } = xColor.Transparent;
        public bool ShowHours { get; set; } = true;
        public bool ShowMinutes { get; set; } = true;
        public bool ShowSeconds { get; set; } = true;

        public bool FlowHours { set; get; } = true;
        public bool FlowMinutes { set; get; } = false;
        public bool FlowSeconds { set; get; } = false;
    }
}
