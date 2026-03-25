using System.ComponentModel.DataAnnotations;

namespace BilgisayarMuhendisligiTasarimi.Data.Entities
{
    public class User : BaseEntity
    {        
        public string Name { get; set; }
        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public int RoleId { get; set; }
        public Role Role { get; set; }

        public int FailedLoginCount { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string? LastLoginIp { get; set; }

        public int? StudioId { get; set; }
        public Studio Studio { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
