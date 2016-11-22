using System;
using System.Collections.Generic;

namespace TeamCalendar.Data
{
    [Serializable]
    public class AdUserData
    {
        public KeyValuePair<string, string>[] AdProps;
        public string Company;
        public string Department;
        public string Email;
        public string FirstName;
        public string JobTitle;
        public string LastName;
        public string Office;
    }
}