namespace PulseERP.Application.DTOs.Auth;

public record AuthResult(
    bool Success,
    string? Token = null,
    string? RefreshToken = null,
    IEnumerable<string>? Errors = null
);
