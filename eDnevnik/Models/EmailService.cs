using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace eDnevnik.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, byte[] attachment, string filename)
        {
            var settings = _config.GetSection("EmailSettings");

            var smtp = new SmtpClient
            {
                Host = settings["SmtpServer"],
                Port = int.Parse(settings["Port"]),
                Credentials = new NetworkCredential(settings["Username"], settings["Password"]),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(settings["From"]),
                Subject = subject,
                Body = body
            };

            mail.To.Add(toEmail);
            mail.Attachments.Add(new Attachment(new MemoryStream(attachment), filename));

            await smtp.SendMailAsync(mail);
        }
    }
}