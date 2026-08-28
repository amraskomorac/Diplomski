using System.Security.Claims;
using System.Globalization;
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

    public async Task<IActionResult> Index(int? voziloId)
    {
        var korisnikId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var vozila = await _context.Vozila.Where(v => v.KorisnikId == korisnikId)
            .OrderBy(v => v.Marka).ThenBy(v => v.Model).ToListAsync();
        // Kada korisnik ima vise vozila, najprije treba odabrati ono za koje zeli
        // vidjeti dashboard. Za jedno vozilo zadrzavamo direktan prikaz dashboarda.
        var odabranoVozilo = voziloId.HasValue
            ? vozila.FirstOrDefault(v => v.Id == voziloId.Value)
            : vozila.Count == 1 ? vozila.First() : null;
        var model = new DashboardViewModel { Vozila = vozila, OdabranoVozilo = odabranoVozilo };

        if (odabranoVozilo is null)
            return View(model);

        model.DatumIstekaRegistracije = odabranoVozilo.DatumRegistracije.Date.AddYears(1);
        model.DaniDoIstekaRegistracije = (model.DatumIstekaRegistracije - DateTime.Today).Days;
        var servisi = await _context.Servisi.Where(s => s.KorisnikId == korisnikId && s.VoziloId == odabranoVozilo.Id && s.Datum.Year == DateTime.Today.Year).SumAsync(s => (decimal?)s.Cijena) ?? 0;
        var gorivo = await _context.Goriva.Where(g => g.KorisnikId == korisnikId && g.VoziloId == odabranoVozilo.Id && g.Datum.Year == DateTime.Today.Year).SumAsync(g => (decimal?)g.Cijena) ?? 0;
        model.UkupniTrosakOveGodine = servisi + gorivo;
        model.HistorijaOdrzavanja = await _context.Servisi.Where(s => s.KorisnikId == korisnikId && s.VoziloId == odabranoVozilo.Id).OrderByDescending(s => s.Datum).Take(12).Select(s => new StavkaHistorijeOdrzavanja
        {
            ServisId = s.Id,
            Datum = s.Datum,
            Naziv = s.Tip,
            Kilometraza = s.Kilometraza,
            Cijena = s.Cijena,
            Serviser = s.Serviser,
            Napomena = s.Napomena,
            PutanjaRacuna = s.PutanjaRacuna
        }).ToListAsync();
        model.HistorijaOdrzavanja.Add(new StavkaHistorijeOdrzavanja { Datum = odabranoVozilo.DatumRegistracije, Naziv = "Registracija", JeRegistracija = true });
        model.HistorijaOdrzavanja = model.HistorijaOdrzavanja.OrderByDescending(h => h.Datum).ToList();
        model.PosljednjiMaliServis = PrikaziPosljednjiServis(odabranoVozilo.KilometrazaMaliServis);
        model.SljedeciMaliServis = PrikaziSljedeciServis(odabranoVozilo.TrenutnaKilometraza, odabranoVozilo.KilometrazaMaliServis, 10_000);
        model.PosljednjiVelikiServis = PrikaziPosljednjiServis(odabranoVozilo.KilometrazaVelikiServis);
        model.SljedeciVelikiServis = PrikaziSljedeciServis(odabranoVozilo.TrenutnaKilometraza, odabranoVozilo.KilometrazaVelikiServis, 100_000);
        DodajPodsjetnike(model, odabranoVozilo);
        const string obavijestiPrikazaneKey = "dashboard_obavijesti_prikazane";
        model.PrikaziObavijesti = model.AktivneObavijesti.Any()
            && HttpContext.Session.GetString(obavijestiPrikazaneKey) is null;
        if (model.PrikaziObavijesti)
            HttpContext.Session.SetString(obavijestiPrikazaneKey, "true");
        return View(model);
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

    private static void DodajPodsjetnike(DashboardViewModel model, Vozilo vozilo)
    {
        var podsjetnici = new List<PodsjetnikViewModel>
        {
            KreirajKilometarskiPodsjetnik("Motorno ulje", "bi-droplet", vozilo.KilometrazaMaliServis, vozilo.TrenutnaKilometraza, 10_000),
            KreirajKilometarskiPodsjetnik("Filteri", "bi-funnel", vozilo.KilometrazaMaliServis, vozilo.TrenutnaKilometraza, 10_000),
            KreirajKilometarskiPodsjetnik("Zupcasti remen", "bi-gear", vozilo.KilometrazaVelikiServis, vozilo.TrenutnaKilometraza, 100_000),
            KreirajNepodeseniPodsjetnik("Gume", "bi-circle", "Dodajte datum ili kilometrazu zamjene guma u servisima."),
            KreirajDatumskiPodsjetnik("Registracija", "bi-card-checklist", model.DatumIstekaRegistracije, model.DaniDoIstekaRegistracije),
            KreirajNepodeseniPodsjetnik("Osiguranje", "bi-shield-check", "Dodajte datum isteka osiguranja za automatsko pracenje.")
        };
        model.AktivneObavijesti = podsjetnici.Where(p => p.JeHitno || p.JeUskoro).ToList();
        model.Podsjetnici = model.AktivneObavijesti;
        model.Upozorenja = model.AktivneObavijesti.Select(p => $"{p.Naziv}: {p.Status}").ToList();
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
