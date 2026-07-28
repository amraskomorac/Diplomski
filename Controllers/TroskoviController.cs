using System.Security.Claims;
using Diplomski.Data;
using Diplomski.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DiplomskiApp.Controllers;

[Authorize]
public class TroskoviController : Controller
{
    private readonly ApplicationDbContext _context;

    public TroskoviController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var troskovi = await _context.Troskovi
            .Include(t => t.Vozilo)
            .Where(t => t.KorisnikId == TrenutniKorisnikId())
            .OrderByDescending(t => t.Datum)
            .ToListAsync();

        return View(troskovi);
    }

    public async Task<IActionResult> Dodaj()
    {
        await UcitajVozila();
        return View(new Trosak { Datum = DateTime.Today });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Dodaj(Trosak trosak)
    {
        var korisnikId = TrenutniKorisnikId();
        var voziloPripadaKorisniku = trosak.VoziloId.HasValue && await _context.Vozila
            .AnyAsync(v => v.Id == trosak.VoziloId.Value && v.KorisnikId == korisnikId);

        if (!voziloPripadaKorisniku)
            ModelState.AddModelError(nameof(Trosak.VoziloId), "Odaberite svoje vozilo.");

        if (!ModelState.IsValid)
        {
            await UcitajVozila();
            return View(trosak);
        }

        trosak.KorisnikId = korisnikId;
        _context.Troskovi.Add(trosak);
        await _context.SaveChangesAsync();

        TempData["Poruka"] = "Trošak je uspješno dodan.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Uredi(int id)
    {
        var trosak = await PronadjiTrosak(id);
        if (trosak is null)
            return NotFound();

        await UcitajVozila();
        return View(trosak);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Uredi(int id, Trosak trosak)
    {
        var postojeciTrosak = await PronadjiTrosak(id);
        if (postojeciTrosak is null)
            return NotFound();

        var korisnikId = TrenutniKorisnikId();
        var voziloPripadaKorisniku = trosak.VoziloId.HasValue && await _context.Vozila
            .AnyAsync(v => v.Id == trosak.VoziloId.Value && v.KorisnikId == korisnikId);

        if (!voziloPripadaKorisniku)
            ModelState.AddModelError(nameof(Trosak.VoziloId), "Odaberite svoje vozilo.");

        if (!ModelState.IsValid)
        {
            trosak.Id = id;
            await UcitajVozila();
            return View(trosak);
        }

        postojeciTrosak.Tip = trosak.Tip;
        postojeciTrosak.Iznos = trosak.Iznos;
        postojeciTrosak.Datum = trosak.Datum;
        postojeciTrosak.VoziloId = trosak.VoziloId;
        await _context.SaveChangesAsync();

        TempData["Poruka"] = "Trošak je uspješno izmijenjen.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Obrisi(int id)
    {
        var trosak = await PronadjiTrosak(id);
        if (trosak is null)
            return NotFound();

        _context.Troskovi.Remove(trosak);
        await _context.SaveChangesAsync();

        TempData["Poruka"] = "Trošak je uspješno obrisan.";
        return RedirectToAction(nameof(Index));
    }

    private async Task UcitajVozila()
    {
        var vozila = await _context.Vozila
            .Where(v => v.KorisnikId == TrenutniKorisnikId())
            .OrderBy(v => v.Marka)
            .ThenBy(v => v.Model)
            .Select(v => new SelectListItem
            {
                Value = v.Id.ToString(),
                Text = $"{v.Marka} {v.Model} ({v.Registracija})"
            })
            .ToListAsync();

        ViewBag.Vozila = vozila;
    }

    private async Task<Trosak?> PronadjiTrosak(int id)
    {
        return await _context.Troskovi
            .FirstOrDefaultAsync(t => t.Id == id && t.KorisnikId == TrenutniKorisnikId());
    }

    private int TrenutniKorisnikId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
