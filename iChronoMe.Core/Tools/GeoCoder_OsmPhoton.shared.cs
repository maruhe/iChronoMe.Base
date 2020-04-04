using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net;

using GeoJSON.Net.Geometry;

using iChronoMe.Core.Classes;

using Newtonsoft.Json;

using static iChronoMe.Core.Classes.GeoInfo;

namespace iChronoMe.Core.Tools
{
    public class GeoCoder_OsmPhoton : IGeoCoder
    {
        public static int urlID { get; private set; } = 0;
        public static string photonUrl { get; private set; } = Secrets.osmPhotonUrls[urlID];

        GeoCoder_Google _google = null;
        GeoCoder_Google Google
        {
            get
            {
                if (_google == null)
                    _google = new GeoCoder_Google();
                return _google;
            }
        }

        public AreaInfo GetAreaInfo(double lat, double lng)
        {
            string cUri = photonUrl+"reverse?lat=" + lat.ToString("0.######", CultureInfo.InvariantCulture) + "&lon=" + lng.ToString("0.######", CultureInfo.InvariantCulture);
            string cGeoInfo = sys.GetUrlContent(cUri).Result;

            if (CheckServerResponse(cGeoInfo) > 0)
            {
                cUri = photonUrl + "reverse?lat=" + lat.ToString("0.######", CultureInfo.InvariantCulture) + "&lon=" + lng.ToString("0.######", CultureInfo.InvariantCulture);
                cGeoInfo = sys.GetUrlContent(cUri).Result;
            }

            var res = AreInfoFromGeoJson(cGeoInfo);
            if (res == null)
                return Google.GetAreaInfo(lat, lng);
            return res;
        }

        public AreaInfo GetPositionByName(string location)
        {
            string cUri = photonUrl+"api?q=" + WebUtility.UrlEncode(location) + "&limit=1&lat=" + sys.lastUserLocation.Latitude.ToString("0.######", CultureInfo.InvariantCulture) + "&lon=" + sys.lastUserLocation.Longitude.ToString("0.######", CultureInfo.InvariantCulture);
            string cGeoInfo = sys.GetUrlContent(cUri).Result;

            if (CheckServerResponse(cGeoInfo) > 0)
            {
                cUri = photonUrl + "api?q=" + WebUtility.UrlEncode(location) + "&limit=1&lat=" + sys.lastUserLocation.Latitude.ToString("0.######", CultureInfo.InvariantCulture) + "&lon=" + sys.lastUserLocation.Longitude.ToString("0.######", CultureInfo.InvariantCulture);
                cGeoInfo = sys.GetUrlContent(cUri).Result;
            }

            var res = AreInfoFromGeoJson(cGeoInfo);
            if (res == null)
                return Google.GetPositionByName(location);
            return res;
        }

        private static int CheckServerResponse(string response)
        {
            if (!string.IsNullOrEmpty(response))
                return 0;
            if (Secrets.osmPhotonUrls.Length <= 1)
                return 0;
            urlID++;
            if (urlID >= Secrets.osmPhotonUrls.Length)
                urlID = 0;
            photonUrl = Secrets.osmPhotonUrls[urlID];
            return 1;
        }

