using System.Security.Claims;
using Diplomski.Data;
using Diplomski.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiplomskiApp.Controllers;

[Authorize]
public class DokumentiController : Controller
{
    private static readonly string[] Tipovi = ["Vozačka dozvola", "Saobraćajna", "Osiguranje"];
    private static readonly string[] DozvoljeneEkstenzije = [".jpg", ".jpeg", ".png", ".pdf"];
    private static readonly string[] DozvoljeniTipovi = ["image/jpeg", "image/png", "application/pdf"];
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public DokumentiController(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<IActionResult> Index()
    {
        var dokumenti = await _context.Dokumenti
            .Where(d => d.KorisnikId == TrenutniKorisnikId())
            .OrderBy(d => d.Tip)
            .ToListAsync();
        return View(new DokumentiViewModel { Dokumenti = dokumenti });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sacuvaj(string tip, IFormFile? datoteka, string? povratak)
    {
        if (!Tipovi.Contains(tip))
            return BadRequest();

        ValidirajDatoteku(datoteka);
        if (!ModelState.IsValid)
        {
            TempData["Greska"] = ModelState.Values.SelectMany(v => v.Errors).First().ErrorMessage;
            return Povratak(povratak);
        }

        var korisnikId = TrenutniKorisnikId();
        var postojeci = await _context.Dokumenti
            .FirstOrDefaultAsync(d => d.KorisnikId == korisnikId && d.Tip == tip);
        var spremljeno = await SacuvajDatoteku(datoteka!);

        if (postojeci is null)
        {
            _context.Dokumenti.Add(new Dokument
            {
                Tip = tip,
                NazivDatoteke = Path.GetFileName(datoteka!.FileName),
                Putanja = spremljeno,
                DatumIzmjene = DateTime.UtcNow,
                KorisnikId = korisnikId
            });
        }
        else
        {
            ObrisiDatoteku(postojeci.Putanja);
            postojeci.NazivDatoteke = Path.GetFileName(datoteka!.FileName);
            postojeci.Putanja = spremljeno;
            postojeci.DatumIzmjene = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        TempData["Poruka"] = $"Dokument \"{tip}\" je uspješno sačuvan.";
        return Povratak(povratak);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Obrisi(int id, string? povratak)
    {
        var dokument = await _context.Dokumenti
            .FirstOrDefaultAsync(d => d.Id == id && d.KorisnikId == TrenutniKorisnikId());
        if (dokument is null)
            return NotFound();

        ObrisiDatoteku(dokument.Putanja);
        _context.Dokumenti.Remove(dokument);
        await _context.SaveChangesAsync();
        TempData["Poruka"] = $"Dokument \"{dokument.Tip}\" je uklonjen.";
        return Povratak(povratak);
    }

    private void ValidirajDatoteku(IFormFile? datoteka)
    {
        if (datoteka is null || datoteka.Length == 0)
        {
            ModelState.AddModelError(nameof(datoteka), "Odaberite dokument za upload.");
            return;
        }

        var ekstenzija = Path.GetExtension(datoteka.FileName).ToLowerInvariant();
        if (!DozvoljeneEkstenzije.Contains(ekstenzija) || !DozvoljeniTipovi.Contains(datoteka.ContentType))
            ModelState.AddModelError(nameof(datoteka), "Dokument mora biti JPG, PNG ili PDF datoteka.");
        if (datoteka.Length > 5 * 1024 * 1024)
            ModelState.AddModelError(nameof(datoteka), "Dokument ne može biti veći od 5 MB.");
    }

    private async Task<string> SacuvajDatoteku(IFormFile datoteka)
    {
        var ekstenzija = Path.GetExtension(datoteka.FileName).ToLowerInvariant();
        var nazivDatoteke = $"{Guid.NewGuid()}{ekstenzija}";
        var direktorij = Path.Combine(_environment.WebRootPath, "uploads", "dokumenti", TrenutniKorisnikId().ToString());
        Directory.CreateDirectory(direktorij);
        await using var stream = System.IO.File.Create(Path.Combine(direktorij, nazivDatoteke));
        await datoteka.CopyToAsync(stream);
        return $"/uploads/dokumenti/{TrenutniKorisnikId()}/{nazivDatoteke}";
    }

    private void ObrisiDatoteku(string relativnaPutanja)
    {
        var nazivDatoteke = Path.GetFileName(relativnaPutanja);
        var putanja = Path.Combine(_environment.WebRootPath, "uploads", "dokumenti", TrenutniKorisnikId().ToString(), nazivDatoteke);
        if (System.IO.File.Exists(putanja))
            System.IO.File.Delete(putanja);
    }

    private IActionResult Povratak(string? povratak)
    {
        if (!string.IsNullOrWhiteSpace(povratak) && Url.IsLocalUrl(povratak))
        {
            TempData["PrikaziDokumente"] = true;
            return LocalRedirect(povratak);
        }

        return RedirectToAction(nameof(Index));
    }

    private int TrenutniKorisnikId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
