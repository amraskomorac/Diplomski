using Diplomski.Data;
using Diplomski.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DiplomskiApp.Controllers;

[Authorize]
public class ProfilController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProfilController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var korisnik = await _context.Korisnici.FindAsync(id);

        return View(new ProfilViewModel
        {
            PunoIme = korisnik!.PunoIme,
            Email = korisnik.Email
        });
    }

    [HttpPost]
    public async Task<IActionResult> Index(ProfilViewModel model)
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var korisnik = await _context.Korisnici.FindAsync(id);

        korisnik!.PunoIme = model.PunoIme;
        korisnik.Email = model.Email;

        await _context.SaveChangesAsync();

        ViewBag.Poruka = "Profil je uspješno ažuriran.";
        return View(model);
    }

    public IActionResult PromjenaLozinke() => View();

    [HttpPost]
    public async Task<IActionResult> PromjenaLozinke(PromjenaLozinkeViewModel model)
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var korisnik = await _context.Korisnici.FindAsync(id);

        if (!BCrypt.Net.BCrypt.Verify(model.TrenutnaLozinka, korisnik!.LozinkaHash))
        {
            ModelState.AddModelError("", "Trenutna lozinka nije ispravna.");
            return View(model);
        }

        korisnik.LozinkaHash = BCrypt.Net.BCrypt.HashPassword(model.NovaLozinka);
        await _context.SaveChangesAsync();

        ViewBag.Poruka = "Lozinka je uspješno promijenjena.";
        return View();
    }
}