using Microsoft.AspNetCore.Mvc;

namespace BilgisayarMuhendisligiTasarimi.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Panel");
            return View();
        }
    }
}
