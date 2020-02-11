using System;
using System.Collections.Generic;
using System.Text;
using iChronoMe.Core.Classes;
using iChronoMe.Core.DataBinding;

namespace iChronoMe.Core.ViewModels
{
    public class MasterConfigViewModel : BaseObservable
    {
        private MainConfig main { get => AppConfigHolder.MainConfig; }
        private CalendarViewConfig cal { get => AppConfigHolder.CalendarViewConfig; }

        private void saveMain() { AppConfigHolder.SaveMainConfig(); }
        private void saveCal() { AppConfigHolder.SaveCalendarViewConfig(); }

        public bool AlwaysShowForegroundNotification
        {
            get => main.AlwaysShowForegroundNotification;
            set
            {
                main.AlwaysShowForegroundNotification = value;
                saveMain();
                OnPropertyChanged();
            }
        }

        public TimeType AppDefaultTimeType
        {
            get => sys.DefaultTimeType;
            set
            {
                sys.DefaultTimeType = value;
                saveMain();
                OnPropertyChanged();
                OnPropertyChanged(nameof(AppDefaultTimeType_SpinnerPosition));
            }
        }

        public int AppDefaultTimeType_SpinnerPosition
        {
            get
            {
                switch (AppDefaultTimeType) { case TimeType.TimeZoneTime: return 2; case TimeType.MiddleSunTime: return 1; default: return 0; }
            }
            set
            {
                switch (value)
                {
                    case 2:
                        AppDefaultTimeType = TimeType.TimeZoneTime;
                        break;
                    case 1:
                        AppDefaultTimeType = TimeType.MiddleSunTime;
                        break;
                    default:
                        AppDefaultTimeType = TimeType.RealSunTime;
                        break;
                }
            }
        }

        public List<string> HideCalendars { get; set; } = new List<string>();

        private TimeType CalendarTimeType
        {
            get => cal.CalendarTimeType;
            set
            {
                cal.CalendarTimeType = value;
                saveCal();
                OnPropertyChanged();
                OnPropertyChanged(nameof(CalendarTimeType_SpinnerPosition));
            }
        }
        public int CalendarTimeType_SpinnerPosition
        {
            get
            {
                switch (CalendarTimeType) { case TimeType.TimeZoneTime: return 2; case TimeType.MiddleSunTime: return 1; default: return 0; }
            }
            set
            {
                switch (value)
                {
                    case 2:
                        CalendarTimeType = TimeType.TimeZoneTime;
                        break;
                    case 1:
                        CalendarTimeType = TimeType.MiddleSunTime;
                        break;
                    default:
                        CalendarTimeType = TimeType.RealSunTime;
                        break;
                }
            }
        }

        public bool CalendarUseAppDefautlTimeType
        {
            get => cal.UseAppDefautlTimeType;
            set
            {
                cal.UseAppDefautlTimeType = value;
                saveCal();
                OnPropertyChanged();
                OnPropertyChanged(nameof(CalendarUseOwnTimeType));
            }
        }

        public bool CalendarUseOwnTimeType { get => !CalendarUseAppDefautlTimeType; set => CalendarUseAppDefautlTimeType = !value; }
    }
}