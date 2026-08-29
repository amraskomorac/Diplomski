using System.Security.Claims;
using Diplomski.Data;
using Diplomski.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiplomskiApp.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index(int? voziloId, bool promijeniVozilo = false)
    {
        var korisnikId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var vozila = await _context.Vozila.Where(v => v.KorisnikId == korisnikId)
            .OrderBy(v => v.Marka).ThenBy(v => v.Model).ToListAsync();
        const string odabranoVoziloKey = "odabrano_vozilo_id";
        if (promijeniVozilo)
            HttpContext.Session.Remove(odabranoVoziloKey);

        var odabranoVozilo = voziloId.HasValue
            ? vozila.FirstOrDefault(v => v.Id == voziloId.Value)
            : null;
        if (odabranoVozilo is not null)
            HttpContext.Session.SetInt32(odabranoVoziloKey, odabranoVozilo.Id);
        else if (!promijeniVozilo && HttpContext.Session.GetInt32(odabranoVoziloKey) is int sacuvaniVoziloId)
            odabranoVozilo = vozila.FirstOrDefault(v => v.Id == sacuvaniVoziloId);
        else if (vozila.Count == 1)
            odabranoVozilo = vozila.First();
        var model = new DashboardViewModel { Vozila = vozila, OdabranoVozilo = odabranoVozilo };

        if (odabranoVozilo is null)
            return View(model);

        model.DatumIstekaRegistracije = odabranoVozilo.DatumIstekaRegistracije?.Date ?? odabranoVozilo.DatumRegistracije.Date.AddYears(1);
        model.DaniDoIstekaRegistracije = (model.DatumIstekaRegistracije - DateTime.Today).Days;
        var servisi = await _context.Servisi.Where(s => s.KorisnikId == korisnikId && s.VoziloId == odabranoVozilo.Id && s.Datum.Year == DateTime.Today.Year).SumAsync(s => (decimal?)s.Cijena) ?? 0;
        var gorivo = await _context.Goriva.Where(g => g.KorisnikId == korisnikId && g.VoziloId == odabranoVozilo.Id && g.Datum.Year == DateTime.Today.Year).SumAsync(g => (decimal?)g.Cijena) ?? 0;
        var registracija = odabranoVozilo.DatumRegistracije.Year == DateTime.Today.Year ? odabranoVozilo.CijenaRegistracije ?? 0 : 0;
        model.UkupniTrosakOveGodine = servisi + gorivo + registracija;
        model.HistorijaRegistracija = await _context.HistorijaRegistracija
            .Where(h => h.VoziloId == odabranoVozilo.Id)
            .OrderByDescending(h => h.DatumRegistracije)
            .ToListAsync();
        var posljednjiMaliServis = await ZadnjaKilometrazaServisa(korisnikId, odabranoVozilo.Id, "Mali servis") ?? odabranoVozilo.KilometrazaMaliServis;
        var posljednjiVelikiServis = await ZadnjaKilometrazaServisa(korisnikId, odabranoVozilo.Id, "Veliki servis") ?? odabranoVozilo.KilometrazaVelikiServis;
        model.PosljednjiMaliServis = PrikaziPosljednjiServis(posljednjiMaliServis);
        model.SljedeciMaliServis = PrikaziSljedeciServis(odabranoVozilo.TrenutnaKilometraza, posljednjiMaliServis, 10_000);
        model.PosljednjiVelikiServis = PrikaziPosljednjiServis(posljednjiVelikiServis);
        model.SljedeciVelikiServis = PrikaziSljedeciServis(odabranoVozilo.TrenutnaKilometraza, posljednjiVelikiServis, 100_000);
        await DodajPodsjetnike(model, odabranoVozilo, korisnikId, posljednjiMaliServis, posljednjiVelikiServis);
        const string obavijestiPrikazaneKey = "dashboard_obavijesti_prikazane";
        model.PrikaziObavijesti = model.AktivneObavijesti.Any()
            && HttpContext.Session.GetString(obavijestiPrikazaneKey) is null;
        if (model.PrikaziObavijesti)
            HttpContext.Session.SetString(obavijestiPrikazaneKey, "true");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AzurirajRegistraciju(DetaljiRegistracijeViewModel detalji)
    {
        var korisnikId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var vozilo = await _context.Vozila.FirstOrDefaultAsync(v => v.Id == detalji.VoziloId && v.KorisnikId == korisnikId);
        if (vozilo is null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            TempData["GreskaRegistracija"] = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage ?? "Podaci registracije nisu ispravni.";
            return RedirectToAction(nameof(Index), new { voziloId = detalji.VoziloId });
        }

        var jeNovaRegistracija = vozilo.DatumRegistracije.Date != detalji.DatumRegistracije.Date
            || vozilo.DatumIstekaRegistracije?.Date != detalji.DatumIstekaRegistracije.Date;
        if (jeNovaRegistracija)
        {
            _context.HistorijaRegistracija.Add(new HistorijaRegistracije
            {
                VoziloId = vozilo.Id,
                DatumRegistracije = vozilo.DatumRegistracije.Date,
                DatumIstekaRegistracije = vozilo.DatumIstekaRegistracije?.Date ?? vozilo.DatumRegistracije.Date.AddYears(1),
                Cijena = vozilo.CijenaRegistracije,
                PolicaOsiguranja = vozilo.PolicaOsiguranja
            });
        }

        vozilo.DatumRegistracije = detalji.DatumRegistracije.Date;
        vozilo.DatumIstekaRegistracije = detalji.DatumIstekaRegistracije.Date;
        vozilo.CijenaRegistracije = detalji.Cijena;
        vozilo.PolicaOsiguranja = detalji.PolicaOsiguranja.Trim();
        await _context.SaveChangesAsync();
        TempData["PorukaRegistracija"] = "Detalji registracije su uspješno ažurirani.";
        return RedirectToAction(nameof(Index), new { voziloId = detalji.VoziloId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AzurirajKilometrazu(int voziloId, int kilometraza)
    {
        var korisnikId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var vozilo = await _context.Vozila.FirstOrDefaultAsync(v => v.Id == voziloId && v.KorisnikId == korisnikId);

        if (vozilo is null)
            return NotFound();

        if (kilometraza < vozilo.TrenutnaKilometraza)
        {
            TempData["GreskaKilometraza"] = $"Nova kilometraza ne moze biti manja od trenutne ({vozilo.TrenutnaKilometraza:N0} km).";
            return RedirectToAction(nameof(Index), new { voziloId });
        }

        var jeIstaKilometraza = kilometraza == vozilo.TrenutnaKilometraza;
        vozilo.TrenutnaKilometraza = kilometraza;
        await _context.SaveChangesAsync();
        TempData["PorukaKilometraza"] = jeIstaKilometraza
            ? "Kilometraza je vec azurna."
            : "Kilometraza je uspjesno azurirana.";
        return RedirectToAction(nameof(Index), new { voziloId });
    }

    private static string PrikaziPosljednjiServis(int kilometraza) => kilometraza > 0 ? $"Na {kilometraza:N0} km" : "Nije evidentiran.";
    private static string PrikaziSljedeciServis(int trenutno, int zadnje, int interval)
    {
        if (zadnje <= 0) return "Nije moguce odrediti.";
        var preostalo = interval - (trenutno - zadnje);
        return preostalo <= 0 ? "Servis je dospio." : $"Za {preostalo:N0} km";
    }

    private async Task<int?> ZadnjaKilometrazaServisa(int korisnikId, int voziloId, params string[] tipovi)
    {
        return await _context.Servisi.Where(s => s.KorisnikId == korisnikId && s.VoziloId == voziloId && tipovi.Contains(s.Tip))
            .OrderByDescending(s => s.Datum).ThenByDescending(s => s.Id).Select(s => (int?)s.Kilometraza).FirstOrDefaultAsync();
    }

    private async Task DodajPodsjetnike(DashboardViewModel model, Vozilo vozilo, int korisnikId, int maliServis, int velikiServis)
    {
        var ulje = await ZadnjaKilometrazaServisa(korisnikId, vozilo.Id, "Mali servis", "Zamjena ulja") ?? maliServis;
        var filteri = await ZadnjaKilometrazaServisa(korisnikId, vozilo.Id, "Mali servis", "Filter ulja", "Filter zraka", "Filter kabine", "Filter goriva") ?? maliServis;
        var remen = await ZadnjaKilometrazaServisa(korisnikId, vozilo.Id, "Veliki servis", "Zupčasti remen") ?? velikiServis;
        var gume = await ZadnjaKilometrazaServisa(korisnikId, vozilo.Id, "Gume");
        var podsjetnici = new List<PodsjetnikViewModel>
        {
            KreirajKilometarskiPodsjetnik("Motorno ulje", "bi-droplet", ulje, vozilo.TrenutnaKilometraza, 10_000),
            KreirajKilometarskiPodsjetnik("Filteri", "bi-funnel", filteri, vozilo.TrenutnaKilometraza, 10_000),
            KreirajKilometarskiPodsjetnik("Zupcasti remen", "bi-gear", remen, vozilo.TrenutnaKilometraza, 100_000),
            KreirajKilometarskiPodsjetnik("Gume", "bi-circle", gume ?? 0, vozilo.TrenutnaKilometraza, 50_000),
            KreirajDatumskiPodsjetnik("Registracija", "bi-card-checklist", model.DatumIstekaRegistracije, model.DaniDoIstekaRegistracije),
            KreirajOsiguranjePodsjetnik(model.DatumIstekaRegistracije, model.DaniDoIstekaRegistracije, vozilo.PolicaOsiguranja)
        };
        model.AktivneObavijesti = podsjetnici.Where(p => p.JeHitno || p.JeUskoro).ToList();
        model.Podsjetnici = model.AktivneObavijesti;
        model.Upozorenja = model.AktivneObavijesti.Select(p => $"{p.Naziv}: {p.Status}").ToList();
    }

    private static PodsjetnikViewModel KreirajOsiguranjePodsjetnik(DateTime datumIsteka, int dana, string? polica)
    {
        if (string.IsNullOrWhiteSpace(polica))
            return KreirajNepodeseniPodsjetnik("Osiguranje", "bi-shield-check", "Dodajte policu osiguranja u detaljima registracije.");
        var podsjetnik = KreirajDatumskiPodsjetnik("Osiguranje", "bi-shield-check", datumIsteka, dana);
        podsjetnik.ZadnjiPut = $"Polica: {polica}";
        return podsjetnik;
    }

    private static PodsjetnikViewModel KreirajKilometarskiPodsjetnik(string naziv, string ikona, int zadnja, int trenutno, int interval)
    {
        if (zadnja <= 0) return KreirajNepodeseniPodsjetnik(naziv, ikona, "Unesite kilometrazu posljednje zamjene.");
        var preostalo = interval - (trenutno - zadnja);
        var jeHitno = preostalo < 0;
        var jeUskoro = !jeHitno && preostalo <= interval / 10;
        return new PodsjetnikViewModel { Naziv = naziv, Ikona = ikona, ZadnjiPut = $"{zadnja:N0} km", Interval = $"{interval:N0} km", Trenutno = $"{trenutno:N0} km", Status = jeHitno ? $"Servis kasni {Math.Abs(preostalo):N0} km" : $"Ostalo jos {preostalo:N0} km", JeHitno = jeHitno, JeUskoro = jeUskoro };
    }

    private static PodsjetnikViewModel KreirajDatumskiPodsjetnik(string naziv, string ikona, DateTime datumIsteka, int dana)
    {
        var jeHitno = dana < 0;
        var jeUskoro = !jeHitno && dana <= 30;
        var status = jeHitno ? $"Isteklo prije {Math.Abs(dana)} dana" : dana == 0 ? "Istice danas" : $"Istice za {dana} dana";
        return new PodsjetnikViewModel { Naziv = naziv, Ikona = ikona, ZadnjiPut = datumIsteka.AddYears(-1).ToString("dd.MM.yyyy."), Interval = "12 mjeseci", Trenutno = DateTime.Today.ToString("dd.MM.yyyy."), Status = status, JeHitno = jeHitno, JeUskoro = jeUskoro };
    }

    private static PodsjetnikViewModel KreirajNepodeseniPodsjetnik(string naziv, string ikona, string poruka) => new() { Naziv = naziv, Ikona = ikona, ZadnjiPut = "Nije evidentirano", Interval = "-", Trenutno = "-", Status = poruka };
}
