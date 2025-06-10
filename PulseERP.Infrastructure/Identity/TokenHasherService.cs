using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Security.Interfaces;

namespace PulseERP.Infrastructure.Identity;

/// <summary>
/// Hashes tokens using SHA256 for secure storage.
/// </summary>
public class TokenHasherService(ILogger<TokenHasherService> logger) : ITokenHasherService
{
    public string Hash(string token)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha.ComputeHash(bytes);
        logger.LogDebug("Hashed token at {TimeLocal}.", DateTime.Now);
        return Convert.ToBase64String(hash);
    }
}
