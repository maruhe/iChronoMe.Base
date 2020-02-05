using System;
using System.Runtime.CompilerServices;

namespace iChronoMe
{
    public static class xLog
    {
        private static void print(string type, string tag, string msg)
        {
            Console.WriteLine(string.Concat(type, ": ", tag, ": ", msg));
        }
        private static void print(string type, string tag, Exception ex, string msg)
        {
            Console.WriteLine(string.Concat(type, ": ", tag, ": ", msg));
            if (ex != null)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        public static void Verbose(string msg, [CallerMemberName] string caller = null, [CallerFilePath] string tag = null)
            => print("Verbose", cleanTag(tag), string.Concat(caller, ": ", msg));
        public static void Verbose(Exception ex, string msg = null, [CallerMemberName] string caller = null, [CallerFilePath] string tag = null)
            => print("Verbose", cleanTag(tag), ex, string.Concat(caller, ": ", msg));

        public static void Debug(string msg, [CallerMemberName] string caller = null, [CallerFilePath] string tag = null)
            => print("Debug", cleanTag(tag), string.Concat(caller, ": ", msg));
        public static void Debug(Exception ex, string msg = null, [CallerMemberName] string caller = null, [CallerFilePath] string tag = null)
            => print("Debug", cleanTag(tag), ex, string.Concat(caller, ": ", msg));

        public static void Info(string msg, [CallerMemberName] string caller = null, [CallerFilePath] string tag = null)
            => print("Info", cleanTag(tag), string.Concat(caller, ": ", msg));
        public static void Info(Exception ex, string msg = null, [CallerMemberName] string caller = null, [CallerFilePath] string tag = null)
            => print("Info", cleanTag(tag), ex, string.Concat(caller, ": ", msg));

        public static void Warn(string msg, [CallerMemberName] string caller = null, [CallerFilePath] string tag = null)
            => print("Warn", cleanTag(tag), string.Concat(caller, ": ", msg));
        public static void Warn(Exception ex, string msg = null, [CallerMemberName] string caller = null, [CallerFilePath] string tag = null)
            => print("Warn", cleanTag(tag), ex, string.Concat(caller, ": ", msg));

        public static void Error(string msg, [CallerMemberName] string caller = null, [CallerFilePath] string tag = null)
            => print("Error", cleanTag(tag), string.Concat(caller, ": ", msg));
        public static void Error(Exception ex, string msg = null, [CallerMemberName] string caller = null, [CallerFilePath] string tag = null)
            => print("Error", cleanTag(tag), ex, string.Concat(caller, ": ", msg));

        public static void Wtf(string msg, [CallerMemberName] string caller = null, [CallerFilePath] string tag = null)
            => print("Wtf", cleanTag(tag), string.Concat(caller, ": ", msg));
        public static void Wtf(Exception ex, string msg = null, [CallerMemberName] string caller = null, [CallerFilePath] string tag = null)
            => print("Wtf", cleanTag(tag), ex, string.Concat(caller, ": ", msg));

        private static string cleanTag(string tag)
        {
            if (!string.IsNullOrEmpty(tag) && tag.LastIndexOf('\\') > 35)
                return tag.Remove(tag.Length - 3).Substring(tag.IndexOf('\\', 30) + 1);
            return tag;
        }
    }
}