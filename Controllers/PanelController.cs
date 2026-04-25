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
        private readonly IEmailService _emailService;

        public PanelController(DBContext context, IWebHostEnvironment webHostEnvironment, IEmailService emailService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _emailService = emailService;
        }



        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var user = await _context.Users.Include(u => u.Studio).FirstOrDefaultAsync(u => u.Id == userId);
                var studioId = user?.StudioId;

                // Fallback: If user has no studio, try to find a studio they own
                if (studioId == null)
                {
                    var ownedStudio = await _context.Studios.FirstOrDefaultAsync(s => s.OwnerUserId == userId);
                    studioId = ownedStudio?.Id;
                }

                var studioName = user?.Studio?.Name ?? (await _context.Studios.FindAsync(studioId))?.Name ?? "Genel Panel";

                // Filter tasks: Show all tasks for SuperAdmin if no studio is selected, otherwise filter by studioId
                var query = _context.TaskItems.Where(t => t.IsActive);
                
                if (studioId != null)
                {
                    query = query.Where(t => t.StudioId == studioId);
                }
                else if (!User.IsInRole("SuperAdmin"))
                {
                    // If not SuperAdmin and no studio, only show tasks related to the user
                    query = query.Where(t => t.CreatorUserId == userId || t.AssignedUserId == userId);
                }

                var allTasks = await query
                    .Include(t => t.AssignedUser)
                    .ToListAsync();

                var recentTasks = allTasks
                    .OrderByDescending(t => t.CreateDate)
                    .Take(10)
                    .ToList();

                // Filter workers by StudioId
                var workers = await _context.Users
                    .Where(u => u.StudioId == studioId && u.IsActive)
                    .ToListAsync();

                // Calculate storage only for this studio's tasks
                var storage = CalculateStudioStorageUsage(allTasks);

                var viewModel = new PanelIndexViewModel
                {
                    RecentActivities = await _context.ActivityLog
                        .Where(a => a.UserId == userId.ToString())
                        .OrderByDescending(a => a.LoginDate)
                        .Take(4)
                        .ToListAsync() ?? new List<ActivityLog>(),
                    ImagesStorageMB = storage.images,
                    FilesStorageMB = storage.files,
                    MediaStorageMB = storage.media,
                    OtherStorageMB = storage.other,
                    StudioName = studioName,
                    RecentTasks = recentTasks,
                    AllTasks = allTasks,
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
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users.Include(u => u.Studio).FirstOrDefaultAsync(u => u.Id == userId);
            var studioId = user?.StudioId;

            if (studioId == null)
            {
                var ownedStudio = await _context.Studios.FirstOrDefaultAsync(s => s.OwnerUserId == userId);
                studioId = ownedStudio?.Id;
            }

            var studioName = user?.Studio?.Name ?? (await _context.Studios.FindAsync(studioId))?.Name ?? "Genel Panel";

            var query = _context.TaskItems.Where(t => t.IsActive);
            if (studioId != null)
            {
                query = query.Where(t => t.StudioId == studioId);
            }
            else if (!User.IsInRole("SuperAdmin"))
            {
                query = query.Where(t => t.CreatorUserId == userId || t.AssignedUserId == userId);
            }

            var tasks = await query
                .Include(t => t.AssignedUser)
                .Include(t => t.CreatorUser)
                .Include(t => t.Studio)
                .ToListAsync();

            var workers = await _context.Users
                .Where(u => u.StudioId == studioId && u.IsActive)
                .ToListAsync();

            var viewModel = new PanelIndexViewModel
            {
                AllTasks = tasks,
                StudioWorkers = workers,
                StudioName = studioName
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Statistics()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users.Include(u => u.Studio).FirstOrDefaultAsync(u => u.Id == userId);
            var studioId = user?.StudioId;
            
            if (studioId == null)
            {
                var ownedStudio = await _context.Studios.FirstOrDefaultAsync(s => s.OwnerUserId == userId);
                studioId = ownedStudio?.Id;
            }

            var studioName = user?.Studio?.Name ?? (await _context.Studios.FindAsync(studioId))?.Name ?? "Genel Panel";

            var query = _context.TaskItems.Where(t => t.IsActive);
            if (studioId != null)
            {
                query = query.Where(t => t.StudioId == studioId);
            }
            else if (!User.IsInRole("SuperAdmin"))
            {
                query = query.Where(t => t.CreatorUserId == userId || t.AssignedUserId == userId);
            }

            var allTasks = await query
                .Include(t => t.AssignedUser)
                .ToListAsync();

            var workersQuery = _context.Users.Where(u => u.IsActive);
            if (studioId != null)
            {
                workersQuery = workersQuery.Where(u => u.StudioId == studioId);
            }
            var workers = await workersQuery.ToListAsync();

            // Calculate storage only for this studio's tasks
            var storage = CalculateStudioStorageUsage(allTasks);

            var viewModel = new PanelIndexViewModel
            {
                AllTasks = allTasks,
                StudioWorkers = workers,
                StudioName = studioName,
                ImagesStorageMB = storage.images,
                FilesStorageMB = storage.files,
                MediaStorageMB = storage.media,
                OtherStorageMB = storage.other
            };

            return View(viewModel);
        }

        private (int images, int files, int media, int other) CalculateStudioStorageUsage(List<TaskItem> tasks)
        {
            int imageSize = 0, fileSize = 0, mediaSize = 0, otherSize = 0;
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var mediaExtensions = new[] { ".mp4", ".mp3", ".wav", ".avi" };
            var fileExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt" };

            foreach (var task in tasks)
            {
                foreach (var attachment in task.Attachments)
                {
                    try
                    {
                        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, attachment.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            var info = new FileInfo(filePath);
                            var ext = Path.GetExtension(filePath).ToLower();
                            
                            if (imageExtensions.Contains(ext)) imageSize += (int)info.Length;
                            else if (mediaExtensions.Contains(ext)) mediaSize += (int)info.Length;
                            else if (fileExtensions.Contains(ext)) fileSize += (int)info.Length;
                            else otherSize += (int)info.Length;
                        }
                    }
                    catch { /* Skip missing files */ }
                }
            }

            return (
                imageSize / (1024 * 1024),
                fileSize / (1024 * 1024),
                mediaSize / (1024 * 1024),
                otherSize / (1024 * 1024)
            );
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

            if (model.AssignedUserId.HasValue)
            {
                var assignedUser = await _context.Users.FindAsync(model.AssignedUserId.Value);
                if (assignedUser != null)
                {
                    // Notify assigned user
                    if (model.AssignedUserId != userId)
                    {
                        await SendNotification(model.AssignedUserId.Value, "Yeni Görev Atandı", $"'{model.Title}' görevi size atandı.", "Assignment", model.Id);
                        await _emailService.SendTaskAssignmentEmailAsync(assignedUser.Email, assignedUser.Name, model.Title, user?.Name ?? "Sistem");
                    }

                    // Notify Manager (Studio Owner) if different from creator and assigned user
                    var studio = await _context.Studios.FindAsync(model.StudioId);
                    if (studio?.OwnerUserId.HasValue == true && studio.OwnerUserId != userId && studio.OwnerUserId != model.AssignedUserId)
                    {
                        var owner = await _context.Users.FindAsync(studio.OwnerUserId.Value);
                        if (owner != null)
                        {
                            await _emailService.SendEmailAsync(owner.Email, $"Yeni Görev Atandı: {model.Title}", $"<p>Stüdyonuzda yeni bir görev oluşturuldu ve {assignedUser.Name} kullanıcısına atandı: <strong>{model.Title}</strong></p>");
                        }
                    }
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTaskStatus(int taskId, int newStatus)
        {
            var task = await _context.TaskItems
                .Include(t => t.AssignedUser)
                .Include(t => t.Studio)
                .FirstOrDefaultAsync(t => t.Id == taskId);
            
            if (task == null) return NotFound();

            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var isDone = (TaskStatusEnum)newStatus == TaskStatusEnum.Done;

            // Permission Check: Only Admin/SuperAdmin can move to Done
            if (isDone && !User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
            {
                return Forbid();
            }

            var oldStatus = task.TaskStatus;
            task.TaskStatus = (TaskStatusEnum)newStatus;
            await _context.SaveChangesAsync();

            if (oldStatus != task.TaskStatus)
            {
                // Notifications and Emails
                var message = $"'{task.Title}' görevi {task.TaskStatus} durumuna geçti.";
                
                // Notify Creator (Manager)
                if (task.CreatorUserId != currentUserId)
                {
                    await SendNotification(task.CreatorUserId, "Görev Durumu Değişti", message, "StatusChange", task.Id);
                    var creator = await _context.Users.FindAsync(task.CreatorUserId);
                    if (creator != null)
                    {
                        await _emailService.SendTaskStatusChangeEmailAsync(creator.Email, creator.Name, task.Title, task.TaskStatus.ToString());
                    }
                }

                // Notify Assigned User
                if (task.AssignedUserId.HasValue && task.AssignedUserId != currentUserId && task.AssignedUserId != task.CreatorUserId)
                {
                    await SendNotification(task.AssignedUserId.Value, "Görev Durumu Değişti", message, "StatusChange", task.Id);
                    if (task.AssignedUser != null)
                    {
                        await _emailService.SendTaskStatusChangeEmailAsync(task.AssignedUser.Email, task.AssignedUser.Name, task.Title, task.TaskStatus.ToString());
                    }
                }

                // Notify Studio Owner if not already notified
                if (task.Studio?.OwnerUserId.HasValue == true && 
                    task.Studio.OwnerUserId != currentUserId && 
                    task.Studio.OwnerUserId != task.CreatorUserId && 
                    task.Studio.OwnerUserId != task.AssignedUserId)
                {
                    var owner = await _context.Users.FindAsync(task.Studio.OwnerUserId.Value);
                    if (owner != null)
                    {
                        await _emailService.SendTaskStatusChangeEmailAsync(owner.Email, owner.Name, task.Title, task.TaskStatus.ToString());
                    }
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

            // Permission Check: Only Admin/SuperAdmin can move to Done
            if (model.TaskStatus == TaskStatusEnum.Done && task.TaskStatus != TaskStatusEnum.Done)
            {
                if (!User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
                {
                    return Forbid();
                }
            }

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
