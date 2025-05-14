using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Utils;
using System.Threading.Tasks;

namespace CineTicket.Repositories
{
    public class GmailSender : IGmailSender, IEmailSender
    {
        private readonly IConfiguration _configuration;

        public GmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmail(string to, string subject, string body)
        {
            await SendEmailAsync(to, subject, body);
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtpSection = _configuration.GetSection("EmailSettings");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("CineTicket", smtpSection["FromEmail"]));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;

            message.Body = new TextPart("html") { Text = htmlMessage };

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpSection["SMTPServer"], int.Parse(smtpSection["Port"]), SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(smtpSection["Username"], smtpSection["Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task SendEmailWithAttachmentAsync(string email, string subject, string htmlMessage, byte[] attachment, string filename)
        {
            var smtpSection = _configuration.GetSection("EmailSettings");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("CineTicket", smtpSection["FromEmail"]));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = htmlMessage
            };

            builder.Attachments.Add(filename, attachment);
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpSection["SMTPServer"], int.Parse(smtpSection["Port"]), SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(smtpSection["Username"], smtpSection["Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
