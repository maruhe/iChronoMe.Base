using Android.App;
using Android.Appwidget;
using Android.Content;
using System;
using System.Collections.Generic;
using System.Text;

namespace iChronoMe.Core.Classes
{
    public static partial class sys
    {
        private static void PlatformInit()
        {
            Init(OsType.Android);
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