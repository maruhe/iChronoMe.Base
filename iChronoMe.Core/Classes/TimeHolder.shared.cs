using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using iChronoMe.Core.Types;

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

        static Random rnd = new Random(DateTime.Now.Millisecond);

        static TimeHolder()
        {
            State = TimeHolderState.Init;
            NewServer();
            Resync();
        }

        public static string ErrorText { get; private set; }
        public static string NtpServer { get; private set; } = "2.pool.ntp.org";

        static void NewServer()
        {
            NtpServer = ntpServers[rnd.Next(ntpServers.Length - 1)];
        }

        public static TimeHolderState State { get; private set; }

        static DateTime mLastNtp = DateTime.MinValue;
        public static TimeSpan mLastNtpDiff { get; private set; }
        public static DateTime GMTTime { get => DateTime.UtcNow - mLastNtpDiff; }

        public static void Resync()
        {

            /* */
            //   NTP DISABLED !!!
            mLastNtpDiff = TimeSpan.FromTicks(0);
            State = TimeHolderState.Synchron;
            mLastNtp = DateTime.Now;
            return;
            /* */
            State = TimeHolderState.Init;

            iErrorCount = 0;

            new Thread(() =>
            {
                requestStart = DateTime.Now;
                try
                {
                    var ping = new Ping();
                    var res = ping.Send(NtpServer, 1500);
                    if (res.Status != IPStatus.Success)
                        throw new Exception(res.Status.ToString());

                    var offset = GetNetworkTimeOffset();
                    if (offset != null)
                    {
                        if (true || offset.Value.ToPositive() < TimeSpan.FromMinutes(15))
                            mLastNtpDiff = offset.Value;
                        else
                            mLastNtpDiff = TimeSpan.FromTicks(0);
                        mLastNtp = DateTime.Now;
                        State = TimeHolderState.Synchron;
                        iErrorCount = 0;
                    }
                }
                catch (Exception ex)
                {
                    State = TimeHolderState.Error;
                    ErrorText = ex.Message;
                    NewServer();
                    iErrorCount++;
                    if (iErrorCount >= 5)
                    {
                        iErrorCount = 3;
                        return;
                    }
                    Task.Delay(5000);
                    Resync();
                }
            }).Start();
        }

        static DateTime requestStart;
        static int iErrorCount;
        
        //https://stackoverflow.com/questions/1193955/how-to-query-an-ntp-server-using-c
        public static TimeSpan? GetNetworkTimeOffset()
        {
            try
            {
                // NTP message size - 16 bytes of the digest (RFC 2030)
                var ntpData = new byte[48];

                //Setting the Leap Indicator, Version Number and Mode values
                ntpData[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

                var addresses = Dns.GetHostEntry(NtpServer).AddressList;

                //The UDP port number assigned to NTP is 123
                var ipEndPoint = new IPEndPoint(addresses[0], 123);
                //NTP uses UDP

                var swStart = DateTime.Now;
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    socket.Connect(ipEndPoint);

                    //Stops code hang if NTP is blocked
                    socket.ReceiveTimeout = 3000;

                    socket.Send(ntpData);
                    socket.Receive(ntpData);
                    socket.Close();
                }
                TimeSpan tsResponse = DateTime.Now - swStart;
                var tReceivedUtc = DateTime.Now.ToUniversalTime();

                //Offset to get to the "Transmit Timestamp" field (time at which the reply 
                //departed the server for the client, in 64-bit timestamp format."
                const byte serverReplyTime = 40;

                //Get the seconds part
                ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);

                //Get the seconds fraction
                ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

                //Convert From big-endian to little-endian
                intPart = SwapEndianness(intPart);
                fractPart = SwapEndianness(fractPart);

                //this should be the DateTime the Server respondet
                ulong intMs = (intPart * 1000);
                //this should be the half of the response-time
                ulong fractMS = (fractPart * 1000) / 0x100000000L;
                //so thist should be the "absolute" GMT-Time at the Moment of Data is received
                var milliseconds = intMs + fractMS;
                //but somehow it is not, so we use the halt of the request-response-time we measured
                milliseconds = intMs + (uint)(tsResponse.TotalMilliseconds / 2);
                var testDiff = fractMS - (uint)(tsResponse.TotalMilliseconds / 2);
                //as fractMS sometimes is greater than Response-Time here should be something fixed!!
                //even as someone cares about 100ms in a DateTime ~~

                //**UTC** time
                var networkDateTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long)milliseconds);

                return tReceivedUtc - networkDateTime;
            }
            catch (Exception ex)
            {
                State = TimeHolderState.Error;
                ErrorText = ex.Message;
                return null;
            }
        }

        // stackoverflow.com/a/3294698/162671
        static uint SwapEndianness(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) +
                           ((x & 0x0000ff00) << 8) +
                           ((x & 0x00ff0000) >> 8) +
                           ((x & 0xff000000) >> 24));
        }
    }
}