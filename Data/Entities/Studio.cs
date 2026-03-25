using System.Collections.Generic;

namespace BilgisayarMuhendisligiTasarimi.Data.Entities
{
    public class Studio : BaseEntity
    {
        public string Name { get; set; }
        public int? OwnerUserId { get; set; }
        public ICollection<User> Users { get; set; }
        public ICollection<TaskItem> TaskItems { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
