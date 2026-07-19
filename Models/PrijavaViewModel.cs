using Microsoft.AspNetCore.Mvc;

namespace Diplomski.Models
{
    public class PrijavaViewModel
    {
        public string Email { get; set; } = "";
        public string Lozinka { get; set; } = "";
    }
}
