using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Security.Interfaces;

namespace PulseERP.Infrastructure.Identity;

/// <summary>
/// Generates secure random tokens.
/// </summary>
public class TokenGeneratorService : ITokenGeneratorService
{
    private readonly ILogger<TokenGeneratorService> _logger;

    public TokenGeneratorService(ILogger<TokenGeneratorService> logger) => _logger = logger;

    public string GenerateToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        var token = Convert.ToBase64String(bytes);
        _logger.LogDebug("Generated new secure token at {TimeLocal}.", DateTime.Now);
        return token;
    }
}
