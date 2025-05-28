namespace PulseERP.Domain.ValueObjects;

public record PhoneNumber
{
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

        if (trimmed.Length < 5)
            throw new ArgumentException("Phone number is too short.", nameof(value));

        // Ici tu peux ajouter d'autres validations, regex, etc.

        return new PhoneNumber(trimmed);
    }

    // Opérateur de conversion explicite vers string
    public static explicit operator string(PhoneNumber phone) => phone.Value;

    public override string ToString() => Value;
}
