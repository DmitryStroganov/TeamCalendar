using System;

namespace TeamCalendar.Common.UI
{
    [Serializable]
    public class CalendarEvent
    {
        [NonSerialized] public CalendarColumn Column;

        public DateTime End;
        public string LegacyFreeBusyStatus;
        public string Location;
        public string Name;
        public string OrganizerName;
        public string PK;
        public string ResourceEmail;
        public DateTime Start;

        public DateTime BoxStart
        {
            get
            {
                if (Start.Minute >= 30)
                {
                    return new DateTime(Start.Year, Start.Month, Start.Day, Start.Hour, 30, 0);
                }

                return new DateTime(Start.Year, Start.Month, Start.Day, Start.Hour, 0, 0);
            }
        }

        public DateTime BoxEnd
        {
            get
            {
                if (End.Minute > 30)
                {
                    var hourPlus = End.AddHours(1);
                    return new DateTime(hourPlus.Year, hourPlus.Month, hourPlus.Day, hourPlus.Hour, 0, 0);
                }

                if (End.Minute > 0)
                {
                    return new DateTime(End.Year, End.Month, End.Day, End.Hour, 30, 0);
                }

                return new DateTime(End.Year, End.Month, End.Day, End.Hour, 0, 0);
            }
        }

        public bool OverlapsWith(CalendarEvent e)
        {
            return (Start < e.End) && (End > e.Start);
        }
    }
}