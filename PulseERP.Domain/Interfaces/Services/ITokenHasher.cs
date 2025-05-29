namespace PulseERP.Domain.Interfaces.Services;

public interface ITokenHasher
{
    string Hash(string token);
}
