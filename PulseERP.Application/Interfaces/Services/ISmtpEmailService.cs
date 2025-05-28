namespace PulseERP.Application.Interfaces.Services;

public interface ISmtpEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
}