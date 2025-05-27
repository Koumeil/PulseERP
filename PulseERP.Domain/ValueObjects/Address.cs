public record Address(string Street, string City, string ZipCode, string Country)
{
    public static Address Create(string street, string city, string zipCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street is required");
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required");
        if (string.IsNullOrWhiteSpace(zipCode))
            throw new ArgumentException("ZipCode is required");
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country is required");

        return new Address(street.Trim(), city.Trim(), zipCode.Trim(), country.Trim());
    }

    public Address Update(
        string? street = null,
        string? city = null,
        string? zipCode = null,
        string? country = null
    )
    {
        var updated = new Address(
            street is not null && !string.IsNullOrWhiteSpace(street) ? street.Trim() : this.Street,
            city is not null && !string.IsNullOrWhiteSpace(city) ? city.Trim() : this.City,
            zipCode is not null && !string.IsNullOrWhiteSpace(zipCode)
                ? zipCode.Trim()
                : this.ZipCode,
            country is not null && !string.IsNullOrWhiteSpace(country)
                ? country.Trim()
                : this.Country
        );

        return updated.Equals(this) ? this : updated;
    }

    public override string ToString() => $"{Street}, {City}, {ZipCode}, {Country}";
}
