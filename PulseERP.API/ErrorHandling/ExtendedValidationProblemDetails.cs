using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PulseERP.API.ErrorHandling;

/// <summary>
/// Détails de validation enrichis.
/// </summary>
public class ExtendedValidationProblemDetails : ValidationProblemDetails
{
    public ExtendedValidationProblemDetails(ModelStateDictionary modelState)
        : base(modelState)
    {
        TimestampUtc = DateTime.UtcNow;
    }

    public DateTime TimestampUtc { get; set; }
    public string RequestId { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
}
