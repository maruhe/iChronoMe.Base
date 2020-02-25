using System;
using System.Collections.Generic;

using Android.App;
using Android.Runtime;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using iChronoMe.Core.Classes;
using static Android.Widget.TextView;

namespace iChronoMe.Core.DataBinding
{
    public partial class DataBinder : Java.Lang.Object, IOnEditorActionListener
    {
        public Activity Activity { get; private set; }
        public ViewGroup RootView { get; private set; }

        public DataBinder(Activity activity, ViewGroup rootView)
        {
            Activity = activity;
            RootView = rootView;
        }

        public bool BindViewProperty(int viewID, string viewProperty, BaseObservable bindable, string bindableProperty, BindMode bindMode = BindMode.OneWay)
        {
            return BindViewProperty(RootView.FindViewById(viewID), viewProperty, bindable, bindableProperty, bindMode);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            Activity = null;
            RootView = null;
        }

        private bool IsWritingToView = false;
        private void DoWriteQueToView(List<KeyValuePair<ViewLink, object>> que)
        {
            xLog.Debug("start writer-thread " + que.Count);
            Activity.RunOnUiThread(() =>
            {
                DoWriteQueToView_func(que);
                PushedValuesToView?.Invoke(this, new PushedValuesToViewEventArgs());
            });
        }

