using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diplomski.Models;

[Table("goriva")]
public class Gorivo
{
    [Column("id")] public int Id { get; set; }
    [Required] [Column("vozilo_id")] public int VoziloId { get; set; }
    [DataType(DataType.Date)] [Column("datum")] public DateTime Datum { get; set; }
    [Range(0.01, 9999)] [Column("litara")] public decimal Litara { get; set; }
    [Range(0.01, 999999)] [Column("cijena")] public decimal Cijena { get; set; }
    [Range(0, int.MaxValue)] [Column("kilometraza")] public int Kilometraza { get; set; }
    [Column("korisnik_id")] public int KorisnikId { get; set; }
    [ForeignKey(nameof(VoziloId))] public Vozilo? Vozilo { get; set; }
    [ForeignKey(nameof(KorisnikId))] public Korisnik? Korisnik { get; set; }
}

public class GorivoIndexViewModel
{
    public List<Vozilo> Vozila { get; set; } = [];
    public int? VoziloId { get; set; }
    public List<Gorivo> Zapisi { get; set; } = [];
    public decimal? ProsjecnaPotrosnja { get; set; }
    public decimal? CijenaPoKilometru { get; set; }
    public decimal UkupnoPotroseno { get; set; }
}
