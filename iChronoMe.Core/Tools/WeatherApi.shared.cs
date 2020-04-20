using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using iChronoMe.Core.Classes;
using iChronoMe.Core.DataModels;
using iChronoMe.Core.Types;
using iChronoMe.Core.Types.xUnit;
using Newtonsoft.Json.Linq;

namespace iChronoMe.Core.Tools
{
    public class WeatherApi
    {
        static object oLock = new object();
        static int requestCount = 0;
        static DateTime limitReset = DateTime.Now.AddDays(1);
        const string climacellUrl = "https://api.climacell.co/v3/weather/forecast/hourly";

        public static bool UpdateWeatherInfo(DateTime gmtNow, double lat, double lng)
        {
            lock (oLock)
            {
                //block multiple Update
                if (WeatherInfo.GetWeatherInfo(gmtNow, lat, lng) != null)
                    return true;

                if (requestCount > 30)
                {
                    if (limitReset < DateTime.Now)
                    {
                        requestCount = 0;
                        limitReset = DateTime.Now.AddDays(1);
                    }
                    else
                    {
                        db.dbAreaCache?.Insert(new WeatherInfo(0, 0, limitReset.ToUniversalTime()) { Error = "api limit" });
                        return false;
                    }
                }

                //Update Weather Info
                requestCount++;
                gmtNow = new DateTime(gmtNow.Year, gmtNow.Month, gmtNow.Day-1, gmtNow.Hour, gmtNow.Minute / 5 * 5, 0, DateTimeKind.Utc).AddMinutes(5);

                string temp = xUnits.GetUnitStringClimacell(Temp.Default);

                string cFields = string.Concat("temp:", temp, ",feels_like:", temp, ",humidity,wind_speed:", xUnits.GetUnitStringClimacell(WindSpeed.Default), ",wind_direction,baro_pressure:", xUnits.GetUnitStringClimacell(BarumPressure.Default), ",precipitation:", xUnits.GetUnitStringClimacell(Precipitation.Default), ",precipitation_type,precipitation_probability,sunrise,sunset,visibility:", xUnits.GetUnitStringClimacell(Distance.Default), ",cloud_cover,weather_code");
                string cUrl = string.Concat(climacellUrl, "?apikey=", Secrets.ClimaCellApiKey, "&lat=", lat.ToString("0.######", CultureInfo.InvariantCulture), "&lon=", lng.ToString("0.######", CultureInfo.InvariantCulture), "&end_time=", DateTime.UtcNow.AddHours(3).ToString("yyyy-MM-ddTHH:mm:ssZ"), "&fields=", cFields);

                string cWeatherInfo = sys.GetUrlContent(cUrl).Result;

                if (!string.IsNullOrEmpty(cWeatherInfo) && cWeatherInfo.StartsWith("{"))
                {
                    try
                    {
                        var error = JObject.Parse(cWeatherInfo);
                        string statusCode = error["statusCode"]?.ToString() ?? "x";
                        string errorCode = error["errorCode"]?.ToString() ?? "x";
                        string message = error["message"]?.ToString() ?? "x";

                        requestCount = 10000;
                        limitReset = DateTime.Now.AddMinutes(30);
                        if (message.ToLower().Contains("api") && message.ToLower().Contains("limit"))
                        {
                            //change weather server
                            db.dbAreaCache?.Insert(new WeatherInfo(0, 0, limitReset.ToUniversalTime()) { Error = "api limit" });
                            return false;
                        }
                        else if (sys.Debugmode)
                        {
                            sys.LogException(new Exception("climacell-error\n" + statusCode + "\n" + errorCode + "\n" + message));
                        }
                    }
                    catch (Exception ex)
                    {
                        xLog.Error(ex, cWeatherInfo);
                    }
                }

                var res = WeatherInfoFromGeoJson(cWeatherInfo, gmtNow, lat, lng);

                return res != null && res.Count > 0;
            }
        }

