﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using iChronoMe.Core.Classes;
using iChronoMe.Core.Types;
using iChronoMe.DeviceCalendar;

namespace iChronoMe.Core.DynamicCalendar
{
    /// <summary>
    /// Class for calendar events (extends Dictionary<DateTime, ICollection>)
    /// </summary>
    public class EventCollection : Dictionary<DateTime, ICollection>
    {
        #region ctor

        public EventCollection() : base()
        { init(); }

        public EventCollection(int capacity) : base(capacity)
        { init(); }

        public EventCollection(IEqualityComparer<DateTime> comparer) : base(comparer)
        { init(); }

        public EventCollection(IDictionary<DateTime, ICollection> dictionary) : base(dictionary)
        { init(); }

        public EventCollection(int capacity, IEqualityComparer<DateTime> comparer) : base(capacity, comparer)
        { init(); }

        public EventCollection(SerializationInfo info, StreamingContext context) : base(info, context)
        { init(); }

        public EventCollection(IDictionary<DateTime, ICollection> dictionary, IEqualityComparer<DateTime> comparer) : base(dictionary, comparer)
        { init(); }

        #endregion

        private void init()
        { }

        public DateTime GetTimeFromLocal(DateTime tTimeZoneTimeNow, TimeType? tType = null)
        {
            if (tType == null)
                tType = timeType;
            return LocationTimeHolder.LocalInstance.GetTime((TimeType)tType, tTimeZoneTimeNow.ToUniversalTime());
        }

        public TimeType timeType { get; set; } = AppConfigHolder.MainConfig.DefaultTimeType;

        public string CalendarFilter { get; set; }

        public async Task LoadCalendarEventsGrouped(DateTime dStart, DateTime dEnd)
        {
            try
            {
                await DoLoadCalendarEventsGrouped(dStart, dEnd);
            }
            catch { }
        }

        public List<CalendarEvent> AllEvents { get; } = new List<CalendarEvent>();
        public List<Object> AllDatesAndEvents { get; } = new List<object>();

        public async Task DoLoadCalendarEventsGrouped(DateTime dStart, DateTime dEnd, int iMax = 0)
        {
            try
            {
                var newEvents = new SortedDictionary<DateTime, SortedDictionary<DateTime, CalendarEvent>>();

                await DoLoadCalendarEventsListed(dStart, dEnd, iMax);
                int iMs = 0;
                foreach (CalendarEvent calEvent in ListedDates)
                {
                    DateTime tFirstDay = calEvent.Start;
                    if (tFirstDay < dStart)
                        tFirstDay = dStart;
                    DateTime tLastDay = calEvent.End;
                    if (tFirstDay > dEnd)
                        tLastDay = dEnd;
                    for (DateTime tDay = tFirstDay.Date; tDay < tLastDay; tDay = tDay.AddDays(1))
                    {
                        iMs++;
                        if (!newEvents.ContainsKey(tDay))
                            newEvents.Add(tDay, new SortedDictionary<DateTime, CalendarEvent>());
                        newEvents[tDay].Add(calEvent.SortTime.AddMilliseconds(iMs), calEvent);
                    }
                }

                base.Clear();
                int iCount = 0;
                var allEvents = new List<CalendarEvent>();
                var allDatesAndEvents = new List<object>();
                foreach (DateTime d in newEvents.Keys)
                {
                    base[d] = newEvents[d].Values;
                    iCount += newEvents[d].Count;
                    allDatesAndEvents.Add(d);
                    allDatesAndEvents.AddRange(newEvents[d].Values);
                    allEvents.AddRange(newEvents[d].Values);
                }
                AllEvents.Clear();
                AllEvents.AddRange(allEvents);
                AllDatesAndEvents.Clear();
                AllDatesAndEvents.AddRange(allDatesAndEvents);
                allEvents.Clear();
                allDatesAndEvents.Clear();
            }
            catch (Exception ex)
            {
                base.Clear();
                ex.ToString();
                Console.WriteLine("Error reading Calendar Events: " + ex.GetType().Name + ": " + ex.Message);
            }
        }

        public async Task LoadCalendarEventsListed(DateTime dStart, DateTime dEnd)
        {
            try
            {
                await DoLoadCalendarEventsListed(dStart, dEnd);
            }
            catch { }
        }

        public List<CalendarEvent> ListedDates { get; } = new List<CalendarEvent>();

