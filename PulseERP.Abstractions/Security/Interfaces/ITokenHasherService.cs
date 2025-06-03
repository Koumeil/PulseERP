namespace PulseERP.Abstractions.Security.Interfaces;

public interface ITokenHasherService
{
    string Hash(string token);
}
