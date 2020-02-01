using System;
using System.Threading;
using System.Threading.Tasks;
using Yort.Ntp;

namespace iChronoMe.Core.Classes
{
    public static class TimeHolder
    {
        public enum TimeHolderState
        {
            Init,
            Synchron,
            Error
        };

        private static NtpClient mNtp;

        static TimeHolder()
        {
            State = TimeHolderState.Init;
            mNtp = new NtpClient("3.at.pool.ntp.org");
            mNtp.TimeReceived += Client_TimeReceived;
            mNtp.ErrorOccurred += Client_ErrorOccurred;
            Resync();
        }

        public static TimeHolderState State { get; private set; }

        static DateTime mLastNtp = DateTime.MinValue;
        public static TimeSpan mLastNtpDiff { get; private set; }
        public static DateTime GMTTime { get => DateTime.UtcNow - mLastNtpDiff; }

        public static void Resync()
        {
            State = TimeHolderState.Init;

            iErrorCount = 0;

            Task.Factory.StartNew(() => { mNtp.BeginRequestTime(); });
        }

        static int iErrorCount;
        private static void Client_ErrorOccurred(object sender, NtpNetworkErrorEventArgs e)
        {
            State = TimeHolderState.Error;
            iErrorCount++;
            if (iErrorCount <= 25)
            {
                Task.Factory.StartNew(async () =>
                {
                    await Task.Delay(500);
                    mNtp.BeginRequestTime();
                });
            }
        }

        private static void Client_TimeReceived(object sender, NtpTimeReceivedEventArgs e)
        {
            mLastNtpDiff = DateTime.UtcNow - e.CurrentTime;
            mLastNtp = e.CurrentTime;
            State = TimeHolderState.Synchron;
        }
    }
}