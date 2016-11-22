using System;

namespace TeamCalendar.Data
{
    public class CalendarGridConfig
    {
        public bool HoverBoxEnabled { get; set; }

        public bool DateShiftEnabled { get; set; }

        public int MaxDaysShift { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int Days { get; set; }

        public virtual int CellWidth { get; set; }

        public int CellDuration { get; set; }

        public string HourNameBackColor { get; set; }

        public string HourNameBorderColor { get; set; }

        public string EventBorderColor { get; set; }

        public string EventBackColor { get; set; }

        public string EventBackColor_OOF { get; set; }

        public string EventBackColor_Tentative { get; set; }

        public string EventBackColor_Free { get; set; }

        public string BackgroundColor { get; set; }

        public string BackgroundColorAlt { get; set; }

        public int BusinessBeginsHour { get; set; }

        public int BusinessEndsHour { get; set; }

        public string HeaderFontFamily { get; set; }

        public string HeaderFontColor { get; set; }

        public string HeaderFontSize { get; set; }

        public string RoomHeaderFontColor { get; set; }

        public string RoomHeaderHeaderFontSize { get; set; }

        public string RoomHeaderBorderColor { get; set; }

        public string HourFontFamily { get; set; }

        public string HourFontSize { get; set; }

        public string EventFontFamily { get; set; }

        public string EventFontSize { get; set; }

        public int EventHeight { get; set; }

        public int HeaderHeight { get; set; }

        public string NonBusinessBackColor { get; set; }

        public string HourBorderColor { get; set; }

        public string BorderColor { get; set; }

        public int Width { get; set; }

        public NonBusinessHoursBehavior NonBusinessHours { get; set; }

        public int RowHeaderWidth { get; set; }

        public bool DurationBarVisible { get; set; }

        public string HoverColor { get; set; }

        public string RoomKeywords { get; set; }

        public int TimeZone { get; set; }

        public int MaxEventTextLenght { get; set; }
    }
}