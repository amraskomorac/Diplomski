using Microsoft.AspNetCore.Mvc;

namespace Diplomski.Models
{
    public class Korisnik : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
