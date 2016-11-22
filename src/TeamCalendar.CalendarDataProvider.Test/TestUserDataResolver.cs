using TeamCalendar.Data;

namespace TeamCalendar.CalendarDataProvider.Test
{
    public class TestUserDataResolver : ITeamCalendarUserDataResolver
    {
        public TestUserDataResolver()
        {
            DataProvider = new TestCalendarDataProvider();
        }

        private TestCalendarDataProvider DataProvider { get; }

        public string ResolveEmailByUsername(string username)
        {
            return DataProvider.ResolveEmailByUsername(username);
        }

        public string ResolveAccountDisplayNameByEmail(string email)
        {
            return DataProvider.ResolveUserNameByEmail(email);
        }
    }
}