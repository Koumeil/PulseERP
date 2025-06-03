namespace PulseERP.Domain.ValueObjects.Adresses;

public sealed record Address(string Street, string City, string ZipCode, string Country)
{
    public static Address Create(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new ArgumentException("Adresse vide.");

        var parts = raw.Split(
            ',',
            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
        );
        if (parts.Length != 4)
            throw new ArgumentException("Adresse attendue : rue, ville, CP, pays.");

        return new Address(parts[0], parts[1], parts[2], parts[3]);
    }

    public Address Update(
        string? street = null,
        string? city = null,
        string? zip = null,
        string? country = null
    ) =>
        new(
            street?.Trim() ?? Street,
            city?.Trim() ?? City,
            zip?.Trim() ?? ZipCode,
            country?.Trim() ?? Country
        );

    public override string ToString() => $"{Street}, {City}, {ZipCode}, {Country}";
}
