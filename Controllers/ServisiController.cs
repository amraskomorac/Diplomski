using System.Security.Claims;
using Diplomski.Data;
using Diplomski.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DiplomskiApp.Controllers;

[Authorize]
public class ServisiController : Controller
{
    private static readonly string[] DozvoljeneEkstenzije = [".jpg", ".jpeg", ".png", ".pdf"];
    private static readonly string[] DozvoljeniTipovi = ["image/jpeg", "image/png", "application/pdf"];
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public ServisiController(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<IActionResult> Index()
    {
        var servisi = await _context.Servisi
            .Include(s => s.Vozilo)
            .Where(s => s.KorisnikId == TrenutniKorisnikId())
            .OrderByDescending(s => s.Datum)
            .ThenByDescending(s => s.Id)
            .ToListAsync();

        return View(servisi);
    }

    public async Task<IActionResult> Dodaj()
    {
        await UcitajVozila();
        return View(new Servis { Datum = DateTime.Today });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Dodaj(Servis servis, IFormFile? racunDatoteka)
    {
        var korisnikId = TrenutniKorisnikId();
        await ValidirajVozilo(servis.VoziloId, servis.Kilometraza, korisnikId);
        ValidirajRacun(racunDatoteka);

        if (!ModelState.IsValid)
        {
            await UcitajVozila();
            return View(servis);
        }

        servis.KorisnikId = korisnikId;
        servis.PutanjaRacuna = await SacuvajRacun(racunDatoteka);
        _context.Servisi.Add(servis);
        await _context.SaveChangesAsync();

        TempData["Poruka"] = "Servis je uspješno dodan.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Uredi(int id)
    {
        var servis = await PronadjiServis(id);
        if (servis is null)
            return NotFound();

        await UcitajVozila();
        return View(servis);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Uredi(int id, Servis servis, IFormFile? racunDatoteka)
    {
        var postojeciServis = await PronadjiServis(id);
        if (postojeciServis is null)
            return NotFound();

        var korisnikId = TrenutniKorisnikId();
        await ValidirajVozilo(servis.VoziloId, servis.Kilometraza, korisnikId);
        ValidirajRacun(racunDatoteka);

        if (!ModelState.IsValid)
        {
            servis.Id = id;
            servis.PutanjaRacuna = postojeciServis.PutanjaRacuna;
            await UcitajVozila();
            return View(servis);
        }

        var novaPutanjaRacuna = await SacuvajRacun(racunDatoteka);
        if (novaPutanjaRacuna is not null)
        {
            ObrisiRacun(postojeciServis.PutanjaRacuna);
            postojeciServis.PutanjaRacuna = novaPutanjaRacuna;
        }

        postojeciServis.Tip = servis.Tip;
        postojeciServis.Datum = servis.Datum;
        postojeciServis.Kilometraza = servis.Kilometraza;
        postojeciServis.Cijena = servis.Cijena;
        postojeciServis.Serviser = servis.Serviser;
        postojeciServis.Napomena = servis.Napomena;
        postojeciServis.VoziloId = servis.VoziloId;
        await _context.SaveChangesAsync();

        TempData["Poruka"] = "Servis je uspješno izmijenjen.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Obrisi(int id)
    {
        var servis = await PronadjiServis(id);
        if (servis is null)
            return NotFound();

        ObrisiRacun(servis.PutanjaRacuna);
        _context.Servisi.Remove(servis);
        await _context.SaveChangesAsync();

        TempData["Poruka"] = "Servis je uspješno obrisan.";
        return RedirectToAction(nameof(Index));
    }

    private async Task UcitajVozila()
    {
        ViewBag.Vozila = await _context.Vozila
            .Where(v => v.KorisnikId == TrenutniKorisnikId())
            .OrderBy(v => v.Marka)
            .ThenBy(v => v.Model)
            .Select(v => new SelectListItem
            {
                Value = v.Id.ToString(),
                Text = $"{v.Marka} {v.Model} ({v.Registracija})"
            })
            .ToListAsync();
    }

    private async Task ValidirajVozilo(int? voziloId, int kilometrazaServisa, int korisnikId)
    {
        var vozilo = voziloId.HasValue
            ? await _context.Vozila.FirstOrDefaultAsync(v => v.Id == voziloId.Value && v.KorisnikId == korisnikId)
            : null;

        if (vozilo is null)
        {
            ModelState.AddModelError(nameof(Servis.VoziloId), "Odaberite svoje vozilo.");
            return;
        }

        if (kilometrazaServisa > vozilo.TrenutnaKilometraza)
            ModelState.AddModelError(nameof(Servis.Kilometraza), "Kilometraža servisa ne može biti veća od trenutne kilometraže vozila.");
    }

    private void ValidirajRacun(IFormFile? racunDatoteka)
    {
        if (racunDatoteka is null || racunDatoteka.Length == 0)
            return;

        var ekstenzija = Path.GetExtension(racunDatoteka.FileName).ToLowerInvariant();
        if (!DozvoljeneEkstenzije.Contains(ekstenzija) || !DozvoljeniTipovi.Contains(racunDatoteka.ContentType))
            ModelState.AddModelError(nameof(racunDatoteka), "Račun mora biti JPG, PNG ili PDF datoteka.");

        if (racunDatoteka.Length > 5 * 1024 * 1024)
            ModelState.AddModelError(nameof(racunDatoteka), "Račun ne može biti veći od 5 MB.");
    }

    private async Task<string?> SacuvajRacun(IFormFile? racunDatoteka)
    {
        if (racunDatoteka is null || racunDatoteka.Length == 0)
            return null;

        var ekstenzija = Path.GetExtension(racunDatoteka.FileName).ToLowerInvariant();
        var nazivDatoteke = $"{Guid.NewGuid()}{ekstenzija}";
        var relativnaPutanja = $"/uploads/racuni/{nazivDatoteke}";
        var direktorij = Path.Combine(_environment.WebRootPath, "uploads", "racuni");
        Directory.CreateDirectory(direktorij);

        await using var stream = System.IO.File.Create(Path.Combine(direktorij, nazivDatoteke));
        await racunDatoteka.CopyToAsync(stream);
        return relativnaPutanja;
    }

    private void ObrisiRacun(string? relativnaPutanja)
    {
        if (string.IsNullOrWhiteSpace(relativnaPutanja))
            return;

        var nazivDatoteke = Path.GetFileName(relativnaPutanja);
        var putanja = Path.Combine(_environment.WebRootPath, "uploads", "racuni", nazivDatoteke);
        if (System.IO.File.Exists(putanja))
            System.IO.File.Delete(putanja);
    }

    private async Task<Servis?> PronadjiServis(int id)
    {
        return await _context.Servisi
            .FirstOrDefaultAsync(s => s.Id == id && s.KorisnikId == TrenutniKorisnikId());
    }

    private int TrenutniKorisnikId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
