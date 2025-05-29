using PulseERP.Domain.Interfaces.Services;

namespace PulseERP.Application.Services;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
