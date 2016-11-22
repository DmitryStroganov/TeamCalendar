using System;

namespace TeamCalendar.Data
{
    public class CalendarInfo
    {
        public enum CalendarAppointmentType
        {
            Single,
            Occurrence,
            Exception,
            RecurringMaster
        }

        /// <summary>
        ///     Mark the event as an ALL Day Event.
        /// </summary>
        public bool AllDayEvent;

        public CalendarAppointmentType AppointmentType;
        public string Description;
        public DateTime EndTime;

        /// <summary>
        ///     Unique identifier assigned be MS exchange server for the calendar event.
        /// </summary>
        public string EventID;

        /// <summary>
        ///     Location of the calendar event.
        /// </summary>
        public string Location;

        public string OrganizerName;
        public string ResourceEmail;

        /// <summary>
        ///     Indicates Level of Sensitivity  (“Private” indicates private check box was ticked, blank makes it a public event)
        /// </summary>
        public string Sensitivity;

        public DateTime StartTime;

        /// <summary>
        ///     OOF, Busy, Free, Tentative Status for the calendar event.
        /// </summary>
        public string Status;

        /// <summary>
        ///     Subject line of the calendar event.
        /// </summary>
        public string Subject;
    }
}