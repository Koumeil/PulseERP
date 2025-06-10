namespace PulseERP.Domain.VO;

using System;
using System.Text.RegularExpressions;
using Errors;

/// <summary>
/// Immutable Value Object representing a validated email address.
/// Invariants:
///   • Must be non-null, non-empty, trimmed.
///   • Must match a simplified RFC 5322-style email regex.
///   • Always stored as lowercase.
/// </summary>
public sealed class EmailAddress : ValueObject, IEquatable<EmailAddress>
{
    #region Fields

    // Simplified regex to validate “local@domain.tld” (does not cover every RFC nuance but suffices for most cases).
    private static readonly Regex EmailRegex = new(
        @"^(?!.*\.\.)([^@\s\.]+(?:\.[^@\s\.]+){0,2})@([^@\s\.]+(?:\.[^@\s\.]+){1,2})$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
    );

    #endregion

    #region Properties

    /// <summary>
    /// Underlying email string, always in lowercase.
    /// </summary>
    public string Value { get; }

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a new <see cref="EmailAddress"/> after validating format.
    /// </summary>
    /// <param name="email">
    /// Email string to validate. Cannot be null/whitespace; must match the regex.
    /// </param>
    /// <exception cref="DomainValidationException">
    /// Thrown if <paramref name="email"/> is null, empty, or does not match the pattern.
    /// </exception>
    public EmailAddress(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainValidationException("EmailAddress cannot be null or whitespace.");

        var trimmed = email.Trim();
        if (!EmailRegex.IsMatch(trimmed))
            throw new DomainValidationException($"Invalid email format: '{email}'.");

        Value = trimmed.ToLowerInvariant();
    }

    #endregion

    #region Equality

    /// <summary>
    /// Implements <see cref="IEquatable{EmailAddress}"/>.
    /// Two <see cref="EmailAddress"/> instances are equal if their <see cref="Value"/> (lowercase) matches exactly.
    /// </summary>
    public bool Equals(EmailAddress? other)
    {
        if (other is null)
            return false;

        return Value.Equals(other.Value, StringComparison.Ordinal);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as EmailAddress);

    /// <inheritdoc/>
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Equality operator for <see cref="EmailAddress"/>.
    /// </summary>
    public static bool operator ==(EmailAddress? left, EmailAddress? right)
    {
        if (left is null && right is null)
            return true;
        if (left is null || right is null)
            return false;
        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator for <see cref="EmailAddress"/>.
    /// </summary>
    public static bool operator !=(EmailAddress? left, EmailAddress? right) => !(left == right);

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
