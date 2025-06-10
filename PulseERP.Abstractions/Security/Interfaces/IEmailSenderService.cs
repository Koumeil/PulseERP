namespace PulseERP.Abstractions.Security.Interfaces;

public interface IEmailSenderService
{
    Task SendAccountLockedEmailAsync(string toEmail, string userFullName, DateTime lockoutEndUtc);
    Task SendPasswordChangedEmailAsync(string toEmail, string userFullName);
    Task SendWelcomeEmailAsync(string toEmail, string userFullName, string loginUrl);
    Task SendActivationEmailAsync(string toEmail, string userFullName, string activationUrl);
    Task SendPasswordResetEmailAsync(
        string toEmail,
        string userFullName,
        string resetUrl,
        DateTime expiresAtUtc
    );
}
