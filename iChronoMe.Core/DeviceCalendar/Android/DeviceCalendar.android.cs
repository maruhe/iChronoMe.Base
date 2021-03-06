﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.Database;
using Android.Provider;

using iChronoMe.Core.Classes;
using iChronoMe.Core.Types;

namespace iChronoMe.DeviceCalendar
{
    public static partial class DeviceCalendar
    {
        #region Constants

        private static readonly Android.Net.Uri _calendarsUri = CalendarContract.Calendars.ContentUri;
        private static readonly Android.Net.Uri _eventsUri = CalendarContract.Events.ContentUri;
        private static readonly Android.Net.Uri _remindersUri = CalendarContract.Reminders.ContentUri;

        private static readonly string[] _calendarsProjection =
            {
                CalendarContract.Calendars.InterfaceConsts.Id,
                CalendarContract.Calendars.InterfaceConsts.CalendarDisplayName,
                CalendarContract.Calendars.InterfaceConsts.CalendarColor,
                CalendarContract.Calendars.InterfaceConsts.AccountName,
                CalendarContract.Calendars.InterfaceConsts.OwnerAccount,
                CalendarContract.Calendars.InterfaceConsts.Visible,
                CalendarContract.Calendars.InterfaceConsts.CalendarAccessLevel,
                CalendarContract.Calendars.InterfaceConsts.AccountType,
                CalendarContract.Calendars.InterfaceConsts.IsPrimary
            };

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name of the account to use for creating/editing calendars.
        /// Defaults to application package label.
        /// </summary>
        /// <value>The name of the account.</value>
        public static string AccountName { get; set; }

        /// <summary>
        /// Gets or sets the owner account to use for creating/editing calendars.
        /// Defaults to application package label.
        /// </summary>
        /// <value>The owner account.</value>
        public static string OwnerAccount { get; set; }

