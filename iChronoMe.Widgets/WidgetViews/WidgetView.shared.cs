using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Xml.Serialization;

using iChronoMe.Core.Classes;
using iChronoMe.Core.Types;
using iChronoMe.Tools;

using SkiaSharp;

namespace iChronoMe.Widgets
{
    public abstract class WidgetView
    {
        protected string cfgInstance = Guid.NewGuid().ToString();

        public virtual bool NeedsWeatherInfo { get; }
    }
}
