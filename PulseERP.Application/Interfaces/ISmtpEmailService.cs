namespace PulseERP.Application.Interfaces;

public interface ISmtpEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
    Task SendAccountLockedEmailAsync(string toEmail, string userFullName, DateTime lockoutEnd);
}