        private void DoWriteQueToView_func(List<KeyValuePair<ViewLink, object>> que)
        { 
            xLog.Debug("start write " + que.Count + " values to view");
            int iDone = 0;
            IsWritingToView = true;
            foreach (var kv in que)
            {
                try
                {
                    var view = kv.Key.View;
                    var prop = kv.Key.Property;
                    object newVal = kv.Value;

                    if (prop.Name == nameof(View.Visibility))
                    {
                        if (newVal is bool)
                            newVal = (bool)newVal == true ? ViewStates.Visible : ViewStates.Gone;
                        else if (newVal is int)
                            newVal = (int)newVal > 0 ? ViewStates.Visible : ViewStates.Gone;
                    }

                    if (newVal is DateTime)
                    {
                        newVal = ((DateTime)newVal).ToShortDateString();
                    }
                    else if (newVal is TimeSpan)
                    {
                        newVal = ((TimeSpan)newVal).ToString(@"hh\:mm");
                    }

                    if (!Equals(prop.GetValue(view), newVal))
                    {
                        if (prop.CanWrite)
                        {
                            if (view is EditText && ViewToModelLinks.ContainsKey(view))
                            {
                                var cLinkId = ViewToModelLinks[view].ID;
                                if (LastViewValues.ContainsKey(cLinkId) && !Equals(prop.GetValue(view), LastViewValues[cLinkId]))
                                {
                                    if (sys.Debugmode && Activity != null)
                                        Toast.MakeText(Activity, DateTime.Now.ToString("HH:mm:ss.fff") + " ignore value: " + newVal.ToString(), ToastLength.Short).Show();
                                    continue;
                                }
                            }

                            int iPos = -1;
                            if (view is EditText)
                                iPos = (view as EditText).SelectionStart;

                            if (view is AutoCompleteTextView && nameof(AutoCompleteTextView.Text).Equals(prop.Name))
                                (view as AutoCompleteTextView).SetText((string)newVal, false);
                            else
                                prop.SetValue(view, newVal);

                            iDone++;
                            if (view is EditText)
                            {
                                if (iPos > 0)// && (view as EditText).Selected)
                                    (view as EditText).SetSelection(Math.Min(iPos, newVal.ToString().Length));
                                /*else if (!(view as EditText).Selected)
                                    Toast.MakeText(Activity, "not selected", ToastLength.Short).Show();
                                else
                                    Toast.MakeText(Activity, "other reason", ToastLength.Short).Show();*/
                            }
                        }
                        else if (view is Spinner)
                        {
                            switch (prop.Name)
                            {
                                case nameof(Spinner.SelectedItemPosition):
                                    (view as Spinner).SetSelection((int)newVal);
                                    iDone++;
                                    break;

                                default:
                                    throw new NotImplementedException("only SelectedItemPosition can be bound with a Spinner");
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    Toast.MakeText(Activity, ex.Message, ToastLength.Long).Show();
                }
            }
            IsWritingToView = false;
            xLog.Debug("writing finished, {iDone} values changed");
        }

        #region from View to Models

        private void StartViewChangeListener()
        {
            ViewToModelLinks.Clear();
            List<object> done = new List<object>();
            foreach (var vl in ViewLinks)
            {
                if (done.Contains(vl.Value.View))
                    continue;
                if (BindModes[vl.Value.ID] != BindMode.TwoWay)
                    continue;

                var link = vl.Value;
                if (link.View is CheckBox)
                {
                    if (link.Property.Name != nameof(CheckBox.Checked))
                        continue;
                    (link.View as CheckBox).CheckedChange += CheckBox_CheckedChange;
                }
                if (link.View is Switch)
                {
                    if (link.Property.Name != nameof(Switch.Checked))
                        continue;
                    (link.View as Switch).CheckedChange += Switch_CheckedChange;
                }
                if (link.View is EditText)
                {
                    if (link.Property.Name != nameof(EditText.Text))
                        continue;
                    (link.View as EditText).AfterTextChanged += EditText_AfterTextChanged;
                    (link.View as EditText).SetOnEditorActionListener(this);
                    (link.View as EditText).FocusChange += EditText_FocusChange;
                }
                if (link.View is Spinner)
                {
                    if (link.Property.Name != nameof(Spinner.SelectedItem) && link.Property.Name != nameof(Spinner.SelectedItemId) && link.Property.Name != nameof(Spinner.SelectedItemPosition))
                        continue;
                    (link.View as Spinner).ItemSelected += Spinner_ItemSelected;
                }

                ViewToModelLinks.Add(link.View, ObjectLinks[link.ID]);

                done.Add(link.View);
            }
        }

        private void Switch_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (!BinderIsRunning || IsWritingToView)
                return;
            if (sender is Switch)
                ProcessViewPropertyChanged(sender as Switch, (sender as Switch).Checked);
        }

        private void CheckBox_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (!BinderIsRunning || IsWritingToView)
                return;
            if (sender is CheckBox)
                ProcessViewPropertyChanged(sender as CheckBox, (sender as CheckBox).Checked);
        }

        Dictionary<EditText, Delayer> EditTextDelayer = new Dictionary<EditText, Delayer>();
        private void EditText_AfterTextChanged(object sender, Android.Text.AfterTextChangedEventArgs e)
        {
            if (!BinderIsRunning || IsWritingToView)
                return;
            if (sender is EditText)
            {
                //ProcessViewPropertyChanged(sender as EditText, (sender as EditText).Text);
                //return;

                var edit = (sender as EditText);
                string cVal = edit.Text;
                if (string.IsNullOrEmpty(cVal.Trim()))
                    ProcessViewPropertyChanged(edit, cVal);
                else
                {
                    lock(EditTextDelayer)
                    {
                        Delayer mDelayer = null;
                        if (EditTextDelayer.ContainsKey(edit))
                            EditTextDelayer[edit].SetDelay(1000, () => ProcessViewPropertyChanged(edit, cVal));
                        else
                        {
                            mDelayer = new Delayer(() => ProcessViewPropertyChanged(edit, cVal), 1000);
                            EditTextDelayer.Add(edit, mDelayer);
                        }
                    }
                }
            }
        }

        private void EditText_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (!BinderIsRunning || IsWritingToView)
                return;
            if (e.HasFocus)
                return;
            if (sender is EditText)
            {
                ProcessViewPropertyChanged(sender as EditText, (sender as EditText).Text);
            }
        }

        public bool OnEditorAction(TextView sender, [GeneratedEnum] ImeAction actionId, KeyEvent e)
        {
            if (!BinderIsRunning || IsWritingToView)
                return false;
            if (sender is EditText)
            {
                if (actionId == ImeAction.Search ||
                    actionId == ImeAction.Done ||
                    (e != null &&
                     (e.Action == KeyEventActions.Down ||
                      e.KeyCode == Keycode.Enter ||
                      e.KeyCode == Keycode.Space
                      )))
                {
                    ProcessViewPropertyChanged(sender as EditText, (sender as EditText).Text);
                }
            }
            return false;
        }

