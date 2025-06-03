using System.Net.Mail;
using System.Text.RegularExpressions;
using PulseERP.Domain.Errors;

namespace PulseERP.Domain.ValueObjects.Adresses;

/// <summary>
/// Value Object for email addresses (centralized validation, normalization).
/// Immutable.
/// </summary>
public sealed partial record EmailAddress
{
    /// <summary>
    /// The normalized email value.
    /// </summary>
    public string Value { get; }

    private EmailAddress(string value) => Value = value;

    /// <summary>
    /// Factory method: creates and validates an email address (normalizes to lowercase, checks regex + RFC).
    /// </summary>
    /// <param name="value">The input email string.</param>
    /// <returns>A valid, normalized EmailAddress.</returns>
    /// <exception cref="DomainException">If the email is invalid.</exception>
    public static EmailAddress Create(string value)
    {
        if (!IsValid(value, out var normalized, out var error))
            throw new DomainException(error!);

        return new EmailAddress(normalized!);
    }

    /// <summary>
    /// Validates and normalizes an email. Returns true/false, normalized email, and error reason if failed.
    /// </summary>
    /// <param name="value">The email to validate.</param>
    /// <param name="normalized">Normalized (lowercase, trimmed) email if valid.</param>
    /// <param name="error">Error message if invalid, null otherwise.</param>
    /// <returns>True if valid, false otherwise.</returns>
    public static bool IsValid(string? value, out string? normalized, out string? error)
    {
        normalized = null;
        error = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            error = "Email cannot be empty.";
            return false;
        }

        var val = value.Trim().ToLowerInvariant();

        if (!EmailRegex().IsMatch(val) || !MailAddress.TryCreate(val, out _))
        {
            error = "Invalid email format.";
            return false;
        }

        normalized = val;
        return true;
    }

    /// <summary>
    /// Updates the email address (validates and normalizes new value).
    /// </summary>
    public EmailAddress Update(string value)
    {
        var normalized = Normalize(value);
        if (normalized == this.Value)
            return this;
        return new EmailAddress(normalized);
    }

    // Méthode de normalisation (trim, lower, etc.)
    private static string Normalize(string email) => email.Trim().ToLowerInvariant();

    public override string ToString() => Value;

    /// <summary>
    /// Explicit conversion to string (returns normalized email).
    /// </summary>
    public static explicit operator string(EmailAddress e) => e.Value;

    /// <summary>
    /// Implicit conversion from string to EmailAddress (validates/normalizes).
    /// </summary>
    public static implicit operator EmailAddress(string v) => Create(v);

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();
}
