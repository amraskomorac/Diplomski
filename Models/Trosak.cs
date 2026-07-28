using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diplomski.Models;

[Table("troskovi")]
public class Trosak
{
    [Column("id")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Unesite tip troška.")]
    [StringLength(150)]
    [Column("tip")]
    public string Tip { get; set; } = "";

    [Range(0.01, 999999999, ErrorMessage = "Unesite iznos veći od nule.")]
    [Column("iznos")]
    public decimal Iznos { get; set; }

    [DataType(DataType.Date)]
    [Column("datum")]
    public DateTime Datum { get; set; }

    [Required(ErrorMessage = "Odaberite vozilo.")]
    [Column("vozilo_id")]
    public int? VoziloId { get; set; }

    [ForeignKey(nameof(VoziloId))]
    public Vozilo? Vozilo { get; set; }

    [Column("korisnik_id")]
    public int KorisnikId { get; set; }

    [ForeignKey(nameof(KorisnikId))]
    public Korisnik? Korisnik { get; set; }
}
