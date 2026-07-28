namespace Diplomski.Models;

public class DashboardViewModel
{
    public List<Vozilo> Vozila { get; set; } = [];
    public Vozilo? OdabranoVozilo { get; set; }
    public decimal UkupniTrosakOveGodine { get; set; }
    public DateTime DatumIstekaRegistracije { get; set; }
    public int DaniDoIstekaRegistracije { get; set; }
    public string PosljednjiMaliServis { get; set; } = "Nije evidentiran.";
    public string SljedeciMaliServis { get; set; } = "Nije moguće odrediti.";
    public string PosljednjiVelikiServis { get; set; } = "Nije evidentiran.";
    public string SljedeciVelikiServis { get; set; } = "Nije moguće odrediti.";
    public List<string> Upozorenja { get; set; } = [];
}