        static bool bIsLoadingCalendarEventsListed = false;
        public async Task DoLoadCalendarEventsListed(DateTime dStart, DateTime dEnd, int iMax = 0, int iError = 0)
        {
            Console.WriteLine("DoLoadCalendarEventsListed: Start: " + bIsLoadingCalendarEventsListed.ToString());
            if (bIsLoadingCalendarEventsListed)
            {
                while (bIsLoadingCalendarEventsListed)
                    Task.Delay(25).Wait();
                bIsLoadingCalendarEventsListed = true;
                Console.WriteLine("DoLoadCalendarEventsListed: Start Now");
            }
            bIsLoadingCalendarEventsListed = true;
            try
            {
                int iMs = 0;
                var newEvents = new SortedDictionary<DateTime, CalendarEvent>();
                LocationTimeHolder lthCheck = LocationTimeHolder.LocalInstanceClone;

                DateTime swStart = DateTime.Now;
                if (sys.Windows)
                {
                    var rnd = new Random(DateTime.Now.Second);
                    for (int i = 0; i < rnd.Next(5) * 2; i++)
                    {
                        string name = "win";
                        var calEvent = new CalendarEvent();
                        calEvent.Name = $"{name} event {i}";
                        calEvent.Description = $"This is {name} event{i}'s description!";
                        calEvent.Start = dStart.AddHours(new Random().Next(8) + 10).AddMinutes(new Random().Next(50));
                        calEvent.End = calEvent.Start.AddHours(new Random().Next(5) + 1);
                        calEvent.EventColor = xColor.FromRgb(rnd.Next(200), rnd.Next(200), rnd.Next(200));
                        calEvent.DisplayColorString = calEvent.EventColorString;

                        //CheckEventLocationTime(lthCheck, calendar, calEvent);
                        newEvents.Add(calEvent.SortTime.AddMilliseconds(iMs++), calEvent);
                    }
                }
                else
                {
                    var calendars = new List<DeviceCalendar.Calendar>(await DeviceCalendar.DeviceCalendar.GetCalendarsAsync());
                    foreach (DeviceCalendar.Calendar calendar in calendars)
                    {
                        if (string.IsNullOrEmpty(CalendarFilter) || CalendarFilter.Contains("|" + calendar.ExternalID + "|"))
                        {

                            var calEvents = await DeviceCalendar.DeviceCalendar.GetEventsAsync(calendar, dStart, dEnd);
                            foreach (CalendarEvent calEvent in calEvents)
                            {
                                CheckEventLocationTime(lthCheck, calendar, calEvent);
                                newEvents.Add(calEvent.SortTime.AddMilliseconds(iMs++), calEvent);

                                if (iMax > 0 && iMs >= iMax)
                                    break;
                                iMs++;
                            }
                        }
                    }
                }
                Console.WriteLine("LoadAndCheckEvents took " + (DateTime.Now - swStart).TotalMilliseconds.ToString() + "ms for " + iMs.ToString() + " Events");
                ListedDates.Clear();
                ListedDates.AddRange(newEvents.Values);
            }
            catch (Exception ex)
            {
                base.Clear();
                ex.ToString();
                Console.WriteLine("Error reading Calendar Events: " + ex.GetType().Name + ": " + ex.Message);

                if (iError < 3)
                {
                    await Task.Delay(250 + new Random().Next(250));
                    bIsLoadingCalendarEventsListed = false;
                    await DoLoadCalendarEventsListed(dStart, dEnd, iMax, iError + 1);
                }
            }
            finally { bIsLoadingCalendarEventsListed = false; }
            Console.WriteLine("DoLoadCalendarEventsListed: Stop");
        }

        /// <summary>
        /// Removed a collection of values for specific date
        /// </summary>
        /// <param name="key">Event DateTime</param>
        /// <returns>true if the element is successfully found and removed; otherwise, false. This method returns false if key is not found in the System.Collections.Generic.Dictionary`2.</returns>
        public new bool Remove(DateTime key)
        {
            var removed = base.Remove(key.Date);

            if (removed)
                CollectionChanged?.Invoke(this, new EventCollectionChangedArgs { Item = key.Date, Type = EventCollectionChangedType.Remove });

            return removed;
        }

        /// <summary>
        /// Add collection of values for specific date
        /// </summary>
        /// <param name="key">Event DateTime</param>
        /// <param name="value">Collection of events for date</param>
        public new void Add(DateTime key, ICollection value)
        {
            base.Add(key.Date, value);
            CollectionChanged?.Invoke(this, new EventCollectionChangedArgs { Item = key.Date, Type = EventCollectionChangedType.Add });
        }

        /// <summary>
        /// Gets/sets collection of values for specific date
        /// </summary>
        /// <param name="key">Event DateTime</param>
        /// <returns>Collection of events for date</returns>
        public new ICollection this[DateTime key]
        {
            get => base[key.Date];
            set
            {
                base[key.Date] = value;
                CollectionChanged?.Invoke(this, new EventCollectionChangedArgs { Item = key.Date, Type = EventCollectionChangedType.Set });
            }
        }

