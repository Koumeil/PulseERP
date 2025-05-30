using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using PulseERP.Application.Interfaces;
using PulseERP.Application.Settings;

public class SmtpEmailService : ISmtpEmailService
{
    private readonly EmailSettings _settings;

    public SmtpEmailService(IOptions<EmailSettings> opts)
    {
        _settings = opts?.Value ?? throw new ArgumentNullException(nameof(opts));
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
            throw new ArgumentException("Recipient email cannot be empty.", nameof(toEmail));
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.FromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = body }.ToMessageBody();
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

    public Task SendAccountLockedEmailAsync(
        string toEmail,
        string userFullName,
        DateTime lockoutEnd
    )
    {
        var subject = "🔒 Votre compte Pulse ERP est temporairement bloqué";
        var body =
            $@"
Bonjour {userFullName},

Suite à plusieurs tentatives de connexion échouées, votre compte a été temporairement verrouillé.

Vous pourrez essayer à nouveau après : {lockoutEnd:HH:mm dd/MM/yyyy}.

Si ce n’est pas vous, nous vous recommandons de changer votre mot de passe dès que possible.

— L’équipe Pulse ERP
";

        return SendEmailAsync(toEmail, subject, body); // méthode existante
    }
}
