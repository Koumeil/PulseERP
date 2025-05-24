namespace PulseERP.Domain.ValueObjects;

public record Address(string Street, string City, string ZipCode, string Country)
{
    public static Address? TryCreateIfValid(
        string? street,
        string? city,
        string? postalCode,
        string? country
    )
    {
        if (
            string.IsNullOrWhiteSpace(street)
            && string.IsNullOrWhiteSpace(city)
            && string.IsNullOrWhiteSpace(postalCode)
            && string.IsNullOrWhiteSpace(country)
        )
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("La rue est obligatoire", nameof(street));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("La ville est obligatoire", nameof(city));
        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("Le code postal est obligatoire", nameof(postalCode));
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Le pays est obligatoire", nameof(country));

        return new Address(street.Trim(), city.Trim(), postalCode.Trim(), country.Trim());
    }
}
