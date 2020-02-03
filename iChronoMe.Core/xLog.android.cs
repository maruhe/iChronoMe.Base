using System;
using System.Runtime.CompilerServices;

using Android.Util;

namespace iChronoMe
{
    public static class xLog
    {
        public static void Verbose(string msg, [CallerMemberName] string caller = null, [CallerFilePath] string tag = null)
            => Log.Verbose(cleanTag(tag), string.Concat(caller, ": ", msg));
        public static void Verbose(Exception ex, string msg = null, [CallerMemberName] string caller = null, [CallerFilePath] string tag = null)
            => Log.Verbose(cleanTag(tag), Java.Lang.Throwable.FromException(ex), string.Concat(caller, ": ", msg));

        public static void Debug(string msg, [CallerMemberName] string caller = null, [CallerFilePath] string tag = null)
            => Log.Debug(cleanTag(tag), string.Concat(caller, ": ", msg));
        public static void Debug(Exception ex, string msg = null, [CallerMemberName] string caller = null, [CallerFilePath] string tag = null)
            => Log.Debug(cleanTag(tag), Java.Lang.Throwable.FromException(ex), string.Concat(caller, ": ", msg));

        public static void Info(string msg, [CallerMemberName] string caller = null, [CallerFilePath] string tag = null)
            => Log.Info(cleanTag(tag), string.Concat(caller, ": ", msg));
        public static void Info(Exception ex, string msg = null, [CallerMemberName] string caller = null, [CallerFilePath] string tag = null)
            => Log.Info(cleanTag(tag), Java.Lang.Throwable.FromException(ex), string.Concat(caller, ": ", msg));

        public static void Warn(string msg, [CallerMemberName] string caller = null, [CallerFilePath] string tag = null)
            => Log.Warn(cleanTag(tag), string.Concat(caller, ": ", msg));
        public static void Warn(Exception ex, string msg = null, [CallerMemberName] string caller = null, [CallerFilePath] string tag = null)
            => Log.Warn(cleanTag(tag), Java.Lang.Throwable.FromException(ex), string.Concat(caller, ": ", msg));

        public static void Error(string msg, [CallerMemberName] string caller = null, [CallerFilePath] string tag = null)
            => Log.Error(cleanTag(tag), string.Concat(caller, ": ", msg));
        public static void Error(Exception ex, string msg = null, [CallerMemberName] string caller = null, [CallerFilePath] string tag = null)
            => Log.Error(cleanTag(tag), Java.Lang.Throwable.FromException(ex), string.Concat(caller, ": ", msg));

        public static void Wtf(string msg, [CallerMemberName] string caller = null, [CallerFilePath] string tag = null)
            => Log.Wtf(cleanTag(tag), string.Concat(caller, ": ", msg));
        public static void Wtf(Exception ex, string msg = null, [CallerMemberName] string caller = null, [CallerFilePath] string tag = null)
            => Log.Wtf(cleanTag(tag), Java.Lang.Throwable.FromException(ex), string.Concat(caller, ": ", msg));

        private static string cleanTag(string tag)
        {
            if (!string.IsNullOrEmpty(tag) && tag.LastIndexOf('\\') > 35)
                return tag.Remove(tag.Length - 3).Substring(tag.IndexOf('\\', 30) + 1);
            return tag;
        }
    }
}