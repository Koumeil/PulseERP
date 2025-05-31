namespace PulseERP.Abstractions.Security.Interfaces;

public interface IPasswordService
{
    string HashPassword(string plainPassword);
    bool VerifyPassword(string plainPassword, string hashedPassword);
    Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    Task ResetPasswordAsync(Guid userId, string newPassword);
    Task ForcePasswordResetAsync(string email, string newPassword);
    Task RequestPasswordResetAsync(string email);
    Task ResetPasswordWithTokenAsync(string resetToken, string newPassword);
}
