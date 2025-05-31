namespace PulseERP.Abstractions.Security.Interfaces;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
