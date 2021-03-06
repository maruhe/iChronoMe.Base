﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using iChronoMe.Core.Abstractions;
using iChronoMe.Core.Interfaces;

using Xamarin.Essentials;

namespace iChronoMe.Core.Classes
{
    public static partial class sys
    {

        public static TimeType DefaultTimeType = TimeType.RealSunTime;

        public const double OneYear = 365.24219052;
        public static int DisplayWidth { get; private set; } = 1024;
        public static int DisplayHeight { get; private set; } = 768;
        public static int DisplayLongSite { get; private set; } = 1024;
        public static int DisplayShortSite { get; private set; } = 768;
        public static float DisplayDensity { get; private set; } = 1;
        public static float DisplayShortSiteDp { get => DisplayShortSite / DisplayDensity; }

        public static IProgressChangedHandler MainUserIO { get; set; } = null;

#if DEBUG
        public const bool Debugmode = true;
#else
        public const bool Debugmode = false;
#endif

        private static IDeviceDisplay _deviceDispay = null;
        public static IDeviceDisplay xDeviceDisplay
        {
            get
            {
                if (_deviceDispay == null)
                    return new DummyDeviceDisplay();
                return _deviceDispay;
            }
            set
            {
                if (value != null)
                    _deviceDispay = value;
            }
        }

        // Orientation (Landscape, Portrait, Square, Unknown)
        public static DisplayOrientation DisplayOrientation { get => DeviceDisplay.MainDisplayInfo.Orientation; }

        // Rotation (0, 90, 180, 270)
        public static DisplayRotation DisplayRotation { get => DeviceDisplay.MainDisplayInfo.Rotation; }

        static OsType _osType = OsType.Undefined;
        public static bool isAndroid { get => OsType.Android.Equals(_osType); }
        public static bool isWindows { get => OsType.Windows.Equals(_osType); }
        public static bool isIOS { get => OsType.iOS.Equals(_osType); }
        public static bool isMacOS { get => OsType.macOS.Equals(_osType); }
        public static OsType OsType { get => _osType; }

        private static void Init(OsType os)//, IDataBaseEngine dbEngine, IDeviceDisplay display)
        {
            if (_osType != OsType.Undefined || os == OsType.Undefined)
                return;
            _osType = os;

            try
            {
                // Get Metrics
                var mDisplayInfo = DeviceDisplay.MainDisplayInfo;

                // Width (in pixels)
                DisplayWidth = (int)mDisplayInfo.Width;

                // Height (in pixels)
                DisplayHeight = (int)mDisplayInfo.Height;

                // Screen density
                DisplayDensity = (float)mDisplayInfo.Density;

                if (DisplayWidth > DisplayHeight)
                {
                    DisplayLongSite = DisplayWidth;
                    DisplayShortSite = DisplayHeight;
                }
                else
                {
                    DisplayLongSite = DisplayHeight;
                    DisplayShortSite = DisplayWidth;
                }
            }
            catch (Exception ex)
            {
                xLog.Error(ex);
            }
        }

        static sys()
        {
            SQLitePCL.Batteries_V2.Init();

            PlatformInit();
        }

        public static int DpPx(double dp)
          => (int)(dp * sys.DisplayDensity);

        public static float PxDp(int px)
          => (float)px / sys.DisplayDensity;

        private static string _DataPath;
        public static string PathData
        {
            get
            {
                if (string.IsNullOrEmpty(_DataPath))
                {
                    if (sys.isWindows)
                        _DataPath = Path.Combine(Path.GetTempPath(), "icmData"); //Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "iChronoMe");
                    else if (sys.isMacOS)
                        Path.Combine(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "iChronoMe"), "icmData");
                    else
                        _DataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
                }
                return _DataPath;
            }
        }

        private static string _ConfigPath;
        public static string PathConfig
        {
            get
            {
                if (string.IsNullOrEmpty(_ConfigPath))
                {
                    _ConfigPath = Path.Combine(PathData, "config");
                    if (!Directory.Exists(_ConfigPath))
                        Directory.CreateDirectory(_ConfigPath);
                }
                return _ConfigPath;
            }
        }

