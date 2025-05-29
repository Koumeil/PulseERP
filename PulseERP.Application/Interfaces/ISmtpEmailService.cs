namespace PulseERP.Application.Interfaces;

public interface ISmtpEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
}
