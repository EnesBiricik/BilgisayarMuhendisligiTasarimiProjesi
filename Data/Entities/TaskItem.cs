using System.ComponentModel.DataAnnotations.Schema;

namespace BilgisayarMuhendisligiTasarimi.Data.Entities
{
    public class TaskItem : BaseEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public TaskTypeEnum TaskType { get; set; }
        public TaskStatusEnum TaskStatus { get; set; }
        public List<string> Attachments { get; set; } = new();

        public int StudioId { get; set; }
        public Studio Studio { get; set; }

        public int CreatorUserId { get; set; }
        public User CreatorUser { get; set; }

        public TaskPriorityEnum Priority { get; set; }
        public int? AssignedUserId { get; set; }
        public User AssignedUser { get; set; }
        public bool IsActive { get; set; }
    }
}
