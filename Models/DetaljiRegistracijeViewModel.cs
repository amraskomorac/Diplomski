using System.ComponentModel.DataAnnotations;

namespace Diplomski.Models;

public class DetaljiRegistracijeViewModel : IValidatableObject
{
    public int VoziloId { get; set; }

    [Required(ErrorMessage = "Unesite datum registracije.")]
    [DataType(DataType.Date)]
    public DateTime DatumRegistracije { get; set; }

    [Required(ErrorMessage = "Unesite datum isteka registracije.")]
    [DataType(DataType.Date)]
    public DateTime DatumIstekaRegistracije { get; set; }

    [Range(0.01, 99999999.99, ErrorMessage = "Unesite cijenu između 0,01 i 99.999.999,99 KM.")]
    public decimal Cijena { get; set; }

    [Required(ErrorMessage = "Unesite broj police osiguranja.")]
    [StringLength(150)]
    public string PolicaOsiguranja { get; set; } = "";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DatumIstekaRegistracije.Date <= DatumRegistracije.Date)
            yield return new ValidationResult("Datum isteka mora biti nakon datuma registracije.", [nameof(DatumIstekaRegistracije)]);
    }
}
