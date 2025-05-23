namespace PulseERP.Domain.ValueObjects;

public record PhoneNumber
{
    public string Value { get; }

    public PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone cannot be empty");

        value = value.Trim();

        if (value.Length < 5)
            throw new ArgumentException("Phone number is too short");

        Value = value;
    }

    public static explicit operator string(PhoneNumber phone) => phone.Value;
}