        internal event EventHandler<EventCollectionChangedArgs> CollectionChanged;

        internal class EventCollectionChangedArgs
        {
            public DateTime Item { get; set; }
            public EventCollectionChangedType Type { get; set; }
        }

        internal enum EventCollectionChangedType
        {
            Add,
            Set,
            Remove
        }

        public bool EventsCheckerIsActive { get => eventsChecker != null; }

        static List<CalendarEvent> eventsToCheck = new List<CalendarEvent>();
        static Task eventsChecker = null;
        static Dictionary<string, string> eventsCheckerStates = new Dictionary<string, string>();

        void CheckEventLocationTime(LocationTimeHolder lthCheck, DeviceCalendar.Calendar calendar, CalendarEvent calEvent)
        {
            DateTime swStart = DateTime.Now;

            //Zeit anpassen wenn Sonn = Cheff
            CalendarEventExtention extEvent = CalendarEventExtention.GetExtention(calEvent.ExternalID, false);

            DateTime tStart = calEvent.Start;
            DateTime tEnd = calEvent.End;
            if (extEvent != null)
            {
                if (extEvent.UseTypedTime)
                {
                    //Check Location and Time
                    if (calEvent.Start.Equals(extEvent.CalendarTimeStart) && calEvent.End.Equals(extEvent.CalendarTimeEnd))
                    {
                        double nLat = extEvent.Latitude;
                        double nLng = extEvent.Longitude;
                        if (nLat == 0 && nLng == 0)
                        {
                            nLat = sys.lastUserLocation.Latitude;
                            nLng = sys.lastUserLocation.Longitude;
                        }

                        if (extEvent.TimeType == timeType)
                        {
                            tStart = extEvent.TimeTypeStart;
                            tEnd = extEvent.TimeTypeEnd;
                        }
                        else
                        {

                            tStart = lthCheck.GetLocationTime(nLat, nLng, timeType, extEvent.TimeTypeStart, extEvent.TimeType);
                            tEnd = lthCheck.GetLocationTime(nLat, nLng, timeType, extEvent.TimeTypeEnd, extEvent.TimeType);
                        }

                        calEvent.SortTime = sys.GetTimeWithoutSeconds(lthCheck.GetLocationTime(nLat, nLng, TimeType.TimeZoneTime, extEvent.TimeTypeStart, extEvent.TimeType));
                        if (calEvent.Start != calEvent.SortTime)
                        {
                            //looks like current location changed => update event time
                            calEvent.Start = calEvent.SortTime;
                            calEvent.End = sys.GetTimeWithoutSeconds(lthCheck.GetLocationTime(nLat, nLng, TimeType.TimeZoneTime, extEvent.TimeTypeEnd, extEvent.TimeType));
                            calEvent.DisplayStart = extEvent.CalendarTimeStart = calEvent.Start;
                            calEvent.DisplayEnd = extEvent.CalendarTimeEnd = calEvent.End;

                            if (calEvent.Start < calEvent.End)
                            {
                                DeviceCalendar.DeviceCalendar.AddOrUpdateEventAsync(calendar, calEvent).Wait();
                                db.dbCalendarExtention.Update(extEvent);
                            }
                            else
                            {
                                calEvent.ToString();
                            }
                        }
                    }
                    else
                    {
                        //event has been edited in another app => stop updating my Location and TypeTime
                        extEvent.UseTypedTime = false;
                        db.dbCalendarExtention.Update(extEvent);
                    }
                }
                if (!extEvent.UseTypedTime && !calEvent.AllDay)
                {
                    tStart = GetTimeFromLocal(calEvent.Start, timeType);
                    tEnd = GetTimeFromLocal(calEvent.End, timeType);
                }
                calEvent.DisplayStart = tStart;
                calEvent.DisplayEnd = tEnd;
            }
            else if (timeType != TimeType.TimeZoneTime && !calEvent.AllDay)
            {
                //standard event in Time-Type-View
                tStart = GetTimeFromLocal(calEvent.Start, timeType);
                tEnd = GetTimeFromLocal(calEvent.End, timeType);
                calEvent.DisplayStart = tStart;
                calEvent.DisplayEnd = tEnd;
            }

#if DEBUG
            //calEvent.Description = (DateTime.Now - swStart).TotalMilliseconds.ToString() + " ms checking";
#endif

            //Koordinaten zu Position finden
            if (string.IsNullOrEmpty(calEvent.Location))
                return;
            string cLast;
            if (eventsCheckerStates.TryGetValue(calEvent.ExternalID, out cLast))
            {
                if (cLast.Equals(calEvent.Location))
                    return;
            }

            if (!eventsToCheck.Contains(calEvent))
                eventsToCheck.Add(calEvent);

            if (eventsChecker == null)
            {
                eventsChecker = Task.Factory.StartNew(() =>
                {
                    Console.WriteLine("EventCollection: EventsChecker: Start");
                    int iChecked = 0;
                    try
                    {
                        Task.Delay(2500).Wait();
                        DateTime tCheckStart = DateTime.Now;
                        bool bDidANotify = false;
                        while (eventsToCheck.Count > 0)
                        {
                            try
                            {
                                CalendarEvent checkEvent = eventsToCheck[0];
                                CalendarEventExtention extEventLoc = CalendarEventExtention.GetExtention(checkEvent.ExternalID);
                                if (!string.IsNullOrEmpty(checkEvent.Location) && !extEventLoc.LocationString.Equals(checkEvent.Location))
                                {
                                    extEventLoc.LocationString = checkEvent.Location;
                                    extEventLoc.GotCorrectPosition = false;

                                    Regex word = new Regex(@"^[-+]?([1-8]?\d(\.\d+)?|90(\.0+)?),\s*[-+]?(180(\.0+)?|((1[0-7]\d)|([1-9]?\d))(\.\d+)?)$");
                                    Match m = word.Match(checkEvent.Location);
                                    if (!string.IsNullOrEmpty(m.Value))
                                    {
                                        try
                                        {
                                            var pos = m.Value.Split(new char[] { ',' });
                                            extEventLoc.Latitude = double.Parse(pos[0].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture);
                                            extEventLoc.Longitude = double.Parse(pos[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture);
                                            extEventLoc.GotCorrectPosition = true;

                                            iChecked++;
                                            Console.WriteLine("EventCollection: EventsChecker: Updateposition: " + checkEvent.ExternalID + ": " + checkEvent.Location);
                                        }
                                        catch { }
                                    }
                                    else
                                    {
                                        String urlString = "https://maps.googleapis.com/maps/api/geocode/xml?key=" + Secrets.GApiKey + "&address=" + WebUtility.UrlEncode(checkEvent.Location);
                                        String cXml = sys.GetUrlContent(urlString).Result;
                                        if (string.IsNullOrEmpty(cXml))
                                            return;
                                        if (cXml.IndexOf("<status>OK</status>") > 0)
                                        {
                                            cXml = cXml.Substring(cXml.IndexOf("<location>"));
                                            cXml = cXml.Substring(cXml.IndexOf("<lat>") + 5);
                                            string cLat = cXml.Substring(0, cXml.IndexOf("<"));
                                            cXml = cXml.Substring(cXml.IndexOf("<lng>") + 5);
                                            string cLng = cXml.Substring(0, cXml.IndexOf("<"));

                                            extEventLoc = CalendarEventExtention.GetExtention(checkEvent.ExternalID);
                                            extEventLoc.LocationString = checkEvent.Location;
                                            extEventLoc.Latitude = double.Parse(cLat, NumberStyles.Any, CultureInfo.InvariantCulture);
                                            extEventLoc.Longitude = double.Parse(cLng, NumberStyles.Any, CultureInfo.InvariantCulture);
                                            extEventLoc.GotCorrectPosition = true;

                                            iChecked++;
                                            Console.WriteLine("EventCollection: EventsChecker: Updateposition: " + checkEvent.ExternalID + ": " + checkEvent.Location);
                                        }
                                    }
                                    if (extEventLoc.RecNo < 0)
                                        db.dbCalendarExtention.Insert(extEventLoc);
                                    else
                                        db.dbCalendarExtention.Update(extEventLoc);
                                }
                                eventsCheckerStates[checkEvent.ExternalID] = checkEvent.Location;
                            }
                            finally
                            {
                                eventsToCheck.RemoveAt(0);
                            }
                            if (eventsToCheck.Count == 0)
                            {
                                Task.Delay(250).Wait();
                            }

                            if (!bDidANotify && tCheckStart.AddSeconds(12) < DateTime.Now)
                            {
                                bDidANotify = true;
                                sys.NotifyCalendarEventsUpdated();
                            }
                        }
                    }
                    finally
                    {
                        Console.WriteLine("EventCollection: EventsChecker: Done: " + iChecked.ToString());
                        eventsChecker = null;
                        if (iChecked > 0)
                        {
                            sys.NotifyCalendarEventsUpdated();
                        }
                    }
                });
            }
        }
    }
}