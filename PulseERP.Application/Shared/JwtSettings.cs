namespace PulseERP.Application.Common;

public class JwtSettings
{
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public string SecretKey { get; set; } = default!;
    public int TokenExpiryMinutes { get; set; }
    public int RefreshTokenExpiryDays { get; set; }
}
