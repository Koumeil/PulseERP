namespace PulseERP.Abstractions.Security.DTOs;

public sealed record RefreshTokenDto(string Token, DateTime Expires);
