using ExpenseTracker.Application.Interfaces.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace ExpenseTracker.Infrastructure.Services.Email;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    private async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_config["Email:SenderName"], _config["Email:SenderEmail"]));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_config["Email:SmtpHost"], int.Parse(_config["Email:SmtpPort"]!), SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_config["Email:Username"], _config["Email:Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
        }
    }

    public async Task SendWelcomeEmailAsync(string email, string fullName, string verificationToken)
    {
        var url = $"{_config["App:BaseUrl"]}/api/auth/verify-email?email={Uri.EscapeDataString(email)}&token={verificationToken}";
        await SendAsync(email, fullName, "Welcome to ExpenseTracker - Verify Your Email", EmailTemplates.Welcome(fullName, url));
    }

    public async Task SendPasswordResetEmailAsync(string email, string fullName, string resetToken)
    {
        var url = $"{_config["App:FrontendUrl"]}/reset-password?email={Uri.EscapeDataString(email)}&token={resetToken}";
        await SendAsync(email, fullName, "Reset Your ExpenseTracker Password", EmailTemplates.PasswordReset(fullName, url));
    }

    public async Task SendEmailVerificationAsync(string email, string fullName, string verificationToken)
    {
        var url = $"{_config["App:BaseUrl"]}/api/auth/verify-email?email={Uri.EscapeDataString(email)}&token={verificationToken}";
        await SendAsync(email, fullName, "Verify Your ExpenseTracker Email", EmailTemplates.EmailVerification(fullName, url));
    }

    public async Task SendPasswordChangedNotificationAsync(string email, string fullName)
        => await SendAsync(email, fullName, "Your Password Has Been Changed", EmailTemplates.PasswordChanged(fullName));

    public async Task SendGenericEmailAsync(string email, string subject, string body)
        => await SendAsync(email, email, subject, body);
}
