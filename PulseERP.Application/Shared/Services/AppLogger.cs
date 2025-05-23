using Microsoft.Extensions.Logging;
using PulseERP.Application.Common.Interfaces;

namespace PulseERP.Application.Shared.Services;

public class AppLogger<T> : IAppLogger<T>
{
    private readonly ILogger<T> _logger;

    public AppLogger(ILogger<T> logger)
    {
        _logger = logger;
    }

    public void LogInformation(string message)
    {
        _logger.LogInformation(message);
    }

    public void LogWarning(string message)
    {
        _logger.LogWarning(message);
    }

    public void LogError(string message, Exception? ex = null)
    {
        if (ex is not null)
            _logger.LogError(ex, message);
        else
            _logger.LogError(message);
    }
}
