using System;
using System.Collections.Generic;

namespace TeamCalendar.Common.UI
{
    internal class CalendarBlock
    {
        private readonly List<CalendarEvent> calendarEvents = new List<CalendarEvent>();
        internal List<CalendarColumn> Columns;

        internal DateTime BoxStart
        {
            get
            {
                var min = DateTime.MaxValue;

                foreach (var ev in calendarEvents)
                {
                    if (ev.BoxStart < min)
                    {
                        min = ev.BoxStart;
                    }
                }

                return min;
            }
        }

        internal DateTime BoxEnd
        {
            get
            {
                var max = DateTime.MinValue;

                foreach (var ev in calendarEvents)
                {
                    if (ev.BoxEnd > max)
                    {
                        max = ev.BoxEnd;
                    }
                }

                return max;
            }
        }


        internal void Add(CalendarEvent ev)
        {
            calendarEvents.Add(ev);
            arrangeColumns();
        }

        private CalendarColumn createColumn()
        {
            var col = new CalendarColumn();
            Columns.Add(col);
            col.Block = this;

            return col;
        }


        private void arrangeColumns()
        {
            Columns = new List<CalendarColumn>();

            foreach (var e in calendarEvents)
            {
                e.Column = null;
            }

            createColumn();

            foreach (var e in calendarEvents)
            {
                foreach (var col in Columns)
                {
                    if (col.CanAdd(e))
                    {
                        col.Add(e);
                        break;
                    }
                }
                if (e.Column == null)
                {
                    var col = createColumn();
                    col.Add(e);
                }
            }
        }

        internal bool OverlapsWith(CalendarEvent e)
        {
            if (calendarEvents.Count == 0)
            {
                return false;
            }

            return (BoxStart < e.BoxEnd) && (BoxEnd > e.BoxStart);
        }
    }
}