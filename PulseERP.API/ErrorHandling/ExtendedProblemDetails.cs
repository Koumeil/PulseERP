using Microsoft.AspNetCore.Mvc;

namespace PulseERP.API.ErrorHandling;

/// <summary>
/// Classe de base enrichie pour les détails de problème.
/// </summary>
public class ExtendedProblemDetails : ProblemDetails
{
    public DateTime TimestampUtc { get; set; }
    public string RequestId { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
}
