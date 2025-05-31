using System.ComponentModel.DataAnnotations;

namespace PulseERP.Abstractions.Settings;

/// <summary>Paramètres JWT injectés via <c>IOptions&lt;JwtSettings&gt;</c>.</summary>
public sealed class JwtSettings
{
    [Required]
    public string SecretKey { get; init; } = null!;

    [Range(1, int.MaxValue)]
    public int AccessTokenExpirationMinutes { get; init; }

    [Range(1, int.MaxValue)]
    public int RefreshTokenExpirationDays { get; init; }

    [Required]
    public string Issuer { get; init; } = null!;

    [Required]
    public string Audience { get; init; } = null!;
}
