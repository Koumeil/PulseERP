using PulseERP.Domain.VO;

namespace PulseERP.Abstractions.Security.Interfaces;

/// <summary>
/// Domain service for managing user passwords (hashing, verification, change, reset).
/// Accepts and returns only Value Objects.
/// </summary>
public interface IPasswordService
{
    Task ChangePasswordAsync(Guid userId, string currentRawPassword, string newRawPassword);
    Task ForcePasswordResetAsync(EmailAddress emailAddress, string newRawPassword);
    string HashPassword(string rawPassword);
    Task RequestPasswordResetAsync(EmailAddress emailAddress);
    Task ResetPasswordAsync(Guid userId, string newRawPassword);
    Task ResetPasswordWithTokenAsync(string resetToken, string newRawPassword);
    bool VerifyPassword(string rawPassword, string storedHash);
}

// public interface IPasswordService
// {
//     string HashPassword(string password);
//     bool VerifyPassword(Password password, string passwordHash);

//     Task ChangePasswordAsync(Guid userId, Password currentPassword, Password newPassword);
//     Task ResetPasswordAsync(Guid userId, Password newPassword);
//     Task ForcePasswordResetAsync(EmailAddress email, Password newPassword);
//     Task RequestPasswordResetAsync(EmailAddress email);
//     Task ResetPasswordWithTokenAsync(string resetToken, Password newPassword);
// }
