using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using PulseERP.Abstractions.Security.Interfaces;
using PulseERP.Abstractions.Settings;
using PulseERP.Infrastructure.Smtp.Templates;

namespace PulseERP.Infrastructure.Smtp;

/// <summary>
/// Email service for PulseERP. Sends account-related emails (reset, welcome, lockout, etc.) via SMTP.
/// All templates are managed via EmailTemplates.
/// </summary>
public class SmtpEmailService(IOptions<EmailSettings> opts, ILogger<SmtpEmailService> logger)
    : IEmailSenderService
{
    private readonly EmailSettings _settings = opts?.Value ?? throw new ArgumentNullException(nameof(opts));

    public Task SendActivationEmailAsync(string toEmail, string userFullName, string activationUrl)
    {
        var subject = "🔔 Activez votre compte Pulse ERP";
        var builder = EmailTemplates.BuildActivation(activationUrl, userFullName);
        return SendEmailAsync(toEmail, subject, builder);
    }

    /// <inheritdoc />
    public Task SendAccountLockedEmailAsync(
        string toEmail,
        string userFullName,
        DateTime lockoutEndUtc
    )
    {
        var subject = "🔒 Your Pulse ERP account is temporarily locked";
        var builder = EmailTemplates.BuildAccountLocked(lockoutEndUtc, userFullName);
        return SendEmailAsync(toEmail, subject, builder);
    }

    /// <inheritdoc />
    public Task SendWelcomeEmailAsync(string toEmail, string userFullName, string loginUrl)
    {
        var subject = "🎉 Welcome to Pulse ERP!";
        var builder = EmailTemplates.BuildWelcome(userFullName, loginUrl);
        return SendEmailAsync(toEmail, subject, builder);
    }

    /// <inheritdoc />
    public Task SendPasswordChangedEmailAsync(string toEmail, string userFullName)
    {
        var subject = "✅ Your Pulse ERP password has been changed";
        var builder = EmailTemplates.BuildPasswordChanged(userFullName);
        return SendEmailAsync(toEmail, subject, builder);
    }

    /// <inheritdoc />
    public Task SendPasswordResetEmailAsync(
        string toEmail,
        string userFullName,
        string resetUrl,
        DateTime expiresAtUtc
    )
    {
        var subject = "🔑 Reset your Pulse ERP password";
        var builder = EmailTemplates.BuildPasswordReset(resetUrl, expiresAtUtc, userFullName);
        return SendEmailAsync(toEmail, subject, builder);
    }

    /// <summary>
    /// Sends an email with the provided subject and HTML/body content.
    /// </summary>
    private async Task SendEmailAsync(string toEmail, string subject, BodyBuilder bodyBuilder)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
            throw new ArgumentException("Recipient email cannot be empty.", nameof(toEmail));

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.FromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = bodyBuilder.ToMessageBody();

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(
                _settings.MailServer,
                _settings.MailPort,
                SecureSocketOptions.StartTls
            );
            await client.AuthenticateAsync(_settings.FromEmail, _settings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            logger.LogInformation(
                "Email '{Subject}' sent to {Recipient} at {SentAt}.",
                subject,
                toEmail,
                DateTime.UtcNow
            );
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to send email '{Subject}' to {Recipient}.",
                subject,
                toEmail
            );
            throw;
        }
    }
}
