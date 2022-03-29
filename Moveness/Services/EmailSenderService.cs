using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Moveness.Services
{
    public class EmailSenderService : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            SmtpClient client = new SmtpClient("smtp.egensajt.se", 587)
            {
                Credentials = new NetworkCredential("robot@motionskampen.se", "d2Jh^!5a7@P$4bQVJH^2ob8U#283AWiC"), //Update this when we have something better
                EnableSsl = true
            };

            MailMessage mailMessage = new MailMessage
            {
                From = new MailAddress("robot@motionskampen.se"),
            };

            mailMessage.To.Add(email);
            mailMessage.Subject = subject;
            mailMessage.Body = message;
            client.Send(mailMessage);

            return Task.FromResult(0);
        }
    }
}