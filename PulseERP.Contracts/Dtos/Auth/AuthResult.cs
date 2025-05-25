namespace PulseERP.Contracts.Dtos.Auth;

public record AuthResult(
    bool Success,
    string? Token = null,
    string? RefreshToken = null,
    IEnumerable<string>? Errors = null
);
