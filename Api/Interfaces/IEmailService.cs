using Api.Models;

namespace Api.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(EmailMessage emailMessage);
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = false);
    Task SendResetPasswordEmailAsync(string to, string token);
    Task SendVerificationEmailAsync(string to, string token);
    Task SendWelcomeEmailAsync(string to, string userName);
    Task<bool> TestConnectionAsync();
}
