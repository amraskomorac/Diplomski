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

    public async Task<IActionResult> Index(int? voziloId)
    {
        var korisnikId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var vozila = await _context.Vozila
            .Where(v => v.KorisnikId == korisnikId)
            .OrderBy(v => v.Marka)
            .ThenBy(v => v.Model)
            .ToListAsync();

        var odabranoVozilo = voziloId.HasValue
            ? vozila.FirstOrDefault(v => v.Id == voziloId.Value)
            : vozila.FirstOrDefault();

        var model = new DashboardViewModel
        {
            Vozila = vozila,
            OdabranoVozilo = odabranoVozilo
        };

        if (odabranoVozilo is null)
            return View(model);

        model.DatumIstekaRegistracije = odabranoVozilo.DatumRegistracije.Date.AddYears(1);
        model.DaniDoIstekaRegistracije = (model.DatumIstekaRegistracije - DateTime.Today).Days;
        model.UkupniTrosakOveGodine = await _context.Troskovi
            .Where(t => t.KorisnikId == korisnikId
                && t.VoziloId == odabranoVozilo.Id
                && t.Datum.Year == DateTime.Today.Year)
            .SumAsync(t => (decimal?)t.Iznos) ?? 0;
        OdrediPodatkeOServisu(model, odabranoVozilo);
        DodajUpozorenja(model, odabranoVozilo);

        return View(model);
    }

    private static void OdrediPodatkeOServisu(DashboardViewModel model, Vozilo vozilo)
    {
        model.PosljednjiMaliServis = PrikaziPosljednjiServis(vozilo.KilometrazaMaliServis);
        model.SljedeciMaliServis = PrikaziSljedeciServis(vozilo.TrenutnaKilometraza, vozilo.KilometrazaMaliServis, 10_000);
        model.PosljednjiVelikiServis = PrikaziPosljednjiServis(vozilo.KilometrazaVelikiServis);
        model.SljedeciVelikiServis = PrikaziSljedeciServis(vozilo.TrenutnaKilometraza, vozilo.KilometrazaVelikiServis, 100_000);
    }

    private static string PrikaziPosljednjiServis(int kilometrazaServisa)
    {
        return kilometrazaServisa > 0
            ? $"Na {kilometrazaServisa:N0} km"
            : "Nije evidentiran.";
    }

    private static string PrikaziSljedeciServis(int trenutnaKilometraza, int kilometrazaServisa, int interval)
    {
        if (kilometrazaServisa <= 0)
            return "Nije moguće odrediti.";

        var preostalo = interval - (trenutnaKilometraza - kilometrazaServisa);
        return preostalo <= 0 ? "Servis je dospio." : $"Za {preostalo:N0} km";
    }

    private static void DodajUpozorenja(DashboardViewModel model, Vozilo vozilo)
    {
        if (model.DaniDoIstekaRegistracije < 0)
            model.Upozorenja.Add("Registracija je istekla.");
        else if (model.DaniDoIstekaRegistracije <= 30)
            model.Upozorenja.Add($"Registracija ističe za {model.DaniDoIstekaRegistracije} dana.");

        if (vozilo.KilometrazaMaliServis > 0 && vozilo.TrenutnaKilometraza - vozilo.KilometrazaMaliServis >= 9_000)
            model.Upozorenja.Add("Mali servis uskoro dospijeva ili je dospio.");

        if (vozilo.KilometrazaVelikiServis > 0 && vozilo.TrenutnaKilometraza - vozilo.KilometrazaVelikiServis >= 99_000)
            model.Upozorenja.Add("Veliki servis uskoro dospijeva ili je dospio.");
    }
}
