using ITAMS.Data;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;

namespace ITAMS.Services;

public interface IEmailService
{
    Task SendAlertEmailAsync(string toEmail, string subject, string body);
    Task SendAlertToRoleAsync(string roleName, string subject, string body, ITAMSDbContext context);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAlertEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            var host = _config["Email:SmtpHost"] ?? "smtp.gmail.com";
            var port = int.Parse(_config["Email:SmtpPort"] ?? "587");
            var user = _config["Email:SmtpUser"] ?? "";
            var pass = _config["Email:SmtpPassword"] ?? "";
            var from = _config["Email:FromAddress"] ?? user;
            var fromName = _config["Email:FromName"] ?? "ITAMS Alerts";

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                _logger.LogWarning("Email not configured — skipping send to {Email}", toEmail);
                return;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, from));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(user, pass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Alert email sent to {Email}: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
        }
    }

    public async Task SendAlertToRoleAsync(string roleName, string subject, string body, ITAMSDbContext context)
    {
        var users = await context.Users
            .Include(u => u.Role)
            .Where(u => u.IsActive && u.Role.Name == roleName && u.Email != null)
            .ToListAsync();

        foreach (var user in users)
        {
            await SendAlertEmailAsync(user.Email, subject, body);
        }
    }
}
