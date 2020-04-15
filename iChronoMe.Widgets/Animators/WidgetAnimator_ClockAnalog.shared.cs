using System;
using System.Threading.Tasks;

namespace iChronoMe.Widgets
{
    public class WidgetAnimator_ClockAnalog
    {
        WidgetView_ClockAnalog vClock;
        Action<double, double, double> aPushFrame = null, aLastRun = null;
        Action aFinally;
        ClockAnalog_AnimationStyle aStyle;
        TimeSpan aDuriation;
        double startH, startM, startS, endH, endM, endS;
        WidgetAnimator_ClockAnalog subAnimator = null;

        public WidgetAnimator_ClockAnalog(WidgetView_ClockAnalog clock, TimeSpan duriation, ClockAnalog_AnimationStyle style)
        {
            vClock = clock;
            aDuriation = duriation;
            aStyle = style;
        }

        public WidgetAnimator_ClockAnalog SetStart(DateTime dt)
        {
            startH = dt.TimeOfDay.TotalHours % 12;
            startM = dt.TimeOfDay.TotalMinutes % 60;
            startS = dt.TimeOfDay.TotalSeconds % 60;
            return this;
        }

        public WidgetAnimator_ClockAnalog SetStart(double h, double m, double s)
        {
            startH = h % 12;
            startM = m % 60;
            startS = s % 60;
            return this;
        }

        public WidgetAnimator_ClockAnalog SetEnd(DateTime dt)
        {
            endH = dt.TimeOfDay.TotalHours % 12;
            endM = dt.TimeOfDay.TotalMinutes % 60;
            endS = dt.TimeOfDay.TotalSeconds % 60;
            return this;
        }

        public WidgetAnimator_ClockAnalog SetEnd(double h, double m, double s)
        {
            endH = h % 12;
            endM = m % 60;
            endS = s % 60;
            return this;
        }

        public WidgetAnimator_ClockAnalog SetPushFrame(Action<double, double, double> action)
        {
            aPushFrame = action;
            return this;
        }

        public WidgetAnimator_ClockAnalog SetLastRun(Action<double, double, double> action)
        {
            aLastRun = action;
            return this;
        }

        public WidgetAnimator_ClockAnalog SetFinally(Action action)
        {
            aFinally = action;
            return this;
        }

