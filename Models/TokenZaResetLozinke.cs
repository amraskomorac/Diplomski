using Diplomski.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diplomski.Models;

[Table("tokeni_za_reset_lozinke")]
public class TokenZaResetLozinke
{
    [Column("id")]
    public int Id { get; set; }

    [Column("korisnik_id")]
    public int KorisnikId { get; set; }

    [Column("token")]
    public string Token { get; set; } = "";

    [Column("istice")]
    public DateTime Istice { get; set; }

    [Column("iskoristen")]
    public bool Iskoristen { get; set; }

    public Korisnik? Korisnik { get; set; }
}