        private static string _ConfigPathCalCfg;
        public static string ConfigPathCalCfg
        {
            get
            {
                if (string.IsNullOrEmpty(_ConfigPathCalCfg))
                {
                    _ConfigPathCalCfg = Path.Combine(PathData, "dynamic_calendars");
                    if (!Directory.Exists(_ConfigPathCalCfg))
                        Directory.CreateDirectory(_ConfigPathCalCfg);
                }
                return _ConfigPathCalCfg;
            }
        }

        private static string _SharePath;
        public static string PathShare
        {
            get
            {
                if (string.IsNullOrEmpty(_SharePath))
                {
                    _SharePath = Path.Combine(PathData, "share");
                    if (!Directory.Exists(_SharePath))
                        Directory.CreateDirectory(_SharePath);
                }
                return _SharePath;
            }
        }

        private static string _WidgetCachePath;
        public static string PathWidgetCache
        {
            get
            {
                if (string.IsNullOrEmpty(_WidgetCachePath))
                {
                    _WidgetCachePath = Path.Combine(PathShare, "WidgetCache");
                    if (!Directory.Exists(_WidgetCachePath))
                        Directory.CreateDirectory(_WidgetCachePath);
                }
                return _WidgetCachePath;
            }
        }

        public static string GetPathWidgetCache(int iWidgetID)
        {
            string cParh = Path.Combine(PathWidgetCache, iWidgetID.ToString());
            if (!Directory.Exists(cParh))
                Directory.CreateDirectory(cParh);
            return cParh;
        }

        private static string _PathDBdata;
        public static string PathDBdata
        {
            get
            {
                if (string.IsNullOrEmpty(_PathDBdata))
                {
                    _PathDBdata = Path.Combine(PathData, "database");
                    if (!Directory.Exists(_PathDBdata))
                        Directory.CreateDirectory(_PathDBdata);
                }
                return _PathDBdata;
            }
        }

