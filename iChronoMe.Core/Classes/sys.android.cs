using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace iChronoMe.Core.Classes
{
    [Android.Runtime.Preserve(AllMembers = true)]
    public static partial class sys
    {
        private static void PlatformInit()
        {
            try
            {
                //make references to prevent removing by linker 
                string xx = Build.Board + Build.Bootloader + Build.Brand + Build.CpuAbi + Build.CpuAbi2 + Build.Device + Build.Display + Build.Fingerprint + Build.Fingerprint + Build.Hardware + Build.Host + Build.Id + Build.Manufacturer + Build.Model + Build.Product + Build.Radio + Build.RadioVersion + Build.Serial + Build.Tags + Build.Type + Build.User;
                xx = Build.VERSION.BaseOs + Build.VERSION.Codename + Build.VERSION.Incremental + Build.VERSION.Release + Build.VERSION.Sdk + Build.VERSION.SdkInt + Build.VERSION.SecurityPatch;
            } catch { }

            var ctx = Application.Context;
            //Toast.MakeText(ctx, "PlatformInit", ToastLength.Long).Show();
            Init(OsType.Android);
            var pi = ctx.PackageManager.GetPackageInfo(ctx.PackageName, 0);
            cAppVersionInfo = pi.VersionName + ", " + pi.VersionCode.ToString(CultureInfo.InvariantCulture);
#if DEBUG
            cAppVersionInfo += ", debug";
#else
                cAppVersionInfo += ", release";
#endif
            List<string> infos = new List<string>();

            string cX = "23";
            var props = typeof(Build).GetProperties(BindingFlags.Public | BindingFlags.Static);
            //Toast.MakeText(ctx, "PlatformInit: "+ props.Length, ToastLength.Long).Show();
            foreach (var prop in props)
            {
                try
                {
                    if ("serial".Equals(prop.Name.ToLower())) //try to generate a anonym device-token for better error-logging
                    {
                        cDeviceToken = sys.CalculateMD5Hash("iChr" + sys.CalculateMD5Hash("iChr" + (string)prop.GetValue(null) + "onoMe") + cX + "onoMe");
                        infos.Add("DeviceToken".PadRight(15) + ": " + cDeviceToken);
                    }
                    else if (prop.PropertyType == typeof(string))
                        infos.Add(prop.Name.PadRight(15) + ": " + (string)prop.GetValue(null));
                    else if (prop.PropertyType == typeof(int))
                        infos.Add(prop.Name.PadRight(15) + ": " + ((int)prop.GetValue(null)).ToString());
                    else
                        prop.ToString();

                    if ("device".Equals(prop.Name.ToLower()) || "id".Equals(prop.Name.ToLower()) || "model".Equals(prop.Name.ToLower()))
                        cX += ": " + (string)prop.GetValue(null);
                }
                catch { }
            }

            props = typeof(Build.VERSION).GetProperties(BindingFlags.Public | BindingFlags.Static);
            foreach (var prop in props)
            {
                try
                {
                    if (prop.PropertyType == typeof(string))
                        infos.Add("Version." + prop.Name.PadRight(15) + ": " + (string)prop.GetValue(null));
                    else if (prop.PropertyType == typeof(int))
                        infos.Add("Version." + prop.Name.PadRight(15) + ": " + ((int)prop.GetValue(null)).ToString());
                    else if (prop.PropertyType == typeof(BuildVersionCodes))
                        infos.Add("Version." + prop.Name.PadRight(15) + ": " + ((int)prop.GetValue(null)).ToString() + "-" + prop.GetValue(null).ToString());

                    else
                        prop.ToString();
                }
                catch { }
            }
            infos.Sort();
            try
            {
                infos.Add("Display.Width".PadRight(23) + ": " + sys.DisplayWidth);
                infos.Add("Display.Height".PadRight(23) + ": " + sys.DisplayHeight);
                infos.Add("Display.Density".PadRight(23) + ": " + sys.DisplayDensity);
                infos.Add("Display.Orientation".PadRight(23) + ": " + sys.DisplayOrientation);
                infos.Add("Display.Rotation".PadRight(23) + ": " + sys.DisplayRotation);
            }
            catch { }
            cDeviceInfo = string.Empty;
            foreach (string c in infos)
                cDeviceInfo += c + "\n";
        }

        public static DateTime DateTimeFromJava(Java.Util.Calendar d)
        {
            return new DateTime(
                d.Get(Java.Util.CalendarField.Year),
                d.Get(Java.Util.CalendarField.Month) + 1,
                d.Get(Java.Util.CalendarField.DayOfMonth),
                d.Get(Java.Util.CalendarField.HourOfDay),
                d.Get(Java.Util.CalendarField.Minute),
                d.Get(Java.Util.CalendarField.Second),
                d.Get(Java.Util.CalendarField.Millisecond),
                DateTimeKind.Utc);

            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddMilliseconds(d.Time.Time);
        }

        public static Java.Util.Calendar DateTimeToJava(DateTime date)
        {
            var calendar = Java.Util.Calendar.Instance;
            calendar.Set(date.Year, date.Month - 1, date.Day, date.Hour, date.Minute, date.Second);
            return calendar;
        }

        public static void NotifyCalendarEventsUpdated()
        {
            try
            {
                var manager = AppWidgetManager.GetInstance(Application.Context);
                int[] appWidgetID1s = manager.GetAppWidgetIds(new ComponentName(Application.Context, "me.ichrono.droid.Widgets.Calendar.CalendarWidget"));

                Intent updateIntent = new Intent(AppWidgetManager.ActionAppwidgetUpdate);
                updateIntent.SetComponent(ComponentName.UnflattenFromString("me.ichrono.droid/me.ichrono.droid.Widgets.Calendar.CalendarWidget"));
                AppWidgetManager widgetManager = AppWidgetManager.GetInstance(Application.Context);
                updateIntent.PutExtra(AppWidgetManager.ExtraAppwidgetIds, appWidgetID1s);
                Application.Context.SendBroadcast(updateIntent);
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        public static Exception currentError;
        public static Activity currentActivity;

        public const string action_ErrorReceiver = "me.ichrono.droid.Receivers.ErrorReceiver";

        public static void AfterExceptionLog(Exception ex, bool bTryShowUser, string cLogFilePath)
        {
            currentError = ex;

            Intent intent = new Intent(action_ErrorReceiver);
            if (!string.IsNullOrEmpty(cLogFilePath))
                intent.PutExtra("LogFilePath", cLogFilePath);

            try
            {
                if (currentActivity != null)
                {
                    var bmp = getScreenShot(currentActivity.Window.DecorView.RootView);
                    string cFile = string.IsNullOrEmpty(cLogFilePath) ? System.IO.Path.Combine(System.IO.Path.GetTempPath(), DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss_fff") + ".png") : cLogFilePath + ".png";

                    using (System.IO.FileStream fs = new System.IO.FileStream(cFile, System.IO.FileMode.OpenOrCreate))
                    {
                        bmp.Compress(Bitmap.CompressFormat.Png, 0, fs);
                    }
                    bmp.Recycle();
                    intent.PutExtra("ScreenFilePath", cFile);
                }
            }
            catch { }

            if (bTryShowUser)
                Application.Context.SendBroadcast(intent);
            //Android.Support.V4.Content.LocalBroadcastManager.GetInstance(Application.Context).SendBroadcast(intent);
        }

        public static string ConvertTimeZoneToSystem(string cTimeZoneID)
            => cTimeZoneID;

        public static Bitmap getScreenShot(View view)
        {
            View screenView = view.RootView;
            screenView.DrawingCacheEnabled = true;
            Bitmap bitmap = Bitmap.CreateBitmap(screenView.GetDrawingCache(true));
            screenView.DrawingCacheEnabled = false;
            return bitmap;
        }
    }
}
