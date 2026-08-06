using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Diplomski.Models;

[Table("servisi")]
public class Servis : IValidatableObject
{
    [Column("id")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Odaberite vrstu servisa.")]
    [StringLength(150)]
    [Column("tip")]
    public string Tip { get; set; } = "";

    [DataType(DataType.Date)]
    [Column("datum")]
    public DateTime Datum { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Kilometraža ne može biti negativna.")]
    [Column("kilometraza")]
    public int Kilometraza { get; set; }

    [Range(0.01, 999999999, ErrorMessage = "Unesite cijenu veću od nule.")]
    [Precision(10, 2)]
    [Column("cijena")]
    public decimal Cijena { get; set; }

    [Required(ErrorMessage = "Unesite naziv servisera.")]
    [StringLength(150)]
    [Column("serviser")]
    public string Serviser { get; set; } = "";

    [StringLength(2000)]
    [Column("napomena")]
    public string? Napomena { get; set; }

    [StringLength(500)]
    [Column("putanja_racuna")]
    public string? PutanjaRacuna { get; set; }

    [Required(ErrorMessage = "Odaberite vozilo.")]
    [Column("vozilo_id")]
    public int? VoziloId { get; set; }

    [ForeignKey(nameof(VoziloId))]
    public Vozilo? Vozilo { get; set; }

    [Column("korisnik_id")]
    public int KorisnikId { get; set; }

    [ForeignKey(nameof(KorisnikId))]
    public Korisnik? Korisnik { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Datum.Date > DateTime.Today)
            yield return new ValidationResult("Datum servisa ne može biti u budućnosti.", [nameof(Datum)]);
    }
}
