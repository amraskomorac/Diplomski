using Microsoft.AspNetCore.Mvc;

namespace Diplomski.Models
{
    public class PrijavaViewModel : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
