using Microsoft.AspNetCore.Mvc;

namespace Diplomski.Models
{
    public class ZaboravljenaLozinkaViewModel : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
