namespace PulseERP.Domain.Interfaces.Services;

public interface ISmtpEmailService
{
    Task SendAccountLockedEmailAsync(string toEmail, string userFullName, DateTime lockoutEndUtc);
    Task SendPasswordChangedEmailAsync(string toEmail, string userFullName);
    Task SendWelcomeEmailAsync(string toEmail, string userFullName, string loginUrl);
    Task SendPasswordResetEmailAsync(
        string toEmail,
        string userFullName,
        string resetUrl,
        DateTime expiresAtUtc
    );
}
