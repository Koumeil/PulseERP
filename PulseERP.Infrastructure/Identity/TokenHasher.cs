using System.Security.Cryptography;
using System.Text;
using PulseERP.Abstractions.Security.Interfaces;

namespace PulseERP.Infrastructure.Identity;

public class TokenHasher : ITokenHasher
{
    public string Hash(string token)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
