using BilgisayarMuhendisligiTasarimi.Data.Context;
using BilgisayarMuhendisligiTasarimi.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BilgisayarMuhendisligiTasarimi.Services
{
    public interface IActivityLogService
    {
        Task LogLoginActivity(string userName, string userId, HttpContext httpContext);
    }

    public class ActivityLogService : IActivityLogService
    {
        private readonly DBContext _context;
        private const int MAX_ACTIVITY_LOGS = 10;

        public ActivityLogService(DBContext context)
        {
            _context = context;
        }

        public async Task LogLoginActivity(string userName, string userId, HttpContext httpContext)
        {
            var userAgent = httpContext.Request.Headers["User-Agent"].ToString();

            var activityLog = new ActivityLog
            {
                UserName = userName,
                UserId = userId,
                IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
                LoginDate = DateTime.UtcNow,
                Browser = GetBrowserInfo(userAgent),
                Platform = GetPlatformInfo(userAgent)
            };

            await _context.ActivityLog.AddAsync(activityLog);

            var userLogs = await _context.ActivityLog
                .Where(log => log.UserId == userId)
                .OrderByDescending(log => log.LoginDate)
                .Skip(MAX_ACTIVITY_LOGS - 1)
                .ToListAsync();
            
            if (userLogs.Any())
            {
                _context.ActivityLog.RemoveRange(userLogs);
            }

            await _context.SaveChangesAsync();
        }

        private string GetBrowserInfo(string userAgent)
        {
            userAgent = userAgent.ToLower();
            if (userAgent.Contains("firefox"))
                return "Firefox";
            if (userAgent.Contains("chrome"))
                return "Chrome";
            if (userAgent.Contains("safari"))
                return "Safari";
            if (userAgent.Contains("edge"))
                return "Edge";
            if (userAgent.Contains("opera"))
                return "Opera";
            return "Diğer";
        }

        private string GetPlatformInfo(string userAgent)
        {
            userAgent = userAgent.ToLower();
            if (userAgent.Contains("windows"))
                return "Windows";
            if (userAgent.Contains("macintosh") || userAgent.Contains("mac os"))
                return "MacOS";
            if (userAgent.Contains("linux"))
                return "Linux";
            if (userAgent.Contains("android"))
                return "Android";
            if (userAgent.Contains("iphone") || userAgent.Contains("ipad"))
                return "iOS";
            return "Diğer";
        }
    }
} 