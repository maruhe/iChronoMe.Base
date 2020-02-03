using System;
using System.Globalization;
using System.Reflection;

using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.OS;

namespace iChronoMe.Core.Classes
{
    public static partial class sys
    {
        private static void PlatformInit()
        {
            var ctx = Application.Context;
            Init(OsType.Android);
            var pi = ctx.PackageManager.GetPackageInfo(ctx.PackageName, 0);
            cAppVersionInfo = pi.VersionName + ", " + pi.VersionCode.ToString(CultureInfo.InvariantCulture);
#if DEBUG
            cAppVersionInfo += ", debug";
#else
                cAppVersionInfo += ", release";
#endif

            var props = typeof(Android.OS.Build).GetProperties(BindingFlags.Public | BindingFlags.Static);
            foreach (var prop in props)
            {
                try
                {
                    if ("serial".Equals(prop.Name.ToLower()))
                        cDeviceInfo += "DeviceToken".PadRight(15) + ": " + sys.CalculateMD5Hash("iChr" + sys.CalculateMD5Hash("iChr" + (string)prop.GetValue(null) + "onoMe") + "onoMe") + "\n";
                    else if (prop.PropertyType == typeof(string))
                        cDeviceInfo += prop.Name.PadRight(15) + ": " + (string)prop.GetValue(null) + "\n";
                    else if (prop.PropertyType == typeof(int))
                        cDeviceInfo += prop.Name.PadRight(15) + ": " + ((int)prop.GetValue(null)).ToString() + "\n";
                    else
                        prop.ToString();
                }
                catch (Exception e)
                {
                    e.ToString();
                }
            }

            props = typeof(Android.OS.Build.VERSION).GetProperties(BindingFlags.Public | BindingFlags.Static);
            foreach (var prop in props)
            {
                try
                {
                    if (prop.PropertyType == typeof(string))
                        cDeviceInfo += "Version." + prop.Name.PadRight(15) + ": " + (string)prop.GetValue(null) + "\n";
                    else if (prop.PropertyType == typeof(int))
                        cDeviceInfo += "Version." + prop.Name.PadRight(15) + ": " + ((int)prop.GetValue(null)).ToString() + "\n";
                    else if (prop.PropertyType == typeof(BuildVersionCodes))
                        cDeviceInfo += "Version." + prop.Name.PadRight(15) + ": " + ((int)prop.GetValue(null)).ToString() + "-" + prop.GetValue(null).ToString() + "\n";

                    else
                        prop.ToString();
                }
                catch (Exception e)
                {
                    e.ToString();
                }
            }
            try
            {
                cDeviceInfo += "Display.Width".PadRight(23) + ": " + sys.DisplayWidth + "\n";
                cDeviceInfo += "Display.Height".PadRight(23) + ": " + sys.DisplayHeight + "\n";
                cDeviceInfo += "Display.Density".PadRight(23) + ": " + sys.DisplayDensity + "\n";
                cDeviceInfo += "Display.Orientation".PadRight(23) + ": " + sys.DisplayOrientation + "\n";
                cDeviceInfo += "Display.Rotation".PadRight(23) + ": " + sys.DisplayRotation + "\n";
            }
            catch { }
        }

        public static DateTime DateTimeFromJava(Java.Util.Calendar d)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
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
    }
}