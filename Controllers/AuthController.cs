using Microsoft.AspNetCore.Mvc;

namespace Diplomski.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
