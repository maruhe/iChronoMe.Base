# iChronoMe.Base
[![GitHub License](https://img.shields.io/badge/license-IDM-lightgrey.svg)](https://github.com/maruhe/iChronoMe.Base/blob/master/LICENSE.md)
[![contributions welcome](https://img.shields.io/badge/contributions-welcome-brightgreen.svg?style=flat)](https://github.com/maruhe/iChronoMe.Base/issues)

iChrono.me cause time is dynamic

currently working features:

supplying real and middle solar time:

    var lth = await iChronoMe.Core.LocationTimeHolder.NewInstanceAsync(41.7448, 12.6703);
    lth.RealSunTime.ToString();
    lth.MidSunTime.ToString();

for any specific location or moment:

    await lth.ChangePositionAsync(64.8938, 45.7553);
    lth.GetTime(TimeType.RealSunTime, new DateTime(2020, 01, 01, 12, 0, 0));

and even continuous:

    var lth = iChronoMe.Core.LocationTimeHolder.NewInstanceDelay(41.7448, 12.6703);
    lth.AreaChanged += Lth_AreaChanged;
    lth.StartTimeChangedHandler(this, TimeType.RealSunTime, (s, e) => {
        RunOnUiThread(() => lTime.Text = lth.RealSunTime.ToShortTimeString();
    });

    private void Lth_AreaChanged(object sender, AreaChangedEventArgs e)
    {
        RunOnUiThread(() => lTitle.Text = lth.AreaName + ", " + lth.CountryName);
    }

..or as long as nessesary:

    lth.StopTimeChangedHandler(this);

the project is full of functions about dynamic calendar-models, solar and lunar information and a hell a lot function you may just ignore in meanwhile (or contact me)
