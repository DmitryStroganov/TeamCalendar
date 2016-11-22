using System;

namespace TeamCalendar.Data
{
    /// <summary>
    ///     The CalendarInfoSearchCriteria class represents search critieria for getting Exchange Calendar events.
    /// </summary>
    public class CalendarInfoSearchCriteria
    {
        /// <summary>
        ///     Gets or sets the email address to get items for.
        /// </summary>
        public string EmailAddress { get; set; }


        /// <summary>
        ///     Gets or sets the maximum number of items to return. Default is 0 which returns all items.
        /// </summary>
        public int MaxItemsToReturn { get; set; }

        /// <summary>
        ///     Gets or sets the start date and time.
        /// </summary>
        public DateTime StartDateAndTime { get; set; }

        /// <summary>
        ///     Gets or sets the end date and time.
        /// </summary>
        public DateTime EndDateAndTime { get; set; }

        /// <summary>
        ///     Gets or sets a value that indicates whether or not to populate the calendar body.
        /// </summary>
        public bool PopulateCalendarBody { get; set; }
    }
}