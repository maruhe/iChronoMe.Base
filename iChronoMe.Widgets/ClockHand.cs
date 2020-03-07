using System;
using System.Collections.Generic;
using System.IO;

using iChronoMe.Core.Classes;
using iChronoMe.Core.Interfaces;
using iChronoMe.Core.Types;

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

        public static ClockfaceInfo GetFaceInfo(string clockface)
        {
            if (FaceInfos.ContainsKey(clockface))
                return FaceInfos[clockface];
            return null;
        }

        static Dictionary<string, ClockHandConfig> HandCollection = new Dictionary<string, ClockHandConfig>();
        static Dictionary<string, List<ClockPath>> PathList = new Dictionary<string, List<ClockPath>>();
        static Dictionary<string, ClockHandConfig> DefaultLinks = new Dictionary<string, ClockHandConfig>();
        static Dictionary<string, ClockfaceInfo> FaceInfos = new Dictionary<string, ClockfaceInfo>();

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
                            var x = CheckHasCustomizablePaths(cfg.HourPathList);
                            cfg.AllowCustomHourStroke = x.stroke;
                            cfg.AllowCustomHourFill = x.fill;
                        }
                        if (!string.IsNullOrEmpty(cfg.MinutePaths) && PathList.ContainsKey(cfg.MinutePaths))
                        {
                            cfg.MinutePathList = new List<ClockPath>(PathList[cfg.MinutePaths]);
                            var x = CheckHasCustomizablePaths(cfg.MinutePathList);
                            cfg.AllowCustomMinuteStroke = x.stroke;
                            cfg.AllowCustomMinuteFill = x.fill;
                        }
                        if (!string.IsNullOrEmpty(cfg.SecondPaths) && PathList.ContainsKey(cfg.SecondPaths))
                        {
                            cfg.SecondPathList = new List<ClockPath>(PathList[cfg.SecondPaths]);
                            var x = CheckHasCustomizablePaths(cfg.SecondPathList);
                            cfg.AllowCustomSecondStroke = x.stroke;
                            cfg.AllowCustomSecondFill = x.fill;
                        }
                        if (!string.IsNullOrEmpty(cfg.CapPaths) && PathList.ContainsKey(cfg.CapPaths))
                        {
                            cfg.CapPathList = new List<ClockPath>(PathList[cfg.CapPaths]);
                        }

                        if (!string.IsNullOrEmpty(cfg.Defaults))
                        {
                            foreach (string c in cfg.Defaults.Split(' '))
                            {
                                DefaultLinks[c] = cfg;
                            }
                        }
                    }
                    FaceInfos.Clear();
                    var faces = db.Query<ClockfaceInfo>("select * from ClockfaceInfo");
                    foreach (var face in faces)
                        FaceInfos[face.Clockface] = face;
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

        private static (bool stroke, bool fill) CheckHasCustomizablePaths(List<ClockPath> pathList)
        {
            int iStrokes = 0, iFills = 0;
            foreach (var p in pathList)
            {
                if (!"-".Equals(p.StrokeColor) && p.StrokeWidth > 1)
                    iStrokes++;
                if (!"-".Equals(p.FillColor))
                    iFills++;
            }
            return (iStrokes > 0, iFills > 0);
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

        [Ignore] public bool AllowCustomHourStroke { get; set; } = false;
        [Ignore] public bool AllowCustomHourFill { get; set; } = false;
        [Ignore] public bool AllowCustomMinuteStroke { get; set; } = false;
        [Ignore] public bool AllowCustomMinuteFill { get; set; } = false;
        [Ignore] public bool AllowCustomSecondStroke { get; set; } = false;
        [Ignore] public bool AllowCustomSecondFill { get; set; } = false;

        [Ignore] public List<ClockPath> HourPathList { get; set; }
        [Ignore] public List<ClockPath> MinutePathList { get; set; }
        [Ignore] public List<ClockPath> SecondPathList { get; set; }
        [Ignore] public List<ClockPath> CapPathList { get; set; }

        public ClockHandConfig Clone()
        {
            return (ClockHandConfig)MemberwiseClone();
        }

        public static ClockHandConfig defaultHands = new ClockHandConfig
        {
            ID = "_",
            HourPathList = new List<ClockPath>(new ClockPath[] { new ClockPath { Path = "M 0 40 0 -200", StrokeWidth = 15, OffsetX = 500, OffsetY = 500, StrokeColor = "#FFF", FillColor = "-" } }),
            MinutePathList = new List<ClockPath>(new ClockPath[] { new ClockPath { Path = "M 0 48 0 -380", StrokeWidth = 15, OffsetX = 500, OffsetY = 500, StrokeColor = "#FFF", FillColor = "-" } }),
            SecondPathList = new List<ClockPath>(new ClockPath[] { new ClockPath { Path = "M 0 50 0 -400", StrokeWidth = 5, OffsetX = 500, OffsetY = 500, StrokeColor = "#FFF", FillColor = "-" } })
        };

        public class ClockPath
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string Path { get; set; }
            public float StrokeWidth { get; set; }
            public string StrokeColor { get; set; }
            public string FillColor { get; set; }
            public int OffsetX { get; set; }
            public int OffsetY { get; set; }

            SKPath _skPath;
            [Ignore]
            public SKPath SkPath
            {
                get
                {
                    if (_skPath == null)
                        _skPath = SKPath.ParseSvgPathData(Path);
                    return _skPath;
                }
            }

            public xColor GetStrokeColor(string bannedColors, string suggestedColor)
            {
                if ("_".Equals(StrokeColor))
                    return xColor.Transparent;

                var clr = xColor.FromHex(StrokeColor);
                if (string.IsNullOrEmpty(bannedColors))
                    return clr;

                int iTry = string.IsNullOrEmpty(suggestedColor) ? 1 : 0;
                while (iTry < 5)
                {
                    iTry++;
                    bool bIsValid = true;
                    foreach (string cBanned in bannedColors.Trim().Split(' '))
                    {
                        if (clr.IsSimilar(xColor.FromHex(cBanned)))
                        {
                            bIsValid = false;
                            continue;
                        }
                    }

                    if (bIsValid)
                        return clr;

                    if (iTry == 1)
                        clr = suggestedColor;
                    if (iTry == 2)
                        clr = clr.Invert();
                    if (iTry == 3)
                        clr = xColor.MaterialBlue;
                    if (iTry == 4)
                        clr = xColor.MaterialGreen;
                    if (iTry == 5)
                        clr = xColor.MaterialRed;
                }
                return clr;
            }

            public xColor GetFillColor(string bannedColors, string suggestedColor)
            {
                if ("_".Equals(StrokeColor))
                    return xColor.Transparent;

                var clr = xColor.FromHex(FillColor);

                if (string.IsNullOrEmpty(bannedColors))
                    return clr;

                int iTry = string.IsNullOrEmpty(suggestedColor) ? 1 : 0;
                while (iTry < 5)
                {
                    iTry++;
                    bool bIsValid = true;
                    foreach (string cBanned in bannedColors.Trim().Split(' '))
                    {
                        if (clr.IsSimilar(xColor.FromHex(cBanned)))
                        {
                            bIsValid = false;
                            continue;
                        }
                    }

                    if (bIsValid)
                        return clr;

                    if (iTry == 1)
                        clr = suggestedColor;
                    if (iTry == 2)
                        clr = clr.Invert();
                    if (iTry == 3)
                        clr = xColor.MaterialBlue;
                    if (iTry == 4)
                        clr = xColor.MaterialGreen;
                    if (iTry == 5)
                        clr = xColor.MaterialRed;
                }
                return clr;
            }
        }

        public class ClockfaceInfo
        {
            public string Clockface { get; set; }
            public string MainColor { get; set; }
            public string HandColorsBanned { get; set; }
            public string HandColorSuggestion { get; set; }
            public bool AskUserBackgroundColor { get; set; }
            public bool AllowTint { get; set; }
            public string Info { get; set; }
        }
    }
}
