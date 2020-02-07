using System;
using System.Collections.Generic;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using iChronoMe.Core.Abstractions;

namespace iChronoMe.Core.DataBinding
{
    public partial class DataBinder : Java.Lang.Object
    {
        public Activity Activity { get; private set; }
        public ViewGroup RootView { get; private set; }

        public DataBinder(Activity activity, ViewGroup rootView)
        {
            Activity = activity;
            RootView = rootView;
        }

        public bool BindViewProperty<T>(int viewID, string viewProperty, BaseObservable bindable, string bindableProperty, BindMode bindMode)
        {
            return BindViewProperty<T>(RootView.FindViewById(viewID), viewProperty, bindable, bindableProperty, bindMode);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            Activity = null;
            RootView = null;
        }

        private bool IsWriting = false;
        private void DoWriteQueToView(List<KeyValuePair<ViewLink, object>> que)
        {
            Activity.RunOnUiThread(() => {
                IsWriting = true;
                foreach (var kv in que)
                {
                    var view = kv.Key.View;
                    var prop = kv.Key.Property;

                    if (!Equals(prop.GetValue(view), kv.Value))
                    {
                        if (prop.CanWrite)
                        {
                            int iPos = -1;
                            if (view is EditText)
                                iPos = (view as EditText).SelectionStart;
                            prop.SetValue(view, kv.Value);
                            if (view is EditText && iPos > 0)
                                (view as EditText).SetSelection(Math.Min(iPos, kv.Value.ToString().Length));
                        }
                    }
                }
                IsWriting = false;
            });
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
                if (link.View is EditText)
                {
                    if (link.Property.Name != nameof(EditText.Text))
                        continue;
                    (link.View as EditText).AfterTextChanged += EditText_AfterTextChanged;
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

        private void CheckBox_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (sender is CheckBox)
                ProcessViewPropertyChanged(sender as CheckBox, (sender as CheckBox).Checked);
        }

        private void EditText_AfterTextChanged(object sender, Android.Text.AfterTextChangedEventArgs e)
        {
            if (IsWriting)
                return;
            if (sender is EditText)
                ProcessViewPropertyChanged(sender as EditText, (sender as EditText).Text);
        }

        private void Spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (sender is Spinner)
                ProcessViewPropertyChanged(sender as Spinner, (sender as Spinner).SelectedItem);
        }

        private void ProcessViewPropertyChanged(View view, object value)
        {
            var link = ViewToModelLinks[view];
            if (link == null)
                return;

            if (link.Property.CanWrite && link.Property.GetValue(link.Object) != value)
                link.Property.SetValue(link.Object, value);
        }

        #endregion
    }
}
