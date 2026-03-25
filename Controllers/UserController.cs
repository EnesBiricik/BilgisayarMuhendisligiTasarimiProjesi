using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BilgisayarMuhendisligiTasarimi.Models;
using BilgisayarMuhendisligiTasarimi.Services;
using BilgisayarMuhendisligiTasarimi.Data.Context;
using BilgisayarMuhendisligiTasarimi.Data.Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BilgisayarMuhendisligiTasarimi.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class UserController : Controller
    {
        private readonly AuthService _authService;
        private readonly DBContext _context;

        public UserController(AuthService authService, DBContext context)
        {
            _authService = authService;
            _context = context;
        }

        public async Task<IActionResult> List()
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

                var query = _context.Users
                    .Include(u => u.Role)
                    .Select(u => new UserListViewModel
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Email = u.Email,
                        RoleName = u.Role.Name,
                        RoleId = u.RoleId,
                        IsActive = u.IsActive,
                        IsLocked = u.LockoutEnd.HasValue && u.LockoutEnd > DateTime.UtcNow,
                        LastLoginAt = u.LastLoginAt,
                        LastLoginIp = u.LastLoginIp,
                        FailedLoginCount = u.FailedLoginCount,
                        LockoutEnd = u.LockoutEnd
                    });

                if (currentUserRole != RoleEnum.SuperAdmin.ToString())
                {
                    query = query.Where(u => u.RoleId == (int)RoleEnum.User || u.RoleId == (int)RoleEnum.Admin);
                }

                // Kendisini listeden çıkar
                query = query.Where(u => u.Id != currentUserId);

                var users = await query.OrderByDescending(u => u.LastLoginAt).ToListAsync();
                return View(users);
            }
            catch (Exception ex)
            {
                TempData["error"] = "Kullanıcılar listelenirken bir hata oluştu: " + ex.Message;
                return View(new List<UserListViewModel>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            try
            {
                var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
                var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                    return Json(new { success = false, message = "Kullanıcı bulunamadı." });

                // Yetki kontrolü
                if (currentUserRole == RoleEnum.Admin.ToString())
                {
                    if (user.RoleId != (int)RoleEnum.User)
                        return Json(new { success = false, message = "Bu kullanıcı üzerinde işlem yapma yetkiniz yok." });
                }
                else if (currentUserRole == user.Role.Name)
                {
                    return Json(new { success = false, message = "Kendinizle aynı role sahip kullanıcılar üzerinde işlem yapamazsınız." });
                }

                // SuperAdmin hesabı deaktif edilemez
                if (user.RoleId == (int)RoleEnum.SuperAdmin)
                    return Json(new { success = false, message = "SuperAdmin hesabı deaktif edilemez." });

                user.IsActive = !user.IsActive;
                user.UpdateDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = user.IsActive ? "Kullanıcı aktif edildi." : "Kullanıcı pasif edildi.",
                    isActive = user.IsActive,
                    statusHtml = user.IsActive ? 
                        "<span class='badge bg-success'>Aktif</span>" : 
                        "<span class='badge bg-secondary'>Pasif</span>"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "İşlem sırasında bir hata oluştu: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserLock(int id)
        {
            try
            {
                var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
                var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                    return Json(new { success = false, message = "Kullanıcı bulunamadı." });

                // Yetki kontrolü
                if (currentUserRole == RoleEnum.Admin.ToString() && user.RoleId != (int)RoleEnum.User)
                    return Json(new { success = false, message = "Bu kullanıcı üzerinde işlem yapma yetkiniz yok." });

                // SuperAdmin hesabı kilitlenemez
                if (user.RoleId == (int)RoleEnum.SuperAdmin)
                    return Json(new { success = false, message = "SuperAdmin hesabı kilitlenemez." });

                if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
                {
                    // Kilidi kaldır
                    user.LockoutEnd = null;
                    user.FailedLoginCount = 0;
                }
                else
                {
                    // Kilitle
                    user.LockoutEnd = DateTime.UtcNow.AddHours(24);
                }

                user.UpdateDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = user.LockoutEnd.HasValue ? "Kullanıcı kilitlendi." : "Kullanıcı kilidi kaldırıldı." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "İşlem sırasında bir hata oluştu: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetLoginAttempts(int id)
        {
            try
            {
                var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
                var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                    return Json(new { success = false, message = "Kullanıcı bulunamadı." });

                // Yetki kontrolü
                if (currentUserRole == RoleEnum.Admin.ToString())
                {
                    if (user.RoleId != (int)RoleEnum.User)
                        return Json(new { success = false, message = "Bu kullanıcı üzerinde işlem yapma yetkiniz yok." });
                }
                else if (currentUserRole == user.Role.Name)
                {
                    return Json(new { success = false, message = "Kendinizle aynı role sahip kullanıcılar üzerinde işlem yapamazsınız." });
                }

                user.FailedLoginCount = 0;
                user.LockoutEnd = null;
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = "Giriş denemeleri sıfırlandı.",
                    failedLoginCount = "0",
                    lockoutEnd = "-"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "İşlem sırasında bir hata oluştu: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
                
                if (id == currentUserId)
                    return Json(new { success = false, message = "Kendinizi silemezsiniz." });

                var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                    return Json(new { success = false, message = "Kullanıcı bulunamadı." });

                // Yetki kontrolü
                if (currentUserRole == RoleEnum.Admin.ToString())
                {
                    if (user.RoleId != (int)RoleEnum.User)
                        return Json(new { success = false, message = "Bu kullanıcı üzerinde işlem yapma yetkiniz yok." });
                }
                else if (currentUserRole == user.Role.Name)
                {
                    return Json(new { success = false, message = "Kendinizle aynı role sahip kullanıcılar üzerinde işlem yapamazsınız." });
                }

                // SuperAdmin hesabı silinemez
                if (user.RoleId == (int)RoleEnum.SuperAdmin)
                    return Json(new { success = false, message = "SuperAdmin hesabı silinemez." });

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Kullanıcı başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Kullanıcı silinirken bir hata oluştu: " + ex.Message });
            }
        }

        public IActionResult AccountSettings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var name = User.FindFirstValue(ClaimTypes.Name);
            var email = User.FindFirstValue(ClaimTypes.Email);

            var model = new UserAccountSettingsModel
            {
                Name = name,
                Email = email
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AccountSettings(UserAccountSettingsModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Lütfen tüm zorunlu alanları doldurun.";
                return View(model);
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var (success, message) = await _authService.UpdateUserAccount(HttpContext, userId, model);

            if (success)
            {
                TempData["success"] = message;
                return RedirectToAction("Login", "Auth");
            }

            TempData["error"] = message;
            return View(model);
        }

        public IActionResult Create()
        {
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            var availableRoles = Enum.GetValues(typeof(RoleEnum))
                        .Cast<RoleEnum>()
                        .Where(r => r != RoleEnum.SuperAdmin)
                        .Select(r => new { Id = (int)r, Name = r.ToString() });


            ViewBag.Roles = new SelectList(availableRoles, "Id", "Name");
            return View(new CreateUserModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserModel model)
        {
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            if (!ModelState.IsValid)
            {
                var availableRoles = Enum.GetValues(typeof(RoleEnum))
                        .Cast<RoleEnum>()
                        .Where(r => r != RoleEnum.SuperAdmin)
                        .Select(r => new { Id = (int)r, Name = r.ToString() });


                ViewBag.Roles = new SelectList(availableRoles, "Id", "Name");
                return View(model);
            }

            // Yetki kontrolü
            if (currentUserRole == RoleEnum.Admin.ToString() && model.Role == RoleEnum.SuperAdmin)
            {
                ModelState.AddModelError(string.Empty, "Admin kullanıcıları SuperAdmin rolünde kullanıcı oluşturamaz.");
                return View(model);
            }

            try
            {
                var (success, message) = await _authService.RegisterAsync(model.Email, model.Password, model.Name, model.Role);
                if (!success)
                {
                    ModelState.AddModelError(string.Empty, message);
                    var availableRoles = Enum.GetValues(typeof(RoleEnum))
                        .Cast<RoleEnum>()
                        .Where(r => r != RoleEnum.SuperAdmin)
                        .Select(r => new { Id = (int)r, Name = r.ToString() });

                    ViewBag.Roles = new SelectList(availableRoles, "Id", "Name");
                    return View(model);
                }

                TempData["success"] = "Kullanıcı başarıyla oluşturuldu.";
                return RedirectToAction(nameof(List));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı oluşturulurken bir hata oluştu: " + ex.Message);
                var availableRoles = Enum.GetValues(typeof(RoleEnum))
                        .Cast<RoleEnum>()
                        .Where(r => r != RoleEnum.SuperAdmin)
                        .Select(r => new { Id = (int)r, Name = r.ToString() });


                ViewBag.Roles = new SelectList(availableRoles, "Id", "Name");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult GetAvailableRoles()
        {
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            
            // User rolü bu sayfaya erişemez
            if (currentUserRole == RoleEnum.User.ToString())
                return Json(new List<object>());

            var availableRoles = Enum.GetValues(typeof(RoleEnum))
                .Cast<RoleEnum>()
                .Where(r => {
                    if (currentUserRole == RoleEnum.SuperAdmin.ToString())
                        return true; // SuperAdmin tüm rolleri görebilir
                    else if (currentUserRole == RoleEnum.Admin.ToString())
                        return r == RoleEnum.User || r == RoleEnum.Admin; // Admin sadece User ve Admin rollerini görebilir
                    return false;
                })
                .Select(r => new { id = (int)r, name = r.ToString() });

            return Json(availableRoles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(int id, int roleId)
        {
            try
            {
                var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
                
                // User rolü bu işlemi yapamaz
                if (currentUserRole == RoleEnum.User.ToString())
                    return Json(new { success = false, message = "Bu işlem için yetkiniz yok." });

                var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                    return Json(new { success = false, message = "Kullanıcı bulunamadı." });

                // SuperAdmin rolü değiştirilemez
                if (user.RoleId == (int)RoleEnum.SuperAdmin)
                    return Json(new { success = false, message = "SuperAdmin rolü değiştirilemez." });

                // Yetki kontrolleri
                if (currentUserRole == RoleEnum.Admin.ToString())
                {
                    // Admin sadece User rolündeki kullanıcıları Admin yapabilir
                    if (user.RoleId == (int)RoleEnum.Admin || user.RoleId == (int)RoleEnum.SuperAdmin)
                        return Json(new { success = false, message = "Bu kullanıcının rolünü değiştirme yetkiniz yok." });

                    // Admin sadece User veya Admin rolü atayabilir
                    if (roleId != (int)RoleEnum.User && roleId != (int)RoleEnum.Admin)
                        return Json(new { success = false, message = "Bu rolü atama yetkiniz yok." });
                }

                // Aynı role sahip kullanıcılar birbirlerinin rollerini değiştiremez
                if (currentUserRole == user.Role.Name)
                    return Json(new { success = false, message = "Kendinizle aynı role sahip kullanıcıların rollerini değiştiremezsiniz." });

                var newRole = await _context.Roles.FindAsync(roleId);
                if (newRole == null)
                    return Json(new { success = false, message = "Geçersiz rol." });

                user.RoleId = roleId;
                user.UpdateDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = "Kullanıcı rolü güncellendi.",
                    roleName = newRole.Name
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "İşlem sırasında bir hata oluştu: " + ex.Message });
            }
        }

        [HttpGet]
        public IActionResult UpdatePassword()
        {
            return View(new UpdatePasswordModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Lütfen tüm zorunlu alanları doldurun.";
                return View(model);
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var (success, message) = await _authService.UpdatePassword(HttpContext, userId, model.CurrentPassword, model.NewPassword);

            if (success)
            {
                TempData["success"] = message;
                return RedirectToAction("Login", "Auth");
            }

            TempData["error"] = message;
            return View(model);
        }
    }
}
