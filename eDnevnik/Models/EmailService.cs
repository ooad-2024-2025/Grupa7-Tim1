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

        public async Task<bool> PošaljiEmailAsync(string toEmail, string subject, string body)
        {
            try
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
                    Body = body,
                    IsBodyHtml = true
                };

                mail.To.Add(toEmail);

                await smtp.SendMailAsync(mail);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
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
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(toEmail);
            mail.Attachments.Add(new Attachment(new MemoryStream(attachment), filename));

            await smtp.SendMailAsync(mail);
        }

        public string KreirajHtmlSadržaj(string naslov, string poruka, string dodatniSadržaj = "")
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 30px; }}
        .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; color: #6c757d; font-size: 14px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>eDnevnik - Obavještenje</h1>
        </div>
        <div class='content'>
            <h2>{naslov}</h2>
            <p>{poruka}</p>
            {dodatniSadržaj}
            <p>Srdačan pozdrav,<br>Tim eDnevnik aplikacije</p>
        </div>
        <div class='footer'>
            <p>Ovo je automatsko obavještenje iz eDnevnik sistema.<br>
            Molimo da ne odgovarate na ovaj email.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}