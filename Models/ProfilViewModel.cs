using Microsoft.AspNetCore.Mvc;

namespace Diplomski.Models
{
    public class ProfilViewModel : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
