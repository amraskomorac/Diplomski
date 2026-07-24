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
        var korisnikId = TrenutniKorisnikId();
        var vozila = await _context.Vozila
            .Where(v => v.KorisnikId == korisnikId)
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
        if (!ModelState.IsValid)
            return View(vozilo);

        vozilo.KorisnikId = TrenutniKorisnikId();
        _context.Vozila.Add(vozilo);
        await _context.SaveChangesAsync();

        TempData["Poruka"] = "Vozilo je uspješno dodano.";
        return RedirectToAction(nameof(Index));
    }

    private int TrenutniKorisnikId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
