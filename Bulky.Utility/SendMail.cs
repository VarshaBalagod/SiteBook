using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Text;
using SmtpClient = System.Net.Mail.SmtpClient;

namespace Bulky.Utility
{
    public class SendMail : IEmailSender
    {
        private readonly MailSettings _mailSettings;
        public SendMail(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // logic to send email on regiser
            // for now its just task completed

            //MailMessage msg = new MailMessage();
            //MailAddress fromAdd = new MailAddress(_mailSettings.Mail);
            //msg.To.Add(email);
            //msg.Subject = subject;
            //msg.From = fromAdd;
            //msg.IsBodyHtml = true;
            //msg.Priority = MailPriority.Normal;
            //msg.BodyEncoding = Encoding.Default;
            //msg.Body = "<center><table><tr><td><h1>"+_mailSettings.DisplayName+"</h1><br/><br/></td></tr>";
            //msg.Body = "<center><table><tr><td><p>" + htmlMessage + "</p><br/><br/></td></tr>";
            //msg.Body = msg.Body + "</table></center>";
            //SmtpClient smtpClient = new SmtpClient(_mailSettings.Host,_mailSettings.Port);
            //smtpClient.EnableSsl = true;
            //smtpClient.UseDefaultCredentials = false;
            //smtpClient.Credentials = new System.Net.NetworkCredential(_mailSettings.Mail, _mailSettings.Password);
            //smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            //smtpClient.Send(msg);
            //smtpClient.Dispose();

            return Task.CompletedTask;
        }

        //public string SendGridSecret { get; set; }
        //public SendMail(IConfiguration _config)
        //{
        //    // get value from config file i.e. application.json
        //    SendGridSecret = _config.GetValue<string>("SendGride:Secreatekey");
        //}

        //public Task SendEmailAsync(string email, string subject, string htmlMessage)
        //{
        //    // logic to send mail

        //    var client = "";
        //    var from = "";
        //    var to = "";
        //    var message = ( from,to,subject,"",htmlMessage);

        //    return client.SendEmailAsync(message);
        //}

    }
}
