using System.Text.RegularExpressions;
using PulseERP.Domain.Errors;

public sealed class Phone : IEquatable<Phone>
{
    public string Value { get; }
    private static readonly Regex PhoneRegex = new(@"^\+?[0-9]{5,15}$", RegexOptions.Compiled);

    private Phone(string value)
    {
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

    /// <summary>
    /// Retourne la même instance si identique, ou une nouvelle instance validée.
    /// </summary>
    public Phone Update(string newValue)
    {
        var trimmed = newValue?.Trim();
        if (string.Equals(Value, trimmed, StringComparison.OrdinalIgnoreCase))
            return this;
        return Create(trimmed!);
    }

    public bool Equals(Phone? other) =>
        other is not null && string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object? obj) => obj is Phone other && Equals(other);

    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

    public override string ToString() => Value;

    public static bool operator ==(Phone? left, Phone? right) => Equals(left, right);

    public static bool operator !=(Phone? left, Phone? right) => !Equals(left, right);
}