        private static string _CachePath;
        public static string PathCache
        {
            get
            {
                if (string.IsNullOrEmpty(_CachePath))
                {
                    if (sys.isWindows)
                        _CachePath = Path.Combine(Path.GetTempPath(), "icmCache"); //Path.Combine(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "iChronoMe"), "AppCache");
                    else if (sys.isMacOS)
                        Path.Combine(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "iChronoMe"), "icmCache");
                    else
                        _CachePath = System.IO.Path.GetTempPath();
                }
                return _CachePath;
            }
        }

        private static string _PathDBcache;
        public static string PathDBcache
        {
            get
            {
                if (string.IsNullOrEmpty(_PathDBcache))
                {
                    _PathDBcache = Path.Combine(PathCache, "database");
                    if (!Directory.Exists(_PathDBcache))
                        Directory.CreateDirectory(_PathDBcache);
                }
                return _PathDBcache;
            }
        }

        public static string DezimalGradToGrad(double nGrad, bool bTrueIsLatFalseIsLong, bool bShowSecondDecimals = false)
        {
            int iDirektion = (int)nGrad;

            if (nGrad < 0)
                nGrad = nGrad * -1;

            int iGrad = (int)nGrad;
            nGrad -= iGrad;
            double nMinutes = 60 * nGrad;
            int iMinutes = (int)nMinutes;
            nMinutes -= iMinutes;
            double nSeconds = 60 * nMinutes;

            string c = iGrad.ToString() + "°" + iMinutes.ToString() + "'";
            if (bShowSecondDecimals)
                c += nSeconds.ToString("0.####") + "\"";
            else
                c += ((int)nSeconds).ToString() + "\"";

            c += (iDirektion >= 0 ? bTrueIsLatFalseIsLong ? "N" : "E" : bTrueIsLatFalseIsLong ? "S" : "W");
            return c;
        }

        public static string DezimalGradToGrad(double Latitude, double Longitude, bool bShowSecondDecimals = false, string cSeparator = ", ")
        {
            return DezimalGradToGrad(Latitude, true, bShowSecondDecimals) + cSeparator + DezimalGradToGrad(Longitude, false, bShowSecondDecimals);
        }
        public static string DezimalGradToString(double Latitude, double Longitude, int iDecimals = 6, string cSeparator = ", ")
        {
            string cFormat = "0";
            if (iDecimals > 0)
            {
                cFormat += ".";
                for (int i = 0; i < iDecimals; i++)
                {
                    cFormat += "#";
                }
            }
            return Latitude.ToString(cFormat, CultureInfo.InvariantCulture) + cSeparator + Longitude.ToString(cFormat, CultureInfo.InvariantCulture);
        }

        public async static Task<string> GetUrlContent(string cUrl, int iMaxTimeout = 10)
        {
            try
            {
                xLog.Debug("gogo: " + cUrl);
                using (HttpClient client = new HttpClient() { Timeout = TimeSpan.FromSeconds(iMaxTimeout) })
                using (HttpResponseMessage response = await client.GetAsync(cUrl))
                using (HttpContent content = response.Content)
                {
                    // ... Read the string.
                    string result = content.ReadAsStringAsync().Result;
                    xLog.Debug("gogo reveived " + result.Length.ToString() + " chars...");
                    return result;

                }
            }
            catch (Exception e)
            {
                e.ToString();
            }
            return null;
        }

        public static string EzMzText(int iCount, string cEzTExt, string cMzText, string cNulltext = "")
        {
            if (iCount == 1 || iCount == -1)
                return cEzTExt;
            else if (iCount == 0 && !string.IsNullOrEmpty(cNulltext))
                return cNulltext;
            else
                return string.Format(cMzText, iCount);
        }

        public static double parseDouble(string cParse, double nDefault = 0)
        {
            double d = nDefault;
            double.TryParse(cParse, NumberStyles.Any, CultureInfo.InvariantCulture, out d);
            return d;
        }

        public static DateTime GetTimeWithoutSeconds(DateTime tSource)
        {
            return new DateTime(tSource.Year, tSource.Month, tSource.Day, tSource.Hour, tSource.Minute, 0);
        }
        public static DateTime GetTimeWithoutMilliSeconds(DateTime tSource)
        {
            return new DateTime(tSource.Year, tSource.Month, tSource.Day, tSource.Hour, tSource.Minute, tSource.Second);
        }

        public static string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static string CalculateFileMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public static T CloneObject<T>(T obj)
        {
            try
            {
                XmlSerializer x = new XmlSerializer(typeof(T));
                MemoryStream mem = new MemoryStream();
                x.Serialize(mem, obj);
                mem.Seek(0, SeekOrigin.Begin);
                return (T)x.Deserialize(mem);
            }
            catch { }
            return default(T);
        }

        public static bool XmlEquals<T>(T obj1, T obj2)
        {
            try
            {
                if (obj1 == null || obj2 == null)
                    return false;

                XmlSerializer x1 = new XmlSerializer(typeof(T));
                var mem1 = new StringWriter();
                x1.Serialize(mem1, obj1);

                XmlSerializer x2 = new XmlSerializer(typeof(T));
                var mem2 = new StringWriter();
                x2.Serialize(mem2, obj2);

                if (mem1.ToString() == mem2.ToString())
                    return true;
                return false;
            }
            catch { }
            return false;
        }

        public static string GetExceptionFullLogText(Exception ex, string cMessage, string cAppVersionInfo, string cDeviceInfo = "")
        {
            if (ex == null)
                return string.Concat("Empty Exception\n", cDeviceInfo);

            string cRes = ex.GetType().FullName;
            if (!string.IsNullOrEmpty(cMessage))
                cRes += "\n" + cMessage;

            try
            {
                if (!string.IsNullOrEmpty(cAppVersionInfo))
                    cRes += "\nApp: " + cAppVersionInfo;

                if (!string.IsNullOrEmpty(ex.Source))
                    cRes += "\nSource:" + ex.Source;
                if (ex.Data != null && ex.Data.Count > 0)
                {
                    cRes += "\nData:";
                    foreach (var dat in ex.Data)
                        cRes += "\n\t" + dat.ToString();
                }
                if (!string.IsNullOrEmpty(ex.Message))
                    cRes += "\nMessage:\n" + ex.Message;
                if (!string.IsNullOrEmpty(ex.StackTrace))
                    cRes += "\nStackTrace:\n" + ex.StackTrace;
            }
            catch { }
            while (ex?.InnerException != null)
            {
                try
                {
                    ex = ex.InnerException;
                    cRes += "\nInnerException:\t" + ex.GetType().FullName;
                    if (!string.IsNullOrEmpty(ex.Source))
                        cRes += "\nSource:" + ex.Source;
                    if (ex.Data != null && ex.Data.Count > 0)
                    {
                        cRes += "\nData:";
                        foreach (var dat in ex.Data)
                            cRes += "\n\t" + dat.ToString();
                    }
                    cRes += "\nMessage:\n" + ex.Message + "\nStackTrace:\n" + ex.StackTrace;
                }
                catch { }
            }

            if (!string.IsNullOrEmpty(cDeviceInfo))
                cRes += "\n\nDeviceInfo:\n" + cDeviceInfo;
            return cRes;
        }

        public static int iAppVersionCode { get; private set; } = 0;
        public static string cAppVersionInfo { get; private set; } = "?";
        public static string cDeviceInfo { get; private set; } = "?";
        public static string cDeviceToken { get; private set; } = "?";

        public static string ErrorLogPath { get => Path.Combine(Path.GetTempPath(), "ErrorLog"); }
        public static List<string> AllDrawables { get; } = new List<string>();

        public static void DebugLogException(Exception exception, string cMessage = null, bool bTryShowUser = true)
        {
#if DEBUG
            LogException(exception, cMessage, bTryShowUser);
#endif
        }

        static Dictionary<string, int> loggedExceptions = new Dictionary<string, int>();
        public static void LogException(Exception exception, string cMessage = null, bool bTryShowUser = true)
        {
            string errorFilePath = null;
            try
            {
                var tNow = DateTime.Now;
                string errorFileName = tNow.ToString("yyyy_MM_dd___hh_mm__ss_fff") + ".log";
                var libraryPath = ErrorLogPath; // iOS: Environment.SpecialFolder.Resources
                if (!Directory.Exists(libraryPath))
                    Directory.CreateDirectory(libraryPath);

                errorFilePath = Path.Combine(libraryPath, errorFileName);
                var errorMessage = sys.GetExceptionFullLogText(exception, cMessage, cAppVersionInfo, cDeviceInfo);

                string cHash = (sys.CalculateMD5Hash(errorMessage));
                int errorCount = 0;
                if (loggedExceptions.ContainsKey(cHash))
                    errorCount = loggedExceptions[cHash];
                errorCount++;
                loggedExceptions[cHash] = errorCount;
                if (errorCount > 3)
                    return;

                errorMessage = tNow.ToString("s") + "." + tNow.Millisecond + "\n" + errorMessage;
                File.WriteAllText(errorFilePath, errorMessage);

                xLog.Error("Crash Report\n" + errorMessage);
            }
            catch
            {
                // just suppress any error logging exceptions
            }

            try { AfterExceptionLog(exception, bTryShowUser, errorFilePath); } catch { }
        }

        public static Location lastUserLocation = new Location(0, 0);

        public static int Sum(int[] v)
        {
            int sum = 0;
            foreach (var n in v)
                sum += n;
            return sum;
        }
        public static float Sum(float[] v)
        {
            float sum = 0;
            foreach (var n in v)
                sum += n;
            return sum;
        }
        public static double Sum(double[] v)
        {
            double sum = 0;
            foreach (var n in v)
                sum += n;
            return sum;
        }
    }

    public enum OsType
    {
        Undefined = 0,
        Windows,
        Android,
        iOS,
        macOS,
        Tizen
    }
}
