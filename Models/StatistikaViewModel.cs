namespace Diplomski.Models;

public class StatistikaViewModel
{
    public List<Vozilo> Vozila { get; set; } = [];
    public Vozilo? OdabranoVozilo { get; set; }
    public List<string> Mjeseci { get; set; } = [];
    public List<decimal> TroskoviPoMjesecima { get; set; } = [];
    public List<string> TipoviServisa { get; set; } = [];
    public List<decimal> TroskoviPoTipu { get; set; } = [];
    public List<int> Godine { get; set; } = [];
    public List<int> BrojServisaPoGodini { get; set; } = [];
    public Servis? NajskupljiServis { get; set; }
    public int? ProsjecnaMjesecnaKilometraza { get; set; }
    public int? MjeseciDoMalogServisa { get; set; }
    public int? MjeseciDoVelikogServisa { get; set; }
}
