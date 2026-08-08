using System.Net;
using System.Net.Mail;

namespace web_do_an1.Services;

public class EmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendOtpAsync(string toEmail, string otp)
    {
        var host = _configuration["Email:Gmail:SmtpHost"] ?? "smtp.gmail.com";
        var port = int.TryParse(_configuration["Email:Gmail:Port"], out var parsedPort) ? parsedPort : 587;
        var userName = _configuration["Email:Gmail:UserName"]
            ?? Environment.GetEnvironmentVariable("GMAIL_USER");
        var appPassword = _configuration["Email:Gmail:AppPassword"]
            ?? Environment.GetEnvironmentVariable("GMAIL_APP_PASSWORD");
        var fromName = _configuration["Email:Gmail:FromName"] ?? "Trung tâm Tiếng Anh";

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(appPassword))
        {
            _logger.LogWarning("Chưa cấu hình Email:Gmail:UserName hoặc Email:Gmail:AppPassword.");
            return false;
        }

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(userName, appPassword)
        };

        using var message = new MailMessage
        {
            From = new MailAddress(userName, fromName),
            Subject = "Mã xác minh đăng ký tài khoản",
            Body = $"Mã OTP của bạn là {otp}. Mã có hiệu lực trong 10 phút.",
            IsBodyHtml = false
        };
        message.To.Add(toEmail);

        try
        {
            await client.SendMailAsync(message);
            return true;
        }
        catch (Exception ex) when (ex is SmtpException or InvalidOperationException or IOException)
        {
            _logger.LogWarning(ex, "Không gửi được OTP qua Gmail SMTP.");
            return false;
        }
    }
}
