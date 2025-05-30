using System.Net.Mail;
using System.Text.RegularExpressions;
using PulseERP.Domain.Errors;

public sealed record Email
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email cannot be empty.");

        var normalized = value.Trim().ToLowerInvariant();

        if (!EmailRegex.IsMatch(normalized))
            throw new DomainException("Email format is invalid.");

        try
        {
            _ = new MailAddress(normalized);
        }
        catch
        {
            throw new DomainException("Email is not valid.");
        }

        return new Email(normalized);
    }

    /// <summary>
    /// Retourne la même instance si identique, ou une nouvelle instance validée.
    /// </summary>
    public Email Update(string newValue)
    {
        if (string.Equals(Value, newValue?.Trim(), StringComparison.OrdinalIgnoreCase))
            return this;
        return Create(newValue);
    }

    public override string ToString() => Value;

    public static explicit operator string(Email email) => email.Value;

    public static implicit operator Email(string value) => Create(value);
}
