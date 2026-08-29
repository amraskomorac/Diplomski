using System.Security.Claims;
using Diplomski.Data;
using Diplomski.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DiplomskiApp.Controllers;

[Authorize]
public class GorivoController : Controller
{
    private readonly ApplicationDbContext _context;
    public GorivoController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index(int? voziloId)
    {
        var korisnikId = TrenutniKorisnikId();
        var vozila = await _context.Vozila.Where(v => v.KorisnikId == korisnikId).OrderBy(v => v.Marka).ToListAsync();
        var odabrano = voziloId.HasValue && vozila.Any(v => v.Id == voziloId.Value)
            ? voziloId
            : HttpContext.Session.GetInt32("odabrano_vozilo_id");
        if (odabrano.HasValue && !vozila.Any(v => v.Id == odabrano.Value))
            odabrano = null;
        if (!odabrano.HasValue && vozila.Count == 1)
            odabrano = vozila[0].Id;
        if (voziloId.HasValue && odabrano.HasValue)
            HttpContext.Session.SetInt32("odabrano_vozilo_id", odabrano.Value);
        var zapisi = odabrano.HasValue ? await _context.Goriva.Where(g => g.KorisnikId == korisnikId && g.VoziloId == odabrano.Value).OrderByDescending(g => g.Kilometraza).ToListAsync() : [];
        var poredani = zapisi.OrderBy(g => g.Kilometraza).ToList();
        var predjeniKm = poredani.Zip(poredani.Skip(1), (a, b) => Math.Max(0, b.Kilometraza - a.Kilometraza)).Sum();
        var ukupno = zapisi.Sum(g => g.Cijena);
        return View(new GorivoIndexViewModel { Vozila = vozila, VoziloId = odabrano, Zapisi = zapisi, UkupnoPotroseno = ukupno, ProsjecnaPotrosnja = predjeniKm > 0 ? zapisi.Skip(1).Sum(g => g.Litara) * 100 / predjeniKm : null, CijenaPoKilometru = predjeniKm > 0 ? ukupno / predjeniKm : null });
    }

    public async Task<IActionResult> Dodaj()
    {
        var odabranoVozilo = await OdabranoVozilo();
        await UcitajVozila(odabranoVozilo);
        return View(new Gorivo { Datum = DateTime.Today, VoziloId = odabranoVozilo?.Id ?? 0 });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Dodaj(Gorivo gorivo)
    {
        var korisnikId = TrenutniKorisnikId();
        var odabranoVozilo = await OdabranoVozilo();
        if (odabranoVozilo is not null)
            gorivo.VoziloId = odabranoVozilo.Id;
        var vozilo = await _context.Vozila.FirstOrDefaultAsync(v => v.Id == gorivo.VoziloId && v.KorisnikId == korisnikId);
        if (vozilo is null) ModelState.AddModelError(nameof(Gorivo.VoziloId), "Odaberite svoje vozilo.");
        else if (gorivo.Kilometraza < vozilo.TrenutnaKilometraza) ModelState.AddModelError(nameof(Gorivo.Kilometraza), $"Kilometraza ne moze biti manja od trenutne kilometraze vozila ({vozilo.TrenutnaKilometraza:N0} km).");
        if (gorivo.Datum.Date > DateTime.Today) ModelState.AddModelError(nameof(Gorivo.Datum), "Datum ne moze biti u buducnosti.");
        if (!ModelState.IsValid) { await UcitajVozila(odabranoVozilo); return View(gorivo); }
        gorivo.KorisnikId = korisnikId; _context.Goriva.Add(gorivo);
        if (vozilo is not null && gorivo.Kilometraza > vozilo.TrenutnaKilometraza) vozilo.TrenutnaKilometraza = gorivo.Kilometraza;
        await _context.SaveChangesAsync();
        TempData["Poruka"] = "Unos goriva je sacuvan."; return RedirectToAction(nameof(Index), new { voziloId = gorivo.VoziloId });
    }

    public async Task<IActionResult> Uredi(int id)
    {
        var gorivo = await PronadjiGorivo(id);
        if (gorivo is null) return NotFound();
        await UcitajVozila();
        return View(gorivo);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Uredi(int id, Gorivo gorivo)
    {
        var postojece = await PronadjiGorivo(id);
        if (postojece is null) return NotFound();
        var korisnikId = TrenutniKorisnikId();
        var vozilo = await _context.Vozila.FirstOrDefaultAsync(v => v.Id == gorivo.VoziloId && v.KorisnikId == korisnikId);
        if (vozilo is null) ModelState.AddModelError(nameof(Gorivo.VoziloId), "Odaberite svoje vozilo.");
        else if (gorivo.Kilometraza < vozilo.TrenutnaKilometraza) ModelState.AddModelError(nameof(Gorivo.Kilometraza), $"Kilometraza ne moze biti manja od trenutne kilometraze vozila ({vozilo.TrenutnaKilometraza:N0} km).");
        if (gorivo.Datum.Date > DateTime.Today) ModelState.AddModelError(nameof(Gorivo.Datum), "Datum ne moze biti u buducnosti.");
        if (!ModelState.IsValid) { gorivo.Id = id; await UcitajVozila(); return View(gorivo); }
        postojece.VoziloId = gorivo.VoziloId; postojece.Datum = gorivo.Datum; postojece.Litara = gorivo.Litara; postojece.Cijena = gorivo.Cijena; postojece.Kilometraza = gorivo.Kilometraza;
        if (vozilo is not null && gorivo.Kilometraza > vozilo.TrenutnaKilometraza) vozilo.TrenutnaKilometraza = gorivo.Kilometraza;
        await _context.SaveChangesAsync();
        TempData["Poruka"] = "Unos goriva je izmijenjen."; return RedirectToAction(nameof(Index), new { voziloId = gorivo.VoziloId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Obrisi(int id)
    {
        var gorivo = await PronadjiGorivo(id);
        if (gorivo is null) return NotFound();
        var voziloId = gorivo.VoziloId; _context.Goriva.Remove(gorivo); await _context.SaveChangesAsync();
        TempData["Poruka"] = "Unos goriva je obrisan."; return RedirectToAction(nameof(Index), new { voziloId });
    }
    private async Task UcitajVozila(Vozilo? odabranoVozilo = null)
    {
        ViewBag.Vozila = await _context.Vozila.Where(v => v.KorisnikId == TrenutniKorisnikId()).Select(v => new SelectListItem { Value = v.Id.ToString(), Text = $"{v.Marka} {v.Model} ({v.Registracija})" }).ToListAsync();
        ViewBag.OdabranoVoziloNaziv = odabranoVozilo is null ? null : $"{odabranoVozilo.Marka} {odabranoVozilo.Model} ({odabranoVozilo.Registracija})";
    }
    private async Task<Vozilo?> OdabranoVozilo()
    {
        var voziloId = HttpContext.Session.GetInt32("odabrano_vozilo_id");
        return voziloId.HasValue ? await _context.Vozila.FirstOrDefaultAsync(v => v.Id == voziloId.Value && v.KorisnikId == TrenutniKorisnikId()) : null;
    }
    private Task<Gorivo?> PronadjiGorivo(int id) => _context.Goriva.FirstOrDefaultAsync(g => g.Id == id && g.KorisnikId == TrenutniKorisnikId());
    private int TrenutniKorisnikId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
