using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WaqfSystem.Core.Interfaces;

namespace WaqfSystem.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendAsync(string to, string subject, string htmlBody)
        {
            try
            {
                using var message = BuildMessage(to, subject, htmlBody);
                using var client = BuildClient();
                await client.SendMailAsync(message);
                _logger.LogInformation("Email sent to {To}", to);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email send failed to {To}", to);
                return false;
            }
        }

        public async Task<bool> SendWithAttachmentAsync(string to, string subject, string htmlBody, byte[] attachment, string attachmentName)
        {
            try
            {
                using var message = BuildMessage(to, subject, htmlBody);
                message.Attachments.Add(new Attachment(new System.IO.MemoryStream(attachment), attachmentName, "application/pdf"));
                using var client = BuildClient();
                await client.SendMailAsync(message);
                _logger.LogInformation("Email with attachment sent to {To}", to);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email with attachment failed to {To}", to);
                return false;
            }
        }

        private MailMessage BuildMessage(string to, string subject, string htmlBody)
        {
            var fromAddress = _configuration["Email:FromAddress"] ?? "noreply@waqf-sunni.iq";
            var fromName = _configuration["Email:FromName"] ?? "هيئة الوقف السني";

            var message = new MailMessage
            {
                From = new MailAddress(fromAddress, fromName),
                Subject = subject,
                Body = $"<div style='direction:rtl;font-family:Arial'>{htmlBody}</div>",
                IsBodyHtml = true
            };

            message.To.Add(to);
            return message;
        }

        private SmtpClient BuildClient()
        {
            var host = _configuration["Email:Host"] ?? string.Empty;
            var port = int.TryParse(_configuration["Email:Port"], out var parsedPort) ? parsedPort : 587;
            var username = _configuration["Email:Username"] ?? string.Empty;
            var password = _configuration["Email:Password"] ?? string.Empty;
            var useSsl = bool.TryParse(_configuration["Email:UseSsl"], out var parsedUseSsl) && parsedUseSsl;

            return new SmtpClient(host, port)
            {
                EnableSsl = useSsl,
                Credentials = new NetworkCredential(username, password)
            };
        }
    }
}
