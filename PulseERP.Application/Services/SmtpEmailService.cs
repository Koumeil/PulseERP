using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using PulseERP.Application.Interfaces.Services;
using PulseERP.Application.Settings;

public class SmtpEmailService : ISmtpEmailService
{
    private readonly EmailSettings _emailSettings;

    public SmtpEmailService(IOptions<EmailSettings> options)
    {
        _emailSettings = options.Value;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.FromEmail));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;

        email.Body = new BodyBuilder { HtmlBody = body }.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(
            _emailSettings.MailServer,
            _emailSettings.MailPort,
            SecureSocketOptions.StartTls
        );

        await smtp.AuthenticateAsync(_emailSettings.FromEmail, _emailSettings.Password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}
