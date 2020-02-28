using System;
using System.Threading.Tasks;

namespace iChronoMe.Core.Classes
{
    public class Delayer : IDisposable
    {
        Action Action;
        DateTime tAllStart;
        DateTime tCurrentDelay;
        DateTime tMaxDelay;
        Task DelayTask = null;
        bool Aborted = false;
        int MaxDelayMs = -1;

        public Delayer(Action action, int initDelayMs, int maxDelayMs = 0)
            : this(action, initDelayMs, maxDelayMs == 0 ? DateTime.MinValue : DateTime.Now.AddMilliseconds(maxDelayMs))
        {
            MaxDelayMs = maxDelayMs;
        }

        public Delayer(Action action, int initDelayMs, DateTime maxDelay)
        {
            Action = action;
            tAllStart = DateTime.Now;
            tMaxDelay = maxDelay;
            MaxDelayMs = (int)(maxDelay - DateTime.Now).TotalMilliseconds;
            SetDelay(initDelayMs);
        }

        public void Dispose()
        {
            Aborted = true;
            Action = null;
            DelayTask = null;
        }

        public void SetDelay(int delayMS, Action action = null)
        {
            if (action != null)
                Action = action;
            SetDelay(DateTime.Now.AddMilliseconds(delayMS));
        }

        public void SetDelay(DateTime delayUntil)
        {
            if (tMaxDelay < DateTime.Now && tCurrentDelay < tMaxDelay)
                tMaxDelay = DateTime.Now.AddMilliseconds(MaxDelayMs);
            tCurrentDelay = delayUntil;
            if (DelayTask == null)
            {
                DelayTask = Task.Factory.StartNew(() =>
                {
                    DateTime tLastCheck = DateTime.Now;
                    try
                    {
                        while (tCurrentDelay > DateTime.Now)
                        {
                            if (Aborted)
                                return;
                            if (tMaxDelay > DateTime.MinValue && tMaxDelay < DateTime.Now)
                                break;
                            Task.Delay(25).Wait();
                        }
                        tLastCheck = DateTime.Now;
                        Action?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        sys.LogException(ex);
                    }
                    finally
                    {
                        DelayTask = null;
                        if (tLastCheck < tCurrentDelay)
                            SetDelay(10);
                    }
                });
            }
        }
    }
}
