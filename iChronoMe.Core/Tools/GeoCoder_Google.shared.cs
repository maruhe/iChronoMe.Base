using System;
using System.Globalization;
using System.Net;
using System.Xml;

using iChronoMe.Core.Classes;

using static iChronoMe.Core.Classes.GeoInfo;

namespace iChronoMe.Core.Tools
{
    public class GeoCoder_Google : IGeoCoder
    {
        private static int RequestCount = 0;
        private static int RequestMax = 250;

        public AreaInfo GetAreaInfo(double lat, double lng)
        {
            if (RequestCount > RequestMax)
                return null;
            RequestCount++;

            string cUri = "https://maps.googleapis.com/maps/api/geocode/xml?key=" + Secrets.GoogleMapsApiKey + "&latlng=" + lat.ToString("0.######", CultureInfo.InvariantCulture) + "," + lng.ToString("0.######", CultureInfo.InvariantCulture) + "&sensor=true";
            string cGeoInfo = sys.GetUrlContent(cUri).Result;

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(cGeoInfo);
                XmlElement eStatus = (XmlElement)doc.DocumentElement.FirstChild;
                if (eStatus.InnerText != "OK")
                    return null;
                XmlElement eArea = (XmlElement)eStatus.NextSibling;

                eArea.ToString();

                AreaInfo ai = new AreaInfo { dataSource = "google", sourceServer = "maps.googleapis.com" };
                if ("result".Equals(eArea.Name))
                {
                    string cType = eArea.SelectSingleNode("type").InnerText;
                    ai.toponymName = eArea.SelectSingleNode("formatted_address").InnerText;

                    xLog.Debug("Found Address:" + ai.toponymName);
                    string cAreaTitle = "";

                    foreach (XmlElement elResultPart in eArea.ChildNodes)
                    {
                        if ("address_component".Equals(elResultPart.Name))
                        {
                            xLog.Debug(elResultPart.SelectSingleNode("type").InnerText + " => " + elResultPart.SelectSingleNode("long_name").InnerText + " => " + elResultPart.SelectSingleNode("short_name").InnerText);

                            string cPartType = elResultPart.SelectSingleNode("type").InnerText;
                            if (!("street_number".Equals(cPartType)))
                            {
                                string cPartNameLong = elResultPart.SelectSingleNode("long_name").InnerText;
                                string cPartNameShort = elResultPart.SelectSingleNode("short_name").InnerText;
                                if (string.IsNullOrEmpty(cAreaTitle))
                                    cAreaTitle = elResultPart.SelectSingleNode("long_name").InnerText;

                                switch (cPartType)
                                {
                                    case "route":
                                        ai.route = cPartNameLong;
                                        if (string.IsNullOrEmpty(cAreaTitle))
                                            cAreaTitle = cPartNameLong;
                                        break;
                                    case "locality":
                                        ai.locality = cPartNameLong;
                                        cAreaTitle = cPartNameLong;
                                        break;
                                    case "administrative_area_level_5":
                                        ai.adminArea5 = cPartNameLong;
                                        break;
                                    case "administrative_area_level_4":
                                        ai.adminArea4 = cPartNameLong;
                                        break;
                                    case "administrative_area_level_3":
                                        ai.adminArea3 = cPartNameLong;
                                        break;
                                    case "administrative_area_level_2":
                                        ai.adminArea2 = cPartNameLong;
                                        break;
                                    case "administrative_area_level_1":
                                        ai.adminArea1 = cPartNameLong;
                                        break;
                                    case "country":
                                        ai.countryName = cPartNameLong;
                                        ai.countryCode = cPartNameShort;
                                        break;
                                    case "postal_code":
                                        ai.postalCode = cPartNameLong;
                                        break;
                                    case "postal_town":
                                        if (string.IsNullOrEmpty(cAreaTitle))
                                            cAreaTitle = cPartNameLong;
                                        if (string.IsNullOrEmpty(ai.locality))
                                            ai.locality = cPartNameLong;
                                        break;
                                    default:
                                        var x = cPartType + cPartNameShort + cPartNameLong;
                                        break;
                                }
                            }
                        }
                        if ("geometry".Equals(elResultPart.Name))
                        {
                            XmlNode elVp = elResultPart.SelectSingleNode("bounds");
                            if (elVp == null)
                                elVp = elResultPart.SelectSingleNode("viewport");
                            if (elVp != null)
                            {
                                XmlNode elVpSW = elVp.SelectSingleNode("southwest");
                                XmlNode elVpNE = elVp.SelectSingleNode("northeast");
                                ai.boxSouth = sys.parseDouble(elVpSW.SelectSingleNode("lat").InnerText);
                                ai.boxWest = sys.parseDouble(elVpSW.SelectSingleNode("lng").InnerText);
                                ai.boxNorth = sys.parseDouble(elVpNE.SelectSingleNode("lat").InnerText);
                                ai.boxEast = sys.parseDouble(elVpNE.SelectSingleNode("lng").InnerText);
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(cAreaTitle))
                    {
                        if ("unnamed road".Equals(cAreaTitle.ToLower()))
                        {
                            if (!string.IsNullOrEmpty(ai.adminArea5))
                                cAreaTitle = ai.adminArea5;
                            else if (!string.IsNullOrEmpty(ai.adminArea4))
                                cAreaTitle = ai.adminArea4;
                            else if (!string.IsNullOrEmpty(ai.adminArea3))
                                cAreaTitle = ai.adminArea3;
                            else if (!string.IsNullOrEmpty(ai.adminArea2))
                                cAreaTitle = ai.adminArea2;
                            else if (!string.IsNullOrEmpty(ai.adminArea1))
                                cAreaTitle = ai.adminArea1;
                            else if (!string.IsNullOrEmpty(ai.postalCode))
                                cAreaTitle += ", " + ai.postalCode;
                            else
                                ai.ToString();
                        }
                        ai.toponymName = cAreaTitle;
                    }

                    if (!string.IsNullOrEmpty(ai.toponymName) && (
                        string.IsNullOrEmpty(ai.adminArea1) && string.IsNullOrEmpty(ai.adminArea2) && string.IsNullOrEmpty(ai.adminArea3) &&
                        string.IsNullOrEmpty(ai.adminArea4) && string.IsNullOrEmpty(ai.adminArea5) &&
                        string.IsNullOrEmpty(ai.locality) && string.IsNullOrEmpty(ai.route) &&
                        ai.toponymName.Contains(",")
                        ))
                    {
                        var cx = ai.toponymName.Split(',');
                        ai.toponymName = cx[0].Trim();
                        ai.countryCode = cx[cx.Length - 1].Trim();
                        if (cx.Length > 3)
                            ai.adminArea3 = cx[1].Trim();
                        if (cx.Length > 4)
                            ai.adminArea2 = cx[2].Trim();
                        if (cx.Length > 5)
                            ai.adminArea1 = cx[3].Trim();
                        ai.ToString();
                    }
                    else if (Equals(ai.toponymName, ai.postalCode))
                    {
                        if (!string.IsNullOrEmpty(ai.adminArea5))
                            ai.toponymName = ai.adminArea5;
                        else if (!string.IsNullOrEmpty(ai.adminArea4))
                            ai.toponymName = ai.adminArea4;
                        else if (!string.IsNullOrEmpty(ai.adminArea3))
                            ai.toponymName = ai.adminArea3;
                        else if (!string.IsNullOrEmpty(ai.adminArea2))
                            ai.toponymName = ai.adminArea2;
                        else if (!string.IsNullOrEmpty(ai.adminArea1))
                            ai.toponymName = ai.adminArea1;
                    }

                    xLog.Debug("Final title: " + ai.toponymName);
                }
                return ai;
            }
            catch (Exception ex)
            {
                xLog.Error(ex);
                return null;
            }
        }

        public AreaInfo GetPositionByName(string location)
        {
            if (RequestCount > RequestMax)
                return null;
            RequestCount++;

            String urlString = "https://maps.googleapis.com/maps/api/geocode/xml?key=" + Secrets.GoogleMapsApiKey + "&address=" + WebUtility.UrlEncode(location);
            String cXml = sys.GetUrlContent(urlString).Result;
            if (string.IsNullOrEmpty(cXml))
                return null;
            if (cXml.IndexOf("<status>OK</status>") > 0)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(cXml);

                string cLatitude = string.Empty;
                string cLongitude = string.Empty;
                string cFormattedAdress = string.Empty;
                string cAdressComponents = string.Empty;

                foreach (XmlElement el in doc.DocumentElement.ChildNodes)
                {
                    if ("result".Equals(el.Name.ToLower()))
                    {

                        foreach (XmlElement elRes in el.ChildNodes)
                        {
                            if ("formatted_address".Equals(elRes.Name.ToLower()))
                                cFormattedAdress = elRes.InnerText;

                            if ("address_component".Equals(elRes.Name.ToLower()))
                            {
                                foreach (XmlElement elAdr in elRes.ChildNodes)
                                {
                                    if ("long_name".Equals(elAdr.Name.ToLower()))
                                    {
                                        if (!string.IsNullOrEmpty(cAdressComponents))
                                            cAdressComponents += ", ";
                                        cAdressComponents += elAdr.InnerText;
                                        break;
                                    }
                                }
                            }
                            if ("geometry".Equals(elRes.Name.ToLower()))
                            {
                                foreach (XmlElement elGeo in elRes.ChildNodes)
                                {
                                    if ("location".Equals(elGeo.Name.ToLower()))
                                    {

                                        foreach (XmlElement elLoc in elGeo.ChildNodes)
                                        {
                                            if ("lat".Equals(elLoc.Name.ToLower()))
                                                cLatitude = elLoc.InnerText;
                                            else if ("lng".Equals(elLoc.Name.ToLower()))
                                                cLongitude = elLoc.InnerText;
                                        }
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                    }
                }

                return new AreaInfo
                {
                    dataSource = "google",
                    sourceServer = "maps.googleapis.com",

                    toponymName = string.IsNullOrEmpty(cFormattedAdress) ? cAdressComponents : cFormattedAdress,
                    centerLat = double.Parse(cLatitude, NumberStyles.Any, CultureInfo.InvariantCulture),
                    centerLong = double.Parse(cLongitude, NumberStyles.Any, CultureInfo.InvariantCulture),
                    pointLat = double.Parse(cLatitude, NumberStyles.Any, CultureInfo.InvariantCulture),
                    pointLng = double.Parse(cLongitude, NumberStyles.Any, CultureInfo.InvariantCulture)
                };
            }
            return null;
        }
    }
}