        private void Spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (!BinderIsRunning || IsWritingToView)
                return;
            if (sender is Spinner)
                ProcessViewPropertyChanged(sender as Spinner, (sender as Spinner).SelectedItemPosition);
        }

        private void ProcessViewPropertyChanged(View view, object value)
        {
            try
            {
                var link = ViewToModelLinks[view];
                if (link == null)
                    return;

                if (link.Property.CanWrite)
                {
                    var old = link.Property.GetValue(link.Object);
                    if (!Equals(old, value))
                    {
                        lock(LastViewValues)
                        {
                            if (LastViewValues.ContainsKey(link.ID))
                                LastViewValues[link.ID] = value;
                            else
                                LastViewValues.Add(link.ID, value);
                        }
                        link.Property.SetValue(link.Object, value);
                        UserChangedProperty?.Invoke(view, new UserChangedPropertyEventArgs(link.Object, link.Property.Name, old, value));
                    }
                }
            } 
            catch (Exception ex)
            {
                xLog.Error(ex);
            }
        }

        #endregion

        void PlatformDispose()
        {
            EditTextDelayer.Clear();
        }
    }
}
/*
    class testView {
        binder = new DataBinder(Activity, RootView);
        binder.BindViewProperty<bool>(Resource.Id.cb_showalways, nameof(CheckBox.Checked), cfg, nameof(MainConfig.AlwaysShowForegroundNotification), BindMode.TwoWay);
            binder.BindViewProperty<string>(Resource.Id.testedit1, nameof(TextView.Text), cfg, nameof(MainConfig.cTest1), BindMode.TwoWay);
            binder.BindViewProperty<string>(Resource.Id.testedit2, nameof(TextView.Text), cfg, nameof(MainConfig.cTest2), BindMode.TwoWay);
            binder.BindViewProperty<string>(Resource.Id.testedit3, nameof(TextView.Text), cfg, nameof(MainConfig.cTest3), BindMode.TwoWay);
            binder.BindViewProperty<string>(Resource.Id.testedit4, nameof(TextView.Text), cfg, nameof(MainConfig.cTest4), BindMode.TwoWay);

            binder.Start();

            Task.Factory.StartNew(() =>
            {
                var cfg = AppConfigHolder.MainConfig;

                Task.Delay(1500).Wait();
                cfg.cTest1 = "Change 1 adjsakjds";
                Task.Delay(500).Wait();
                cfg.cTest2 = "Change 2 adjsakjds";
                Task.Delay(1500).Wait();
                cfg.cTest3 = "Change 3 adjsakjds";

                Task.Delay(1500).Wait();
                cfg.cTest1 = "all";
                cfg.cTest2 = "all";
                cfg.cTest3 = "all";

                Task.Delay(500).Wait();
                cfg.cTest1 = "same";
                cfg.cTest2 = "same";
                cfg.cTest3 = "same";

                Task.Delay(500).Wait();
                cfg.cTest1 = "time";
                cfg.cTest2 = "time";
                cfg.cTest3 = "time";

                Task.Delay(1500).Wait();

                for (int i = 1; i < 100; i++)
                {
                    if (i % 2 == 0)
                        cfg.cTest2 = i.ToString();
                    else if (i % 3 == 0)
                        cfg.cTest3 = i.ToString();
                    else
                        cfg.cTest1 = i.ToString();

                    Task.Delay(75).Wait();
                }
            });


        }

    class testClass : BaseObservable
    {
        private string _cTest1 = "lala";
        public string cTest1
        {
            get => _cTest1;
            set { _cTest1 = value; OnPropertyChanged(); }
        }
        private string _cTest2 = "gfdgfdgsdgf";
        public string cTest2
        {
            get => _cTest2;
            set { _cTest2 = value; OnPropertyChanged(); }
        }
        private string _cTest3 = "123231231";
        public string cTest3
        {
            get => _cTest3;
            set { _cTest3 = value; OnPropertyChanged(); OnPropertyChanged(nameof(cTest4)); }
        }

        public string cTest4
        {
            get => _cTest3 + "__AddOn";
            set
            {
                cTest3 = value.EndsWith("__AddOn") ? value.Substring(0, value.Length - 7) : value;
            }
        }
    }
*/
