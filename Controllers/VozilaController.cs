using System.Security.Claims;
using Diplomski.Data;
using Diplomski.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiplomskiApp.Controllers;

[Authorize]
public class VozilaController : Controller
{
    private readonly ApplicationDbContext _context;

    public VozilaController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var vozila = await _context.Vozila
            .Where(v => v.KorisnikId == TrenutniKorisnikId())
            .OrderBy(v => v.Marka)
            .ToListAsync();

        return View(vozila);
    }

    public IActionResult Dodaj() => View(new Vozilo
    {
        DatumKupovine = DateTime.Today,
        DatumRegistracije = DateTime.Today
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Dodaj(Vozilo vozilo)
    {
        NormalizujPodatke(vozilo);

        if (!ModelState.IsValid)
            return View(vozilo);

        vozilo.KorisnikId = TrenutniKorisnikId();
        _context.Vozila.Add(vozilo);
        await _context.SaveChangesAsync();

        TempData["Poruka"] = "Vozilo je uspješno dodano.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Uredi(int id)
    {
        var vozilo = await PronadjiVozilo(id);
        return vozilo is null ? NotFound() : View(vozilo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Uredi(int id, Vozilo vozilo)
    {
        var postojeceVozilo = await PronadjiVozilo(id);
        if (postojeceVozilo is null)
            return NotFound();

        NormalizujPodatke(vozilo);
        if (!ModelState.IsValid)
        {
            vozilo.Id = id;
            return View(vozilo);
        }

        postojeceVozilo.Marka = vozilo.Marka;
        postojeceVozilo.Model = vozilo.Model;
        postojeceVozilo.GodinaProizvodnje = vozilo.GodinaProizvodnje;
        postojeceVozilo.Registracija = vozilo.Registracija;
        postojeceVozilo.TipGoriva = vozilo.TipGoriva;
        postojeceVozilo.TipMjenjaca = vozilo.TipMjenjaca;
        postojeceVozilo.TrenutnaKilometraza = vozilo.TrenutnaKilometraza;
        postojeceVozilo.DatumKupovine = vozilo.DatumKupovine;
        postojeceVozilo.DatumRegistracije = vozilo.DatumRegistracije;
        postojeceVozilo.KilometrazaMaliServis = vozilo.KilometrazaMaliServis;
        postojeceVozilo.KilometrazaVelikiServis = vozilo.KilometrazaVelikiServis;
        await _context.SaveChangesAsync();

        TempData["Poruka"] = "Vozilo je uspješno izmijenjeno.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Obrisi(int id)
    {
        var vozilo = await PronadjiVozilo(id);
        if (vozilo is null)
            return NotFound();

        _context.Vozila.Remove(vozilo);
        await _context.SaveChangesAsync();

        TempData["Poruka"] = "Vozilo je uspješno obrisano.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<Vozilo?> PronadjiVozilo(int id)
    {
        return await _context.Vozila
            .FirstOrDefaultAsync(v => v.Id == id && v.KorisnikId == TrenutniKorisnikId());
    }

    private static void NormalizujPodatke(Vozilo vozilo)
    {
        vozilo.Marka = vozilo.Marka.Trim();
        vozilo.Model = vozilo.Model.Trim();
        vozilo.Registracija = vozilo.Registracija.Trim().ToUpperInvariant();
        vozilo.TipMjenjaca = vozilo.TipMjenjaca.Trim().ToLowerInvariant();
    }

    private int TrenutniKorisnikId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
