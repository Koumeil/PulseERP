using System.Text.RegularExpressions;
using PulseERP.Domain.Errors;

namespace PulseERP.Domain.VO;

/// <summary>
/// Immutable Value Object representing a phone number.
/// Invariant: Must comply with a valid E.164-style pattern (e.g. "+1234567890") or a configurable regex.
/// </summary>
public sealed class Phone : ValueObject, IEquatable<Phone>
{
    #region Fields

    // Simple E.164 pattern: “+” followed by 1–15 digits.
    // You can replace or extend this with a more complex pattern if needed.
    private static readonly Regex _e164Regex = new(@"^\+[1-9]\d{1,14}$", RegexOptions.Compiled);

    #endregion

    #region Properties

    /// <summary>
    /// Phone number in E.164 format (e.g., "+14155552671").
    /// </summary>
    public string Value { get; }

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a new <see cref="Phone"/> after validating against E.164 format.
    /// </summary>
    /// <param name="phone">Phone number as string (must match E.164).</param>
    /// <exception cref="DomainValidationException">
    /// Thrown if <paramref name="phone"/> is null, empty, or does not match E.164.
    /// </exception>
    public Phone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainValidationException("Phone number cannot be null or whitespace.");

        var trimmed = phone.Trim();
        if (!_e164Regex.IsMatch(trimmed))
            throw new DomainValidationException(
                $"Invalid phone format (expected E.164); got '{phone}'."
            );

        Value = trimmed;
    }

    #endregion

    #region Equality

    /// <summary>
    /// Implements <see cref="IEquatable{Phone}"/>.
    /// Two <see cref="Phone"/> instances are equal if their <see cref="Value"/> matches exactly.
    /// </summary>
    public bool Equals(Phone? other)
    {
        if (other is null)
            return false;

        return Value.Equals(other.Value, StringComparison.Ordinal);
    }

    /// <summary>
    /// Overrides <see cref="object.Equals(object)"/> to call <see cref="Equals(Phone)"/>.
    /// </summary>
    public override bool Equals(object? obj) => Equals(obj as Phone);

    /// <inheritdoc/>
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Equality operator for <see cref="Phone"/>.
    /// </summary>
    public static bool operator ==(Phone? left, Phone? right)
    {
        if (left is null && right is null)
            return true;
        if (left is null || right is null)
            return false;
        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator for <see cref="Phone"/>.
    /// </summary>
    public static bool operator !=(Phone? left, Phone? right) => !(left == right);

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
