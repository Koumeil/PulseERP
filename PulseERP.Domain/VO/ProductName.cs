using PulseERP.Domain.Errors;

namespace PulseERP.Domain.VO;

public sealed class ProductName : ValueObject, IEquatable<ProductName>
{
    #region Properties

    /// <summary>
    /// The actual name text (trimmed).
    /// </summary>
    public string Value { get; }

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a new <see cref="ProductName"/> after validating length and content.
    /// </summary>
    /// <param name="name">
    /// Product name (must be 1-200 chars, non-whitespace).
    /// </param>
    /// <exception cref="DomainValidationException">
    /// Thrown if <paramref name="name"/> is null, empty, too long, or all whitespace.
    /// </exception>
    public ProductName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("ProductName cannot be null or whitespace.");

        var trimmed = name.Trim();
        if (trimmed.Length is < 1 or > 200)
            throw new DomainValidationException(
                $"ProductName must be between 1 and 200 characters; got length {trimmed.Length}."
            );

        Value = trimmed;
    }

    #endregion

    #region Equality

    /// <summary>
    /// Implements <see cref="IEquatable{ProductName}"/>.
    /// Two <see cref="ProductName"/> instances are equal if their <see cref="Value"/> matches (ordinal).
    /// </summary>
    public bool Equals(ProductName? other)
    {
        if (other is null)
            return false;

        return Value.Equals(other.Value, StringComparison.Ordinal);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as ProductName);

    /// <inheritdoc/>
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Equality operator for <see cref="ProductName"/>.
    /// </summary>
    public static bool operator ==(ProductName? left, ProductName? right)
    {
        if (left is null && right is null)
            return true;
        if (left is null || right is null)
            return false;
        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator for <see cref="ProductName"/>.
    /// </summary>
    public static bool operator !=(ProductName? left, ProductName? right) => !(left == right);

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

    public static implicit operator string(ProductName name) => name.Value;


    #endregion
}
