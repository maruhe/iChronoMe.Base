using System;
using System.IO;
using iChronoMe.Core.Classes;

namespace iChronoMe
{
    public static class AppConfigHolder
    {
        private static LocationConfig _locationConfig = null;
        private static MainConfig _mainConfig = null;
        private static DashboardConfig _dashboardConfig = null;
        private static CalendarViewConfig _calendarViewConfig = null;

        public static LocationConfig LocationConfig
        {
            get
            {
                if (_locationConfig == null)
                {
                    _locationConfig = LoadFromFile<LocationConfig>();
                }
                return _locationConfig;
            }
        }

        public static void SaveLocationConfig()
        {
            if (_locationConfig != null)
                SaveConfig(_locationConfig);
        }

        public static MainConfig MainConfig
        {
            get
            {
                if (_mainConfig == null)
                {
                    _mainConfig = LoadFromFile<MainConfig>();
                }
                return _mainConfig;
            }
        }

        public static void SaveMainConfig()
        {
            if (_mainConfig != null)
                SaveConfig(_mainConfig);
        }

        public static DashboardConfig DashboardConfig
        {
            get
            {
                if (_dashboardConfig == null)
                {
                    _dashboardConfig = LoadFromFile<DashboardConfig>();
                }
                return _dashboardConfig;
            }
        }

        public static void SaveDashboardConfig()
        {
            if (_dashboardConfig != null)
                SaveConfig(_dashboardConfig);
        }

        public static CalendarViewConfig CalendarViewConfig
        {
            get
            {
                if (_calendarViewConfig == null)
                {
                    _calendarViewConfig = LoadFromFile<CalendarViewConfig>();
                }
                return _calendarViewConfig;
            }
        }

        public static void SaveCalendarViewConfig()
        {
            if (_calendarViewConfig != null)
                SaveConfig(_calendarViewConfig);
        }

        private static void SaveConfig<T>(T cfg)
        {
            string cfgFile = Path.Combine(sys.PathConfig, typeof(T).Name + ".cfg");
            try
            {
                try
                {
                    if (File.Exists(cfgFile))
                        File.Copy(cfgFile, cfgFile + ".bak", true);
                }
                catch { }
                SmoothXmlSerializer x = new SmoothXmlSerializer();
                TextWriter writer = new StreamWriter(cfgFile + ".new");
                x.Serialize(writer, cfg);
                writer.Flush();
                writer.Close();

                File.Delete(cfgFile);
                File.Move(cfgFile + ".new", cfgFile);

            }
            catch (Exception e)
            {
                e.ToString();
                try
                {
                    if (File.Exists(cfgFile + ".bak"))
                        File.Copy(cfgFile + ".bak", cfgFile, true);
                }
                catch { }
            }
        }
        private static T LoadFromFile<T>()
        {
            string cfgFile = Path.Combine(sys.PathConfig, typeof(T).Name + ".cfg");
            try
            {
                using (var stream = new StreamReader(cfgFile))
                {
                    var serializer = new SmoothXmlSerializer();
                    var data = serializer.Deserialize<T>(stream);
                    stream.Close();
                    return data;
                }
            }
            catch (Exception e)
            {
                e.ToString();
                return (T)Activator.CreateInstance(typeof(T));
            }
        }
    }
}
