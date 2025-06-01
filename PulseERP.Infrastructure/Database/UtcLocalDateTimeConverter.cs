using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace PulseERP.Infrastructure.Database;

/// <summary>
/// Helper pour appliquer un ValueConverter “UTC ↔ Local” à tous les
/// DateTime et DateTime? de votre modèle EF Core.
///
/// Appeler ApplyConversions(builder) depuis CoreDbContext.OnModelCreating.
/// </summary>
public static class UtcLocalDateTimeConverter
{
    public static void ApplyConversions(ModelBuilder modelBuilder)
    {
        // On crée deux ValueConverter qui appellent nos méthodes statiques.
        var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
            v => DateTimeConverters.ToUtc(v), // Côté “write” (Value → Provider)
            v => DateTimeConverters.ToLocal(v) // Côté “read”  (Provider → Value)
        );

        var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
            v => DateTimeConverters.ToUtcNullable(v),
            v => DateTimeConverters.ToLocalNullable(v)
        );

        // On parcourt toutes les entités du modèle
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            var properties = clrType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                // Filtre uniquement les DateTime et DateTime?
                .Where(p =>
                    p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?)
                );

            foreach (var prop in properties)
            {
                if (prop.PropertyType == typeof(DateTime))
                {
                    modelBuilder
                        .Entity(clrType)
                        .Property(prop.Name)
                        .HasConversion(dateTimeConverter)
                        .HasColumnType("datetime2");
                }
                else // DateTime?
                {
                    modelBuilder
                        .Entity(clrType)
                        .Property(prop.Name)
                        .HasConversion(nullableDateTimeConverter)
                        .HasColumnType("datetime2");
                }
            }
        }
    }
}
