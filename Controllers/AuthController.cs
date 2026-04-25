using Microsoft.AspNetCore.Mvc;
using BilgisayarMuhendisligiTasarimi.Services;
using BilgisayarMuhendisligiTasarimi.Data.Entities;
using BilgisayarMuhendisligiTasarimi.Models;

namespace BilgisayarMuhendisligiTasarimi.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Panel");
            return View(new LoginModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _authService.LoginAsync(HttpContext, model.Email, model.Password);
            if (result.Success)
            {
                TempData["info"] = "Hoşgeldiniz!";
                return RedirectToAction("Index", "Panel");
            }

            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Panel");
            
            ViewBag.Roles = Enum.GetValues(typeof(RoleEnum))
                .Cast<RoleEnum>()
                .Select(r => new { Id = (int)r, Name = r.ToString() });
            
            return View(new RegisterModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = Enum.GetValues(typeof(RoleEnum))
                    .Cast<RoleEnum>()
                    .Select(r => new { Id = (int)r, Name = r.ToString() });
                return View(model);
            }

            var (success, message) = await _authService.RegisterAsync(model.Email, model.Password, model.Name, model.Role, model.StudioName);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                ViewBag.Roles = Enum.GetValues(typeof(RoleEnum))
                    .Cast<RoleEnum>()
                    .Select(r => new { Id = (int)r, Name = r.ToString() });
                return View(model);
            }

            await _authService.LoginAsync(HttpContext, model.Email, model.Password);
            return RedirectToAction("Index", "Panel");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Panel");
            return View(new ForgotPasswordModel());
        }

        [HttpPost]
        public IActionResult ForgotPassword(ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            ModelState.AddModelError(string.Empty, "Şifre sıfırlama bağlantısı email adresinize gönderildi.");
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _authService.SignOutAsync(HttpContext);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return Redirect("Error/Status?code=401");
        }
    }
}
