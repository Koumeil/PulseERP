using PulseERP.Abstractions.Security.Interfaces;

namespace PulseERP.Infrastructure.Provider;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
