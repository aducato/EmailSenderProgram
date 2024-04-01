using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailSenderProgram
{
    public class CustomerRepository
    {
        public  async Task<bool> CustomerMailSender(string vouchorCode)
        {
            try
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

                        await SendEmailToCustomer(customer, vouchorCode);
                    }

                    skip += batchSize;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("CustomerMailSender Exception:{0} ,DateAndTime:{1}", ex.Message, DateTime.Now);
                return false;
            }

        }
        public  async Task<bool> SendEmailToNewCustomers()
        {
            try
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
                        await SendNewCusomerEmail(customer);
                    }
                    skip += batchSize;
                }
                //All mails are sent! Success!
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("SendEmailToNewCustomers Exception:{0} ,DateAndTime:{1}", ex.Message, DateTime.Now);
                return false;
            }
        }



        private static async Task SendEmailToCustomer(Customer customer, string vouchorCode)
        {


            //Create a new MailMessage
            System.Net.Mail.MailMessage mailMessage = new System.Net.Mail.MailMessage();

            //Add customer to reciever list
            mailMessage.To.Add(customer.Email);
            //Add subject
            mailMessage.Subject = "We miss you as a customer";
            //Send mail from info@EO.com
            mailMessage.From = new System.Net.Mail.MailAddress("infor@EO.com");
            //Add body to mail
            mailMessage.Body = "Hi " + customer.Email +
                     "<br>We miss you as a customer. Our shop is filled with nice products. Here is a voucher that gives you 50 kr to shop for." +
                     "<br>Voucher: " + vouchorCode +
                     "<br><br>Best Regards,<br>EO Team";
#if DEBUG
            //Don't send mails in debug mode, just write the emails in console
            Console.WriteLine("Send customer mail to:" + customer.Email);
#else


await EmailSender.SendEmail(mailMessage);

#endif
        }


        private static async Task SendNewCusomerEmail(Customer customer)
        {
            System.Net.Mail.MailMessage mailMessage = new System.Net.Mail.MailMessage();
            //Add customer to reciever list
            mailMessage.To.Add(customer.Email);
            //Add subject
            mailMessage.Subject = "Welcome as a new customer at EO!";
            //Send mail from info@EO.com
            mailMessage.From = new System.Net.Mail.MailAddress("info@EO.com");
            //Add body to mail
            mailMessage.Body = "Hi " + customer.Email +
                     "<br>We would like to welcome you as customer on our site!<br><br>Best Regards,<br>EO Team";
#if DEBUG
            //Don't send mails in debug mode, just write the emails in console
            Console.WriteLine("Send new customer mail to:" + customer.Email);
#else
	await SendEmail(mailMessage);
#endif
        }
    }
}
