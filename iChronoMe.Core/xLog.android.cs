using System;

using Android.Util;

namespace iChronoMe.Core
{
    public static class xLog
    {
        public static void Verbose(string msg)
           => Log.Verbose("NoTag", msg);
        public static void Verbose(string tag, string msg)
           => Log.Verbose(tag, msg);
        public static void Verbose(string tag, Exception ex, string msg)
            => Log.Verbose(tag, Java.Lang.Throwable.FromException(ex), msg);
        public static void Verbose(string tag, Exception ex, string format, params object[] args)
            => Log.Verbose(tag, Java.Lang.Throwable.FromException(ex), format, args);
        public static void Verbose(string tag, string format, params object[] args)
            => Log.Verbose(tag, format, args);

        public static void Debug(string msg)
            => Log.Debug("NoTag", msg);
        public static void Debug(string tag, string msg)
            => Log.Debug(tag, msg);
        public static void Debug(string tag, Exception ex, string msg)
            => Log.Debug(tag, Java.Lang.Throwable.FromException(ex), msg);
        public static void Debug(string tag, Exception ex, string format, params object[] args)
            => Log.Debug(tag, Java.Lang.Throwable.FromException(ex), format, args);
        public static void Debug(string tag, string format, params object[] args)
            => Log.Debug(tag, format, args);

        public static void Info(string msg)
            => Log.Info("NoTag", msg);
        public static void Info(string tag, string msg)
            => Log.Info(tag, msg);
        public static void Info(string tag, Exception ex, string msg)
            => Log.Info(tag, Java.Lang.Throwable.FromException(ex), msg);
        public static void Info(string tag, Exception ex, string format, params object[] args)
            => Log.Info(tag, Java.Lang.Throwable.FromException(ex), format, args);
        public static void Info(string tag, string format, params object[] args)
            => Log.Info(tag, format, args);

        public static void Warn(string msg)
            => Log.Warn("NoTag", msg);
        public static void Warn(string tag, string msg)
            => Log.Warn(tag, msg);
        public static void Warn(string tag, Exception ex, string msg)
            => Log.Warn(tag, Java.Lang.Throwable.FromException(ex), msg);
        public static void Warn(string tag, Exception ex, string format, params object[] args)
            => Log.Warn(tag, Java.Lang.Throwable.FromException(ex), format, args);
        public static void Warn(string tag, string format, params object[] args)
            => Log.Warn(tag, format, args);

        public static void Error(string msg)
            => Log.Error("NoTag", msg);
        public static void Error(string tag, string msg)
            => Log.Error(tag, msg);
        public static void Error(string tag, Exception ex, string msg)
            => Log.Error(tag, Java.Lang.Throwable.FromException(ex), msg);
        public static void Error(string tag, Exception ex, string format, params object[] args)
            => Log.Error(tag, Java.Lang.Throwable.FromException(ex), format, args);
        public static void Error(string tag, string format, params object[] args)
            => Log.Error(tag, format, args);

        public static void Wtf(string msg)
            => Log.Wtf("NoTag", msg);
        public static void Wtf(string tag, string msg)
            => Log.Wtf(tag, msg);
        public static void Wtf(string tag, Exception ex, string msg)
            => Log.Wtf(tag, Java.Lang.Throwable.FromException(ex), msg);
        public static void Wtf(string tag, Exception ex, string format, params object[] args)
            => Log.Wtf(tag, Java.Lang.Throwable.FromException(ex), format, args);
        public static void Wtf(string tag, string format, params object[] args)
            => Log.Wtf(tag, format, args);
    }
}