using System.Net.Mail;
using System.Text.RegularExpressions;
using PulseERP.Domain.Errors;

namespace PulseERP.Domain.ValueObjects;

public sealed partial record EmailAddress
{
    public string Value { get; }

    private EmailAddress(string value) => Value = value;

    public static EmailAddress Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email vide.");

        // 1) On normalise toujours en minuscules invariante
        var normalized = value.Trim().ToLowerInvariant();

        // 2) On utilise la regex générée au compile-time
        if (!EmailRegex().IsMatch(normalized) || !MailAddress.TryCreate(normalized, out _))
            throw new DomainException("Email invalide.");

        return new EmailAddress(normalized);
    }

    public EmailAddress Update(string newValue)
    {
        if (string.Equals(Value, newValue?.Trim(), StringComparison.OrdinalIgnoreCase))
            return this;

        // Revalidate et recrée si différent
        return Create(newValue!);
    }

    public override string ToString() => Value;

    public static explicit operator string(EmailAddress e) => e.Value;

    public static implicit operator EmailAddress(string v) => Create(v);

    // Méthode partielle décorée pour générer la Regex au build
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();
}
