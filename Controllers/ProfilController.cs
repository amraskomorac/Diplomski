using Microsoft.AspNetCore.Mvc;

namespace Diplomski.Controllers
{
    public class ProfilController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
