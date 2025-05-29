using System.ComponentModel.DataAnnotations;

namespace PulseERP.Application.Settings;

public class JwtSettings
{
    [Required]
    public string SecretKey { get; set; } = null!;

    [Range(1, int.MaxValue)]
    public int AccessTokenExpirationMinutes { get; set; }

    [Range(1, int.MaxValue)]
    public int RefreshTokenExpirationDays { get; set; }

    [Required]
    public string Issuer { get; set; } = null!;

    [Required]
    public string Audience { get; set; } = null!;
}
