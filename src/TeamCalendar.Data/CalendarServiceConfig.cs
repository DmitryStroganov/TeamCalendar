using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace TeamCalendar.Data
{
    [DataContract]
    [Serializable]
    public class CalendarServiceConfig
    {
        public int TimeZone { get; set; }
        public CultureInfo UiCulture { get; set; }
        public string TeamList { get; set; }
        public string RoomKeywords { get; set; }
        public string ExchangeUrl { get; set; }
        public string AccountUser { get; set; }
        public string AccountPassword { get; set; }
        public string AccountDomain { get; set; }
    }
}