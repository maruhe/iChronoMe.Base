using System;
using System.Threading.Tasks;
using iChronoMe.Core.Classes;

namespace iChronoMe.DeviceCalendar
{
    public partial class CalendarEvent
    {
        /*
        public Java.Util.Calendar javaStart
        {
            get => sys.DateTimeToJava(Start);
            set => Start = sys.DateTimeFromJava(value);
        }

        public Java.Util.Calendar javaEnd
        {
            get => sys.DateTimeToJava(End);
            set => End = sys.DateTimeFromJava(value);
        }
        */
        public Java.Util.Calendar guiDisplayStart
        {
            get => sys.DateTimeToJava(DisplayStart);
            set
            {
                DisplayStart = sys.DateTimeFromJava(value);
                OnPropertyChanged();

                Task.Factory.StartNew(() =>
                {
                    Task.Delay(100).Wait();
                    guiPropertiesChanged?.Invoke();
                });
            }
        }

        public Java.Util.Calendar guiDisplayEnd
        {
            get => sys.DateTimeToJava(DisplayEnd);
            set {
                DisplayEnd = sys.DateTimeFromJava(value);
                OnPropertyChanged();
            }
        }

        public bool guiAllDay
        {
            get => AllDay;
            set
            {
                AllDay = value;
                OnPropertyChanged();
            }
        }

        public Action guiPropertiesChanged { get; set; } = null;

        public Int32 javaColor
        {
            get => DisplayColor.ToAndroid();
        }
    }
}
