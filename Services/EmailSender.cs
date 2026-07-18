using System.Net;
using System.Net.Mail;

namespace ShoppingApp.Services
{
    public class EmailSettings
    {
        public bool Enabled { get; set; }
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = "ShopAI";
    }

    public interface IEmailSender
    {
        Task SendAsync(string toEmail, string subject, string body);
    }

    public class DevEmailSender : IEmailSender
    {
        private readonly ILogger<DevEmailSender> _logger;

        public DevEmailSender(ILogger<DevEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(string toEmail, string subject, string body)
        {
            _logger.LogInformation("DEV EMAIL => To: {To}, Subject: {Subject}, Body: {Body}", toEmail, subject, body);
            return Task.CompletedTask;
        }
    }

    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;

        public SmtpEmailSender(EmailSettings settings)
        {
            _settings = settings;
        }

        public async Task SendAsync(string toEmail, string subject, string body)
        {
            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.EnableSsl,
                Credentials = new NetworkCredential(_settings.UserName, _settings.Password)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            message.To.Add(toEmail);

            await client.SendMailAsync(message);
        }
    }
}

