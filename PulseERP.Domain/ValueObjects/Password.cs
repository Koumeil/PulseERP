using System.Text.RegularExpressions;
using PulseERP.Domain.Errors;

namespace PulseERP.Domain.ValueObjects;

public sealed class Password
{
    private const int MinLength = 8;
    private const int MaxLength = 20;

    private static readonly Regex HasUpperCase = new(@"[A-Z]", RegexOptions.Compiled);
    private static readonly Regex HasLowerCase = new(@"[a-z]", RegexOptions.Compiled);
    private static readonly Regex HasDigit = new(@"\d", RegexOptions.Compiled);
    private static readonly Regex HasSpecial = new(@"[^a-zA-Z0-9]", RegexOptions.Compiled);

    public string Value { get; }

    private Password(string value) => Value = value;

    public static Password Create(string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
            throw new DomainException("Password cannot be empty.");

        if (plainText.Length < MinLength)
            throw new DomainException($"Password must be at least {MinLength} characters long.");

        if (plainText.Length > MaxLength)
            throw new DomainException($"Password must not exceed {MaxLength} characters.");

        if (!HasUpperCase.IsMatch(plainText))
            throw new DomainException("Password must contain at least one uppercase letter.");

        if (!HasLowerCase.IsMatch(plainText))
            throw new DomainException("Password must contain at least one lowercase letter.");

        if (!HasDigit.IsMatch(plainText))
            throw new DomainException("Password must contain at least one digit.");

        if (!HasSpecial.IsMatch(plainText))
            throw new DomainException("Password must contain at least one special character.");

        return new Password(plainText);
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj) => obj is Password other && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();
}
