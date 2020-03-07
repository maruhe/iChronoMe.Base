using System;
using System.Collections.Generic;

using iChronoMe.Core.Interfaces;

namespace iChronoMe.Widgets
{
    public class WidgetConfigAssistant<T> : IWidgetConfigAssistant<T>
        where T : WidgetCfg
    {
        public WidgetConfigAssistant()
        {

        }

        WidgetCfgSample<T> _baseSample;
        public WidgetCfgSample<T> BaseSample
        {
            get
            {
                if (_baseSample == null)
                    return new WidgetCfgSample<T>("default", default(T));
                return _baseSample;
            }
            protected set => _baseSample = value;
        }

        public IList<WidgetCfgSample<T>> Samples { get; } = new List<WidgetCfgSample<T>>();

        public string Title { get; set; }

        public bool ShowPreviewImage { get; set; } = true;

        public bool ShowFirstPreviewImage { get; set; } = false;

        public virtual void PerformPreperation(IUserIO handler) { }

        public virtual void AfterSelect(IUserIO handler, WidgetCfgSample<T> sample) { }

        public object PrevStepAssistant { get; set; }

        public Type NextStepAssistantType { get; set; }

        public bool AllowCustom { get; protected set; } = false;

        public string CurstumButtonText { get; protected set; } = "custom";

        public bool ShowColors { get; set; } = false;

        public virtual void ExecCustom(IUserIO handler) { }

        //public virtual void AfterCustom(WidgetCfgSample<T> sample) { }
    }

    public interface IWidgetConfigAssistant<T>
    where T : WidgetCfg
    {
        WidgetCfgSample<T> BaseSample { get; }
        IList<WidgetCfgSample<T>> Samples { get; }
        string Title { get; }
        bool ShowPreviewImage { get; set; }
        bool ShowFirstPreviewImage { get; set; }
        void PerformPreperation(IUserIO handler);
        void AfterSelect(IUserIO handler, WidgetCfgSample<T> sample);
        object PrevStepAssistant { get; set; }
        Type NextStepAssistantType { get; set; }
        bool AllowCustom { get; }
        string CurstumButtonText { get; }
        bool ShowColors { get; }

        void ExecCustom(IUserIO handler);
        //void AfterCustom(WidgetCfgSample<T> sample);
    }
}