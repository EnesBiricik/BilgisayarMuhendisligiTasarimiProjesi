using BilgisayarMuhendisligiTasarimi.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace BilgisayarMuhendisligiTasarimi.Models
{
    public class PanelIndexViewModel
    {
        [Display(Name = "Depolama Alanı (MB)")]
        public int StorageUsage { get; set; }

        [Display(Name = "Toplam Alan")]
        public int TotalStorageGB { get; set; } = 1; // Varsayılan 10GB

        [Display(Name = "Resimler (MB)")]
        public int ImagesStorageMB { get; set; }

        [Display(Name = "Dosyalar (MB)")]
        public int FilesStorageMB { get; set; }

        [Display(Name = "Medya (MB)")]
        public int MediaStorageMB { get; set; }

        [Display(Name = "Diğer (MB)")]
        public int OtherStorageMB { get; set; }

        public int TotalUsedStorageMB => ImagesStorageMB + FilesStorageMB + MediaStorageMB + OtherStorageMB;

        public double UsagePercentage => (TotalUsedStorageMB * 100.0) / (TotalStorageGB * 1024);
        public List<ActivityLog> RecentActivities { get; set; }

        // Core Task Management
        public string StudioName { get; set; }
        public List<TaskItem> RecentTasks { get; set; }
        public List<TaskItem> AllTasks { get; set; }
        public List<User> StudioWorkers { get; set; }
    }
}