using System;
using System.Collections.Generic;
using System.IO;
using iChronoMe.Core.Classes;
using iChronoMe.Core.Interfaces;
using SkiaSharp;
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
        static Dictionary<string, List<ClockPath>> PathList = new Dictionary<string, List<ClockPath>>();
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

        public static bool ReloadHands(string ovveridedbpath = null)
        {
            if (!string.IsNullOrEmpty(ovveridedbpath))
                dbFile = ovveridedbpath;
            if (!File.Exists(dbFile))
                return false;
            try
            {
                var db = new mySQLiteConnection(dbFile, SQLiteOpenFlags.ReadOnly | SQLiteOpenFlags.FullMutex);
                var hands = db.Query<ClockHandConfig>("select * from ClockHandConfig");
                if (hands.Count == 0)
                    return false;
                var paths = db.Query<ClockPath>("select * from ClockPath");
                lock (HandCollection)
                {
                    HandCollection.Clear();
                    PathList.Clear();
                    DefaultLinks.Clear();
                    foreach (var path in paths)
                    {
                        if (!PathList.ContainsKey(path.Name))
                            PathList.Add(path.Name, new List<ClockPath>());
                        PathList[path.Name].Add(path);
                    }
                    foreach (var cfg in hands)
                    {
                        HandCollection.Add(cfg.ID, cfg);
                        if (!string.IsNullOrEmpty(cfg.HourPaths) && PathList.ContainsKey(cfg.HourPaths))
                        {
                            cfg.HourPathList = new List<ClockPath>(PathList[cfg.HourPaths]);
                            foreach (var p in cfg.HourPathList)
                            {
                                cfg.AllowHourStroke = cfg.AllowHourStroke && !"-".Equals(p.StrokeColor);
                                cfg.AllowHourFill = cfg.AllowHourFill && !"-".Equals(p.FillColor);
                            }
                        }
                        if (!string.IsNullOrEmpty(cfg.MinutePaths) && PathList.ContainsKey(cfg.MinutePaths))
                        { cfg.MinutePathList = new List<ClockPath>(PathList[cfg.MinutePaths]);
                            foreach (var p in cfg.HourPathList)
                            {
                                cfg.AllowHourStroke = cfg.AllowHourStroke && !"-".Equals(p.StrokeColor);
                                cfg.AllowHourFill = cfg.AllowHourFill && !"-".Equals(p.FillColor);
                            }
                        }
                        if (!string.IsNullOrEmpty(cfg.SecondPaths) && PathList.ContainsKey(cfg.SecondPaths))
                        { cfg.SecondPathList = new List<ClockPath>(PathList[cfg.SecondPaths]);
                            foreach (var p in cfg.HourPathList)
                            {
                                cfg.AllowMinuteStroke = cfg.AllowMinuteStroke && !"-".Equals(p.StrokeColor);
                                cfg.AllowMinuteFill = cfg.AllowMinuteFill && !"-".Equals(p.FillColor);
                            }
                        }
                        if (!string.IsNullOrEmpty(cfg.CapPaths) && PathList.ContainsKey(cfg.CapPaths))
                        { cfg.CapPathList = new List<ClockPath>(PathList[cfg.CapPaths]);
                            foreach (var p in cfg.HourPathList)
                            {
                                cfg.AllowSecondStroke = cfg.AllowSecondStroke && !"-".Equals(p.StrokeColor);
                                cfg.AllowSecondFill = cfg.AllowSecondFill && !"-".Equals(p.FillColor);
                            }
                        }

                        if (!string.IsNullOrEmpty(cfg.Defaults))
                        {
                            foreach (string c in cfg.Defaults.Split('|'))
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

        public static List<ClockPath> GetPaths(string pathName)
        {
            if (PathList.ContainsKey(pathName))
                return new List<ClockPath>(PathList[pathName]);
            return null;
        }

        public string ID { get; set; }
        public string Description { get; set; }
        public string Defaults { get; set; }
        public string HourPaths { get; set; }
        public string MinutePaths { get; set; }
        public string SecondPaths { get; set; }
        public string CapPaths { get; set; }

        [Ignore] public bool AllowHourStroke { get; set; } = true;
        [Ignore] public bool AllowHourFill { get; set; } = true;
        [Ignore] public bool AllowMinuteStroke { get; set; } = true;
        [Ignore] public bool AllowMinuteFill { get; set; } = true;
        [Ignore] public bool AllowSecondStroke { get; set; } = true;
        [Ignore] public bool AllowSecondFill { get; set; } = true;

        [Ignore]public List<ClockPath> HourPathList { get; set; }
        [Ignore] public List<ClockPath> MinutePathList { get; set; }
        [Ignore] public List<ClockPath> SecondPathList { get; set; }
        [Ignore] public List<ClockPath> CapPathList { get; set; }

        public ClockHandConfig Clone()
        {
            return (ClockHandConfig)MemberwiseClone();
        }

        static ClockHandConfig defaultHands = new ClockHandConfig
        {
            ID = "_",
            HourPathList = new List<ClockPath>(new ClockPath[] { new ClockPath { Path = "M 0 40 0 -200", StorkeWidth = 15, OffsetX = 500, OffsetY = 500 } }),
            MinutePathList = new List<ClockPath>(new ClockPath[] { new ClockPath { Path = "M 0 48 0 -380", StorkeWidth = 15, OffsetX = 500, OffsetY = 500 } }),
            CapPathList = new List<ClockPath>(new ClockPath[] { new ClockPath { Path = "M 0 50 0 -400", StorkeWidth = 5, OffsetX = 500, OffsetY = 500 } })
        };

        public class ClockPath
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string Path { get; set; }
            public float StorkeWidth { get; set; }
            public string StrokeColor { get; set; }
            public string FillColor { get; set; }
            public int OffsetX { get; set; }
            public int OffsetY { get; set; }

            SKPath _skPath;
            [Ignore] public SKPath SkPath { get
                {
                    if (_skPath == null)
                        _skPath = SKPath.ParseSvgPathData(Path);
                    return _skPath;
                }
            }
        }
    }
}
