using System.ComponentModel.DataAnnotations.Schema;

namespace BilgisayarMuhendisligiTasarimi.Data.Entities
{
    public class Notification : BaseEntity
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; }
        public int? RelatedTaskId { get; set; }
        public bool IsActive { get; set; }
    }
}
