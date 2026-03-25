using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using BilgisayarMuhendisligiTasarimi.Data.Context;
using BilgisayarMuhendisligiTasarimi.Data.Entities;
using BilgisayarMuhendisligiTasarimi.Models;

namespace BilgisayarMuhendisligiTasarimi.Services
{
    public class AuthService
    {
        private readonly DBContext _db;
        private readonly IActivityLogService _activityLogService;
        private const int MAX_PASSWORD_ATTEMPTS = 5;

        public AuthService(
            DBContext db,
            IActivityLogService activityLogService)
        {
            _db = db;
            _activityLogService = activityLogService;
        }

        public async Task<(bool Success, string Message)> LoginAsync(HttpContext httpContext, string email, string password)
        {
            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || !user.IsActive)
                return (false, "Kullanıcı bulunamadı veya hesap pasif durumda.");

            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
                return (false, $"Hesabınız {user.LockoutEnd.Value.ToLocalTime():HH:mm} saatine kadar kilitli.");

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                user.FailedLoginCount++;
                if (user.FailedLoginCount >= MAX_PASSWORD_ATTEMPTS)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddHours(1);
                    await _db.SaveChangesAsync();
                    return (false, "5 kez hatalı giriş yaptınız. Hesabınız 1 saat kilitlendi.");
                }
                await _db.SaveChangesAsync();
                return (false, $"Hatalı şifre. Kalan deneme hakkınız: {5 - user.FailedLoginCount}");
            }

            user.FailedLoginCount = 0;
            user.LockoutEnd = null;
            user.LastLoginAt = DateTime.UtcNow;
            user.LastLoginIp = GetIpAddress(httpContext);
            await _db.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role.Name)
            };

            var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await httpContext.SignInAsync("MyCookieAuth", 
                new ClaimsPrincipal(claimsIdentity), 
                authProperties);

            await _activityLogService.LogLoginActivity(
                userName: user.Name,
                userId: user.Id.ToString(),
                httpContext: httpContext
            );

            return (true, "Giriş başarılı.");
        }

        public async Task<(bool Success, string Message)> RegisterAsync(string email, string password, string name, RoleEnum role = RoleEnum.User, string? studioName = null)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                if (await _db.Users.AnyAsync(u => u.Email == email))
                    return (false, "Bu email adresi zaten kayıtlı.");

                if (!await _db.Users.AnyAsync())
                {
                    role = RoleEnum.SuperAdmin;
                }
                else
                {
                    var roleExists = await _db.Roles.AnyAsync(r => r.Id == (int)role);
                    if (!roleExists)
                        return (false, $"Seçilen rol ({role}) sistemde bulunamadı.");
                }

                var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

                var user = new User
                {
                    Email = email,
                    Name = name,
                    PasswordHash = passwordHash,
                    RoleId = (int)role,
                    IsActive = true,
                    CreateDate = DateTime.UtcNow
                };

                _db.Users.Add(user);
                await _db.SaveChangesAsync();

                if (!string.IsNullOrWhiteSpace(studioName))
                {
                    var studio = new Studio
                    {
                        Name = studioName.Trim(),
                        OwnerUserId = user.Id,
                        CreateDate = DateTime.UtcNow,
                        IsActive = true
                    };
                    _db.Studios.Add(studio);
                    await _db.SaveChangesAsync();

                    user.StudioId = studio.Id;
                    await _db.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return (true, "Kayıt başarılı.");
            }
            catch (Exception ex)
            {
                if (transaction != null) await transaction.RollbackAsync();
                return (false, "Kayıt işlemi sırasında bir hata oluştu: " + ex.Message);
            }
        }

        public async Task SignOutAsync(HttpContext httpContext)
        {
            await httpContext.SignOutAsync("MyCookieAuth");
        }

        private string? GetIpAddress(HttpContext context)
        {
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            return context.Connection.RemoteIpAddress?.ToString();
        }

        public async Task<(bool Success, string Message)> UpdateUserAccount(HttpContext httpContext, int userId, UserAccountSettingsModel model)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null || !user.IsActive)
                return (false, "Kullanıcı bulunamadı veya hesap pasif durumda.");

            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
            {
                user.FailedLoginCount++;
                if (user.FailedLoginCount >= 5)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddHours(1);
                    await _db.SaveChangesAsync();
                    await SignOutAsync(httpContext);
                    return (false, "5 kez hatalı şifre girdiniz. Hesabınız 1 saat kilitlendi.");
                }
                await _db.SaveChangesAsync();
                return (false, $"Hatalı şifre. Kalan deneme hakkınız: {5 - user.FailedLoginCount}");
            }

            user.FailedLoginCount = 0;
            user.LockoutEnd = null;
            user.Name = model.Name;
            user.Email = model.Email;
            user.UpdateDate = DateTime.UtcNow;

            try 
            {
                await _db.SaveChangesAsync();
                await SignOutAsync(httpContext);
                return (true, "Hesap bilgileriniz başarıyla güncellendi. Lütfen tekrar giriş yapın.");
            }
            catch (Exception ex)
            {
                return (false, "Güncelleme sırasında bir hata oluştu: " + ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> UpdatePassword(HttpContext httpContext, int userId, string currentPassword, string newPassword)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null || !user.IsActive)
                return (false, "Kullanıcı bulunamadı veya hesap pasif durumda.");

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            {
                user.FailedLoginCount++;
                if (user.FailedLoginCount >= MAX_PASSWORD_ATTEMPTS)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddHours(1);
                    await _db.SaveChangesAsync();
                    await SignOutAsync(httpContext);
                    return (false, "5 kez hatalı şifre girdiniz. Hesabınız 1 saat kilitlendi.");
                }
                await _db.SaveChangesAsync();
                return (false, $"Hatalı şifre. Kalan deneme hakkınız: {5 - user.FailedLoginCount}");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.FailedLoginCount = 0;
            user.LockoutEnd = null;
            user.UpdateDate = DateTime.UtcNow;

            try 
            {
                await _db.SaveChangesAsync();
                await SignOutAsync(httpContext);
                return (true, "Şifreniz başarıyla değiştirildi. Lütfen tekrar giriş yapın.");
            }
            catch (Exception ex)
            {
                return (false, "Şifre değiştirme sırasında bir hata oluştu: " + ex.Message);
            }
        }
    }
}