using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace EmailSenderProgram
{
   public enum EmailType
    {
        Welcome,
        WelcomeBack,
        GoodBye
    }

    public class EmailContentViewModel
    {
        public string Subject { get; set; }
        public string Address { get; set; }
        public string SpecificTypeText { get; set; }
        public string VouchorCode { get; set; } = null;
    }
    public interface ICustomerRepository
    {
        Task<bool> CustomerMailSender(EmailType emailType, string vouchorCode);

        //Task<bool> CustomerMailSender( string vouchorCode);
        //Task<bool> SendEmailToNewCustomers();
    }
    public class CustomerRepository : ICustomerRepository
    {
        private readonly IEmailSender emailSender;

        public CustomerRepository(IEmailSender emailSender)
        {
            this.emailSender = emailSender;
        }

        public async Task<bool> CustomerMailSender(EmailType emailType, string vouchorCode = null)
        {
            try
            {
                switch (emailType)
                {
                    case EmailType.Welcome:
                        return await SendEmailToNewCustomers();

                    case EmailType.WelcomeBack:
                        return await SendEmailToOldCustomers(vouchorCode);

                    default:
                        throw new ArgumentException("Please provide valid Type");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Email Type : {0}  Exception:{1} ,DateAndTime:{2}",Enum.GetName(typeof(EmailType), emailType), ex.Message, DateTime.Now);
                return false;
            }

        }

        private async Task<bool> SendEmailToOldCustomers(string vouchorCode)
        {

                int customerCount = DataLayer.ListCustomers().Count();

                int batchSize = 2;
                int skip = 0;
                //using paggination if there is alot of data for better performance
                for (int i = 0; i < customerCount; i += batchSize)
                {

                    var cutonmers = DataLayer.ListCustomers()
                        .Skip(skip)
                        .Take(batchSize);
                    //Also we can send it in background Parallel using Thread
                    foreach (var customer in cutonmers)
                    {
                        if (DataLayer.ListOrders().Any(s => s.CustomerEmail == customer.Email))
                            continue;

                        await SendEmailToCustomer(
                            customer,
                            new EmailContentViewModel()
                            {
                                Subject = "We miss you as a customer",
                                Address = "info@EO.com",
                                SpecificTypeText = "<br>We miss you as a customer. Our shop is filled with nice products. Here is a voucher that gives you 50 kr to shop for.",
                                VouchorCode = vouchorCode
                            });
                    }

                    skip += batchSize;
                }

                return true;
            
        }
        private async Task<bool> SendEmailToNewCustomers()
        {


                int customerCount = DataLayer.ListCustomers().Count(s => s.CreatedDateTime > DateTime.Now.AddDays(-1));

                int batchSize = 2;
                int skip = 0;

                //using paggination if there is alot of data for better performance
                for (int i = 0; i < customerCount; i += batchSize)
                {

                    // customers 
                    List<Customer> newCustomers = DataLayer.ListCustomers()
                        .Where(s => s.CreatedDateTime > DateTime.Now.AddDays(-1))
                        .Skip(skip)
                        .Take(batchSize)
                        .ToList();
                    //Also we can send it in background Parallel using Thread
                    foreach (var customer in newCustomers)
                    {                  
                        
                       await SendEmailToCustomer(customer,
                           new EmailContentViewModel { 
                               Subject = "Welcome as a new customer at EO!",
                               Address = "info@EO.com", 
                               SpecificTypeText = "<br>We would like to welcome you as customer on our site!<br>" }
                           );

                    }
                    skip += batchSize;
                }
                //All mails are sent! Success!
                return true;
            
        }


        private async Task SendEmailToCustomer(Customer customer,EmailContentViewModel emailContent)
        {


            //Create a new MailMessage
            MailMessage mailMessage = new System.Net.Mail.MailMessage();

            //Add customer to reciever list
            mailMessage.To.Add(customer.Email);
            //Add subject
            mailMessage.Subject = emailContent.Subject;
            //Send mail from info@EO.com
            mailMessage.From = new System.Net.Mail.MailAddress(emailContent.Address);
            //Add body to mail
            mailMessage.Body = "Hi " + customer.Email;
            mailMessage.Body += emailContent.SpecificTypeText;
            if (emailContent.VouchorCode != null)
                mailMessage.Body += "<br>Voucher: " + emailContent.VouchorCode;
            mailMessage.Body += "<br><br>Best Regards,<br>EO Team";
#if DEBUG
            //Don't send mails in debug mode, just write the emails in console
            Console.WriteLine("Send customer mail to:" + customer.Email);
#else
            await emailSender.SendEmail(mailMessage);
#endif
        }

    }
}
