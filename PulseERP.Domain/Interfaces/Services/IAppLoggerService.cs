namespace PulseERP.Domain.Interfaces.Services;

public interface ISerilogAppLoggerService<T>
{
    void LogInformation(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(string message, Exception? ex = null, params object[] args);
}
