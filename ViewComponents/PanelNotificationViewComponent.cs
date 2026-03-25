using Microsoft.AspNetCore.Mvc;
using BilgisayarMuhendisligiTasarimi.Data;
using BilgisayarMuhendisligiTasarimi.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace BilgisayarMuhendisligiTasarimi.ViewComponents
{
    public class PanelNotificationViewComponent : ViewComponent
    {
        private readonly DBContext _context;

        public PanelNotificationViewComponent(DBContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = int.Parse(UserClaimsPrincipal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            var unreadCount = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();

            var latestNotifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreateDate)
                .Take(5)
                .ToListAsync();

            ViewBag.UnreadCount = unreadCount;
            return View(latestNotifications);
        }
    }
} 