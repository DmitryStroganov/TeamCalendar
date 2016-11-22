namespace TeamCalendar.Data
{
    public interface ITeamCalendarUserDataResolver
    {
        string ResolveEmailByUsername(string username);
        string ResolveAccountDisplayNameByEmail(string email);
    }
}