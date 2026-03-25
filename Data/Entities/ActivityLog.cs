using System;

namespace BilgisayarMuhendisligiTasarimi.Data.Entities
{
    public class ActivityLog
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string IpAddress { get; set; }
        public DateTime LoginDate { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
    }
} 