using Microsoft.AspNetCore.Mvc;

namespace Diplomski.Models
{
    public class ResetLozinkeViewModel : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
