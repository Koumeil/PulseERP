using PulseERP.Domain.Interfaces.Services;

namespace PulseERP.Infrastructure.Provider;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
