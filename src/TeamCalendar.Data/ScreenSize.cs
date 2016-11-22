using System.Runtime.Serialization;

namespace TeamCalendar.Data
{
    [DataContract]
    public class ScreenSize
    {
        [DataMember]
        public int Width { get; set; }

        [DataMember]
        public int Height { get; set; }

        public bool IsEmpty => (Width == 0) && (Height == 0);
    }
}