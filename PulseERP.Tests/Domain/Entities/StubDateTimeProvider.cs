using PulseERP.Domain.Interfaces;

namespace PulseERP.Tests.Domain.Entities;

public class StubDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow { get; set; }

    public DateTime NowLocal => UtcNow.ToLocalTime();

    public DateTime ConvertToLocal(DateTime utcDateTime) => utcDateTime.ToLocalTime();

    public string ToBrusselsTimeString(DateTime utcDateTime) =>
        TimeZoneInfo
            .ConvertTimeFromUtc(
                utcDateTime,
                TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time")
            )
            .ToString("yyyy-MM-dd HH:mm:ss");
}
