namespace PulseERP.API.DTOs.Auth;

public record AuthResponse(string Token, string RefreshToken, DateTime Expiration);
