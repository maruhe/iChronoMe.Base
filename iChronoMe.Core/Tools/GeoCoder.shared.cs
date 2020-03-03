using System;
using System.Collections.Generic;
using System.Text;
using static iChronoMe.Core.Classes.GeoInfo;

namespace iChronoMe.Core.Tools
{
    public static class GeoCoder
    {
        private static IGeoCoder _instance;
        public static IGeoCoder Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GeoCoder_PhotonKomoot();
                return _instance;
            }
        }

        public static AreaInfo GetAreaInfo(double lat, double lng)
        {
            return Instance.GetAreaInfo(lat, lng);
        }

        public static AreaInfo GetPositionByName(string location)
        {
            return Instance.GetPositionByName(location);
        }
    }

    public interface IGeoCoder
    {
        AreaInfo GetAreaInfo(double lat, double lng);

        AreaInfo GetPositionByName(string location);

    }
}
