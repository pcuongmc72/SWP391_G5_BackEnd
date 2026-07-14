namespace SWP.BLL.Interfaces;

/// <summary>
/// Service gửi email qua SMTP
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Gửi email với nội dung HTML
    /// </summary>
    Task SendEmailAsync(string toEmail, string subject, string htmlBody);
}
