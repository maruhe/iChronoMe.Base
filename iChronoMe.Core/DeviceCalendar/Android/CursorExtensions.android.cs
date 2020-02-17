using System;

using Android.Database;
using iChronoMe.Core.Classes;

namespace iChronoMe.DeviceCalendar
{
    /// <summary>
    /// Cursor extensions.
    /// </summary>
    internal static class CursorExtensions
    {
        /// <summary>
        /// Returns the value of the requested column as a string.
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="cursor">Cursor.</param>
        /// <param name="column">Column name.</param>
        public static string GetString(this ICursor cursor, string column)
        {
            return cursor.GetString(cursor.GetColumnIndex(column));
        }

        /// <summary>
        /// Returns the value of the requested column as an int.
        /// </summary>
        /// <returns>The int.</returns>
        /// <param name="cursor">Cursor.</param>
        /// <param name="column">Column name.</param>
        public static int GetInt(this ICursor cursor, string column)
        {
            return cursor.GetInt(cursor.GetColumnIndex(column));
        }

        /// <summary>
        /// Returns the value of the requested column as a long.
        /// </summary>
        /// <returns>The long.</returns>
        /// <param name="cursor">Cursor.</param>
        /// <param name="column">Column name.</param>
        public static long GetLong(this ICursor cursor, string column)
        {
            return cursor.GetLong(cursor.GetColumnIndex(column));
        }

        /// <summary>
        /// Returns the value of the requested column as a DateTime
        /// </summary>
        /// <returns>The DateTime.</returns>
        /// <param name="cursor">Cursor.</param>
        /// <param name="column">Column name.</param>
        /// <param name="allDay">Whether the event is all-day</param>
        public static DateTime GetDateTime(this ICursor cursor, string column, bool allDay)
        {
            var ms = cursor.GetLong(cursor.GetColumnIndex(column));

            var dt = DateConversions.GetDateFromAndroidMS(ms);

            // All day events should not be affected by time zones, so we simply take the
            // UTC time and treat it as local (e.g., Christmas doesn't start on the 24th in California...)
            //
            return allDay ? DateTime.SpecifyKind(dt.ToUniversalTime(), DateTimeKind.Local) : dt.ToLocalTime();
        }

        /// <summary>
        /// Returns the value of the requested column as a boolean.
        /// </summary>
        /// <returns>The boolean.</returns>
        /// <param name="cursor">Cursor.</param>
        /// <param name="column">Column name.</param>
        public static bool GetBoolean(this ICursor cursor, string column, bool bDefault = false)
        {
            try
            {
                return cursor.GetInt(column) != 0; // cursor.GetColumnIndex(
            } 
            catch(Exception ex)
            {
                string cAll = "";
                foreach (var c in cursor.GetColumnNames())
                    cAll += c + ", ";
                sys.LogException(ex, "columnname: " + column + ", all: " + cAll);
                return bDefault;
            }
        }
    }
}