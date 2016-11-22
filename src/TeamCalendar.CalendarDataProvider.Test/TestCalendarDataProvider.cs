using System;
using System.Collections.Generic;
using System.Linq;
using TeamCalendar.Data;

namespace TeamCalendar.CalendarDataProvider
{
    public class TestCalendarDataProvider : ITeamCalendarDataProvider
    {
        public TestCalendarDataProvider() :
            this(
                new Dictionary<string, string>
                {
                    {"dm@devcell.com", "Dmitry Stroganov"},
                    {"RR@devcell.com", "Rasmus Rasmussen"},
                    {"SS@devcell.com", "Søren Sørensen"},
                    {"JS@devcell.com", "John Smith"},
                    {"Room01@meetings.dk", "Room01"}
                },
                new List<CalendarInfo>
                {
                    new CalendarInfo
                    {
                        ResourceEmail = "dm@devcell.com",
                        Subject = "On Duty",
                        Location = "Support room",
                        AppointmentType = Data.CalendarInfo.CalendarAppointmentType.Single,
                        StartTime = DateTime.UtcNow.Date.AddHours(9),
                        EndTime = DateTime.UtcNow.Date.AddHours(15)
                    },
                    new CalendarInfo
                    {
                        ResourceEmail = "RR@devcell.com",
                        Subject = "Brainstorm",
                        Location = "Standby room",
                        AppointmentType = Data.CalendarInfo.CalendarAppointmentType.Single,
                        StartTime = DateTime.UtcNow.Date.AddHours(10),
                        EndTime = DateTime.UtcNow.Date.AddHours(11)
                    },
                    new CalendarInfo
                    {
                        ResourceEmail = "SS@devcell.com",
                        Subject = "Private",
                        Location = "Home",
                        AppointmentType = Data.CalendarInfo.CalendarAppointmentType.Single,
                        Status = "OOF",
                        Sensitivity = "Private",
                        StartTime = DateTime.UtcNow.Date.AddHours(09),
                        EndTime = DateTime.UtcNow.Date.AddHours(10)
                    },
                    new CalendarInfo
                    {
                        ResourceEmail = "JS@devcell.com",
                        Subject = "Board status meeting",
                        Location = "Board room",
                        AppointmentType = Data.CalendarInfo.CalendarAppointmentType.Single,
                        Status = "Busy",
                        StartTime = DateTime.UtcNow.Date.AddHours(13),
                        EndTime = DateTime.UtcNow.Date.AddHours(14)
                    },
                    new CalendarInfo
                    {
                        ResourceEmail = "Room01",
                        Subject = "UK team meeting",
                        AppointmentType = Data.CalendarInfo.CalendarAppointmentType.Single,
                        StartTime = DateTime.UtcNow.Date.AddHours(11),
                        EndTime = DateTime.UtcNow.Date.AddHours(12)
                    }
                }
            )
        {
        }

        public TestCalendarDataProvider(Dictionary<string, string> userAliasMap, IEnumerable<CalendarInfo> calendarInfo)
        {
            UserAliasMap = userAliasMap;
            CalendarInfo = calendarInfo;
        }

        private Dictionary<string, string> UserAliasMap { get; }
        private IEnumerable<CalendarInfo> CalendarInfo { get; }

        public void Initialize(CalendarServiceConfig serviceConfig)
        {
        }

        public IEnumerable<CalendarInfo> GetCalendarInfo(string teamAccount, DateTime eventsStartDate,
            DateTime eventsEndDate)
        {
            return CalendarInfo.Where(item => item.ResourceEmail == teamAccount);
        }

        public string ResolveEmailByUsername(string username)
        {
            var result = UserAliasMap.FirstOrDefault(kvp => kvp.Value == username);
            return result.Key;
        }

        public string ResolveUserNameByEmail(string email)
        {
            if (!UserAliasMap.ContainsKey(email))
            {
                throw new InvalidOperationException();
            }

            return UserAliasMap[email];
        }
    }
}