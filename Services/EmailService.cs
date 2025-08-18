using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
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
        // true = SSL (465); false = STARTTLS (587)
        public bool UseSsl { get; set; } = false;
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

            try
            {
                // 465 => SSL directo; 587 => STARTTLS (forzado)
                var secure = _opt.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
                _logger.LogInformation("SMTP connect to {Host}:{Port} (Secure={Secure})", _opt.Host, _opt.Port, secure);
                await client.ConnectAsync(_opt.Host, _opt.Port, secure);
            }
            catch (SslHandshakeException ex) when (_opt.Host.Equals("smtp-relay.brevo.com", StringComparison.OrdinalIgnoreCase))
            {
                // Fallback por mismatch de CN/SAN (Brevo presenta *.sendinblue.com)
                _logger.LogWarning(ex, "Handshake falló con {Host}. Reintentando con smtp-relay.sendinblue.com ...", _opt.Host);
                await client.ConnectAsync("smtp-relay.sendinblue.com", _opt.Port, SecureSocketOptions.StartTls);
            }

            if (!string.IsNullOrWhiteSpace(_opt.User))
            {
                await client.AuthenticateAsync(_opt.User, _opt.Pass);
            }

            await client.SendAsync(msg);
            await client.DisconnectAsync(true);
        }
    }
}
