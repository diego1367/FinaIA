using System.ComponentModel.DataAnnotations;

namespace CreditRequests.Api.Contracts;

public sealed class CreateCreditRequestDto : IValidatableObject
{
    [Required, RegularExpression("^[A-Za-z0-9]{6,20}$", ErrorMessage = "El documento debe tener entre 6 y 20 caracteres alfanuméricos.")]
    public string DocumentNumber { get; init; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string FullName { get; init; } = string.Empty;

    public decimal RequestedAmount { get; init; }

    [Range(1, 120, ErrorMessage = "El plazo debe estar entre 1 y 120 meses.")]
    public int TermMonths { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FullName.Trim().Length is < 2 or > 100)
            yield return new("El nombre debe tener entre 2 y 100 caracteres.", [nameof(FullName)]);

        if (RequestedAmount is < 0.01m or > 9_999_999_999_999.99m ||
            decimal.Round(RequestedAmount, 2) != RequestedAmount)
            yield return new("Ingresa un monto positivo, con hasta 13 dígitos enteros y dos decimales.",
                [nameof(RequestedAmount)]);
    }
}
