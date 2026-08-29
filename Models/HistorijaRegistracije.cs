using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Diplomski.Models;

[Table("historija_registracija")]
public class HistorijaRegistracije
{
    [Column("id")]
    public int Id { get; set; }

    [Column("vozilo_id")]
    public int VoziloId { get; set; }

    [DataType(DataType.Date)]
    [Column("datum_registracije")]
    public DateTime DatumRegistracije { get; set; }

    [DataType(DataType.Date)]
    [Column("datum_isteka_registracije")]
    public DateTime DatumIstekaRegistracije { get; set; }

    [Precision(10, 2)]
    [Column("cijena")]
    public decimal? Cijena { get; set; }

    [StringLength(150)]
    [Column("polica_osiguranja")]
    public string? PolicaOsiguranja { get; set; }

    [ForeignKey(nameof(VoziloId))]
    public Vozilo? Vozilo { get; set; }
}
