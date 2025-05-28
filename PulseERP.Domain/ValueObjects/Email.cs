using System.Net.Mail;

namespace PulseERP.Domain.ValueObjects;

public sealed record Email
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty.");

        var normalized = value.Trim().ToLowerInvariant();

        try
        {
            var mail = new MailAddress(normalized);
            if (mail.Address != normalized)
                throw new ArgumentException("Invalid email format.");
        }
        catch
        {
            throw new ArgumentException("Invalid email format.");
        }

        return new Email(normalized);
    }

    public override string ToString() => Value;

    public static explicit operator string(Email email) => email.Value;
}
