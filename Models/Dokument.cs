using System.ComponentModel.DataAnnotations.Schema;

namespace Diplomski.Models;

[Table("dokumenti")]
public class Dokument
{
    [Column("id")]
    public int Id { get; set; }

    [Column("tip")]
    public string Tip { get; set; } = "";

    [Column("naziv_datoteke")]
    public string NazivDatoteke { get; set; } = "";

    [Column("putanja")]
    public string Putanja { get; set; } = "";

    [Column("datum_izmjene")]
    public DateTime DatumIzmjene { get; set; }

    [Column("korisnik_id")]
    public int KorisnikId { get; set; }

    public Korisnik Korisnik { get; set; } = null!;
}
