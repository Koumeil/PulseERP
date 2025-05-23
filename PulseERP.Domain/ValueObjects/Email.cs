using System.ComponentModel.DataAnnotations;

namespace PulseERP.Domain.ValueObjects;

public record Email
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty");

        value = value.Trim().ToLowerInvariant();

        if (!new EmailAddressAttribute().IsValid(value))
            throw new ArgumentException("Invalid email format");

        Value = value;
    }

    public static explicit operator string(Email email) => email.Value;
}
