using System.Security.Claims;
using Diplomski.Data;
using Diplomski.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            .Where(t => t.KorisnikId == TrenutniKorisnikId())
            .OrderByDescending(t => t.Id)
            .ToListAsync();

        return View(troskovi);
    }

    public IActionResult Dodaj() => View(new Trosak());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Dodaj(Trosak trosak)
    {
        if (!ModelState.IsValid)
            return View(trosak);

        trosak.KorisnikId = TrenutniKorisnikId();
        _context.Troskovi.Add(trosak);
        await _context.SaveChangesAsync();

        TempData["Poruka"] = "Trošak je uspješno dodan.";
        return RedirectToAction(nameof(Index));
    }

    private int TrenutniKorisnikId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
