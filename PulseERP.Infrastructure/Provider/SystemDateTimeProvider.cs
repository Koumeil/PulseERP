using System.Runtime.InteropServices;
using PulseERP.Abstractions.Security.Interfaces;

namespace PulseERP.Infrastructure.Provider;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime NowLocal
    {
        get
        {
            var tzBrussels = GetBrusselsTimeZone();
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzBrussels);
        }
    }

    public string ToBrusselsTimeString(DateTime utcDateTime)
    {
        var tzBrussels = GetBrusselsTimeZone();
        var local = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, tzBrussels);
        return $"{local:HH:mm} (CEST)";
    }

    public DateTime ConvertToLocal(DateTime utcDateTime)
    {
        var tzBrussels = GetBrusselsTimeZone();
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, tzBrussels);
    }

    private static TimeZoneInfo GetBrusselsTimeZone()
    {
        string tzId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "Romance Standard Time"
            : "Europe/Brussels";
        return TimeZoneInfo.FindSystemTimeZoneById(tzId);
    }
}
