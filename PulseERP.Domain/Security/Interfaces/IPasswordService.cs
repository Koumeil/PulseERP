using PulseERP.Domain.ValueObjects.Adresses;
using PulseERP.Domain.ValueObjects.Passwords;

namespace PulseERP.Domain.Security.Interfaces;

/// <summary>
/// Domain service for managing user passwords (hashing, verification, change, reset).
/// Accepts and returns only Value Objects.
/// </summary>
public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(Password password, string passwordHash);

    Task ChangePasswordAsync(Guid userId, Password currentPassword, Password newPassword);
    Task ResetPasswordAsync(Guid userId, Password newPassword);
    Task ForcePasswordResetAsync(EmailAddress email, Password newPassword);
    Task RequestPasswordResetAsync(EmailAddress email);
    Task ResetPasswordWithTokenAsync(string resetToken, Password newPassword);
}
