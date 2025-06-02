using Api.Configuration;
using Api.Exceptions;
using Api.Interfaces;
using Api.Models;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Api.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings?.Value ?? throw new ArgumentNullException(nameof(emailSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ValidateConfiguration();
    }

    public async Task SendEmailAsync(EmailMessage emailMessage)
    {
        if (emailMessage == null)
            throw new ArgumentNullException(nameof(emailMessage));

        await SendEmailAsync(emailMessage.To, emailMessage.Subject, emailMessage.Body, emailMessage.IsHtml);
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
    {
        if (string.IsNullOrWhiteSpace(to))
            throw new ArgumentException("Recipient email cannot be null or empty", nameof(to));

        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Email subject cannot be null or empty", nameof(subject));

        try
        {
            using var client = CreateSmtpClient();
            using var message = CreateMailMessage(to, subject, body, isHtml);

            await client.SendMailAsync(message);

            _logger.LogInformation("Email sent successfully to {EmailAddress} with subject: {Subject}",
                to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {EmailAddress} with subject: {Subject}",
                to, subject);
            throw new EmailDeliveryException(to, ex.Message, ex);
        }
    }

    public async Task SendResetPasswordEmailAsync(string to, string token)
    {
        var resetUrl = $"{_emailSettings.Urls.BaseUrl}{_emailSettings.Urls.ResetPasswordPath}?token={token}";

        var template = CreateResetPasswordTemplate(resetUrl);

        await SendEmailAsync(to, template.Subject, template.HtmlBody, isHtml: true);
    }

    public async Task SendVerificationEmailAsync(string to, string token)
    {
        var verificationUrl = $"{_emailSettings.Urls.BaseUrl}{_emailSettings.Urls.VerifyEmailPath}?token={token}";

        var template = CreateVerificationEmailTemplate(verificationUrl);

        await SendEmailAsync(to, template.Subject, template.HtmlBody, isHtml: true);
    }

    public async Task SendWelcomeEmailAsync(string to, string userName)
    {
        var template = CreateWelcomeEmailTemplate(userName);

        await SendEmailAsync(to, template.Subject, template.HtmlBody, isHtml: true);
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            using var client = CreateSmtpClient();

            await client.SendMailAsync(new MailMessage());

            _logger.LogInformation("SMTP connection test successful");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SMTP connection test failed: {ErrorMessage}", ex.Message);
            return false;
        }
    }

    private SmtpClient CreateSmtpClient()
    {
        var client = new SmtpClient(_emailSettings.Smtp.Host, _emailSettings.Smtp.Port)
        {
            EnableSsl = _emailSettings.Smtp.EnableSsl,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_emailSettings.Smtp.Username, _emailSettings.Smtp.Password)
        };

        return client;
    }

    private MailMessage CreateMailMessage(string to, string subject, string body, bool isHtml)
    {
        var message = new MailMessage
        {
            From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = isHtml
        };

        message.To.Add(to);

        return message;
    }

    private EmailTemplate CreateResetPasswordTemplate(string resetUrl)
    {
        var subject = "Reset Your Password";

        var htmlBody = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>{subject}</title>
                </head>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <div style='background: #f8f9fa; padding: 30px; border-radius: 10px; text-align: center;'>
                        <h1 style='color: #2c3e50; margin-bottom: 30px;'>Reset Your Password</h1>
                        
                        <p style='font-size: 16px; margin-bottom: 30px;'>
                            We received a request to reset your password. Click the button below to create a new password:
                        </p>
                        
                        <a href='{resetUrl}' 
                           style='display: inline-block; background: #007bff; color: white; padding: 12px 30px; 
                                  text-decoration: none; border-radius: 5px; font-weight: bold; margin-bottom: 30px;'>
                            Reset Password
                        </a>
                        
                        <p style='font-size: 14px; color: #666; margin-bottom: 20px;'>
                            This link will expire in 10 minutes for security reasons.
                        </p>
                        
                        <p style='font-size: 14px; color: #666;'>
                            If you didn't request a password reset, please ignore this email. Your password will remain unchanged.
                        </p>
                        
                        <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'>
                        
                        <p style='font-size: 12px; color: #999;'>
                            If the button doesn't work, copy and paste this link into your browser:<br>
                            <a href='{resetUrl}' style='color: #007bff; word-break: break-all;'>{resetUrl}</a>
                        </p>
                    </div>
                </body>
                </html>";

        return new EmailTemplate
        {
            Subject = subject,
            HtmlBody = htmlBody
        };
    }

    private EmailTemplate CreateVerificationEmailTemplate(string verificationUrl)
    {
        var subject = "Verify Your Email Address";

        var htmlBody = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>{subject}</title>
                </head>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <div style='background: #f8f9fa; padding: 30px; border-radius: 10px; text-align: center;'>
                        <h1 style='color: #28a745; margin-bottom: 30px;'>Welcome! Please Verify Your Email</h1>
                        
                        <p style='font-size: 16px; margin-bottom: 30px;'>
                            Thank you for signing up! To complete your registration and secure your account, 
                            please verify your email address by clicking the button below:
                        </p>
                        
                        <a href='{verificationUrl}' 
                           style='display: inline-block; background: #28a745; color: white; padding: 12px 30px; 
                                  text-decoration: none; border-radius: 5px; font-weight: bold; margin-bottom: 30px;'>
                            Verify Email Address
                        </a>
                        
                        <p style='font-size: 14px; color: #666; margin-bottom: 20px;'>
                            This verification link will expire in 60 minutes.
                        </p>
                        
                        <p style='font-size: 14px; color: #666;'>
                            If you didn't create an account with us, please ignore this email.
                        </p>
                        
                        <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'>
                        
                        <p style='font-size: 12px; color: #999;'>
                            If the button doesn't work, copy and paste this link into your browser:<br>
                            <a href='{verificationUrl}' style='color: #28a745; word-break: break-all;'>{verificationUrl}</a>
                        </p>
                    </div>
                </body>
                </html>";

        return new EmailTemplate
        {
            Subject = subject,
            HtmlBody = htmlBody
        };
    }

    private EmailTemplate CreateWelcomeEmailTemplate(string userName)
    {
        var subject = "Welcome to  MyApp!";

        var htmlBody = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>{subject}</title>
                </head>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <div style='background: #f8f9fa; padding: 30px; border-radius: 10px; text-align: center;'>
                        <h1 style='color: #007bff; margin-bottom: 30px;'>Welcome to YourApp, {userName}!</h1>
                        
                        <p style='font-size: 16px; margin-bottom: 30px;'>
                            Your account has been successfully created and verified. We're excited to have you on board!
                        </p>
                        
                        <p style='font-size: 14px; color: #666; margin-bottom: 30px;'>
                            You can now start using all the features of our platform. If you have any questions, 
                            feel free to reach out to our support team.
                        </p>
                        
                        <a href='{_emailSettings.Urls.BaseUrl}' 
                           style='display: inline-block; background: #007bff; color: white; padding: 12px 30px; 
                                  text-decoration: none; border-radius: 5px; font-weight: bold;'>
                            Get Started
                        </a>
                    </div>
                </body>
                </html>";

        return new EmailTemplate
        {
            Subject = subject,
            HtmlBody = htmlBody
        };
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_emailSettings.Smtp.Host))
            throw new EmailConfigurationException("SMTP host is not configured");

        if (string.IsNullOrWhiteSpace(_emailSettings.FromEmail))
            throw new EmailConfigurationException("From email is not configured");

        if (string.IsNullOrWhiteSpace(_emailSettings.Urls.BaseUrl))
            throw new EmailConfigurationException("Base URL is not configured");
    }
}
