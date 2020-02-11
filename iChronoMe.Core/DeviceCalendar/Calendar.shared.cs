namespace iChronoMe.DeviceCalendar
{
    /// <summary>
    /// Device calendar abstraction
    /// </summary>
    public class Calendar
    {
        public Calendar() { }
        public Calendar(string id)
        {
            ExternalID = id;
        }

        /// <summary>
        /// Calendar display name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Calendar display color, as a string in hex notation
        /// </summary>
        /// <remarks>Cannot be changed on WinPhone</remarks>
        public string Color { get; set; }

        /// <summary>
        /// Platform-specific unique calendar identifier
        /// </summary>
        public string ExternalID { get; set; }

        /// <summary>
        /// Whether or not the calendar itself (name/color) can be edited
        /// </summary>
        public bool CanEditCalendar { get; set; }

        /// <summary>
        /// Whether or not events can be created/edited/deleted for the calendar
        /// </summary>
        public bool CanEditEvents { get; set; }

        /// <summary>
        /// Display name of associated calendar account
        /// </summary>
        public string AccountName { get; set; }

        public string OwnerAccount { get; set; }

        public bool IsPrimary { get; set; }
    }
}
