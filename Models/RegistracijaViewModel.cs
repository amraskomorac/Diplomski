using Microsoft.AspNetCore.Mvc;

namespace Diplomski.Models
{
    public class RegistracijaViewModel
    {
        public string PunoIme { get; set; } = "";
        public string Email { get; set; } = "";
        public string Lozinka { get; set; } = "";
    }
}
