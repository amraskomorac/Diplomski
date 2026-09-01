using Diplomski.Data;
using Diplomski.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DiplomskiApp.Controllers;

[AllowAnonymous]
public class AuthController : Controller
{
    private readonly ApplicationDbContext _context;

    public AuthController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Registracija()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Registracija(RegistracijaViewModel model)
    {
        if (await _context.Korisnici.AnyAsync(k => k.Email == model.Email))
        {
            ModelState.AddModelError("", "Korisnik sa ovim emailom već postoji.");
            return View(model);
        }

        var korisnik = new Korisnik
        {
            PunoIme = model.PunoIme,
            Email = model.Email,
            LozinkaHash = BCrypt.Net.BCrypt.HashPassword(model.Lozinka),
            DatumKreiranja = DateTime.Now
        };

        _context.Korisnici.Add(korisnik);
        await _context.SaveChangesAsync();

        await PrijaviKorisnika(korisnik);
        return RedirectToAction("Dodaj", "Vozila");
    }

    public IActionResult Prijava()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Prijava(PrijavaViewModel model)
    {
        var korisnik = await _context.Korisnici.FirstOrDefaultAsync(k => k.Email == model.Email);

        if (korisnik == null || !BCrypt.Net.BCrypt.Verify(model.Lozinka, korisnik.LozinkaHash))
        {
            ModelState.AddModelError("", "Pogrešan email ili lozinka.");
            return View(model);
        }

        await PrijaviKorisnika(korisnik);

        return RedirectToAction("Index", "Dashboard");
    }

    private async Task PrijaviKorisnika(Korisnik korisnik)
    {
        HttpContext.Session.SetString("prijavljeni_korisnik_id", korisnik.Id.ToString());
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, korisnik.Id.ToString()),
            new Claim(ClaimTypes.Name, korisnik.PunoIme),
            new Claim(ClaimTypes.Email, korisnik.Email)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    }

    public IActionResult ZaboravljenaLozinka() => View();

    [HttpPost]
    public async Task<IActionResult> ZaboravljenaLozinka(ZaboravljenaLozinkaViewModel model)
    {
        var korisnik = await _context.Korisnici.FirstOrDefaultAsync(k => k.Email == model.Email);

        if (korisnik == null)
        {
            ViewBag.Poruka = "Ako email postoji, link za reset lozinke je kreiran.";
            return View();
        }

        var token = Guid.NewGuid().ToString();

        _context.TokeniZaResetLozinke.Add(new TokenZaResetLozinke
        {
            KorisnikId = korisnik.Id,
            Token = token,
            Istice = DateTime.Now.AddMinutes(30),
            Iskoristen = false
        });

        await _context.SaveChangesAsync();

        ViewBag.ResetLink = Url.Action("ResetLozinke", "Auth", new { token }, Request.Scheme);
        return View();
    }

    public IActionResult ResetLozinke(string token)
    {
        return View(new ResetLozinkeViewModel { Token = token });
    }

    [HttpPost]
    public async Task<IActionResult> ResetLozinke(ResetLozinkeViewModel model)
    {
        var token = await _context.TokeniZaResetLozinke
            .FirstOrDefaultAsync(t => t.Token == model.Token && !t.Iskoristen && t.Istice > DateTime.Now);

        if (token == null)
        {
            ModelState.AddModelError("", "Link nije validan ili je istekao.");
            return View(model);
        }

        var korisnik = await _context.Korisnici.FindAsync(token.KorisnikId);
        korisnik!.LozinkaHash = BCrypt.Net.BCrypt.HashPassword(model.NovaLozinka);

        token.Iskoristen = true;
        await _context.SaveChangesAsync();

        return RedirectToAction("Prijava");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Odjava()
    {
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Prijava");
    }
}
