using Microsoft.AspNetCore.Mvc;

namespace Diplomski.Models
{
    public class ResetLozinkeViewModel
    {
        public string Token { get; set; } = "";
        public string NovaLozinka { get; set; } = "";
    }
}
