namespace PulseERP.Abstractions.Security.Interfaces;

public interface ITokenHasher
{
    string Hash(string token);
}
