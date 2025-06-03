using System.Runtime.InteropServices;
using PulseERP.Abstractions.Security.Interfaces;

namespace PulseERP.Infrastructure.Provider;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime NowLocal
    {
        get
        {
            var tzBrussels = GetBrusselsTimeZone();
            return TimeZoneInfo.ConvertTimeFromUtc(UtcNow, tzBrussels);
        }
    }

    /// <summary>
    /// Convertit un DateTime (peu importe Kind) vers string heure locale Brussels.
    /// </summary>
    public string ToBrusselsTimeString(DateTime dateTime)
    {
        // Défensif : on force UTC même si la provenance n’est pas garantie
        var utc = EnsureUtc(dateTime);
        var tzBrussels = GetBrusselsTimeZone();
        var local = TimeZoneInfo.ConvertTimeFromUtc(utc, tzBrussels);
        return $"{local:HH:mm} (CEST)";
    }

    /// <summary>
    /// Convertit un DateTime (peu importe Kind) vers heure locale Brussels.
    /// </summary>
    public DateTime ConvertToLocal(DateTime dateTime)
    {
        var utc = EnsureUtc(dateTime);
        var tzBrussels = GetBrusselsTimeZone();
        return TimeZoneInfo.ConvertTimeFromUtc(utc, tzBrussels);
    }

    /// <summary>
    /// Force un DateTime en UTC, quelle que soit sa provenance.
    /// </summary>
    private static DateTime EnsureUtc(DateTime value)
    {
        if (value.Kind == DateTimeKind.Utc)
            return value;
        if (value.Kind == DateTimeKind.Unspecified)
            return DateTime.SpecifyKind(value, DateTimeKind.Utc);
        // Si Local, convertit en UTC
        return value.ToUniversalTime();
    }

    private static TimeZoneInfo GetBrusselsTimeZone()
    {
        string tzId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "Romance Standard Time"
            : "Europe/Brussels";
        return TimeZoneInfo.FindSystemTimeZoneById(tzId);
    }
}
