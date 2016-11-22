using System.Runtime.Serialization;

namespace TeamCalendar.Data
{
    public class ServerApiControllerConfig
    {
        [DataContract]
        public class SetDateConfig
        {
            [DataMember]
            public int DateShift { get; set; }

            [DataMember]
            public string ApiKey { get; set; }
        }
    }
}