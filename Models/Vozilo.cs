using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diplomski.Models;

[Table("vozila")]
public class Vozilo
{
    [Column("id")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Unesite marku vozila.")]
    [StringLength(60)]
    [Column("marka")]
    public string Marka { get; set; } = "";

    [Required(ErrorMessage = "Unesite model vozila.")]
    [StringLength(60)]
    [Column("model")]
    public string Model { get; set; } = "";

    [Range(1886, 2100, ErrorMessage = "Unesite ispravnu godinu proizvodnje.")]
    [Column("godina_proizvodnje")]
    public int GodinaProizvodnje { get; set; }

    [Required(ErrorMessage = "Unesite registraciju.")]
    [StringLength(20)]
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
}
