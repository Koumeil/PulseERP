namespace PulseERP.Domain.ValueObjects;

public record Address(string Street, string City, string ZipCode, string Country)
{
    public static Address Create(string rawAddress)
    {
        if (string.IsNullOrWhiteSpace(rawAddress))
            throw new ArgumentException("L'adresse est vide.");

        var parts = rawAddress.Split(
            ',',
            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
        );

        if (parts.Length != 4)
            throw new ArgumentException(
                "L'adresse doit contenir 4 parties : rue, ville, code postal, pays"
            );

        var street = parts[0];
        var city = parts[1];
        var zip = parts[2];
        var country = parts[3];

        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("La rue est requise");
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("La ville est requise");
        if (string.IsNullOrWhiteSpace(zip))
            throw new ArgumentException("Le code postal est requis");
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Le pays est requis");

        return new Address(street, city, zip, country);
    }

    public Address Update(
        string? street = null,
        string? city = null,
        string? zipCode = null,
        string? country = null
    )
    {
        var updated = new Address(
            !string.IsNullOrWhiteSpace(street) ? street.Trim() : Street,
            !string.IsNullOrWhiteSpace(city) ? city.Trim() : City,
            !string.IsNullOrWhiteSpace(zipCode) ? zipCode.Trim() : ZipCode,
            !string.IsNullOrWhiteSpace(country) ? country.Trim() : Country
        );

        return updated.Equals(this) ? this : updated;
    }

    public override string ToString() => $"{Street}, {City}, {ZipCode}, {Country}";
}
