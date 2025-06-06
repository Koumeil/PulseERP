namespace PulseERP.Domain.VO;

using System;
using PulseERP.Domain.Errors;

/// <summary>
/// Immutable Value Object representing a product’s description text.
/// Invariants:
///   • Cannot be null (but may be empty).
///   • Maximum length of 1000 characters.
///   • Trimmed of leading/trailing whitespace.
/// </summary>
public sealed class ProductDescription : ValueObject, IEquatable<ProductDescription>
{
    #region Properties

    /// <summary>
    /// The description text (trimmed).
    /// Can be an empty string if no description is provided.
    /// </summary>
    public string Value { get; }

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a new <see cref="ProductDescription"/> after validating maximum length.
    /// </summary>
    /// <param name="description">
    /// Description text (null is not allowed; pass empty string if none).
    /// </param>
    /// <exception cref="DomainValidationException">
    /// Thrown if <paramref name="description"/> exceeds 1000 characters.
    /// </exception>
    public ProductDescription(string description)
    {
        if (description is null)
            throw new DomainValidationException(
                "ProductDescription cannot be null. Use empty string if no description."
            );

        var trimmed = description.Trim();
        if (trimmed.Length > 1000)
            throw new DomainValidationException(
                $"ProductDescription cannot exceed 1000 characters; got {trimmed.Length}."
            );

        Value = trimmed;
    }

    #endregion

    #region Equality

    /// <summary>
    /// Implements <see cref="IEquatable{ProductDescription}"/>.
    /// Two <see cref="ProductDescription"/> instances are equal if their <see cref="Value"/> matches (ordinal).
    /// </summary>
    public bool Equals(ProductDescription? other)
    {
        if (other is null)
            return false;

        return Value.Equals(other.Value, StringComparison.Ordinal);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as ProductDescription);

    /// <inheritdoc/>
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Equality operator for <see cref="ProductDescription"/>.
    /// </summary>
    public static bool operator ==(ProductDescription? left, ProductDescription? right)
    {
        if (left is null && right is null)
            return true;
        if (left is null || right is null)
            return false;
        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator for <see cref="ProductDescription"/>.
    /// </summary>
    public static bool operator !=(ProductDescription? left, ProductDescription? right) =>
        !(left == right);

    #endregion

    #region ValueObject Component

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    #endregion

    #region ToString Override

    /// <inheritdoc/>
    public override string ToString() => Value;

    #endregion
}
