using Xamarin.Essentials;

namespace iChronoMe.Core.Abstractions
{
    public interface IDeviceDisplay
    {
        DisplayInfo GetMainDisplayInfo();
    }

    public class DummyDeviceDisplay : IDeviceDisplay
    {
        static DisplayInfo di;

        public DisplayInfo GetMainDisplayInfo()
        {
            if (di == null || di.Width == 0)
                di = new DisplayInfo(1024, 768, 1, DisplayOrientation.Landscape, DisplayRotation.Rotation0);
            return di;
        }
    }
}
