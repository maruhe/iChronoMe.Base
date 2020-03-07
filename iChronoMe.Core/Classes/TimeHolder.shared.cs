using System;
using System.Threading.Tasks;

using Yort.Ntp;

namespace iChronoMe.Core.Classes
{
    public static class TimeHolder
    {
        static string[] ntpServers = { "0.pool.ntp.org",
                                       "1.pool.ntp.org",
                                       "2.pool.ntp.org",
                                       "3.pool.ntp.org"};

        public enum TimeHolderState
        {
            Init,
            Synchron,
            Error
        };

        private static NtpClient mNtp;

        static Random rnd = new Random(DateTime.Now.Millisecond);

        static TimeHolder()
        {
            State = TimeHolderState.Init;
            NewServer();
            Resync();
        }

        static void NewServer()
        {
            if (mNtp != null)
            {
                mNtp.TimeReceived -= Client_TimeReceived;
                mNtp.ErrorOccurred -= Client_ErrorOccurred;
            }
            mNtp = new NtpClient(ntpServers[rnd.Next(ntpServers.Length - 1)]);
            mNtp.TimeReceived += Client_TimeReceived;
            mNtp.ErrorOccurred += Client_ErrorOccurred;
        }

        public static TimeHolderState State { get; private set; }

        static DateTime mLastNtp = DateTime.MinValue;
        public static TimeSpan mLastNtpDiff { get; private set; }
        public static DateTime GMTTime { get => DateTime.UtcNow - mLastNtpDiff; }

        public static void Resync()
        {
            //   NTP DISABLED !!!
            mLastNtpDiff = TimeSpan.FromTicks(0);
            State = TimeHolderState.Synchron;
            mLastNtp = DateTime.Now;
            return;

            State = TimeHolderState.Init;

            iErrorCount = 0;

            Task.Factory.StartNew(() =>
            {
                requestStart = DateTime.Now;
                mNtp.BeginRequestTime();
            });
        }

        static DateTime requestStart;
        static int iErrorCount;
        private static void Client_ErrorOccurred(object sender, NtpNetworkErrorEventArgs e)
        {
            State = TimeHolderState.Error;
            iErrorCount++;
            if (iErrorCount <= 5)
            {
                Task.Factory.StartNew(async () =>
                {
                    await Task.Delay(500);
                    NewServer();
                    requestStart = DateTime.Now;
                    mNtp.BeginRequestTime();
                });
            }
        }

        private static void Client_TimeReceived(object sender, NtpTimeReceivedEventArgs e)
        {
            var tsResponse = DateTime.Now - requestStart;
            var tsDeviceTimeOffset = e.ReceivedAt - e.CurrentTime - tsResponse;

            if (tsDeviceTimeOffset.TotalMinutes > -15 && tsDeviceTimeOffset.TotalMinutes < 15)
            {
                mLastNtpDiff = tsDeviceTimeOffset;
                State = TimeHolderState.Synchron;
            }
            else
            {
                mLastNtpDiff = TimeSpan.FromTicks(0);
                State = TimeHolderState.Error;
            }
            mLastNtp = e.ReceivedAt;
        }
    }
}