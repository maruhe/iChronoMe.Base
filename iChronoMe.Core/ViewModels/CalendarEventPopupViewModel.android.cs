using System;
using System.Collections.Generic;
using System.Text;
using iChronoMe.Core.Classes;

namespace iChronoMe.Core.ViewModels
{
    public partial class CalendarEventPopupViewModel
    {
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

        public Java.Util.Calendar javaDisplayStart
        {
            get => sys.DateTimeToJava(DisplayStart);
            set => DisplayStart = sys.DateTimeFromJava(value);
        }

        public Java.Util.Calendar javaDisplayEnd
        {
            get => sys.DateTimeToJava(DisplayEnd);
            set => DisplayEnd = sys.DateTimeFromJava(value);
        }

        public Int32 javaColor
        {
            get => DisplayColor.ToAndroid();
        }
    }
}
