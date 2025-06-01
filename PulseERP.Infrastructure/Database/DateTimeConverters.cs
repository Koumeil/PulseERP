namespace PulseERP.Infrastructure.Database;

/// <summary>
/// Contient la logique “Local ↔ UTC” pour DateTime/DateTime?,
/// isolée dans des méthodes statiques afin de simplifier le ValueConverter.
/// </summary>
public static class DateTimeConverters
{
    /// <summary>
    /// Prend un DateTime “Local” (ou “Unspecified”) et retourne son équivalent UTC.
    /// </summary>
    public static DateTime ToUtc(DateTime value)
    {
        // Si c’est déjà UTC, on renvoie directement
        if (value.Kind == DateTimeKind.Utc)
            return value;

        // Si c’est Unspecified, on suppose que c’est Local
        if (value.Kind == DateTimeKind.Unspecified)
            value = DateTime.SpecifyKind(value, DateTimeKind.Local);

        // Convertit Local → UTC
        return value.ToUniversalTime();
    }

    /// <summary>
    /// Prend un DateTime (en principe UTC) et retourne son équivalent en heure locale.
    /// </summary>
    public static DateTime ToLocal(DateTime value)
    {
        // Si c’est Unspecified, on le traite comme UTC
        if (value.Kind == DateTimeKind.Unspecified)
            value = DateTime.SpecifyKind(value, DateTimeKind.Utc);

        // Si c’est déjà Local, on renvoie directement
        if (value.Kind == DateTimeKind.Local)
            return value;

        // Convertit UTC → Local
        return value.ToLocalTime();
    }

    /// <summary>
    /// Même logique, mais pour DateTime?.
    /// </summary>
    public static DateTime? ToUtcNullable(DateTime? value)
    {
        if (!value.HasValue)
            return null;

        return ToUtc(value.Value);
    }

    public static DateTime? ToLocalNullable(DateTime? value)
    {
        if (!value.HasValue)
            return null;

        return ToLocal(value.Value);
    }
}
