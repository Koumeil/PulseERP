using System.Text.RegularExpressions;

namespace PulseERP.Domain.ValueObjects;

public record PhoneNumber
{
    private static readonly Regex PhoneRegex = new(@"^\+?[0-9]{5,15}$", RegexOptions.Compiled);

    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(
                "Phone number cannot be empty or whitespace.",
                nameof(value)
            );

        var trimmed = value.Trim();

        if (!PhoneRegex.IsMatch(trimmed))
            throw new ArgumentException(
                "Invalid phone number format. It must be digits only and optionally start with a '+'.",
                nameof(value)
            );

        return new PhoneNumber(trimmed);
    }

    public static explicit operator string(PhoneNumber phone) => phone.Value;

    public override string ToString() => Value;
}
