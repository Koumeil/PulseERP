namespace PulseERP.Abstractions.Security.Interfaces;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateTime NowLocal { get; }
    string ToBrusselsTimeString(DateTime utcDateTime);

    /// <summary>
    /// Convertit un DateTime UTC en DateTime local Europe/Bruxelles.
    /// </summary>
    DateTime ConvertToLocal(DateTime utcDateTime);
}
