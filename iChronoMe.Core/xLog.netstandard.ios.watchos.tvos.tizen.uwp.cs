using System;

namespace iChronoMe.Core
{
    public static class xLog
    {
        private static void print(string msg)
        { }
        private static void print(string tag, string msg)
        { }
        private static void print(string tag, Exception ex, string msg)
        { }
        private static void print(string tag, Exception ex, string format, params object[] args)
        { }
        private static void print(string tag, string format, params object[] args)
        { }

        public static void Verbose(string msg)
           => print(msg);
        public static void Verbose(string tag, string msg)
           => print(tag, msg);
        public static void Verbose(string tag, Exception ex, string msg)
            => print(tag, ex, msg);
        public static void Verbose(string tag, Exception ex, string format, params object[] args)
            => print(tag, ex, format, args);
        public static void Verbose(string tag, string format, params object[] args)
            => print(tag, format, args);

        public static void Debug(string msg)
            => print(msg);
        public static void Debug(string tag, string msg)
            => print(tag, msg);
        public static void Debug(string tag, Exception ex, string msg)
            => print(tag, ex, msg);
        public static void Debug(string tag, Exception ex, string format, params object[] args)
            => print(tag, ex, format, args);
        public static void Debug(string tag, string format, params object[] args)
            => print(tag, format, args);

        public static void Info(string msg)
            => print(msg);
        public static void Info(string tag, string msg)
            => print(tag, msg);
        public static void Info(string tag, Exception ex, string msg)
            => print(tag, ex, msg);
        public static void Info(string tag, Exception ex, string format, params object[] args)
            => print(tag, ex, format, args);
        public static void Info(string tag, string format, params object[] args)
            => print(tag, format, args);

        public static void Warn(string msg)
            => print(msg);
        public static void Warn(string tag, string msg)
            => print(tag, msg);
        public static void Warn(string tag, Exception ex, string msg)
            => print(tag, ex, msg);
        public static void Warn(string tag, Exception ex, string format, params object[] args)
            => print(tag, ex, format, args);
        public static void Warn(string tag, string format, params object[] args)
            => print(tag, format, args);

        public static void Error(string msg)
            => print(msg);
        public static void Error(string tag, string msg)
            => print(tag, msg);
        public static void Error(string tag, Exception ex, string msg)
            => print(tag, ex, msg);
        public static void Error(string tag, Exception ex, string format, params object[] args)
            => print(tag, ex, format, args);
        public static void Error(string tag, string format, params object[] args)
            => print(tag, format, args);

        public static void Wtf(string msg)
            => print(msg);
        public static void Wtf(string tag, string msg)
            => print(tag, msg);
        public static void Wtf(string tag, Exception ex, string msg)
            => print(tag, ex, msg);
        public static void Wtf(string tag, Exception ex, string format, params object[] args)
            => print(tag, ex, format, args);
        public static void Wtf(string tag, string format, params object[] args)
            => print(tag, format, args);
    }
}