namespace PulseERP.Domain.VO;

using System;
using System.Security.Cryptography;
using Errors;

/// <summary>
/// Immutable Value Object representing a hashed password.
/// Invariants:
///  • Cannot be created with a null or empty raw password.
///  • Hashed value stored internally; raw password is never retained.
///  • Password must satisfy complexity:
///      – Minimum 10 characters.
///      – At least one uppercase letter, one lowercase letter, one digit, one special character.
/// </summary>
public sealed class Password : ValueObject, IEquatable<Password>
{
    #region Constants

    // Minimum length for the plain-text password
    private const int MinimumLength = 10;

    #endregion

    #region Properties

    /// <summary>
    /// Salted+hashed password (Base64-encoded string).
    /// Never exposed in plain text.
    /// </summary>
    public string HashedValue { get; }

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a new <see cref="Password"/> by hashing the provided raw password.
    /// The raw password is immediately discarded; only the hash is stored.
    /// </summary>
    /// <param name="rawPassword">
    /// Plain-text password. Must be ≥ 8 chars, include uppercase, lowercase, digit, special char.
    /// </param>
    /// <exception cref="DomainValidationException">
    /// Thrown if <paramref name="rawPassword"/> is null, empty, or fails complexity requirements.
    /// </exception>
    public Password(string rawPassword)
    {
        if (string.IsNullOrWhiteSpace(rawPassword))
            throw new DomainValidationException("Password cannot be null or whitespace.");

        if (rawPassword.Length < MinimumLength)
            throw new DomainValidationException(
                $"Password must be at least {MinimumLength} characters."
            );

        if (!HasUppercase(rawPassword))
            throw new DomainValidationException(
                "Password must contain at least one uppercase letter."
            );

        if (!HasLowercase(rawPassword))
            throw new DomainValidationException(
                "Password must contain at least one lowercase letter."
            );

        if (!HasDigit(rawPassword))
            throw new DomainValidationException("Password must contain at least one digit.");

        if (!HasSpecialChar(rawPassword))
            throw new DomainValidationException(
                "Password must contain at least one special character."
            );

        HashedValue = ComputeHash(rawPassword);
    }

    #endregion

    #region Complexity Helpers

    private static bool HasUppercase(string input) =>
        input.IndexOfAny("ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray()) >= 0;

    private static bool HasLowercase(string input) =>
        input.IndexOfAny("abcdefghijklmnopqrstuvwxyz".ToCharArray()) >= 0;

    private static bool HasDigit(string input) => input.IndexOfAny("0123456789".ToCharArray()) >= 0;

    private static bool HasSpecialChar(string input)
    {
        // You can tweak the set of allowed special chars as needed
        const string specials = @"!@#$%^&*()-_=+[{]}\|;:'"",<.>/?";
        return input.IndexOfAny(specials.ToCharArray()) >= 0;
    }

    #endregion

    #region Hashing Logic

    /// <summary>
    /// Computes a salted hash of the input password using PBKDF2 (Rfc2898) with HMACSHA256.
    /// The returned string is: Base64( salt(16 bytes) + hashedPassword(32 bytes) ).
    /// </summary>
    private static string ComputeHash(string rawPassword)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(16);

        using var deriveBytes = new Rfc2898DeriveBytes(
            rawPassword,
            saltBytes,
            100_000,
            HashAlgorithmName.SHA256
        );
        var hashBytes = deriveBytes.GetBytes(32);

        var base64Salt = Convert.ToBase64String(saltBytes);
        var base64Hash = Convert.ToBase64String(hashBytes);

        return $"{base64Salt}:{base64Hash}";
    }

    #endregion

    #region Verification

    /// <summary>
    /// Verifies a provided plain-text password against the stored hash.
    /// </summary>
    /// <param name="plainText">
    /// The plain-text password to verify.
    /// </param>
    /// <returns>
    /// True if <paramref name="plainText"/> produces the same hash; otherwise false.
    /// </returns>
    public bool Verify(string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
            return false;

        var parts = HashedValue.Split(':');
        if (parts.Length != 2)
            return false;

        var saltBytes = Convert.FromBase64String(parts[0]);
        var expectedHash = Convert.FromBase64String(parts[1]);

        using var deriveBytes = new Rfc2898DeriveBytes(
            plainText,
            saltBytes,
            100_000,
            HashAlgorithmName.SHA256
        );
        var actualHash = deriveBytes.GetBytes(32);

        return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
    }

    #endregion

    #region Equality

    /// <summary>
    /// Implements <see cref="IEquatable{Password}"/>.
    /// Two <see cref="Password"/> instances are equal if their <see cref="HashedValue"/> matches exactly.
    /// </summary>
    public bool Equals(Password? other)
    {
        if (other is null)
            return false;

        return HashedValue.Equals(other.HashedValue, StringComparison.Ordinal);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as Password);

    /// <inheritdoc/>
    public override int GetHashCode() => HashedValue.GetHashCode(StringComparison.Ordinal);

    /// <summary>
    /// Equality operator for <see cref="Password"/>.
    /// </summary>
    public static bool operator ==(Password? left, Password? right)
    {
        if (left is null && right is null)
            return true;
        if (left is null || right is null)
            return false;
        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator for <see cref="Password"/>.
    /// </summary>
    public static bool operator !=(Password? left, Password? right) => !(left == right);

    #endregion

    #region ValueObject Component

    /// <inheritdoc/>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return HashedValue;
    }

    #endregion

    #region ToString Override

    /// <summary>
    /// Returns the hashed value.
    /// (Never returns the plain-text password.)
    /// </summary>
    public override string ToString() => HashedValue;
    #endregion
}
