using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using PulseERP.Domain.Interfaces.Services;
using PulseERP.Infrastructure.Smtp.Template;
using PulseERP.Shared.Settings;

namespace PulseERP.Infrastructure.Smtp;

public class SmtpEmailService : ISmtpEmailService
{
    private readonly EmailSettings _settings;

    public SmtpEmailService(IOptions<EmailSettings> opts) =>
        _settings = opts?.Value ?? throw new ArgumentNullException(nameof(opts));

    public Task SendAccountLockedEmailAsync(
        string toEmail,
        string userFullName,
        DateTime lockoutEnd
    )
    {
        var subject = "🔒 Votre compte Pulse ERP est temporairement bloqué";
        var builder = EmailTemplates.BuildAccountLocked(lockoutEnd, userFullName);
        return SendEmailAsync(toEmail, subject, builder);
    }

    public Task SendWelcomeEmailAsync(string toEmail, string userFullName, string loginUrl)
    {
        var subject = "🎉 Bienvenue sur Pulse ERP !";
        var builder = EmailTemplates.BuildWelcome(userFullName, loginUrl);
        return SendEmailAsync(toEmail, subject, builder);
    }

    public Task SendPasswordResetEmailAsync(
        string toEmail,
        string userFullName,
        string resetUrl,
        DateTime expiresAtUtc
    )
    {
        var subject = "🔑 Réinitialisation de votre mot de passe Pulse ERP";
        var builder = EmailTemplates.BuildPasswordReset(resetUrl, expiresAtUtc, userFullName);
        return SendEmailAsync(toEmail, subject, builder);
    }

    // Nouvelle méthode pour confirmer le changement de mot de passe
    public Task SendPasswordChangedEmailAsync(string toEmail, string userFullName)
    {
        var subject = "✅ Votre mot de passe Pulse ERP a bien été changé";
        var builder = EmailTemplates.BuildPasswordChanged(userFullName);
        return SendEmailAsync(toEmail, subject, builder);
    }

    // Méthode unique d’envoi
    private async Task SendEmailAsync(string toEmail, string subject, BodyBuilder bodyBuilder)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
            throw new ArgumentException("Recipient email cannot be empty.", nameof(toEmail));

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.FromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(
            _settings.MailServer,
            _settings.MailPort,
            SecureSocketOptions.StartTls
        );
        await client.AuthenticateAsync(_settings.FromEmail, _settings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
