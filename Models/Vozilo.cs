using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diplomski.Models;

[Table("vozila")]
public class Vozilo : IValidatableObject
{
    [Column("id")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Unesite marku vozila.")]
    [StringLength(60)]
    [RegularExpression(@"^[A-Za-zČĆŽŠĐčćžšđ]+(?: [A-Za-zČĆŽŠĐčćžšđ]+)*$", ErrorMessage = "Marka može sadržavati samo slova i razmake.")]
    [Column("marka")]
    public string Marka { get; set; } = "";

    [Required(ErrorMessage = "Unesite model vozila.")]
    [StringLength(60)]
    [RegularExpression(@"^[A-Za-zČĆŽŠĐčćžšđ]+(?: [A-Za-zČĆŽŠĐčćžšđ]+)*$", ErrorMessage = "Model može sadržavati samo slova i razmake.")]
    [Column("model")]
    public string Model { get; set; } = "";

    [Range(1886, 2100, ErrorMessage = "Godina proizvodnje mora biti između 1886. i tekuće godine.")]
    [Column("godina_proizvodnje")]
    public int GodinaProizvodnje { get; set; }

    [Required(ErrorMessage = "Unesite registraciju.")]
    [RegularExpression(@"^[A-Za-z][0-9]{2}-[A-Za-z]-[0-9]{3}$", ErrorMessage = "Registracija mora biti u formatu A12-B-345.")]
    [Column("registracija")]
    public string Registracija { get; set; } = "";

    [Required(ErrorMessage = "Odaberite tip goriva.")]
    [StringLength(30)]
    [Column("tip_goriva")]
    public string TipGoriva { get; set; } = "";

    [Required(ErrorMessage = "Odaberite tip mjenjača.")]
    [StringLength(30)]
    [Column("tip_mjenjaca")]
    public string TipMjenjaca { get; set; } = "";

    [Range(0, int.MaxValue, ErrorMessage = "Kilometraža ne može biti negativna.")]
    [Column("trenutna_kilometraza")]
    public int TrenutnaKilometraza { get; set; }

    [DataType(DataType.Date)]
    [Column("datum_kupovine")]
    public DateTime DatumKupovine { get; set; }

    [DataType(DataType.Date)]
    [Column("datum_registracije")]
    public DateTime DatumRegistracije { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Kilometraža ne može biti negativna.")]
    [Column("kilometraza_mali_servis")]
    public int KilometrazaMaliServis { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Kilometraža ne može biti negativna.")]
    [Column("kilometraza_veliki_servis")]
    public int KilometrazaVelikiServis { get; set; }

    [Column("korisnik_id")]
    public int KorisnikId { get; set; }

    [ForeignKey(nameof(KorisnikId))]
    public Korisnik? Korisnik { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var danas = DateTime.Today;

        if (GodinaProizvodnje > danas.Year)
            yield return new ValidationResult("Godina proizvodnje ne može biti u budućnosti.", [nameof(GodinaProizvodnje)]);

        if (DatumKupovine.Date > danas)
            yield return new ValidationResult("Datum kupovine ne može biti u budućnosti.", [nameof(DatumKupovine)]);

        if (DatumRegistracije.Date > danas)
            yield return new ValidationResult("Datum registracije ne može biti u budućnosti.", [nameof(DatumRegistracije)]);

        if (DatumKupovine.Year < 1886)
            yield return new ValidationResult("Unesite ispravan datum kupovine.", [nameof(DatumKupovine)]);

        if (DatumRegistracije.Year < 1886)
            yield return new ValidationResult("Unesite ispravan datum registracije.", [nameof(DatumRegistracije)]);

        if (KilometrazaMaliServis > TrenutnaKilometraza)
            yield return new ValidationResult("Kilometraža malog servisa ne može biti veća od trenutne kilometraže.", [nameof(KilometrazaMaliServis)]);

        if (KilometrazaVelikiServis > TrenutnaKilometraza)
            yield return new ValidationResult("Kilometraža velikog servisa ne može biti veća od trenutne kilometraže.", [nameof(KilometrazaVelikiServis)]);

        var dozvoljeniMjenjaci = new[] { "manuelni", "automatski", "poluautomatski" };
        if (!dozvoljeniMjenjaci.Contains(TipMjenjaca, StringComparer.OrdinalIgnoreCase))
            yield return new ValidationResult("Odaberite važeći tip mjenjača.", [nameof(TipMjenjaca)]);
    }
}
