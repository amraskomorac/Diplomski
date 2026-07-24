using System.ComponentModel.DataAnnotations.Schema;

namespace Diplomski.Models;

[Table("korisnici")]
public class Korisnik
{
    [Column("id")]
    public int Id { get; set; }

    [Column("puno_ime")]
    public string PunoIme { get; set; } = "";

    [Column("email")]
    public string Email { get; set; } = "";

    [Column("lozinka_hash")]
    public string LozinkaHash { get; set; } = "";

    [Column("datum_kreiranja")]
    public DateTime DatumKreiranja { get; set; }

    public ICollection<Vozilo> Vozila { get; set; } = new List<Vozilo>();
    public ICollection<Trosak> Troskovi { get; set; } = new List<Trosak>();
}
