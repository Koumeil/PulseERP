namespace PulseERP.Domain.VO;

using System;
using PulseERP.Domain.Errors;

/// <summary>
/// Immutable Value Object representing a postal address.
/// Invariants:
///   • Street, City, PostalCode, and Country must each be non-null and non-empty/whitespace.
///   • PostalCode maximum length 20 (adjust as needed per country).
///   • Country must be a valid country name or ISO code (generic validation here).
///   • All components are trimmed.
/// </summary>
public sealed class Address : ValueObject, IEquatable<Address>
{
    #region Properties

    /// <summary>
    /// Street address (e.g., "123 Main St").
    /// </summary>
    public string Street { get; }

    /// <summary>
    /// City or locality (e.g., "Brussels").
    /// </summary>
    public string City { get; }

    /// <summary>
    /// Postal code (e.g., "1000").
    /// </summary>
    public string PostalCode { get; }

    /// <summary>
    /// Country name or ISO code (e.g., "Belgium" or "BE").
    /// </summary>
    public string Country { get; }

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a new <see cref="Address"/> after validating each component.
    /// </summary>
    /// <param name="street">Street address (non-null, non-empty, trimmed).</param>
    /// <param name="city">City or locality (non-null, non-empty, trimmed).</param>
    /// <param name="postalCode">
    /// Postal code (non-null, non-empty, trimmed, max length 20).
    /// </param>
    /// <param name="country">
    /// Country name or ISO code (non-null, non-empty, trimmed).
    /// </param>
    /// <exception cref="DomainValidationException">
    /// Thrown if any component is null/empty or violates length constraints.
    /// </exception>
    ///
    public Address(string street, string city, string postalCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new DomainValidationException("Street cannot be null or whitespace.");
        if (string.IsNullOrWhiteSpace(city))
            throw new DomainValidationException("City cannot be null or whitespace.");
        if (string.IsNullOrWhiteSpace(postalCode))
            throw new DomainValidationException("PostalCode cannot be null or whitespace.");
        if (string.IsNullOrWhiteSpace(country))
            throw new DomainValidationException("Country cannot be null or whitespace.");

        var trimmedStreet = street.Trim();
        var trimmedCity = city.Trim();
        var trimmedPostal = postalCode.Trim();
        var trimmedCountry = country.Trim();

        if (trimmedPostal.Length > 20)
            throw new DomainValidationException(
                $"PostalCode cannot exceed 20 characters; got {trimmedPostal.Length}."
            );

        Street = trimmedStreet;
        City = trimmedCity;
        PostalCode = trimmedPostal;
        Country = trimmedCountry;
    }

    #endregion

    #region Equality

    /// <summary>
    /// Implements <see cref="IEquatable{Address}"/>.
    /// Two <see cref="Address"/> instances are equal if all components match (ordinal).
    /// </summary>
    public bool Equals(Address? other)
    {
        if (other is null)
            return false;

        return Street.Equals(other.Street, StringComparison.Ordinal)
            && City.Equals(other.City, StringComparison.Ordinal)
            && PostalCode.Equals(other.PostalCode, StringComparison.Ordinal)
            && Country.Equals(other.Country, StringComparison.Ordinal);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as Address);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Street, City, PostalCode, Country);

    /// <summary>
    /// Equality operator for <see cref="Address"/>.
    /// </summary>
    public static bool operator ==(Address? left, Address? right)
    {
        if (left is null && right is null)
            return true;
        if (left is null || right is null)
            return false;
        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator for <see cref="Address"/>.
    /// </summary>
    public static bool operator !=(Address? left, Address? right) => !(left == right);

    #endregion

    #region ValueObject Component

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return PostalCode;
        yield return Country;
    }

    #endregion

    #region ToString Override

    /// <summary>
    /// Returns a multi-line representation:
    ///  Street
    ///  PostalCode City
    ///  Country
    /// </summary>
    public override string ToString()
    {
        return $"{Street}{Environment.NewLine}{PostalCode} {City}{Environment.NewLine}{Country}";
    }

    #endregion
}
