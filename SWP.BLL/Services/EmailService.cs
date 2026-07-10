using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using SWP.BLL.Interfaces;

namespace SWP.BLL.Services;

/// <summary>
/// Gửi email thực qua SMTP (Gmail, Brevo, v.v.)
/// Cấu hình trong appsettings.Development.json → "EmailSettings"
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var s = _config.GetSection("EmailSettings");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(s["SenderName"], s["SenderEmail"]));
        message.To.Add(new MailboxAddress(string.Empty, toEmail));
        message.Subject = subject;

        var body = new BodyBuilder { HtmlBody = htmlBody };
        message.Body = body.ToMessageBody();

        using var client = new SmtpClient();

        // Kết nối SMTP (Gmail port 587 / StartTLS)
        await client.ConnectAsync(
            s["SmtpServer"],
            int.Parse(s["SmtpPort"]!),
            SecureSocketOptions.StartTls
        );

        await client.AuthenticateAsync(s["Username"], s["Password"]);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
