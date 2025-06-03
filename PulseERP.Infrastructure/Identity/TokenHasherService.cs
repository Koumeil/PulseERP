using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Security.Interfaces;

namespace PulseERP.Infrastructure.Identity;

/// <summary>
/// Hashes tokens using SHA256 for secure storage.
/// </summary>
public class TokenHasherService : ITokenHasherService
{
    private readonly ILogger<TokenHasherService> _logger;

    public TokenHasherService(ILogger<TokenHasherService> logger) => _logger = logger;

    public string Hash(string token)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha.ComputeHash(bytes);
        _logger.LogDebug("Hashed token at {TimeLocal}.", DateTime.Now);
        return Convert.ToBase64String(hash);
    }
}
