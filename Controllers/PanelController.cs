using BilgisayarMuhendisligiTasarimi.Data.Context;
using BilgisayarMuhendisligiTasarimi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using BilgisayarMuhendisligiTasarimi.Data.Entities;
using BilgisayarMuhendisligiTasarimi.Services;

namespace BilgisayarMuhendisligiTasarimi.Controllers
{
    [Authorize]
    public class PanelController : Controller
    {
        private readonly DBContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PanelController(DBContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        private (int images, int files, int media, int other) CalculateStorageUsage()
        {
            try
            {
                string wwwrootPath = _webHostEnvironment.WebRootPath;
                if (!Directory.Exists(wwwrootPath))
                    return (0, 0, 0, 0);

                
                var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var imageSize = Directory.GetFiles(wwwrootPath, "*.*", SearchOption.AllDirectories)
                    .Where(f => imageExtensions.Contains(Path.GetExtension(f).ToLower()))
                    .Sum(f => new FileInfo(f).Length);

                
                var mediaExtensions = new[] { ".mp4", ".mp3", ".wav", ".avi" };
                var mediaSize = Directory.GetFiles(wwwrootPath, "*.*", SearchOption.AllDirectories)
                    .Where(f => mediaExtensions.Contains(Path.GetExtension(f).ToLower()))
                    .Sum(f => new FileInfo(f).Length);

                
                var fileExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt" };
                var fileSize = Directory.GetFiles(wwwrootPath, "*.*", SearchOption.AllDirectories)
                    .Where(f => fileExtensions.Contains(Path.GetExtension(f).ToLower()))
                    .Sum(f => new FileInfo(f).Length);

                
                var totalSize = Directory.GetFiles(wwwrootPath, "*.*", SearchOption.AllDirectories)
                    .Sum(f => new FileInfo(f).Length);
                var otherSize = totalSize - (imageSize + mediaSize + fileSize);

                return (
                    (int)(imageSize / (1024 * 1024)), 
                    (int)(fileSize / (1024 * 1024)),
                    (int)(mediaSize / (1024 * 1024)),
                    (int)(otherSize / (1024 * 1024))
                );
            }
            catch (Exception)
            {
                return (0, 0, 0, 0);
            }
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var storage = CalculateStorageUsage();
                
                
                
                var recentTasks = await _context.TaskItems
                    .Include(t => t.AssignedUser)
                    .OrderByDescending(t => t.CreateDate)
                    .Take(10)
                    .ToListAsync();

                var workers = await _context.Users
                    .Where(u => u.IsActive)
                    .ToListAsync();

                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var user = await _context.Users.Include(u => u.Studio).FirstOrDefaultAsync(u => u.Id == userId);
                var studioName = user?.Studio?.Name ?? "Şahsi Panel";
                
                var viewModel = new PanelIndexViewModel
                {
                    RecentActivities = await _context.ActivityLog.OrderByDescending(a => a.LoginDate).Take(4).ToListAsync() ?? new List<ActivityLog>(),
                    ImagesStorageMB = storage.images,
                    FilesStorageMB = storage.files,
                    MediaStorageMB = storage.media,
                    OtherStorageMB = storage.other,
                    StudioName = studioName,
                    RecentTasks = recentTasks,
                    AllTasks = await _context.TaskItems.Include(t => t.AssignedUser).ToListAsync(),
                    StudioWorkers = workers
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return View(new PanelIndexViewModel 
                {
                    RecentActivities = new List<ActivityLog>(),
                    RecentTasks = new List<TaskItem>(),
                    StudioWorkers = new List<User>()
                });
            }
        }

        public async Task<IActionResult> Board()
        {
            var tasks = await _context.TaskItems
                .Include(t => t.AssignedUser)
                .Include(t => t.CreatorUser)
                .Include(t => t.Studio)
                .ToListAsync();

            return View(tasks);
        }

        public async Task<IActionResult> Statistics()
        {
            var storage = CalculateStorageUsage();
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users.Include(u => u.Studio).FirstOrDefaultAsync(u => u.Id == userId);
            var studioName = user?.Studio?.Name ?? "Şahsi Panel";

            var workers = await _context.Users.Where(u => u.IsActive).ToListAsync();

            var viewModel = new PanelIndexViewModel
            {
                ImagesStorageMB = storage.images,
                FilesStorageMB = storage.files,
                MediaStorageMB = storage.media,
                OtherStorageMB = storage.other,
                StudioName = studioName,
                StudioWorkers = workers
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask(TaskItem model, List<IFormFile> files)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users.Include(u => u.Studio).FirstOrDefaultAsync(u => u.Id == userId);
            
            model.CreatorUserId = userId;
            model.StudioId = user?.StudioId ?? 1;
            model.CreateDate = DateTime.Now;
            model.IsActive = true;

            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    var fileName = await FileHelper.CreateFile(file, "uploads");
                    model.Attachments.Add($"/uploads/{fileName}");
                }
            }
            
            _context.TaskItems.Add(model);
            await _context.SaveChangesAsync();

            if (model.AssignedUserId.HasValue && model.AssignedUserId != userId)
            {
                await SendNotification(model.AssignedUserId.Value, "Yeni Görev Atandı", $"'{model.Title}' görevi size atandı.", "Assignment", model.Id);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTaskStatus(int taskId, int newStatus)
        {
            var task = await _context.TaskItems.FindAsync(taskId);
            if (task == null) return NotFound();

            var oldStatus = task.TaskStatus;
            task.TaskStatus = (TaskStatusEnum)newStatus;
            await _context.SaveChangesAsync();

            if (oldStatus != task.TaskStatus)
            {
                var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (task.CreatorUserId != currentUserId)
                {
                    await SendNotification(task.CreatorUserId, "Görev Durumu Değişti", $"'{task.Title}' görevi {task.TaskStatus} durumuna geçti.", "StatusChange", task.Id);
                }
                if (task.AssignedUserId.HasValue && task.AssignedUserId != currentUserId && task.AssignedUserId != task.CreatorUserId)
                {
                    await SendNotification(task.AssignedUserId.Value, "Görev Durumu Değişti", $"'{task.Title}' görevi {task.TaskStatus} durumuna geçti.", "StatusChange", task.Id);
                }
            }

            return Ok();
        }

        private async Task SendNotification(int userId, string title, string message, string type, int? relatedTaskId)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                RelatedTaskId = relatedTaskId,
                IsRead = false,
                CreateDate = DateTime.Now,
                IsActive = true
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }


        [HttpGet]
        public async Task<IActionResult> GetTaskDetails(int id)
        {
            var task = await _context.TaskItems
                .Include(t => t.AssignedUser)
                .Include(t => t.CreatorUser)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return NotFound();

            return Json(new
            {
                id = task.Id,
                title = task.Title,
                description = task.Description,
                priority = (int)task.Priority,
                priorityName = task.Priority.ToString(),
                status = (int)task.TaskStatus,
                statusName = task.TaskStatus.ToString(),
                type = (int)task.TaskType,
                typeName = task.TaskType.ToString(),
                assignedUserId = task.AssignedUserId,
                assignedUser = task.AssignedUser?.Name ?? "Atanmadı",
                creatorUser = task.CreatorUser?.Name ?? "Sistem",
                createDate = task.CreateDate.ToString("dd MMM yyyy HH:mm"),
                attachments = task.Attachments
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTask(TaskItem model)
        {
            var task = await _context.TaskItems.FindAsync(model.Id);
            if (task == null) return NotFound();

            task.Title = model.Title;
            task.Description = model.Description;
            task.Priority = model.Priority;
            task.TaskStatus = model.TaskStatus;
            task.TaskType = model.TaskType;
            task.AssignedUserId = model.AssignedUserId;
            task.UpdateDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
