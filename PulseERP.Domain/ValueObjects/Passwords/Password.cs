using System.Text.RegularExpressions;
using PulseERP.Domain.Errors;

namespace PulseERP.Domain.ValueObjects.Passwords;

/// <summary>
/// Secure Value Object representing a user password.
/// Centralized validation: length, uppercase, lowercase, digits, special characters.
/// Immutable. Never exposes the real value via ToString.
/// </summary>
public sealed record Password
{
    public const int MinLength = 8;
    public const int MaxLength = 20;

    private static readonly Regex Uppercase = new(@"[A-Z]");
    private static readonly Regex Lowercase = new(@"[a-z]");
    private static readonly Regex Digit = new(@"\d");
    private static readonly Regex Special = new(@"[^a-zA-Z0-9]");

    /// <summary>
    /// The actual password value. Not exposed publicly.
    /// </summary>
    public string Value { get; }

    private Password(string value) => Value = value;

    /// <summary>
    /// Factory method: creates and validates a password. Throws an exception if invalid.
    /// </summary>
    /// <param name="plain">Raw password string.</param>
    /// <returns>A valid Password object.</returns>
    /// <exception cref="DomainException">If the password does not meet the requirements.</exception>
    public static Password Create(string plain)
    {
        if (!IsValid(plain, out var error))
            throw new DomainException(error!);
        return new Password(plain);
    }

    /// <summary>
    /// Checks if a string meets all password policy requirements.
    /// </summary>
    /// <param name="plain">Raw password string.</param>
    /// <param name="error">Error message if validation fails, null otherwise.</param>
    /// <returns>True if valid, false otherwise.</returns>
    public static bool IsValid(string? plain, out string? error)
    {
        error = plain switch
        {
            null => "Password cannot be empty.",
            var p when p.Length < MinLength || p.Length > MaxLength =>
                $"Password must be {MinLength}-{MaxLength} characters long.",
            var p when !Uppercase.IsMatch(p) => "At least one uppercase letter is required.",
            var p when !Lowercase.IsMatch(p) => "At least one lowercase letter is required.",
            var p when !Digit.IsMatch(p) => "At least one digit is required.",
            var p when !Special.IsMatch(p) => "At least one special character is required.",
            _ => null,
        };
        return error is null;
    }

    /// <summary>
    /// Explicit conversion: extracts the string value from the Password object.
    /// </summary>
    /// <param name="password">Password object.</param>
    public static explicit operator string(Password password) => password.Value;

    /// <summary>
    /// Overrides ToString to avoid leaking the actual password value.
    /// </summary>
    /// <returns>Masked string.</returns>
    public override string ToString() => "***";
}
