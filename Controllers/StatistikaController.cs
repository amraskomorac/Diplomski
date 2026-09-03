using System.Globalization;
using System.Security.Claims;
using Diplomski.Data;
using Diplomski.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiplomskiApp.Controllers;

[Authorize]
public class StatistikaController : Controller
{
    private readonly ApplicationDbContext _context;
    public StatistikaController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index(int? voziloId)
    {
        var korisnikId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var vozila = await _context.Vozila.Where(v => v.KorisnikId == korisnikId)
            .OrderBy(v => v.Marka).ThenBy(v => v.Model).ToListAsync();
        var odabranoId = voziloId ?? HttpContext.Session.GetInt32("odabrano_vozilo_id");
        var vozilo = odabranoId.HasValue ? vozila.FirstOrDefault(v => v.Id == odabranoId.Value) : null;
        if (vozilo is null && vozila.Count == 1) vozilo = vozila[0];
        if (vozilo is not null) HttpContext.Session.SetInt32("odabrano_vozilo_id", vozilo.Id);

        var model = new StatistikaViewModel { Vozila = vozila, OdabranoVozilo = vozilo };
        if (vozilo is null) return View(model);

        var servisi = await _context.Servisi.Where(s => s.KorisnikId == korisnikId && s.VoziloId == vozilo.Id)
            .OrderBy(s => s.Datum).ToListAsync();
        model.NajskupljiServis = servisi.OrderByDescending(s => s.Cijena).ThenByDescending(s => s.Datum).FirstOrDefault();

        var mjeseci = Enumerable.Range(0, 12).Select(i => new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(i - 11)).ToList();
        model.Mjeseci = mjeseci.Select(m => m.ToString("MMM yy", CultureInfo.GetCultureInfo("bs-BA"))).ToList();
        model.TroskoviPoMjesecima = mjeseci.Select(m => servisi.Where(s => s.Datum.Year == m.Year && s.Datum.Month == m.Month).Sum(s => s.Cijena)).ToList();

        var poTipu = servisi.GroupBy(s => s.Tip).OrderByDescending(g => g.Sum(s => s.Cijena)).Take(8).ToList();
        model.TipoviServisa = poTipu.Select(g => g.Key).ToList();
        model.TroskoviPoTipu = poTipu.Select(g => g.Sum(s => s.Cijena)).ToList();
        var poGodini = servisi.GroupBy(s => s.Datum.Year).OrderBy(g => g.Key).ToList();
        model.Godine = poGodini.Select(g => g.Key).ToList();
        model.BrojServisaPoGodini = poGodini.Select(g => g.Count()).ToList();

        var ocitanja = (await _context.Goriva.Where(g => g.KorisnikId == korisnikId && g.VoziloId == vozilo.Id)
                .Select(g => new Ocitanje(g.Datum, g.Kilometraza)).ToListAsync())
            .Concat(servisi.Select(s => new Ocitanje(s.Datum, s.Kilometraza)))
            .Append(new Ocitanje(DateTime.Today, vozilo.TrenutnaKilometraza))
            .GroupBy(o => o.Datum.Date).Select(g => g.OrderByDescending(o => o.Kilometraza).First())
            .OrderBy(o => o.Datum).ToList();
        if (ocitanja.Count >= 2)
        {
            var prvo = ocitanja.First();
            var zadnje = ocitanja.Last();
            var dani = (zadnje.Datum.Date - prvo.Datum.Date).Days;
            var kilometri = zadnje.Kilometraza - prvo.Kilometraza;
            if (dani > 0 && kilometri >= 0)
                model.ProsjecnaMjesecnaKilometraza = (int)Math.Round(kilometri / (dani / 30.44m));
        }

        if (model.ProsjecnaMjesecnaKilometraza is > 0)
        {
            model.MjeseciDoMalogServisa = ProcijeniMjesece(vozilo.TrenutnaKilometraza, ZadnjiServisKm(servisi, "Mali servis") ?? vozilo.KilometrazaMaliServis, 10_000, model.ProsjecnaMjesecnaKilometraza.Value);
            model.MjeseciDoVelikogServisa = ProcijeniMjesece(vozilo.TrenutnaKilometraza, ZadnjiServisKm(servisi, "Veliki servis") ?? vozilo.KilometrazaVelikiServis, 100_000, model.ProsjecnaMjesecnaKilometraza.Value);
        }
        return View(model);
    }

    private static int? ZadnjiServisKm(IEnumerable<Servis> servisi, string tip) => servisi.Where(s => s.Tip == tip).OrderByDescending(s => s.Datum).ThenByDescending(s => s.Id).Select(s => (int?)s.Kilometraza).FirstOrDefault();
    private static int? ProcijeniMjesece(int trenutno, int zadnji, int interval, int mjesecno)
    {
        if (zadnji <= 0) return null;
        var preostalo = interval - (trenutno - zadnji);
        return Math.Max(0, (int)Math.Ceiling(preostalo / (decimal)mjesecno));
    }
    private sealed record Ocitanje(DateTime Datum, int Kilometraza);
}
