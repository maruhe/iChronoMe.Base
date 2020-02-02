using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;

using iChronoMe.Core.DataModels;
using iChronoMe.Core.DynamicCalendar;
using iChronoMe.Core.Extentions;
using iChronoMe.DeviceCalendar;

using SQLite;

using static iChronoMe.Core.Classes.GeoInfo;

namespace iChronoMe.Core.Classes
{
    public static class db
    {
        static mySQLiteConnection _dbConfig;
        public static mySQLiteConnection dbConfig
        {
            get
            {
                if (_dbConfig == null)
                {
                    _dbConfig = new mySQLiteConnection(Path.Combine(sys.PathDBdata, "config.db"), SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex);
                    var c = _dbConfig.GetTableInfo("SelectPositionResult");
                    try { _dbConfig.CreateTable<SelectPositionResult>(); }
                    catch { _dbConfig.DropTable<SelectPositionResult>(); _dbConfig.CreateTable<SelectPositionResult>(); }
                    try { _dbConfig.CreateTable<Creature>(); }
                    catch { _dbConfig.DropTable<Creature>(); _dbConfig.CreateTable<Creature>(); }
                }
                return (_dbConfig);
            }
        }

        static mySQLiteConnection _dbAreaCache;
        public static mySQLiteConnection dbAreaCache
        {
            get
            {
                if (_dbAreaCache == null)
                {
                    _dbAreaCache = new mySQLiteConnection(Path.Combine(sys.PathDBcache, "area_cache.db"), SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex);
                    try { _dbAreaCache.CreateTable<AreaInfo>(); }
                    catch { _dbAreaCache.DropTable<AreaInfo>(); _dbAreaCache.CreateTable<AreaInfo>(); }
                    try { _dbAreaCache.CreateTable<TimeZoneInfoCache>(); }
                    catch { _dbAreaCache.DropTable<TimeZoneInfoCache>(); _dbAreaCache.CreateTable<TimeZoneInfoCache>(); }
                }
                return (_dbAreaCache);
            }
        }

        static mySQLiteConnection _dbCalendarExtention;
        public static mySQLiteConnection dbCalendarExtention
        {
            get
            {
                if (_dbCalendarExtention == null)
                {
                    _dbCalendarExtention = new mySQLiteConnection(Path.Combine(sys.PathDBcache, "calendar_extention.db"), SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex);
                    try { _dbCalendarExtention.CreateTable<CalendarEventExtention>(); }
                    catch { _dbCalendarExtention.DropTable<CalendarEventExtention>(); _dbCalendarExtention.CreateTable<CalendarEventExtention>(); }
                }
                return (_dbCalendarExtention);
            }
        }

        public static mySQLiteConnection GetCalendarModelUserData(string cModelId)
        {
            var _calendarModelCache = new mySQLiteConnection(Path.Combine(sys.PathDBdata, "calendar_model_userdata_" + cModelId + ".db"), SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex);
            try { _calendarModelCache.CreateTable<YearInfo>(); }
            catch { _calendarModelCache.DropTable<YearInfo>(); _calendarModelCache.CreateTable<YearInfo>(); }
            try { _calendarModelCache.CreateTable<MonthInfo>(); }
            catch { _calendarModelCache.DropTable<MonthInfo>(); _calendarModelCache.CreateTable<MonthInfo>(); }
            return (_calendarModelCache);
        }
        public static mySQLiteConnection GetCalendarModelCache(string cModelId)
        {
            var _calendarModelCache = new mySQLiteConnection(Path.Combine(sys.PathDBcache, "calendar_model_cache_" + cModelId + ".db"), SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex);
            try { _calendarModelCache.CreateTable<YearInfo>(); }
            catch { _calendarModelCache.DropTable<YearInfo>(); _calendarModelCache.CreateTable<YearInfo>(); }
            try { _calendarModelCache.CreateTable<MonthInfo>(); }
            catch { _calendarModelCache.DropTable<MonthInfo>(); _calendarModelCache.CreateTable<MonthInfo>(); }
            return (_calendarModelCache);
        }
    }

