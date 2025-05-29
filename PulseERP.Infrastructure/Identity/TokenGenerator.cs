using System.Security.Cryptography;
using PulseERP.Domain.Interfaces.Services;

namespace PulseERP.Infrastructure.Identity;

public class TokenGenerator : ITokenGenerator
{
    public string GenerateToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
