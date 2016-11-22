using System;
using System.Collections.Generic;

namespace TeamCalendar.Common.UI
{
    public class CalendarColumn
    {
        private readonly List<CalendarEvent> calendarEvents = new List<CalendarEvent>();
        internal CalendarBlock Block;

        internal CalendarColumn()
        {
        }

        public int WidthPct
        {
            get
            {
                if (Block == null)
                {
                    throw new ApplicationException("This Column does not belong to any Block.");
                }

                if (Block.Columns.Count == 0)
                {
                    throw new ApplicationException("Invalid Block.Column.Counts.");
                }

                if (isLastInBlock)
                {
                    return 100/Block.Columns.Count + 100%Block.Columns.Count;
                }
                return 100/Block.Columns.Count;
            }
        }

        public int StartsAtPct
        {
            get
            {
                if (Block == null)
                {
                    throw new ApplicationException("This Column does not belong to any Block.");
                }

                if (Block.Columns.Count == 0)
                {
                    throw new ApplicationException("Invalid Block.Column.Counts.");
                }

                return 100/Block.Columns.Count*Number;
            }
        }

        private bool isLastInBlock
        {
            get { return Block.Columns[Block.Columns.Count - 1] == this; }
        }

        public int Number
        {
            get
            {
                if (Block == null)
                {
                    throw new ApplicationException("This Column doesn't belong to any Block.");
                }

                return Block.Columns.IndexOf(this);
            }
        }

        internal bool CanAdd(CalendarEvent e)
        {
            foreach (var ev in calendarEvents)
            {
                if (ev.OverlapsWith(e))
                {
                    return false;
                }
            }
            return true;
        }

        internal void Add(CalendarEvent e)
        {
            if (e.Column != null)
            {
                throw new ApplicationException("This Event was already placed into a Column.");
            }

            calendarEvents.Add(e);
            e.Column = this;
        }
    }
}