        /// <summary>
        /// Gets or sets the account-Type to use for creating new calendars.
        /// Defaults to application package name.
        /// </summary>
        /// <value>The owner account.</value>
        public static string AccountType { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Plugin.Calendars.CalendarsImplementation"/> class.
        /// </summary>
        static DeviceCalendar()
        {
            AccountName = OwnerAccount = Application.Context.ApplicationInfo.LoadLabel(Application.Context.PackageManager);
            AccountType = Application.Context.ApplicationInfo.PackageName;
        }

        #endregion

        #region ICalendars implementation

        /// <summary>
        /// Gets a list of all calendars on the device.
        /// </summary>
        /// <returns>Calendars</returns>
        /// <exception cref="UnauthorizedAccessException">Calendar access denied</exception>
        /// <exception cref="Plugin.Calendars.Abstractions.PlatformException">Unexpected platform-specific error</exception>
        public static Task<IList<Calendar>> GetCalendarsAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    var cursor = Query(_calendarsUri, _calendarsProjection);

                    var calendars = IterateCursor(cursor, () => GetCalendar(cursor));

                    return calendars;
                }
                catch (Exception ex)
                {
                    xLog.Error(ex);
                    return null;
                }
            });
        }

        /// <summary>
        /// Gets a single calendar by platform-specific ID.
        /// </summary>
        /// <param name="externalId">Platform-specific calendar identifier</param>
        /// <returns>The corresponding calendar, or null if not found</returns>
        /// <exception cref="UnauthorizedAccessException">Calendar access denied</exception>
        /// <exception cref="Plugin.Calendars.Abstractions.PlatformException">Unexpected platform-specific error</exception>
        public static Task<Calendar> GetCalendarByIdAsync(string externalId)
        {
            long calendarId = -1;

            if (!long.TryParse(externalId, out calendarId))
            {
                return null;
            }

            return Task.Run(() =>
            {
                var cursor = Query(
                    ContentUris.WithAppendedId(_calendarsUri, calendarId),
                    _calendarsProjection);

                var calendar = SingleItemFromCursor(cursor, () => GetCalendar(cursor));

                return calendar;
            });
        }

        /// <summary>
        /// Gets all events for a calendar within the specified time range.
        /// </summary>
        /// <param name="calendar">Calendar containing events</param>
        /// <param name="start">Start of event range</param>
        /// <param name="end">End of event range</param>
        /// <returns>Calendar events</returns>
        /// <exception cref="ArgumentException">Calendar does not exist on device</exception>
        /// <exception cref="UnauthorizedAccessException">Calendar access denied</exception>
        /// <exception cref="Plugin.Calendars.Abstractions.PlatformException">Unexpected platform-specific error</exception>
        public static async Task<IList<CalendarEvent>> GetEventsAsync(Calendar calendar, DateTime start, DateTime end)
        {
            try
            {
                var deviceCalendar = await GetCalendarByIdAsync(calendar.ExternalID).ConfigureAwait(false);

                if (deviceCalendar == null)
                {
                    throw new ArgumentException("Specified calendar not found on device");
                }

                var eventsUriBuilder = CalendarContract.Instances.ContentUri.BuildUpon();

                // Note that this is slightly different from the GetEventById projection
                // due to the Instances API vs. Event API (specifically, IDs and start/end times)
                //
                string[] eventsProjection =
                {
                CalendarContract.Instances.EventId,
                CalendarContract.Events.InterfaceConsts.Title,
                CalendarContract.Events.InterfaceConsts.DisplayColor,
                CalendarContract.Events.InterfaceConsts.EventColor,
                CalendarContract.Events.InterfaceConsts.Description,
                CalendarContract.Instances.Begin,
                CalendarContract.Instances.End,
                CalendarContract.Events.InterfaceConsts.AllDay,
                CalendarContract.Events.InterfaceConsts.EventLocation
            };

                ContentUris.AppendId(eventsUriBuilder, DateConversions.GetDateAsAndroidMS(start));
                ContentUris.AppendId(eventsUriBuilder, DateConversions.GetDateAsAndroidMS(end));
                var eventsUri = eventsUriBuilder.Build();

                return await Task.Run(() =>
                {
                    try
                    {
                        var cursor = Query(eventsUri, eventsProjection,
                           string.Format("{0} = {1}", CalendarContract.Events.InterfaceConsts.CalendarId, calendar.ExternalID),
                           null, CalendarContract.Events.InterfaceConsts.Dtstart + " ASC");

                        var events = IterateCursor(cursor, () =>
                        {
                            try
                            {
                                bool allDay = cursor.GetBoolean(CalendarContract.Events.InterfaceConsts.AllDay);

                                var calendarEvent = new CalendarEvent
                                {
                                    ExternalID = cursor.GetString(CalendarContract.Instances.EventId),
                                    CalendarId = calendar.ExternalID,
                                    Title = cursor.GetString(CalendarContract.Events.InterfaceConsts.Title),
                                    DisplayColorString = string.Format("#{0:x8}", cursor.GetInt(CalendarContract.Events.InterfaceConsts.DisplayColor)),
                                    EventColorString = string.Format("#{0:x8}", cursor.GetInt(CalendarContract.Events.InterfaceConsts.EventColor)),
                                    Description = cursor.GetString(CalendarContract.Events.InterfaceConsts.Description),
                                    Start = cursor.GetDateTime(CalendarContract.Instances.Begin, allDay),
                                    End = cursor.GetDateTime(CalendarContract.Instances.End, allDay),
                                    Location = cursor.GetString(CalendarContract.Events.InterfaceConsts.EventLocation),
                                    //AccessLevel = cursor.GetInt(CalendarContract.Events.InterfaceConsts.AccessLevel),
                                    //tmp = cursor.GetString(CalendarContract.Events.InterfaceConsts.HasExtendedProperties), int mit Count?
                                    //tmp = "1" + cursor.GetString(CalendarContract.Events.InterfaceConsts.IsPrimary),

                                    AllDay = allDay
                                };
                                //calendarEvent.Reminders = GetEventReminders(calendarEvent.ExternalID); => Dauert zu lange!

                                return calendarEvent;
                            }
                            catch (Exception ex)
                            {
                                sys.LogException(ex);
                                int rnd = new Random().Next(8);
                                return new CalendarEvent()
                                {
                                    Start = DateTime.Today.AddHours(10 + rnd),
                                    End = DateTime.Today.AddHours(11 + rnd),
                                    Title = ex.Message
                                };
                            }
                        });

                        return events;
                    }
                    catch (Exception ex)
                    {
                        sys.LogException(ex);
                        return null;
                    }
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                sys.LogException(ex);
                return null;
            }
        }

        /// <summary>
        /// Gets a single calendar event by platform-specific ID.
        /// </summary>
        /// <param name="externalId">Platform-specific calendar event identifier</param>
        /// <returns>The corresponding calendar event, or null if not found</returns>
        /// <exception cref="UnauthorizedAccessException">Calendar access denied</exception>
        /// <exception cref="Plugin.Calendars.Abstractions.PlatformException">Unexpected platform-specific error</exception>
        public static Task<CalendarEvent> GetEventByIdAsync(string externalId)
        {
            return Task.Run(() => GetEventById(externalId));
        }

        /// <summary>
        /// Creates a new calendar or updates the name and color of an existing one.
        /// If a new calendar was created, the ExternalID property will be set on the Calendar object,
        /// to support future queries/updates.
        /// </summary>
        /// <param name="calendar">The calendar to create/update</param>
        /// <exception cref="ArgumentException">Calendar does not exist on device or is read-only</exception>
        /// <exception cref="UnauthorizedAccessException">Calendar access denied</exception>
        /// <exception cref="Plugin.Calendars.Abstractions.PlatformException">Unexpected platform-specific error</exception>
        public static async Task AddOrUpdateCalendarAsync(Calendar calendar)
        {
            bool updateExisting = false;
            long existingId = -1;

            if (long.TryParse(calendar.ExternalID, out existingId))
            {
                var existingCalendar = await GetCalendarByIdAsync(calendar.ExternalID).ConfigureAwait(false);

                if (existingCalendar != null)
                {
                    if (!existingCalendar.CanEditCalendar)
                    {
                        throw new ArgumentException("Destination calendar is not writeable");
                    }

                    updateExisting = true;
                }
                else
                {
                    throw new ArgumentException("Specified calendar does not exist on device", nameof(calendar));
                }
            }

            var values = new ContentValues();
            values.Put(CalendarContract.Calendars.InterfaceConsts.CalendarDisplayName, calendar.Name);
            values.Put(CalendarContract.Calendars.Name, calendar.Name);

            // Unlike iOS/WinPhone, Android does not automatically assign a color for us,
            // so we use our own default of blue.
            //
            int colorInt = unchecked((int)0xFF0000FF);

            if (!string.IsNullOrEmpty(calendar.Color))
            {
                int.TryParse(calendar.Color.Trim('#'), NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture, out colorInt);
            }

            values.Put(CalendarContract.Calendars.InterfaceConsts.CalendarColor, colorInt);

            if (!updateExisting)
            {
                values.Put(CalendarContract.Calendars.InterfaceConsts.CalendarAccessLevel, (int)CalendarAccess.AccessOwner);
                values.Put(CalendarContract.Calendars.InterfaceConsts.AccountName, AccountName);
                values.Put(CalendarContract.Calendars.InterfaceConsts.OwnerAccount, OwnerAccount);
                values.Put(CalendarContract.Calendars.InterfaceConsts.Visible, true);
                values.Put(CalendarContract.Calendars.InterfaceConsts.SyncEvents, true);

                values.Put(CalendarContract.Calendars.InterfaceConsts.AccountType, AccountType);
            }

            await Task.Run(() =>
            {
                if (updateExisting)
                {
                    Update(_calendarsUri, existingId, values);
                }
                else
                {
                    var uri = _calendarsUri.BuildUpon()
                        .AppendQueryParameter(CalendarContract.CallerIsSyncadapter, "true")
                        .AppendQueryParameter(CalendarContract.Calendars.InterfaceConsts.AccountName, AccountName)
                        .AppendQueryParameter(CalendarContract.Calendars.InterfaceConsts.AccountType, AccountType)
                        .Build();

                    calendar.ExternalID = Insert(uri, values);

                    calendar.CanEditCalendar = true;
                    calendar.CanEditEvents = true;
                    calendar.Color = "#" + colorInt.ToString("x8");
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Add new event to a calendar or update an existing event.
        /// If a new event was added, the ExternalID property will be set on the CalendarEvent object,
        /// to support future queries/updates.
        /// </summary>
        /// <param name="calendar">Destination calendar</param>
        /// <param name="calendarEvent">Event to add or update</param>
        /// <exception cref="ArgumentException">Calendar is not specified, does not exist on device, or is read-only</exception>
        /// <exception cref="UnauthorizedAccessException">Calendar access denied</exception>
        /// <exception cref="InvalidOperationException">Editing recurring events is not supported</exception>
        /// <exception cref="Plugin.Calendars.Abstractions.PlatformException">Unexpected platform-specific error</exception>
        public static async Task AddOrUpdateEventAsync(Calendar calendar, CalendarEvent calendarEvent)
        {
            if (string.IsNullOrEmpty(calendar.ExternalID))
            {
                throw new ArgumentException("Missing calendar identifier", nameof(calendar));
            }
            else
            {
                // Verify calendar exists (Android actually allows using a nonexistent calendar ID...)
                //
                var deviceCalendar = await GetCalendarByIdAsync(calendar.ExternalID).ConfigureAwait(false);

                if (deviceCalendar == null)
                {
                    throw new ArgumentException("Specified calendar not found on device");
                }
            }

            // Validate times
            if (calendarEvent.End < calendarEvent.Start)
            {
                throw new ArgumentException("End time may not precede start time", nameof(calendarEvent));
            }

            bool updateExisting = false;
            long existingId = -1;
            CalendarEvent existingEvent = null;

            await Task.Run(() =>
            {
                if (long.TryParse(calendarEvent.ExternalID, out existingId))
                {
                    if (IsEventRecurring(calendarEvent.ExternalID))
                    {
                        throw new InvalidOperationException("Editing recurring events is not supported");
                    }

                    var calendarId = GetCalendarIdForEventId(calendarEvent.ExternalID);

                    if (calendarId.HasValue && calendarId.Value.ToString() == calendar.ExternalID)
                    {
                        updateExisting = true;
                        existingEvent = GetEventById(calendarEvent.ExternalID);
                    }
                }

                bool allDay = calendarEvent.AllDay;
                var start = allDay
                    ? DateTime.SpecifyKind(calendarEvent.Start.Date, DateTimeKind.Utc)
                    : calendarEvent.Start;
                var end = allDay
                    ? DateTime.SpecifyKind(calendarEvent.End.Date, DateTimeKind.Utc)
                    : calendarEvent.End;

                var eventValues = new ContentValues();
                eventValues.Put(CalendarContract.Events.InterfaceConsts.CalendarId,
                    calendar.ExternalID);
                eventValues.Put(CalendarContract.Events.InterfaceConsts.Title,
                    calendarEvent.Title);
                eventValues.Put(CalendarContract.Events.InterfaceConsts.Description,
                    calendarEvent.Description);
                eventValues.Put(CalendarContract.Events.InterfaceConsts.Dtstart,
                    DateConversions.GetDateAsAndroidMS(start));
                eventValues.Put(CalendarContract.Events.InterfaceConsts.Dtend,
                    DateConversions.GetDateAsAndroidMS(end));
                eventValues.Put(CalendarContract.Events.InterfaceConsts.AllDay,
                                allDay ? 1 : 0);
                eventValues.Put(CalendarContract.Events.InterfaceConsts.EventLocation,
                    calendarEvent.Location ?? string.Empty);

                try
                {
                    if (!string.IsNullOrEmpty(calendarEvent.EventColorString) && calendarEvent.EventColor.A > 0)
                        eventValues.Put(CalendarContract.Events.InterfaceConsts.EventColor,
                            calendarEvent.EventColor.ToAndroid());
                }
                catch { }

                // If we're updating an existing event, don't mess with the existing
                // time zone (since we don't support explicitly setting it yet).
                // *Unless* we're toggling the "all day" setting
                // (because that would mean the time zone is UTC rather than local...).
                //
                if (!updateExisting || allDay != existingEvent?.AllDay)
                {
                    eventValues.Put(CalendarContract.Events.InterfaceConsts.EventTimezone,
                        allDay
                        ? Java.Util.TimeZone.GetTimeZone("UTC")?.ID ?? string.Empty
                        : Java.Util.TimeZone.Default.ID);
                }

                var operations = new List<ContentProviderOperation>();

                var eventOperation = updateExisting
                    ? ContentProviderOperation.NewUpdate(ContentUris.WithAppendedId(_eventsUri, existingId))
                    : ContentProviderOperation.NewInsert(_eventsUri);

                eventOperation = eventOperation.WithValues(eventValues);
                operations.Add(eventOperation.Build());

                operations.AddRange(BuildReminderUpdateOperations(calendarEvent.Reminders, existingEvent));

                try
                {
                    var results = ApplyBatch(operations);

                    if (!updateExisting)
                    {
                        calendarEvent.ExternalID = results[0].Uri.LastPathSegment;
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds an event reminder to specified calendar event
        /// </summary>
        /// <param name="calendarEvent">Event to add the reminder to</param>
        /// <param name="reminder">The reminder</param>
        /// <exception cref="ArgumentException">Calendar event is not created or not valid</exception>
        /// <exception cref="InvalidOperationException">Editing recurring events is not supported</exception>
        /// <exception cref="Plugin.Calendars.Abstractions.PlatformException">Unexpected platform-specific error</exception>
        public static async Task AddEventReminderAsync(CalendarEvent calendarEvent, CalendarEventReminder reminder)
        {
            if (string.IsNullOrEmpty(calendarEvent.ExternalID))
            {
                throw new ArgumentException("Missing calendar event identifier", nameof(calendarEvent));
            }

            // Verify calendar event exists 
            var existingAppt = await GetEventByIdAsync(calendarEvent.ExternalID).ConfigureAwait(false);

            if (existingAppt == null)
            {
                throw new ArgumentException("Specified calendar event not found on device");
            }

            await Task.Run(() =>
            {
                if (IsEventRecurring(calendarEvent.ExternalID))
                {
                    throw new InvalidOperationException("Editing recurring events is not supported");
                }

                Insert(_remindersUri, reminder.ToContentValues(calendarEvent.ExternalID));
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Removes a calendar and all its events from the system.
        /// </summary>
        /// <param name="calendar">Calendar to delete</param>
        /// <returns>True if successfully removed</returns>
        /// <exception cref="ArgumentException">Calendar is read-only</exception>
        /// <exception cref="UnauthorizedAccessException">Calendar access denied</exception>
        /// <exception cref="Plugin.Calendars.Abstractions.PlatformException">Unexpected platform-specific error</exception>
        public static async Task<bool> DeleteCalendarAsync(Calendar calendar)
        {
            var existing = await GetCalendarByIdAsync(calendar.ExternalID).ConfigureAwait(false);

            if (existing == null)
            {
                return false;
            }
            else if (!existing.CanEditCalendar)
            {
                throw new ArgumentException("Cannot delete calendar (probably because it's non-local)", nameof(calendar));
            }

            return await Task.Run(() => Delete(_calendarsUri, long.Parse(calendar.ExternalID))).ConfigureAwait(false);
        }

        /// <summary>
        /// Removes an event from the specified calendar.
        /// </summary>
        /// <param name="calendar">Calendar to remove event from</param>
        /// <param name="calendarEvent">Event to remove</param>
        /// <returns>True if successfully removed</returns>
        /// <exception cref="ArgumentException">Calendar is read-only</exception>
        /// <exception cref="UnauthorizedAccessException">Calendar access denied</exception>
        /// <exception cref="InvalidOperationException">Editing recurring events is not supported</exception>
        /// <exception cref="Plugin.Calendars.Abstractions.PlatformException">Unexpected platform-specific error</exception>
        public static async Task<bool> DeleteEventAsync(Calendar calendar, CalendarEvent calendarEvent)
        {
            long existingId = -1;

            // Even though a Calendar was passed-in, we get this to both verify
            // that the calendar exists and to make sure we have accurate permissions
            // (rather than trusting the permissions that were passed to us...)
            //
            var existingCal = await GetCalendarByIdAsync(calendar.ExternalID).ConfigureAwait(false);

            if (existingCal == null)
            {
                return false;
            }
            else if (!existingCal.CanEditEvents)
            {
                throw new ArgumentException("Cannot delete event from readonly calendar", nameof(calendar));
            }

            if (long.TryParse(calendarEvent.ExternalID, out existingId))
            {
                return await Task.Run(() =>
                {
                    if (IsEventRecurring(calendarEvent.ExternalID))
                    {
                        throw new InvalidOperationException("Editing recurring events is not supported");
                    }

                    var calendarId = GetCalendarIdForEventId(calendarEvent.ExternalID);

                    if (calendarId.HasValue && calendarId.Value.ToString() == calendar.ExternalID)
                    {
                        var eventsUri = CalendarContract.Events.ContentUri;
                        return Delete(eventsUri, existingId);
                    }
                    return false;
                }).ConfigureAwait(false);
            }

            return false;
        }

        #endregion

        #region Private Methods

        private static bool IsCalendarWriteable(int accessLevel)
        {
            switch ((CalendarAccess)accessLevel)
            {
                case CalendarAccess.AccessContributor:
                case CalendarAccess.AccessEditor:
                case CalendarAccess.AccessOwner:
                case CalendarAccess.AccessRoot:
                    return true;
                default:
                    return false;
            }
        }

        private static long? GetCalendarIdForEventId(string externalId)
        {
            string[] eventsProjection =
            {
                CalendarContract.Events.InterfaceConsts.CalendarId
            };

            var cursor = Query(
                ContentUris.WithAppendedId(_eventsUri, long.Parse(externalId)),
                eventsProjection);

            var calendarId = SingleItemFromCursor<long?>(cursor, () => cursor.GetLong(CalendarContract.Events.InterfaceConsts.CalendarId));

            return calendarId;
        }

        private static bool IsEventRecurring(string externalId)
        {
            string[] eventsProjection =
            {
                // There are a number of properties related to recurrence that
                // we could check. The Android docs state: "For non-recurring events,
                // you must include DTEND. For recurring events, you must include a
                // DURATION in addition to RRULE or RDATE." The API will also throw
                // an exception if you try to set both DTEND and DURATION on an
                // event. Thus, it seems reasonable to trust that DURATION will
                // only be present if the event is recurring.
                //
                CalendarContract.Events.InterfaceConsts.Duration
            };

            var cursor = Query(
                ContentUris.WithAppendedId(_eventsUri, long.Parse(externalId)),
                eventsProjection);

            bool isRecurring = SingleItemFromCursor(cursor,
                () => !string.IsNullOrEmpty(cursor.GetString(CalendarContract.Events.InterfaceConsts.Duration)));

            return isRecurring;
        }

        /// <summary>
        /// Gets a single calendar event by platform-specific ID.
        /// </summary>
        /// <param name="externalId">Platform-specific calendar event identifier</param>
        /// <returns>The corresponding calendar event, or null if not found</returns>
        /// <exception cref="UnauthorizedAccessException">Calendar access denied</exception>
        /// <exception cref="Plugin.Calendars.Abstractions.PlatformException">Unexpected platform-specific error</exception>
        private static CalendarEvent GetEventById(string externalId)
        {
            // Note that this is slightly different from the GetEvents projection
            // due to the Instances API vs Events API (specifically, IDs and start/end times)
            //
            string[] eventsProjection =
            {
                CalendarContract.Events.InterfaceConsts.Id,
                CalendarContract.Events.InterfaceConsts.CalendarId,
                CalendarContract.Events.InterfaceConsts.Title,
                CalendarContract.Events.InterfaceConsts.DisplayColor,
                CalendarContract.Events.InterfaceConsts.EventColor,
                CalendarContract.Events.InterfaceConsts.Description,
                CalendarContract.Events.InterfaceConsts.Dtstart,
                CalendarContract.Events.InterfaceConsts.Dtend,
                CalendarContract.Events.InterfaceConsts.EventLocation,
                CalendarContract.Events.InterfaceConsts.AllDay
            };

            var cursor = Query(
                ContentUris.WithAppendedId(_eventsUri, long.Parse(externalId)),
                eventsProjection);

            var calendarEvent = SingleItemFromCursor(cursor, () =>
            {
                bool allDay = cursor.GetBoolean(CalendarContract.Events.InterfaceConsts.AllDay);
                string externalID = cursor.GetString(CalendarContract.Events.InterfaceConsts.Id);

                var res = new CalendarEvent
                {
                    ExternalID = externalId,
                    CalendarId = cursor.GetString(CalendarContract.Events.InterfaceConsts.CalendarId),
                    Title = cursor.GetString(CalendarContract.Events.InterfaceConsts.Title),
                    DisplayColorString = cursor.GetString(CalendarContract.Events.InterfaceConsts.DisplayColor),
                    EventColorString = cursor.GetString(CalendarContract.Events.InterfaceConsts.EventColor),

                    Description = cursor.GetString(CalendarContract.Events.InterfaceConsts.Description),
                    Start = cursor.GetDateTime(CalendarContract.Events.InterfaceConsts.Dtstart, allDay),
                    End = cursor.GetDateTime(CalendarContract.Events.InterfaceConsts.Dtend, allDay),
                    Location = cursor.GetString(CalendarContract.Events.InterfaceConsts.EventLocation),
                    AllDay = allDay,
                    Reminders = GetEventReminders(externalID)
                };
                return res;
            });

            try
            {
                if (!string.IsNullOrEmpty(calendarEvent.CalendarColorString) && calendarEvent.CalendarColorString.StartsWith('-'))
                    calendarEvent.CalendarColorString = new Android.Graphics.Color(int.Parse(calendarEvent.CalendarColorString)).ToColor().ToHex();

                if (!string.IsNullOrEmpty(calendarEvent.DisplayColorString) && calendarEvent.DisplayColorString.StartsWith('-'))
                    calendarEvent.DisplayColorString = new Android.Graphics.Color(int.Parse(calendarEvent.DisplayColorString)).ToColor().ToHex();

                if (!string.IsNullOrEmpty(calendarEvent.EventColorString) && calendarEvent.EventColorString.StartsWith('-'))
                    calendarEvent.EventColorString = new Android.Graphics.Color(int.Parse(calendarEvent.EventColorString)).ToColor().ToHex();
            }
            catch (Exception ex)
            {
                xLog.Error(ex, "onParseColors");
            }

            return calendarEvent;
        }

        /// <summary>
        /// Get reminders for an event.
        /// Assumes that event existence/validity has already been verified.
        /// </summary>
        /// <param name="eventID">Event ID for which to retrieve reminders</param>
        /// <returns>Reminders</returns>
        private static IList<CalendarEventReminder> GetEventReminders(string eventID)
        {
            if (string.IsNullOrEmpty(eventID))
            {
                throw new ArgumentException("Missing calendar event identifier", nameof(eventID));
            }

            // Not bothering to verify that event exists because this is intended for internal use
            // so we should already know that it exists.

            string[] remindersProjection =
            {
                CalendarContract.Reminders.InterfaceConsts.Minutes,
                CalendarContract.Reminders.InterfaceConsts.Method
            };

            var cursor = Query(CalendarContract.Reminders.ContentUri, remindersProjection,
                $"{CalendarContract.Reminders.InterfaceConsts.EventId} = {eventID}",
                null, null);

            var reminders = IterateCursor(cursor, () => new CalendarEventReminder
            {
                TimeBefore = TimeSpan.FromMinutes(cursor.GetInt(CalendarContract.Reminders.InterfaceConsts.Minutes)),
                Method = ((RemindersMethod)cursor.GetInt(CalendarContract.Reminders.InterfaceConsts.Method)).ToCalendarReminderMethod()
            });

            return reminders.Count > 0 ? reminders : null;
        }

        /// <summary>
        /// Builds ContentProviderOperations for adding/removing reminders.
        /// Specifically intended for use by AddOrUpdateEvent, as if existingEvent is omitted,
        /// this requires that the operations are added to a batch in which the first operation
        /// inserts the event.
        /// </summary>
        /// <param name="reminders">New reminders (replacing existing reminders, if any)</param>
        /// <param name="existingEvent">(optional) Existing event to update reminders for</param>
        /// <returns>List of ContentProviderOperations for applying reminder updates as part of a batched update</returns>
        private static IList<ContentProviderOperation> BuildReminderUpdateOperations(IList<CalendarEventReminder> reminders, CalendarEvent existingEvent = null)
        {
            var operations = new List<ContentProviderOperation>();

            // If reminders haven't changed, do nothing
            //
            if (reminders == existingEvent?.Reminders ||
                (reminders != null && existingEvent?.Reminders != null && reminders.SequenceEqual(existingEvent.Reminders)))
            {
                return operations;
            }

            // Build operations that remove all existing reminders and add new ones

            if (existingEvent?.Reminders != null)
            {
                operations.AddRange(existingEvent.Reminders.Select(reminder =>
                    ContentProviderOperation.NewDelete(_remindersUri)
                                            .WithSelection($"{CalendarContract.Reminders.InterfaceConsts.EventId} = {existingEvent.ExternalID}", null)
                                            .Build()));
            }

            if (reminders != null)
            {
                if (existingEvent != null)
                {
                    operations.AddRange(reminders.Select(reminder =>
                        ContentProviderOperation.NewInsert(_remindersUri)
                                                .WithValues(reminder.ToContentValues(existingEvent.ExternalID))
                                                .Build()));
                }
                else
                {
                    // This assumes that these operations are being added to a batch in which the first operation
                    // is the event insertion operation
                    //
                    operations.AddRange(reminders.Select(reminder =>
                        ContentProviderOperation.NewInsert(_remindersUri)
                                                .WithValues(reminder.ToContentValues())
                                                .WithValueBackReference(CalendarContract.Reminders.InterfaceConsts.EventId, 0)
                                                .Build()));
                }
            }

            return operations;
        }

        private static Calendar GetCalendar(ICursor cursor)
        {
            var accessLevel = cursor.GetInt(CalendarContract.Calendars.InterfaceConsts.CalendarAccessLevel);
            var accountType = cursor.GetString(CalendarContract.Calendars.InterfaceConsts.AccountType);
            var colorInt = cursor.GetInt(CalendarContract.Calendars.InterfaceConsts.CalendarColor);
            var colorString = string.Format("#{0:x8}", colorInt);
            var visible = cursor.GetInt(CalendarContract.Calendars.InterfaceConsts.Visible);

            return new Calendar
            {
                Name = cursor.GetString(CalendarContract.Calendars.InterfaceConsts.CalendarDisplayName),
                ExternalID = cursor.GetString(CalendarContract.Calendars.InterfaceConsts.Id),
                CanEditCalendar = accountType == CalendarContract.AccountTypeLocal || accountType == AccountType,
                CanEditEvents = IsCalendarWriteable(accessLevel),
                Color = colorString,
                AccountName = cursor.GetString(CalendarContract.Calendars.InterfaceConsts.AccountName),
                AccountType = accountType,
                OwnerAccount = cursor.GetString(CalendarContract.Calendars.InterfaceConsts.OwnerAccount),
                IsPrimary = cursor.GetBoolean(cursor.ColumnCount - 1, true) //ToDo: Hardcoded Col-Index could be quite bad on changes!!!
            };
        }

        private static ICursor Query(Android.Net.Uri uri, string[] projection, string selection = null,
            string[] selectionArgs = null, string sortOrder = null)
        {
            try
            {
                return Application.Context.ContentResolver.Query(uri, projection, selection, selectionArgs, sortOrder);
            }
            catch (Java.Lang.Exception ex)
            {
                throw TranslateException(ex);
            }
        }

        /// <summary>
        /// Returns ID of new item
        /// </summary>
        private static string Insert(Android.Net.Uri uri, ContentValues values)
        {
            try
            {
                return Application.Context.ContentResolver.Insert(uri, values).LastPathSegment;
            }
            catch (Java.Lang.Exception ex)
            {
                throw TranslateException(ex);
            }
        }

        private static void Update(Android.Net.Uri uri, long id, ContentValues values)
        {
            try
            {
                Application.Context.ContentResolver.Update(ContentUris.WithAppendedId(uri, id), values, null, null);
            }
            catch (Java.Lang.Exception ex)
            {
                throw TranslateException(ex);
            }
        }

        private static bool Delete(Android.Net.Uri uri, long id)
        {
            try
            {
                return 0 < Application.Context.ContentResolver.Delete(ContentUris.WithAppendedId(uri, id), null, null);
            }
            catch (Java.Lang.Exception ex)
            {
                throw TranslateException(ex);
            }
        }

        private static ContentProviderResult[] ApplyBatch(IList<ContentProviderOperation> operations)
        {
            try
            {
                return Application.Context.ContentResolver.ApplyBatch(CalendarContract.Authority, operations);
            }
            catch (Java.Lang.Exception ex)
            {
                throw TranslateException(ex);
            }
        }

        private static Exception TranslateException(Java.Lang.Exception ex)
        {
            if (ex is Java.Lang.SecurityException)
            {
                return new UnauthorizedAccessException(ex.Message, ex);
            }
            else
            {
                return new PlatformException(ex.Message, ex);
            }
        }

        private static IList<T> IterateCursor<T>(ICursor cursor, Func<T> func)
        {
            var list = new List<T>();

            try
            {
                if (cursor.MoveToFirst())
                {
                    do
                    {
                        list.Add(func());
                    } while (cursor.MoveToNext());
                }
            }
            catch (Java.Lang.Exception ex)
            {
                throw new PlatformException(ex.Message, ex);
            }
            finally
            {
                cursor.Close();
            }

            return list;
        }

        private static T SingleItemFromCursor<T>(ICursor cursor, Func<T> func)
        {
            try
            {
                if (cursor.MoveToFirst())
                {
                    return func();
                }
            }
            catch (Java.Lang.Exception ex)
            {
                throw new PlatformException(ex.Message, ex);
            }
            finally
            {
                cursor.Close();
            }

            return default(T);
        }

        #endregion
    }
}
