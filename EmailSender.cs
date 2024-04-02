using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace EmailSenderProgram
{
    public interface IEmailSender
    {
        Task SendEmail(MailMessage mailMessage);
    }

    public  class EmailSender : IEmailSender
    {
        public async Task SendEmail(MailMessage mailMessage)
        {
            //Create a SmtpClient to our smtphost: yoursmtphost
            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("yoursmtphost");
            //Send mail
            smtp.Send(mailMessage);
        }
    }
}