    public abstract class dbObject
    {
        [PrimaryKey, AutoIncrement, Indexed]
        public int RecNo { get; set; } = -1;

        public virtual void OnInstanceCreatedDB()
        {

            // Can be overridden.

        }
    }

    public class mySQLiteConnection : SQLiteConnection
    {

        public mySQLiteConnection(SQLiteConnectionString connectionString)
            : base(connectionString)
        {
            this.ExecuteScalar<string>("PRAGMA journal_mode = TRUNCATE");
        }

        public mySQLiteConnection(string databasePath, bool storeDateTimeAsTicks = true)
            : base(databasePath, storeDateTimeAsTicks)
        {
            this.ExecuteScalar<string>("PRAGMA journal_mode = TRUNCATE");
        }

        public mySQLiteConnection(string databasePath, SQLiteOpenFlags openFlags, bool storeDateTimeAsTicks = true)
            : base(databasePath, openFlags, storeDateTimeAsTicks)
        {
            this.ExecuteScalar<string>("PRAGMA journal_mode = TRUNCATE");
        }

        public List<AreaInfo> QueryByLocation(double lat, double lng, double radius)
        {
            PointF center = new PointF((float)lat, (float)lng);
            double mult = 1.1; // mult = 1.1; is more reliable
            PointF p1 = calculateDerivedPosition(center, mult * radius, 0);
            PointF p2 = calculateDerivedPosition(center, mult * radius, 90);
            PointF p3 = calculateDerivedPosition(center, mult * radius, 180);
            PointF p4 = calculateDerivedPosition(center, mult * radius, 270);

            string cSql = "select * from AreaInfo WHERE "
                    + "centerLat > " + p3.X.ToString(CultureInfo.InvariantCulture) + " AND "
                    + "centerLat < " + p1.X.ToString(CultureInfo.InvariantCulture) + " AND "
                    + "centerLong < " + p2.Y.ToString(CultureInfo.InvariantCulture) + " AND "
                    + "centerLong > " + p4.Y.ToString(CultureInfo.InvariantCulture);

            return Query<AreaInfo>(cSql, new object[0]);
        }

        public static PointF calculateDerivedPosition(PointF point, double range, double bearing)
        {
            double EarthRadius = 6371000; // m

            double latA = DoubleExtensions.ToRadians(point.X);
            double lonA = DoubleExtensions.ToRadians(point.Y);
            double angularDistance = range / EarthRadius;
            double trueCourse = DoubleExtensions.ToRadians(bearing);

            double lat = Math.Asin(
                    Math.Sin(latA) * Math.Cos(angularDistance) +
                            Math.Cos(latA) * Math.Sin(angularDistance)
                            * Math.Cos(trueCourse));

            double dlon = Math.Atan2(
                    Math.Sin(trueCourse) * Math.Sin(angularDistance)
                            * Math.Cos(latA),
                    Math.Cos(angularDistance) - Math.Sin(latA) * Math.Sin(lat));

            double lon = ((lonA + dlon + Math.PI) % (Math.PI * 2)) - Math.PI;

            lat = lat.ToDegrees();
            lon = lon.ToDegrees();

            PointF newPoint = new PointF((float)lat, (float)lon);

            return newPoint;

        }

        protected override SQLiteCommand NewCommand()
        {
            return base.NewCommand();
        }
    }
    /*
    public class mySQLiteCommand : SQLiteCommand
    {
        public mySQLiteCommand(SQLiteConnection conn) : base(conn) { }

        protected override void OnInstanceCreated(object obj)
        {
            base.OnInstanceCreated(obj);
            if (obj is dbObject)
                (obj as dbObject).OnInstanceCreatedDB();
        }
    }*/
}
