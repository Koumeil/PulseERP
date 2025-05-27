using Microsoft.Extensions.Logging;
using PulseERP.Application.Interfaces.Services;

namespace PulseERP.Infrastructure.Logging;

public class SerilogAppLoggerService<T> : ISerilogAppLoggerService<T>
{
    private readonly ILogger<T> _logger;

    public SerilogAppLoggerService(ILogger<T> logger)
    {
        _logger = logger;
    }

    public void LogInformation(string message, params object[] args) =>
        _logger.LogInformation(message, args);

    public void LogWarning(string message, params object[] args) =>
        _logger.LogWarning(message, args);

    public void LogError(string message, Exception? ex = null, params object[] args)
    {
        if (ex is null)
            _logger.LogError(message, args);
        else
            _logger.LogError(ex, message, args);
    }
}
