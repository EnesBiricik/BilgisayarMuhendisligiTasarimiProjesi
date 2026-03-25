using System.ComponentModel.DataAnnotations;
using BilgisayarMuhendisligiTasarimi.Data.Entities;
using Microsoft.AspNetCore.Http;

namespace BilgisayarMuhendisligiTasarimi.Models
{
    public class CreateTaskItemModel
    {
        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Task Type is required.")]
        [Display(Name = "Task Type")]
        public TaskTypeEnum TaskType { get; set; }

        [Display(Name = "Build Document")]
        public IFormFile? BuildDocument { get; set; }

        [Required(ErrorMessage = "Studio is required.")]
        public int StudioId { get; set; }

        [Display(Name = "Assign To")]
        public int? AssignedUserId { get; set; }

        public TaskPriorityEnum Priority { get; set; }
    }

    public class UpdateTaskItemModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Task Status")]
        public TaskStatusEnum? TaskStatus { get; set; }

        [Display(Name = "Build Document")]
        public IFormFile? BuildDocument { get; set; }
        
        [Display(Name = "Assign To")]
        public int? AssignedUserId { get; set; }

        public TaskPriorityEnum? Priority { get; set; }
    }

    public class TaskItemListViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Display(Name = "Type")]
        public TaskTypeEnum TaskType { get; set; }
        
        [Display(Name = "Status")]
        public TaskStatusEnum TaskStatus { get; set; }
        
        [Display(Name = "Assigned To")]
        public string AssignedUserName { get; set; }
        
        [Display(Name = "Created Date")]
        public DateTime CreateDate { get; set; }

        public TaskPriorityEnum Priority { get; set; }
    }

    public class TaskItemDetailViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Title")]
        public string Title { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Type")]
        public TaskTypeEnum TaskType { get; set; }

        [Display(Name = "Status")]
        public TaskStatusEnum TaskStatus { get; set; }

        [Display(Name = "Build Document")]
        public string? BuildDocumentPath { get; set; }

        public int StudioId { get; set; }
        public string StudioName { get; set; }

        public int CreatorUserId { get; set; }
        public string CreatorUserName { get; set; }

        public int? AssignedUserId { get; set; }
        public string AssignedUserName { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreateDate { get; set; }
        
        [Display(Name = "Last Updated")]
        public DateTime? UpdateDate { get; set; }

        public TaskPriorityEnum Priority { get; set; }
    }
}
