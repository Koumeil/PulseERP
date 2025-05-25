using Microsoft.Extensions.Logging;
using PulseERP.Contracts.Services;

namespace PulseERP.Infrastructure.Logging;

public class SerilogAppLogger<T> : IAppLogger<T>
{
    private readonly ILogger<T> _logger;

    public SerilogAppLogger(ILogger<T> logger)
    {
        _logger = logger;
    }

    public void LogInformation(string message) => _logger.LogInformation(message);

    public void LogWarning(string message) => _logger.LogWarning(message);

    public void LogError(string message, Exception? ex = null) => _logger.LogError(ex, message);
}
