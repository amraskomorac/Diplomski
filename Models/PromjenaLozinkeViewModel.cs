using Microsoft.AspNetCore.Mvc;

namespace Diplomski.Models
{
    public class PromjenaLozinkeViewModel : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
