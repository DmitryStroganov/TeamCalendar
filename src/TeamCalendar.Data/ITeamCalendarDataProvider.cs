using System;
using System.Collections.Generic;

namespace TeamCalendar.Data
{
    public interface ITeamCalendarDataProvider
    {
        void Initialize(CalendarServiceConfig serviceConfig);
        IEnumerable<CalendarInfo> GetCalendarInfo(string teamAccount, DateTime eventsStartDate, DateTime eventsEndDate);
    }
}