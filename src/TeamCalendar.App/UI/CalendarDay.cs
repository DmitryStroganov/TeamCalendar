using System;
using System.Collections.Generic;

namespace TeamCalendar.Common.UI
{
    internal class CalendarDay
    {
        private readonly List<CalendarBlock> calendarBlocks = new List<CalendarBlock>();
        internal DateTime EndDate;
        internal List<CalendarEvent> Events = new List<CalendarEvent>();
        internal string ResourceEmail;

        internal string ResourceName;

        internal DateTime StartDate;

        private CalendarBlock LastBlock
        {
            get
            {
                if (calendarBlocks.Count == 0)
                {
                    return null;
                }
                return calendarBlocks[calendarBlocks.Count - 1];
            }
        }

        public DateTime BoxStart
        {
            get
            {
                var min = DateTime.MaxValue;

                foreach (var block in calendarBlocks)
                {
                    if (block.BoxStart < min)
                    {
                        min = block.BoxStart;
                    }
                }

                return min;
            }
        }

        public DateTime BoxEnd
        {
            get
            {
                var max = DateTime.MinValue;

                foreach (var block in calendarBlocks)
                {
                    if (block.BoxEnd > max)
                    {
                        max = block.BoxEnd;
                    }
                }

                return max;
            }
        }

        public bool IsRoom { get; set; }

        public void LoadEventData(List<CalendarEvent> events)
        {
            if (events == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(ResourceEmail))
            {
                StripAndAddEvents(events.FindAll(e => (e.ResourceEmail != null) && e.ResourceEmail.Equals(ResourceEmail)));
            }

            PutIntoBlocks();
        }

        private void StripAndAddEvents(List<CalendarEvent> eventsCollection)
        {
            if (eventsCollection == null)
            {
                return;
            }

            foreach (var e in eventsCollection)
            {
                if (e.End <= StartDate)
                {
                    return;
                }

                if (e.Start >= EndDate)
                {
                    return;
                }

                if (e.Start >= e.End)
                {
                    return;
                }

                if (e.Start < StartDate)
                {
                    e.Start = StartDate;
                }

                if (e.End > EndDate)
                {
                    e.End = EndDate;
                }

                Events.Add(e);
            }
        }

        private void PutIntoBlocks()
        {
            foreach (var e in Events)
            {
                if (LastBlock == null)
                {
                    calendarBlocks.Add(new CalendarBlock());
                }
                else if (!LastBlock.OverlapsWith(e))
                {
                    calendarBlocks.Add(new CalendarBlock());
                }

                LastBlock.Add(e);
            }
        }

        internal int MaxColumns()
        {
            var i = 1;
            foreach (var b in calendarBlocks)
            {
                if (b.Columns.Count > i)
                {
                    i = b.Columns.Count;
                }
            }
            return i;
        }
    }
}