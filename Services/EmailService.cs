using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging; // 👈
using Microsoft.Extensions.Options;
using MimeKit;

namespace RecoleccionResiduosApi.Services
{
    public class SmtpOptions
    {
        public string Host { get; set; } = "";
        public int Port { get; set; } = 25;
        public string User { get; set; } = "";
        public string Pass { get; set; } = "";
        public bool UseSsl { get; set; } = false; // true = SSL; false = StartTLS/cuando esté disponible
        public string From { get; set; } = "no-reply@tuapp.com";
        public string FromName { get; set; } = "Soporte";
    }

    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string htmlBody);
    }

    public class EmailService : IEmailService
    {
        private readonly SmtpOptions _opt;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<SmtpOptions> opt, ILogger<EmailService> logger)
        {
            _opt = opt.Value;
            _logger = logger;
        }

        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(_opt.FromName, _opt.From));
            msg.To.Add(MailboxAddress.Parse(to));
            msg.Subject = subject;
            msg.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new SmtpClient();
            var secure = _opt.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable;
            await client.ConnectAsync(_opt.Host, _opt.Port, secure);
            if (!string.IsNullOrWhiteSpace(_opt.User))
                await client.AuthenticateAsync(_opt.User, _opt.Pass);

            await client.SendAsync(msg);
            await client.DisconnectAsync(true);
        }
    }
}
