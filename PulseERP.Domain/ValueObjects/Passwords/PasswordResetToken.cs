namespace PulseERP.Domain.ValueObjects.Passwords;

/// <summary>
/// Represents a secure password reset token value object.
/// </summary>
public sealed record PasswordResetToken
{
    public string Value { get; }

    private PasswordResetToken(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length < 32)
            throw new ArgumentException("Token must be a secure, non-empty string.", nameof(value));
        Value = value;
    }

    public static PasswordResetToken Create(string value) => new(value);

    public override string ToString() => Value;
}
