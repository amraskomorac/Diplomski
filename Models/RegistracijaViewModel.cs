using Microsoft.AspNetCore.Mvc;

namespace Diplomski.Models
{
    public class RegistracijaViewModel : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
