using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

using iChronoMe.Core.Interfaces;

namespace iChronoMe.Core.DataBinding
{
    public partial class DataBinder : IDisposable
    {

        public int PushToViewDelayInterval { get; set; } = 15; 
        public int PushToViewMaxDelay { get; set; } = 100;

        public event EventHandler<UserChangedPropertyEventArgs> UserChangedProperty;

        public bool BindViewProperty(object view, string viewProperty, BaseObservable bindable, string bindableProperty, BindMode bindMode)
        {
            if (view == null)
                return false;
            if (string.IsNullOrEmpty(viewProperty))
                return false;
            if (bindable == null)
                return false;
            if (string.IsNullOrEmpty(bindableProperty))
                return false;

            var vProp = view.GetType().GetProperty(viewProperty);
            if (vProp == null)
                return false;
            var bProp = bindable.GetType().GetProperty(bindableProperty);
            if (bProp == null)
                return false;

            //if (!bProp.PropertyType.Equals(typeof(T)) && !bProp.PropertyType.IsSubclassOf(typeof(T)))
            //  return false;

            string cViewPropID = string.Concat(view.GetType().Name, view.GetHashCode(), viewProperty);
            if (ObjectLinks.ContainsKey(cViewPropID))
                return false;

            string cObjectPropID = string.Concat(bindable.GetType().Name, bindable.GetHashCode(), bindableProperty);

            ObjectLinks.Add(cViewPropID, new ObjectLink { ID = cObjectPropID, Object = bindable, Property = bProp });
            BindModes.Add(cViewPropID, bindMode);


            ViewLinks.Add(new KeyValuePair<string, ViewLink>(cObjectPropID, new ViewLink { ID = cViewPropID, View = view, Property = vProp }));

            if (!ObservedObjects.Contains(bindable))
                ObservedObjects.Add(bindable);

            return true;
        }

        private bool BinderIsRunning = false;
        public void Start()
        {
            BinderIsRunning = true;
            foreach (var o in ObservedObjects)
                o.PropertyChanged += Bindable_PropertyChanged;

            foreach (var olnk in ObjectLinks)
            {
                if (olnk.Value.Object is ICanBeReady && !((ICanBeReady)olnk.Value.Object).IsReady)
                    continue;
                ProcessBindable_PropertyChanged(olnk.Value.Object, olnk.Value.Property.Name, true);
            }

            StartViewChangeListener();
        }

        public void PushDataToViewOnce()
        {
            foreach (var olnk in ObjectLinks)
            {
                ProcessBindable_PropertyChanged(olnk.Value.Object, olnk.Value.Property.Name, true);
            }
        }

        private void Bindable_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ProcessBindable_PropertyChanged(sender, e.PropertyName);
        }

        public void ProcessBindable_PropertyChanged(object bindable, string property, bool isInitial = false)
        {
            if ("*".Equals(property))
            {
                foreach (var olnk in ObjectLinks)
                {
                    ProcessBindable_PropertyChanged(olnk.Value.Object, olnk.Value.Property.Name, isInitial);
                }
            }
            else
            {
                string cObjectPropID = string.Concat(bindable.GetType().Name, bindable.GetHashCode(), property);

                var links = ViewLinks.Where(x => x.Key == cObjectPropID);
                if (links.Count() > 0)
                {
                    var newVal = bindable.GetType().GetProperty(property).GetValue(bindable);
                    SendNewValueToViews(newVal, links, isInitial);
                }
            }
        }

        public void Stop()
        {
            BinderIsRunning = false;
            foreach (var o in ObservedObjects)
                o.PropertyChanged -= Bindable_PropertyChanged;
        }

        public void Dispose()
        {
            if (BinderIsRunning)
            {
                Stop();
            }
            ObjectLinks.Clear();
            ObjectLinks = null;
            ViewLinks.Clear();
            ViewLinks = null;
            ObservedObjects.Clear();
            ObservedObjects = null;
            ValuesToViewsQue.Clear();
            ValuesToViewsQue = null;
        }

        private Dictionary<string, ObjectLink> ObjectLinks = new Dictionary<string, ObjectLink>();
        private Dictionary<string, BindMode> BindModes = new Dictionary<string, BindMode>();
        private Dictionary<object, ObjectLink> ViewToModelLinks = new Dictionary<object, ObjectLink>();
        private List<KeyValuePair<string, ViewLink>> ViewLinks = new List<KeyValuePair<string, ViewLink>>();
        private List<BaseObservable> ObservedObjects = new List<BaseObservable>();

        private List<KeyValuePair<ViewLink, object>> ValuesToViewsQue = new List<KeyValuePair<ViewLink, object>>();

        #region from Models to View

        Thread trSendNewValueToViews = null;
        DateTime tSendLastPropertyChanged = DateTime.MinValue;
        private void SendNewValueToViews(object newVal, IEnumerable<KeyValuePair<string, ViewLink>> links, bool isInitial = false)
        {
            tSendLastPropertyChanged = DateTime.Now;
            lock (ValuesToViewsQue)
            {
                foreach (var vl in links)
                {
                    //if (isInitial || BindModes[vl.Key] > BindMode.OneTime)
                    ValuesToViewsQue.Add(new KeyValuePair<ViewLink, object>(vl.Value, newVal));
                }
            }
            tSendLastPropertyChanged = DateTime.Now;

            if (trSendNewValueToViews == null)
            {
                lock (ValuesToViewsQue)
                {
                    trSendNewValueToViews = GetNewSenderThread();
                }
                trSendNewValueToViews.Start();
            }
        }

        private Thread GetNewSenderThread()
            => new Thread(() =>
            {
                DateTime tThreadStart = DateTime.Now;
                DateTime tWaitInterval = tSendLastPropertyChanged;
                try
                {
                    if (PushToViewDelayInterval > 0)
                    {
                        //whait some milliseconds to push several values in one package
                        while (DateTime.Now > tThreadStart.AddMilliseconds(PushToViewMaxDelay))
                        {
                            Thread.Sleep(PushToViewDelayInterval);
                            if (tSendLastPropertyChanged > tWaitInterval)
                                tWaitInterval = tSendLastPropertyChanged;
                            else
                                break;
                        }
                    }

                    //no send the stuff
                    lock (ValuesToViewsQue)
                    {
                        List<KeyValuePair<ViewLink, object>> myQue = new List<KeyValuePair<ViewLink, object>>();
                        myQue.AddRange(ValuesToViewsQue);
                        ValuesToViewsQue.Clear();
                        trSendNewValueToViews = null;

                        DoWriteQueToView(myQue);
                    }
                }
                catch (Exception ex)
                {
                    xLog.Error(ex);
                    trSendNewValueToViews = null;
                }
            });

        #endregion

        private class ViewLink : IDisposable
        {
            public string ID;
            public object View;
            public PropertyInfo Property;

            public void Dispose()
            {
                View = null;
                Property = null;
            }
        }

        private class ObjectLink : IDisposable
        {
            public string ID;
            public BaseObservable Object;
            public PropertyInfo Property;

            public void Dispose()
            {
                Object = null;
                Property = null;
            }
        }
    }

    public enum BindMode
    {
        //OneTime = 0,
        OneWay = 10,
        TwoWay = 20
    }

    public class UserChangedPropertyEventArgs : EventArgs
    {
        public object Bindable { get; }
        public string PropertyName { get; }
        public object OldValue { get; }
        public object NewValue { get; }

        public UserChangedPropertyEventArgs(object bindable, string propName, object oldVal, object newVal)
        {
            Bindable = bindable;
            PropertyName = propName;
            OldValue = oldVal;
            NewValue = newVal;
        }
    }
}