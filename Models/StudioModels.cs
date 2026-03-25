using System.ComponentModel.DataAnnotations;
using BilgisayarMuhendisligiTasarimi.Data.Entities;

namespace BilgisayarMuhendisligiTasarimi.Models
{
    public class CreateStudioModel
    {
        [Required(ErrorMessage = "Studio name is required.")]
        [MaxLength(100, ErrorMessage = "Studio name cannot exceed 100 characters.")]
        [Display(Name = "Studio Name")]
        public string Name { get; set; }
    }

    public class UpdateStudioModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Studio name is required.")]
        [MaxLength(100, ErrorMessage = "Studio name cannot exceed 100 characters.")]
        [Display(Name = "Studio Name")]
        public string Name { get; set; }
    }

    public class StudioListViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Studio Name")]
        public string Name { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreateDate { get; set; }

        public int UserCount { get; set; }
        public int TaskCount { get; set; }
    }

    public class StudioDetailViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Studio Name")]
        public string Name { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreateDate { get; set; }

        public List<UserListViewModel> Users { get; set; } = new();
        public List<TaskItemListViewModel> ActiveTasks { get; set; } = new();
    }
}