        bool bRunning = false;
        public WidgetAnimator_ClockAnalog StartAnimation()
        {
            bRunning = true;
            var tStart = DateTime.Now;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (!vClock.FlowSeconds)
                    {
                        startS = Math.Truncate(startS);
                        endS = Math.Truncate(endS);
                    }
                    if (!vClock.FlowMinutes)
                    {
                        startM = Math.Truncate(startM);
                        endM = Math.Truncate(endM);
                    }
                    if (!vClock.FlowHours)
                    {
                        startH = Math.Truncate(startH);
                        endH = Math.Truncate(endH);
                    }

                    if (aStyle == ClockAnalog_AnimationStyle.HandsNatural)
                    {
                        while (tStart.Add(aDuriation) > DateTime.Now)
                        {
                            double nElapsed = (DateTime.Now - tStart).TotalMilliseconds;
                            //xLog.Verbose(string.Format("{0}ms of {1}ms elapsed => {2}%", nElapsed, aDuriation.TotalMilliseconds, 100 * nElapsed / aDuriation.TotalMilliseconds));
                            var h = (startH + (endH - startH) / aDuriation.TotalMilliseconds * (DateTime.Now - tStart).TotalMilliseconds);
                            var m = (startM + (endM - startM) / aDuriation.TotalMilliseconds * (DateTime.Now - tStart).TotalMilliseconds);
                            var s = (startS + (endS - startS) / aDuriation.TotalMilliseconds * (DateTime.Now - tStart).TotalMilliseconds);
                            //xLog.Verbose(string.Format("Hour min {0:F} max {1:F} => {2:F}", startH, endH, h));
                            //xLog.Verbose(string.Format("Minute min {0:F} max {1:F} => {2:F}", startM, endM, m));
                            //xLog.Verbose(string.Format("Second min {0:F} max {1:F} => {2:F}", startS, endS, s)); 
                            if (!bRunning)
                                return;
                            aPushFrame?.Invoke(h, m, s);
                            Task.Delay(1000 / 60).Wait();
                        }
                    }
                    else if (aStyle == ClockAnalog_AnimationStyle.HandsDirect)
                    {
                        double wayH = endH - startH;
                        if (endH > startH)
                        {
                            if (wayH > 6)
                                wayH = wayH - 12;
                        }
                        else
                        {
                            if (wayH < -6)
                                wayH = wayH + 12;
                        }

                        double wayM = endM - startM;
                        if (endM > startM)
                        {
                            if (wayM > 30)
                                wayM = wayM - 60;
                        }
                        else
                        {
                            if (wayM < -30)
                                wayM = wayM + 60;
                        }

                        double wayS = endS - startS;
                        if (endS > startS)
                        {
                            if (wayS > 30)
                                wayS = wayS - 60;
                        }
                        else
                        {
                            if (wayS < -30)
                                wayS = wayS + 60;
                        }

                        while (tStart.Add(aDuriation) > DateTime.Now)
                        {
                            double nElapsed = (DateTime.Now - tStart).TotalMilliseconds;
                            //xLog.Verbose(string.Format("{0}ms of {1}ms elapsed => {2}%", nElapsed, aDuriation.TotalMilliseconds, 100 * nElapsed / aDuriation.TotalMilliseconds));
                            var h = (startH + wayH / aDuriation.TotalMilliseconds * (DateTime.Now - tStart).TotalMilliseconds);
                            var m = (startM + wayM / aDuriation.TotalMilliseconds * (DateTime.Now - tStart).TotalMilliseconds);
                            var s = (startS + wayS / aDuriation.TotalMilliseconds * (DateTime.Now - tStart).TotalMilliseconds);
                            //xLog.Verbose(string.Format("Hour min {0:F} max {1:F} => {2:F}", startH, endH, h));
                            //xLog.Verbose(string.Format("Minute min {0:F} max {1:F} => {2:F}", startM, endM, m));
                            //xLog.Verbose(string.Format("Second min {0:F} max {1:F} => {2:F}", startS, endS, s)); 
                            if (!bRunning)
                                return;
                            aPushFrame?.Invoke(h, m, s);
                            Task.Delay(1000 / 60).Wait();
                        }
                    }
                    else if (aStyle == ClockAnalog_AnimationStyle.OnceAround)
                    {
                        double wayH = endH - startH;
                        if (endH > startH)
                        {
                            if (wayH < 6)
                                wayH = wayH - 12;
                        }
                        else
                        {
                            if (wayH > -6)
                                wayH = wayH + 12;
                        }

                        double wayM = endM - startM;
                        if (endM > startM)
                        {
                            if (wayM < 30)
                                wayM = wayM - 60;
                        }
                        else
                        {
                            if (wayM > -30)
                                wayM = wayM + 60;
                        }

                        double wayS = endS - startS;
                        if (endS > startS)
                        {
                            if (wayS < 30)
                                wayS = wayS - 60;
                        }
                        else
                        {
                            if (wayS > -30)
                                wayS = wayS + 60;
                        }

                        while (tStart.Add(aDuriation) > DateTime.Now)
                        {
                            double nElapsed = (DateTime.Now - tStart).TotalMilliseconds;
                            //xLog.Verbose(string.Format("{0}ms of {1}ms elapsed => {2}%", nElapsed, aDuriation.TotalMilliseconds, 100 * nElapsed / aDuriation.TotalMilliseconds));
                            var h = (startH + wayH / aDuriation.TotalMilliseconds * (DateTime.Now - tStart).TotalMilliseconds);
                            var m = (startM + wayM / aDuriation.TotalMilliseconds * (DateTime.Now - tStart).TotalMilliseconds);
                            var s = (startS + wayS / aDuriation.TotalMilliseconds * (DateTime.Now - tStart).TotalMilliseconds);
                            //xLog.Verbose(string.Format("Hour min {0:F} max {1:F} => {2:F}", startH, endH, h));
                            //xLog.Verbose(string.Format("Minute min {0:F} max {1:F} => {2:F}", startM, endM, m));
                            //xLog.Verbose(string.Format("Second min {0:F} max {1:F} => {2:F}", startS, endS, s)); 
                            if (!bRunning)
                                return;
                            aPushFrame?.Invoke(h, m, s);
                            Task.Delay(1000 / 60).Wait();
                        }
                    }
                    else if (aStyle == ClockAnalog_AnimationStyle.Over0)
                    {
                        bRunning = false;
                        subAnimator = new WidgetAnimator_ClockAnalog(vClock, TimeSpan.FromTicks(aDuriation.Ticks / 2), ClockAnalog_AnimationStyle.HandsNatural)
                        .SetStart(startH, startM, startS)
                        .SetEnd(0, 0, 0)
                        .SetPushFrame((h, m, s) =>
                        {
                            aPushFrame?.Invoke(h, m, s);
                        })
                        .SetFinally(() =>
                        {
                            subAnimator = new WidgetAnimator_ClockAnalog(vClock, TimeSpan.FromTicks(aDuriation.Ticks / 2), ClockAnalog_AnimationStyle.HandsNatural)
                            .SetStart(0, 0, 0)
                            .SetEnd(endH, endM, endS)
                            .SetPushFrame((h, m, s) =>
                            {
                                aPushFrame?.Invoke(h, m, s);
                            })
                            .SetLastRun((h, m, s) =>
                            {
                                aLastRun?.Invoke(h, m, s);
                            })
                            .SetFinally(() =>
                            {
                                aFinally.Invoke();
                            }).StartAnimation();

                        }).StartAnimation();
                    }
                    else if (aStyle == ClockAnalog_AnimationStyle.Over12)
                    {
                        bRunning = false;
                        subAnimator = new WidgetAnimator_ClockAnalog(vClock, TimeSpan.FromTicks(aDuriation.Ticks / 2), ClockAnalog_AnimationStyle.HandsDirect)
                        .SetStart(startH, startM, startS)
                        .SetEnd(0, 0, 0)
                        .SetPushFrame((h, m, s) =>
                        {
                            aPushFrame?.Invoke(h, m, s);
                        })
                        .SetFinally(() =>
                        {
                            subAnimator = new WidgetAnimator_ClockAnalog(vClock, TimeSpan.FromTicks(aDuriation.Ticks / 2), ClockAnalog_AnimationStyle.HandsDirect)
                            .SetStart(0, 0, 0)
                            .SetEnd(endH, endM, endS)
                            .SetPushFrame((h, m, s) =>
                            {
                                aPushFrame?.Invoke(h, m, s);
                            })
                            .SetLastRun((h, m, s) =>
                            {
                                aLastRun?.Invoke(h, m, s);
                            })
                            .SetFinally(() =>
                            {
                                aFinally.Invoke();
                            }).StartAnimation();

                        }).StartAnimation();
                    }

                    if (bRunning)
                        aLastRun?.Invoke(endH, endM, endS);
                }
                catch (Exception ex)
                {
                    xLog.Error(ex);
                }
                finally
                {
                    if (bRunning)
                        aFinally?.Invoke();
                }
            });
            return this;
        }

        public void AbortAnimation()
        {
            subAnimator?.AbortAnimation();
            bRunning = false;
        }

    }

    public enum ClockAnalog_AnimationStyle
    {
        HandsNatural = 0,
        HandsDirect = 1,
        OnceAround = 8,
        Over0 = 10,
        Over12 = 12
    }
}
