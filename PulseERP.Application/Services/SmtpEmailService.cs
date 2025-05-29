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
}
