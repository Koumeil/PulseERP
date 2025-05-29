using System.Text.RegularExpressions;
using PulseERP.Domain.Exceptions;

namespace PulseERP.Domain.ValueObjects;

public sealed class Phone : IEquatable<Phone>
{
    public string Value { get; }
    private static readonly Regex PhoneRegex = new(@"^\+?[0-9]{5,15}$", RegexOptions.Compiled);

    private Phone(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentNullException(nameof(value));

        Value = value;
    }

    public static Phone Create(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new DomainException("Phone number cannot be empty.");

        var trimmed = number.Trim();

        if (!PhoneRegex.IsMatch(trimmed))
            throw new DomainException(
                "Invalid phone number format. It must contain 5-15 digits and optionally start with a '+'."
            );

        return new Phone(trimmed);
    }

    public bool Equals(Phone? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is Phone other && Equals(other));
    }

    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
    }

    public override string ToString() => Value;

    public static bool operator ==(Phone? left, Phone? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Phone? left, Phone? right)
    {
        return !Equals(left, right);
    }
}
