using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace TeamCalendar.Common
{
    public static class AdExtensions
    {
        public static string GetProperty(this Principal principal, string property)
        {
            var directoryEntry = principal.GetUnderlyingObject() as DirectoryEntry;

            if (directoryEntry.Properties.Contains(property))
            {
                return directoryEntry.Properties[property].Value.ToString();
            }
            return string.Empty;
        }

        public static string GetCompany(this Principal principal)
        {
            return GetProperty(principal, "company");
        }

        public static string GetDepartment(this Principal principal)
        {
            return GetProperty(principal, "department");
        }
    }
}