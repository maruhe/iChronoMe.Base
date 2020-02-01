# iChronoMe.Base
iChrono.me cause time is dynamic

currently working features:

supplying middle and real solar time:

    var lth = new iChronoMe.Core.LocationTimeHolder(41.7448, 12.6703, false);    
    lth.RealSunTime.ToString();    
    lth.MidSunTime.ToString();    

for any specific location or moment:

    lth.ChangePosition(64.8938, 45.7553);
    lth.GetTime(TimeType.RealSunTime, new DateTime(2020, 01, 01, 12, 0, 0));

and even continuous:

    lth.TimeChanged += Lth_TimeChanged;
    lth.AreaChanged += Lth_AreaChanged;
    lth.Start(true, false, false);
    private void Lth_TimeChanged()
    {
        RunOnUiThread(() => lTime.Text = lth.RealSunTime);
    }
    private void Lth_AreaChanged()
    {
        RunOnUiThread(() => lTitle.Text = lth.AreaName + ", " + lth.CountryName);
    }

..or as long as nessesary:

    lth.Stop();

the project is full of functions about dynamic calendar-models, solar and lunar information and a hell a lot function you may just ignore in meanwhile (or contact me)