        public static List<WeatherInfo> WeatherInfoFromGeoJson(string cWeatherInfo, DateTime gmtNow, double lat, double lng)
        {
            try
            {
                if (string.IsNullOrEmpty(cWeatherInfo))
                    return null;

                var jlist = JArray.Parse(cWeatherInfo);

                List<WeatherInfo> res = new List<WeatherInfo>();

                var cache = db.dbAreaCache;

                foreach (JObject wInfo in jlist)
                {
                    var wi = new WeatherInfo((double)wInfo["lat"], (double)wInfo["lon"], (DateTime)wInfo["observation_time"]["value"]);

                    try { wi.WeatherCode = (string)wInfo["weather_code"]["value"]; } catch { }
                    try { wi.Temp = (double)wInfo["temp"]["value"]; } catch { }
                    try { wi.FeelsLike = (double)wInfo["feels_like"]["value"]; } catch { }
                    try { wi.Humidity = (double)wInfo["humidity"]["value"]; } catch { }
                    try { wi.WindSpeed = (double)wInfo["wind_speed"]["value"]; } catch { }
                    try { wi.WindDirection = (int)wInfo["wind_direction"]["value"]; } catch { }
                    try { wi.BaroPressure = (double)wInfo["baro_pressure"]["value"]; } catch { }
                    try { wi.Precipitation = (double)wInfo["precipitation"]["value"]; } catch { }
                    try { wi.PrecipitationType = (string)wInfo["precipitation_type"]["value"]; } catch { }
                    try { wi.Sunrise = (DateTime)wInfo["sunrise"]["value"]; } catch { }
                    try { wi.Sunset = (DateTime)wInfo["sunset"]["value"]; } catch { }
                    try { wi.Visibility = (double)wInfo["visibility"]["value"]; } catch { }
                    try { wi.CloudCover = (double)wInfo["cloud_cover"]["value"]; } catch { }

                    res.Add(wi);
                }
                
                cache?.InsertAll(res);
                int del = cache.Execute("delete from WeatherInfo where ObservationTime < ?", gmtNow.AddDays(-1));

                return res;
            }
            catch (Exception ex)
            {
                xLog.Error(ex);
            }
            
            return null;
        }
    }
}


