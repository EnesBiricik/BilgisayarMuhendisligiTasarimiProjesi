using Microsoft.AspNetCore.Mvc;

namespace BilgisayarMuhendisligiTasarimi.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
