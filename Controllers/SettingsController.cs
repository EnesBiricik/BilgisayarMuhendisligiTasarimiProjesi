using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BilgisayarMuhendisligiTasarimi.Data.Context;
using BilgisayarMuhendisligiTasarimi.Models;
using BilgisayarMuhendisligiTasarimi.Services;
using Microsoft.AspNetCore.Authorization;

namespace BilgisayarMuhendisligiTasarimi.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly DBContext _db;

        public SettingsController(DBContext dbContext)
        {
            _db = dbContext;
        }

        public async Task<IActionResult> UpdateSettings()
        {
            var settings = await _db.Settings.FirstAsync();
            var model = new UpdateSettingsModel
            {
                Id = settings.Id,
                Logo = settings.Logo,
                Icon = settings.Icon,
                Slogan = settings.Slogan,
                PhoneNumber = settings.PhoneNumber,
                Email = settings.Email
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> UpdateSettings(UpdateSettingsModel model, IFormFile? logo, IFormFile? icon)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Lütfen tüm zorunlu alanları doldurun.";
                return View(model);
            }

            var entity = await _db.Settings.FirstAsync();
            if (entity == null)
            {
                TempData["error"] = "Ayarlar bulunamadı";
                return View(model);
            }

            if (logo != null)
            {
                var path = await FileHelper.ReplaceFile(model.Logo, logo);
                entity.Logo = path;
            }

            if (icon != null)
            {
                var path = await FileHelper.ReplaceFile(model.Icon, icon);
                entity.Icon = path;
            }

            entity.Slogan = model.Slogan;
            entity.PhoneNumber = model.PhoneNumber;
            entity.Email = model.Email;
            entity.UpdateDate = DateTime.Now;

            await _db.SaveChangesAsync();
            TempData["success"] = "Ayarlar Güncellendi";
            return RedirectToAction(nameof(UpdateSettings));
        }
    }
}
