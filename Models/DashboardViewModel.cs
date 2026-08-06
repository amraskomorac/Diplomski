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
    public List<PodsjetnikViewModel> Podsjetnici { get; set; } = [];
    public List<PodsjetnikViewModel> AktivneObavijesti { get; set; } = [];
    public bool PrikaziObavijesti { get; set; }
}

public class PodsjetnikViewModel
{
    public string Naziv { get; set; } = "";
    public string Ikona { get; set; } = "bi-bell";
    public string ZadnjiPut { get; set; } = "";
    public string Interval { get; set; } = "";
    public string Trenutno { get; set; } = "";
    public string Status { get; set; } = "";
    public bool JeHitno { get; set; }
    public bool JeUskoro { get; set; }
}
