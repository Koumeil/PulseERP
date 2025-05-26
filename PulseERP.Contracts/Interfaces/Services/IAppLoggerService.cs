namespace PulseERP.Contracts.Interfaces.Services;

public interface IAppLoggerService<T>
{
    void LogInformation(string message);
    void LogWarning(string message);
    void LogError(string message, Exception? ex = null);
}