        public static AreaInfo AreInfoFromGeoJson(string geoJson)
        {
            try
            {
                if (string.IsNullOrEmpty(geoJson))
                    return null; 

                var fc = JsonConvert.DeserializeObject<GeoJSON.Net.Feature.FeatureCollection>(geoJson);
                var ai = new AreaInfo();
                try {
                    ai.dataSource = "osm"; ai.sourceServer = new Uri(photonUrl).Host.Split('.')[1]; } catch { }

                if (fc != null && fc.Features.Count > 0)
                {
                    var fe = fc.Features[0];
                    if (fe.Geometry is Point)
                    {
                        var p = fe.Geometry as Point;
                        ai.pointLat = p.Coordinates.Latitude;
                        ai.pointLng = p.Coordinates.Longitude;
                    }
                    if (fe.Properties.ContainsKey("name"))
                        ai.locality = fe.Properties["name"].ToString();
                    if (fe.Properties.ContainsKey("street"))
                        ai.route = fe.Properties["street"].ToString();
                    if (fe.Properties.ContainsKey("postcode"))
                        ai.postalCode = fe.Properties["postcode"].ToString();
                    if (fe.Properties.ContainsKey("city"))
                        ai.adminArea3 = fe.Properties["city"].ToString();
                    if (fe.Properties.ContainsKey("state"))
                        ai.adminArea1 = fe.Properties["state"].ToString();
                    if (fe.Properties.ContainsKey("country"))
                        ai.countryName = fe.Properties["country"].ToString();

                    if (Countries.ContainsKey(ai.countryName))
                        ai.countryCode = Countries[ai.countryName];
                    else
                        ai.ToString();

                    string cArea = string.IsNullOrEmpty(ai.adminArea3) ? string.IsNullOrEmpty(ai.adminArea2) ? (string.IsNullOrEmpty(ai.adminArea1) ? ai.countryName : ai.adminArea1) : ai.adminArea2 : ai.adminArea3;

                    if (!string.IsNullOrEmpty(ai.locality))
                        ai.ToString();
                    if (!string.IsNullOrEmpty(ai.route))
                        ai.ToString();
                    ai.toponymName = string.IsNullOrEmpty(ai.locality) ? (string.IsNullOrEmpty(ai.route) ? cArea : ai.route) : ai.locality;
                    if (string.IsNullOrEmpty(ai.locality) || !string.IsNullOrEmpty(string.Concat(ai.route, ai.postalCode, ai.adminArea3)))
                        ai.toponymName = cArea;

                    if (fe.Properties.ContainsKey("extent"))
                    {
                        try
                        {
                            var ex = fe.Properties["extent"] as ICollection;
                            var e = ex.GetEnumerator();
                            if (e.MoveNext())
                            {
                                ai.boxWest = sys.parseDouble(e.Current.ToString().Replace(",", "."));
                                if (e.MoveNext())
                                {
                                    ai.boxNorth = sys.parseDouble(e.Current.ToString().Replace(",", "."));
                                    if (e.MoveNext())
                                    {
                                        ai.boxEast = sys.parseDouble(e.Current.ToString().Replace(",", "."));
                                        if (e.MoveNext())
                                            ai.boxSouth = sys.parseDouble(e.Current.ToString().Replace(",", "."));
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            xLog.Error(ex);
                        }
                    }
                    ai.ToString();

                    return ai;
                }
                else
                    geoJson.ToString();
            }
            catch (Exception ex)
            {
                xLog.Error(ex);
            }
            return null;
        }

        static Dictionary<string, string> _countries = null;
        static Dictionary<string, string> Countries
        {
            get
            {
                if (_countries == null)
                {
                    var tmp = new Dictionary<string, string>();
                    tmp.Add("Afghanistan", "AF");
                    tmp.Add("Albania", "AL");
                    tmp.Add("Algeria", "DZ");
                    tmp.Add("American Samoa", "AS");
                    tmp.Add("Andorra", "AD");
                    tmp.Add("Angola", "AO");
                    tmp.Add("Anguilla", "AI");
                    tmp.Add("Antarctica", "AQ");
                    tmp.Add("Antigua and Barbuda", "AG");
                    tmp.Add("Argentina", "AR");
                    tmp.Add("Armenia", "AM");
                    tmp.Add("Aruba", "AW");
                    tmp.Add("Australia", "AU");
                    tmp.Add("Austria", "AT");
                    tmp.Add("Azerbaijan", "AZ");
                    tmp.Add("Bahamas", "BS");
                    tmp.Add("Bahrain", "BH");
                    tmp.Add("Bangladesh", "BD");
                    tmp.Add("Barbados", "BB");
                    tmp.Add("Belarus", "BY");
                    tmp.Add("Belgium", "BE");
                    tmp.Add("Belize", "BZ");
                    tmp.Add("Benin", "BJ");
                    tmp.Add("Bermuda", "BM");
                    tmp.Add("Bhutan", "BT");
                    tmp.Add("Bolivia", "BO");
                    tmp.Add("Bosnia and Herzegovina", "BA");
                    tmp.Add("Botswana", "BW");
                    tmp.Add("Brazil", "BR");
                    tmp.Add("British Indian Ocean Territory", "IO");
                    tmp.Add("British Virgin Islands", "VG");
                    tmp.Add("Brunei", "BN");
                    tmp.Add("Bulgaria", "BG");
                    tmp.Add("Burkina Faso", "BF");
                    tmp.Add("Burundi", "BI");
                    tmp.Add("Cambodia", "KH");
                    tmp.Add("Cameroon", "CM");
                    tmp.Add("Canada", "CA");
                    tmp.Add("Cape Verde", "CV");
                    tmp.Add("Cayman Islands", "KY");
                    tmp.Add("Central African Republic", "CF");
                    tmp.Add("Chad", "TD");
                    tmp.Add("Chile", "CL");
                    tmp.Add("China", "CN");
                    tmp.Add("Christmas Island", "CX");
                    tmp.Add("Cocos Islands", "CC");
                    tmp.Add("Colombia", "CO");
                    tmp.Add("Comoros", "KM");
                    tmp.Add("Cook Islands", "CK");
                    tmp.Add("Costa Rica", "CR");
                    tmp.Add("Croatia", "HR");
                    tmp.Add("Cuba", "CU");
                    tmp.Add("Curacao", "CW");
                    tmp.Add("Cyprus", "CY");
                    tmp.Add("Czech Republic", "CZ");
                    tmp.Add("Democratic Republic of the Congo", "CD");
                    tmp.Add("Denmark", "DK");
                    tmp.Add("Djibouti", "DJ");
                    tmp.Add("Dominica", "DM");
                    tmp.Add("Dominican Republic", "DO");
                    tmp.Add("East Timor", "TL");
                    tmp.Add("Ecuador", "EC");
                    tmp.Add("Egypt", "EG");
                    tmp.Add("El Salvador", "SV");
                    tmp.Add("Equatorial Guinea", "GQ");
                    tmp.Add("Eritrea", "ER");
                    tmp.Add("Estonia", "EE");
                    tmp.Add("Ethiopia", "ET");
                    tmp.Add("Falkland Islands", "FK");
                    tmp.Add("Faroe Islands", "FO");
                    tmp.Add("Fiji", "FJ");
                    tmp.Add("Finland", "FI");
                    tmp.Add("France", "FR");
                    tmp.Add("French Polynesia", "PF");
                    tmp.Add("Gabon", "GA");
                    tmp.Add("Gambia", "GM");
                    tmp.Add("Georgia", "GE");
                    tmp.Add("Germany", "DE");
                    tmp.Add("Ghana", "GH");
                    tmp.Add("Gibraltar", "GI");
                    tmp.Add("Greece", "GR");
                    tmp.Add("Greenland", "GL");
                    tmp.Add("Grenada", "GD");
                    tmp.Add("Guam", "GU");
                    tmp.Add("Guatemala", "GT");
                    tmp.Add("Guernsey", "GG");
                    tmp.Add("Guinea", "GN");
                    tmp.Add("Guinea-Bissau", "GW");
                    tmp.Add("Guyana", "GY");
                    tmp.Add("Haiti", "HT");
                    tmp.Add("Honduras", "HN");
                    tmp.Add("Hong Kong", "HK");
                    tmp.Add("Hungary", "HU");
                    tmp.Add("Iceland", "IS");
                    tmp.Add("India", "IN");
                    tmp.Add("Indonesia", "ID");
                    tmp.Add("Iran", "IR");
                    tmp.Add("Iraq", "IQ");
                    tmp.Add("Ireland", "IE");
                    tmp.Add("Isle of Man", "IM");
                    tmp.Add("Israel", "IL");
                    tmp.Add("Italy", "IT");
                    tmp.Add("Ivory Coast", "CI");
                    tmp.Add("Jamaica", "JM");
                    tmp.Add("Japan", "JP");
                    tmp.Add("Jersey", "JE");
                    tmp.Add("Jordan", "JO");
                    tmp.Add("Kazakhstan", "KZ");
                    tmp.Add("Kenya", "KE");
                    tmp.Add("Kiribati", "KI");
                    tmp.Add("Kosovo", "XK");
                    tmp.Add("Kuwait", "KW");
                    tmp.Add("Kyrgyzstan", "KG");
                    tmp.Add("Laos", "LA");
                    tmp.Add("Latvia", "LV");
                    tmp.Add("Lebanon", "LB");
                    tmp.Add("Lesotho", "LS");
                    tmp.Add("Liberia", "LR");
                    tmp.Add("Libya", "LY");
                    tmp.Add("Liechtenstein", "LI");
                    tmp.Add("Lithuania", "LT");
                    tmp.Add("Luxembourg", "LU");
                    tmp.Add("Macau", "MO");
                    tmp.Add("Macedonia", "MK");
                    tmp.Add("Madagascar", "MG");
                    tmp.Add("Malawi", "MW");
                    tmp.Add("Malaysia", "MY");
                    tmp.Add("Maldives", "MV");
                    tmp.Add("Mali", "ML");
                    tmp.Add("Malta", "MT");
                    tmp.Add("Marshall Islands", "MH");
                    tmp.Add("Mauritania", "MR");
                    tmp.Add("Mauritius", "MU");
                    tmp.Add("Mayotte", "YT");
                    tmp.Add("Mexico", "MX");
                    tmp.Add("Micronesia", "FM");
                    tmp.Add("Moldova", "MD");
                    tmp.Add("Monaco", "MC");
                    tmp.Add("Mongolia", "MN");
                    tmp.Add("Montenegro", "ME");
                    tmp.Add("Montserrat", "MS");
                    tmp.Add("Morocco", "MA");
                    tmp.Add("Mozambique", "MZ");
                    tmp.Add("Myanmar", "MM");
                    tmp.Add("Namibia", "NA");
                    tmp.Add("Nauru", "NR");
                    tmp.Add("Nepal", "NP");
                    tmp.Add("Netherlands", "NL");
                    tmp.Add("Netherlands Antilles", "AN");
                    tmp.Add("New Caledonia", "NC");
                    tmp.Add("New Zealand", "NZ");
                    tmp.Add("Nicaragua", "NI");
                    tmp.Add("Niger", "NE");
                    tmp.Add("Nigeria", "NG");
                    tmp.Add("Niue", "NU");
                    tmp.Add("North Korea", "KP");
                    tmp.Add("Northern Mariana Islands", "MP");
                    tmp.Add("Norway", "NO");
                    tmp.Add("Oman", "OM");
                    tmp.Add("Pakistan", "PK");
                    tmp.Add("Palau", "PW");
                    tmp.Add("Palestine", "PS");
                    tmp.Add("Panama", "PA");
                    tmp.Add("Papua New Guinea", "PG");
                    tmp.Add("Paraguay", "PY");
                    tmp.Add("Peru", "PE");
                    tmp.Add("Philippines", "PH");
                    tmp.Add("Pitcairn", "PN");
                    tmp.Add("Poland", "PL");
                    tmp.Add("Portugal", "PT");
                    tmp.Add("Puerto Rico", "PR");
                    tmp.Add("Qatar", "QA");
                    tmp.Add("Republic of the Congo", "CG");
                    tmp.Add("Reunion", "RE");
                    tmp.Add("Romania", "RO");
                    tmp.Add("Russia", "RU");
                    tmp.Add("Rwanda", "RW");
                    tmp.Add("Saint Barthelemy", "BL");
                    tmp.Add("Saint Helena", "SH");
                    tmp.Add("Saint Kitts and Nevis", "KN");
                    tmp.Add("Saint Lucia", "LC");
                    tmp.Add("Saint Martin", "MF");
                    tmp.Add("Saint Pierre and Miquelon", "PM");
                    tmp.Add("Saint Vincent and the Grenadines", "VC");
                    tmp.Add("Samoa", "WS");
                    tmp.Add("San Marino", "SM");
                    tmp.Add("Sao Tome and Principe", "ST");
                    tmp.Add("Saudi Arabia", "SA");
                    tmp.Add("Senegal", "SN");
                    tmp.Add("Serbia", "RS");
                    tmp.Add("Seychelles", "SC");
                    tmp.Add("Sierra Leone", "SL");
                    tmp.Add("Singapore", "SG");
                    tmp.Add("Sint Maarten", "SX");
                    tmp.Add("Slovakia", "SK");
                    tmp.Add("Slovenia", "SI");
                    tmp.Add("Solomon Islands", "SB");
                    tmp.Add("Somalia", "SO");
                    tmp.Add("South Africa", "ZA");
                    tmp.Add("South Korea", "KR");
                    tmp.Add("South Sudan", "SS");
                    tmp.Add("Spain", "ES");
                    tmp.Add("Sri Lanka", "LK");
                    tmp.Add("Sudan", "SD");
                    tmp.Add("Suriname", "SR");
                    tmp.Add("Svalbard and Jan Mayen", "SJ");
                    tmp.Add("Swaziland", "SZ");
                    tmp.Add("Sweden", "SE");
                    tmp.Add("Switzerland", "CH");
                    tmp.Add("Syria", "SY");
                    tmp.Add("Taiwan", "TW");
                    tmp.Add("Tajikistan", "TJ");
                    tmp.Add("Tanzania", "TZ");
                    tmp.Add("Thailand", "TH");
                    tmp.Add("Togo", "TG");
                    tmp.Add("Tokelau", "TK");
                    tmp.Add("Tonga", "TO");
                    tmp.Add("Trinidad and Tobago", "TT");
                    tmp.Add("Tunisia", "TN");
                    tmp.Add("Turkey", "TR");
                    tmp.Add("Turkmenistan", "TM");
                    tmp.Add("Turks and Caicos Islands", "TC");
                    tmp.Add("Tuvalu", "TV");
                    tmp.Add("U.S. Virgin Islands", "VI");
                    tmp.Add("Uganda", "UG");
                    tmp.Add("Ukraine", "UA");
                    tmp.Add("United Arab Emirates", "AE");
                    tmp.Add("United Kingdom", "GB");
                    tmp.Add("United States", "US");
                    tmp.Add("United States of America", "US");                    
                    tmp.Add("Uruguay", "UY");
                    tmp.Add("Uzbekistan", "UZ");
                    tmp.Add("Vanuatu", "VU");
                    tmp.Add("Vatican", "VA");
                    tmp.Add("Venezuela", "VE");
                    tmp.Add("Vietnam", "VN");
                    tmp.Add("Wallis and Futuna", "WF");
                    tmp.Add("Western Sahara", "EH");
                    tmp.Add("Yemen", "YE");
                    tmp.Add("Zambia", "ZM");
                    tmp.Add("Zimbabwe", "ZW");
                    _countries = tmp;
                }

                return _countries;
            }
        }
    }
}