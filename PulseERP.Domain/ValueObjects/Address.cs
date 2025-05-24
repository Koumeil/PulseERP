namespace PulseERP.Domain.ValueObjects;

public record Address(string Street, string City, string ZipCode, string Country)
{
    public static Address? TryCreateOrUpdate(
        Address? existingAddress,
        string? street,
        string? city,
        string? zipCode,
        string? country
    )
    {
        // Cas 1 : Tous les champs sont vides/nulls => retourne null
        if (AllFieldsAreNullOrEmpty(street, city, zipCode, country))
        {
            return null;
        }

        // Cas 2 : Création d'une nouvelle adresse
        if (existingAddress is null)
        {
            ValidateRequiredFields(street, city, zipCode, country);
            return new Address(street!.Trim(), city!.Trim(), zipCode!.Trim(), country!.Trim());
        }

        // Cas 3 : Mise à jour partielle de l'adresse existante
        var updated = new Address(
            !string.IsNullOrWhiteSpace(street) ? street.Trim() : existingAddress.Street,
            !string.IsNullOrWhiteSpace(city) ? city.Trim() : existingAddress.City,
            !string.IsNullOrWhiteSpace(zipCode) ? zipCode.Trim() : existingAddress.ZipCode,
            !string.IsNullOrWhiteSpace(country) ? country.Trim() : existingAddress.Country
        );

        // ✅ Si rien n’a changé, retourne null
        if (updated.Equals(existingAddress))
            return null;

        return updated;
    }

    private static bool AllFieldsAreNullOrEmpty(params string?[] fields)
    {
        return fields.All(string.IsNullOrWhiteSpace);
    }

    private static void ValidateRequiredFields(
        string? street,
        string? city,
        string? postalCode,
        string? country
    )
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("La rue est obligatoire", nameof(street));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("La ville est obligatoire", nameof(city));
        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("Le code postal est obligatoire", nameof(postalCode));
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Le pays est obligatoire", nameof(country));
    }
}
