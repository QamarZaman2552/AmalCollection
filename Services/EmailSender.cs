using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

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
        public string FromName { get; set; } = "Amal Collection";

        // HTTPS transactional email API (e.g. Brevo). Preferred on hosts
        // that block outbound SMTP (Railway free/trial plans).
        public string ApiKey { get; set; } = string.Empty;
        public string ApiUrl { get; set; } = "https://api.brevo.com/v3/smtp/email";

        // Where admin notifications are sent (new registration, new order).
        // Falls back to FromEmail when empty.
        public string AdminEmail { get; set; } = string.Empty;
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
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            await client.SendMailAsync(message);
        }
    }

    public class HttpEmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;
        private readonly HttpClient _httpClient;

        public HttpEmailSender(EmailSettings settings, HttpClient httpClient)
        {
            _settings = settings;
            _httpClient = httpClient;
        }

        public async Task SendAsync(string toEmail, string subject, string body)
        {
            var payload = new
            {
                sender = new { name = _settings.FromName, email = _settings.FromEmail },
                to = new[] { new { email = toEmail } },
                subject,
                htmlContent = body
            };

            var json = JsonSerializer.Serialize(payload);
            using var request = new HttpRequestMessage(HttpMethod.Post, _settings.ApiUrl)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.TryAddWithoutValidation("api-key", _settings.ApiKey);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Email API error {(int)response.StatusCode}: {error}");
            }
        }
    }
}

