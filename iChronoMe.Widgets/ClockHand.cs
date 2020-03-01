using System;
using System.Collections.Generic;
using System.IO;
using iChronoMe.Core.Classes;
using iChronoMe.Core.Interfaces;
using SQLite;

namespace iChronoMe.Widgets
{
    public class ClockHandConfig
    {
        static string dbFile = Path.Combine(sys.PathDBdata, "clockhands.db");
        public static int Count { get => HandCollection.Count; }

        public static ClockHandConfig Get(string id)
        {
            try
            {
                lock (HandCollection)
                {
                    if (HandCollection.ContainsKey(id))
                        return HandCollection[id].Clone();
                    return HandCollection["_"].Clone();
                }
            }
            catch
            {
                return defaultHands.Clone();
            }
        }
        public static string GetDefaultID(string filter)
        {
            try
            {
                lock (HandCollection)
                {
                    if (DefaultLinks.ContainsKey(filter))
                        return DefaultLinks[filter].ID;
                }
            }
            catch { }
            return null;
        }

        public static List<string> GetAllIds()
        {
            return new List<string>(HandCollection.Keys);
        }

        static Dictionary<string, ClockHandConfig> HandCollection = new Dictionary<string, ClockHandConfig>();
        static Dictionary<string, ClockHandConfig> DefaultLinks = new Dictionary<string, ClockHandConfig>();

        static ClockHandConfig()
        {
            ReloadHands();
        }

        public static void CheckUpdateLocalData(IProgressChangedHandler handler)
        {
            if (AppConfigHolder.MainConfig.LastCheckClockHands.AddDays(7) < DateTime.Now || !File.Exists(dbFile))
            {
                if (DataLoader.CheckDataPackage(handler, DataLoader.filter_clockhands, sys.PathDBdata, localize.ProgressUpdateBaseData))
                {
                    ReloadHands();
                    AppConfigHolder.MainConfig.LastCheckClockHands = DateTime.Now;
                    AppConfigHolder.SaveMainConfig();
                }
            }
        }

        public static bool ReloadHands()
        {
            if (!File.Exists(dbFile))
                return false;
            try
            {
                var db = new mySQLiteConnection(dbFile, SQLiteOpenFlags.ReadOnly | SQLiteOpenFlags.FullMutex);
                var hands = db.Query<ClockHandConfig>("select * from ClockHands");
                if (hands.Count == 0)
                    return false;
                lock (HandCollection)
                {
                    HandCollection.Clear();
                    DefaultLinks.Clear();
                    foreach (var cfg in hands)
                    {
                        HandCollection.Add(cfg.ID, cfg);
                        if (!string.IsNullOrEmpty(cfg.Defaults))
                        {
                            foreach (string c in cfg.Defaults.Split(' '))
                            {
                                DefaultLinks[c] = cfg;
                            }
                        }
                    }
                }
                db.Close();
                return true;
            }
            catch (Exception ex)
            {
                xLog.Error(ex);
                try { File.Delete(dbFile); } catch { }
            }
            finally
            {
                if (HandCollection.Count == 0)
                    HandCollection.Add("_", defaultHands);
            }
            return false;
        }

        public string ID { get; set; }
        public string Description { get; set; }
        public string Defaults { get; set; }
        public string HourPath { get; set; }
        public float HourStorkeWidth { get; set; }
        public string HourStrokeColor { get; set; }
        public string HourFillColor { get; set; }
        public int HourOffsetX { get; set; }
        public int HourOffsetY { get; set; }
        public string MinutePath { get; set; }
        public float MinuteStorkeWidth { get; set; }
        public string MinuteStrokeColor { get; set; }
        public string MinuteFillColor { get; set; }
        public int MinuteOffsetX { get; set; }
        public int MinuteOffsetY { get; set; }
        public string SecondPath { get; set; }
        public float SecondStorkeWidth { get; set; }
        public string SecondStrokeColor { get; set; }
        public string SecondFillColor { get; set; }
        public int SecondOffsetX { get; set; }
        public int SecondOffsetY { get; set; }
        public string CapPath { get; set; }
        public float CapStorkeWidth { get; set; }
        public string CapStrokeColor { get; set; }
        public string CapFillColor { get; set; }
        public int CapOffsetX { get; set; }
        public int CapOffsetY { get; set; }

        public ClockHandConfig Clone()
        {
            return (ClockHandConfig)MemberwiseClone();
        }

        static ClockHandConfig defaultHands = new ClockHandConfig
        {
            ID = "_",
            HourPath = "M 0 40 0 -200",
            HourStorkeWidth = 15,
            HourOffsetX = 500,
            HourOffsetY = 500,
            MinutePath = "M 0 48 0 -380",
            MinuteStorkeWidth = 15,
            MinuteOffsetX = 500,
            MinuteOffsetY = 500,
            SecondPath = "M 0 50 0 -400",
            SecondStorkeWidth = 5,
            SecondOffsetX = 500,
            SecondOffsetY = 500
        };
    }
}
