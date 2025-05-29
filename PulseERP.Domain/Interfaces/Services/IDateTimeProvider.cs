using System;

namespace PulseERP.Domain.Interfaces.Services;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
