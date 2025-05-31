using System.Net.Mail;
using System.Text.RegularExpressions;
using PulseERP.Domain.Errors;

namespace PulseERP.Domain.ValueObjects;

public sealed record EmailAddress
{
    private static readonly Regex SimpleRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public string Value { get; }

    private EmailAddress(string value) => Value = value;

    public static EmailAddress Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email vide.");

        var normalized = value.Trim().ToLowerInvariant();
        if (!SimpleRegex.IsMatch(normalized) || !MailAddress.TryCreate(normalized, out _))
            throw new DomainException("Email invalide.");

        return new EmailAddress(normalized);
    }

    public EmailAddress Update(string newValue) =>
        string.Equals(Value, newValue?.Trim(), StringComparison.OrdinalIgnoreCase)
            ? this
            : Create(newValue!);

    public override string ToString() => Value;

    public static explicit operator string(EmailAddress e) => e.Value;

    public static implicit operator EmailAddress(string v) => Create(v);
}
