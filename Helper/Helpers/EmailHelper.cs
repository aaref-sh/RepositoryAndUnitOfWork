using MimeKit;
using MailKit.Net.Smtp;
using Serilog;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Helper.Helpers;
public class EmailCredentials
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public class EmailService(ILogger logger, IConfiguration configuration)
{
    private readonly string? smtpHost = configuration["EmailSettings:SMTPHost"]?.ToString(); // Replace with your SMTP server
    private readonly int smtpPort = int.Parse(configuration["EmailSettings:SMTPPort"]?.ToString() ?? "0"); // Usually 587 for TLS/STARTTLS

    /// <summary>
    /// DO NOT await this function
    /// </summary>
    public async Task SendEmailAsync(EmailCredentials credentials, string to, string subject, string body)
    {
        try
        {
            Validate(credentials, to, subject, body);
            using MimeMessage message = new()
            {
                Body = new TextPart("html") { Text = body },
                Subject = subject
            };
            message.From.Add(new MailboxAddress("system", credentials.Email));
            message.To.Add(new MailboxAddress(to, to));

            using SmtpClient client = new();
            await client.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(credentials.Email, credentials.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to send email: ");
        }
    }

    private static void Validate(EmailCredentials credentials, string to, string subject, string body)
    {
        if (credentials == null) { throw new ArgumentNullException(nameof(credentials), "Email credentials cannot be null"); }
        if (string.IsNullOrEmpty(to)) { throw new ArgumentNullException(nameof(to), "Recipient email address cannot be null or empty"); }
        if (string.IsNullOrEmpty(subject)) { throw new ArgumentNullException(nameof(subject), "Email subject cannot be null or empty"); }
        if (string.IsNullOrEmpty(body)) { throw new ArgumentNullException(nameof(body), "Email body cannot be null or empty"); }
    }

    public EmailCredentials GetEmailCredentials(string accountName)
    {
        return configuration.GetSection($"EmailSettings:EmailsAddresses:{accountName}").Get<EmailCredentials>();
    }
}