/*
Parameter	to load	SI	US	Description	Beschreibung
temp	temp:C,temp:F,	C	F	Temperature	Temperatur
feels_like	feels_like:C,feels_like:F	C	F	Wind chill and heat window based on season	Windkühl- und Wärmefenster je nach Jahreszeit
dewpoint		C	F	Temperature of the dew point	Temperatur des Taupunktes
humidity	humidity,	%		Percent relative humidity from 0 - 100 %	Prozent relative Luftfeuchtigkeit von 0 - 100%
wind_speed*	wind_speed:m/s,wind_speed:kph,wind_speed:mph,wind_speed:knots,wind_speed:beaufort,	m/s, kph	mph	Wind speed	Windgeschwindigkeit
wind_direction	wind_direction,	degrees		Wind direction in polar degrees 0-360 where 0 = North	Windrichtung in polaren Graden 0-360 mit 0 = Nord
wind_gust*		m/s, kph	mph	Wind gust speed	Windböengeschwindigkeit
baro_pressure	baro_pressure:hPa,baro_pressure:mmHg,baro_pressure:inHg, 	hPa, mmHg	inHg	Barometric pressure (at surface)	Luftdruck (an der Oberfläche)
precipitation	precipitation:in/hr,precipitation:mm/hr,	mm/hr	in/hr	Precipitation intensity	Niederschlagsintensität
precipitation_type	precipitation_type,			Types are: none, rain, snow, ice pellets, and freezing rain	Typen sind: keine, Regen, Schnee, Eispellets und Eisregen
precipitation_probability		%	%	The chance that precipitation will occur at the forecast time within the hour or day	Die Wahrscheinlichkeit, dass Niederschlag zur Vorhersagezeit innerhalb einer Stunde oder eines Tages auftritt
sunrise / sunset	sunrise,sunset,			Provides the times of sunrise & sunset based on location	Bietet die Zeiten von Sonnenaufgang und Sonnenuntergang basierend auf dem Standort
visibility	visibility:km,visibility:mi 	km	mi	Visibility distance	Sichtweite
cloud_cover	cloud_cover,	%		Fraction of the sky obscured by clouds	Bruchteil des Himmels von Wolken verdeckt
cloud_base		m	ft	The lowest level at which the air contains a perceptible quantity of cloud particles	Das niedrigste Niveau, in dem die Luft eine wahrnehmbare Menge an Wolkenteilchen enthält
cloud_ceiling		m	ft	The height of the lowest layer of clouds which covers more than half of the sky	Die Höhe der untersten Wolkenschicht, die mehr als die Hälfte des Himmels bedeckt
surface_shortwave_radiation		w/sqm		Solar radiation reaching the surface	Sonnenstrahlung erreicht die Oberfläche
moon_phase				This field is available in hourly and daily endpoints. Available values include new_moon, waxing_crescent, first_quarter, waxing_gibbous, full, waning_gibbous, third_quarter, waning_crescent	Dieses Feld ist in stündlichen und täglichen Endpunkten verfügbar. Zu den verfügbaren Werten gehören Neumond, wachsendes Wachsen, erstes Viertel, wachsendes Gibbous, volles, abnehmendes Gibbous, drittes Viertel, abnehmendes Viertel
weather_code	weather_code,			A textual field that conveys the weather conditions. Possible values are freezing_rain_heavy, freezing_rain, freezing_rain_light, freezing_drizzle, ice_pellets_heavy, ice_pellets, ice_pellets_light, snow_heavy, snow, snow_light, flurries, tstorm, rain_heavy, rain, rain_light, drizzle, fog_light, fog, cloudy, mostly_cloudy, partly_cloudy, mostly_clear, clear	Ein Textfeld, das die Wetterbedingungen vermittelt. Mögliche Werte sind gefrierender Regen schwer, gefrierender Regen, gefrierender Regen leicht, gefrierender Nieselregen, Eispellets schwer, Eispellets, Eispellets leicht, schneebedeckt, Schnee, Schneelicht, Unwetter, tstorm, Regen schwer, Regen, Regenlicht, nieselregen, neblig, neblig
fire_index	fire_index,	1-100		Indicates the level of risk on a scale of 1-100, relative to conditions that play a major role in fires	Zeigt das Risiko auf einer Skala von 1 bis 100 im Verhältnis zu Bedingungen an, die bei Bränden eine wichtige Rolle spielen
road_risk				A textual field that conveys the road condition (available only in the US). Possible values are low_risk, moderate_risk, mod_hi_risk, high_risk, extreme_risk	Ein Textfeld, das den Straßenzustand anzeigt (nur in den USA verfügbar). Mögliche Werte sind geringes risiko, mäßiges risiko, mod hi risiko, hohes risiko, extremes risiko
observation_time		UTC		When the reported conditions occurred	Wann traten die gemeldeten Zustände auf?
pm25		μg/m3	μg/m3	Particulate Matter < 2.5 μm	Feinstaub <2,5 μm
pm10		μg/m3	μg/m3	Particulate Matter < 10 μm	Feinstaub <10 μm
o3		ppb	ppb	Ozone	Ozon
no2		ppb	ppb	Nitrogen Dioxide	Stickstoffdioxid
co		ppm	ppm	Carbon Monoxide	Kohlenmonoxid
so2		ppb	ppb	Sulfur Dioxide	Schwefeldioxid
epa_aqi				Air Quality Index per US EPA standard	Luftqualitätsindex gemäß US EPA Standard
epa_primary_pollutant				index of Primary Pollutant per US EPA standard	Index des Primärschadstoffs gemäß US-EPA-Standard
china_aqi				Air Quality Index per China MEP standard	Luftqualitätsindex gemäß China MEP Standard
china_primary_pollutant				Index of Primary Pollutant per China MEP standard	Index des Primärschadstoffs gemäß China MEP-Standard
epa_health_concern				Health concern level based on EPA air quality index	Gesundheitsbedenken basierend auf dem EPA-Luftqualitätsindex
china_health_concern				Health concern level based on china air quality index	Gesundheitsbedenken basierend auf dem Luftqualitätsindex in China

 */
