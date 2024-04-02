using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Threading.Tasks;

namespace EmailSenderProgram
{
    internal class Program
    {
        private readonly CustomerRepository _customerRepository;
        public Program(CustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        /// <summary>
        /// This application is run everyday
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            Console.WriteLine("Send Welcomemail");
            
            Program program = new Program(new CustomerRepository(new EmailSender()));

            program.RunProgram();

            Console.ReadKey();
        }


        private async Task RunProgram()
        {
            // using await if second one is dependent on first one else not required
            bool success = await _customerRepository.CustomerMailSender(EmailType.Welcome);

            if (!success)
                Console.WriteLine("Something went wrong at SendEmailToNewCustomers");

            success = await _customerRepository.CustomerMailSender(EmailType.WelcomeBack, "EOComebackToUs");


            if (!success)
                Console.WriteLine("Something went wrong at CustomerMailSender");


            success = await _customerRepository.CustomerMailSender(EmailType.GoodBye, "EOComebackToUs");


            if (!success)
                Console.WriteLine("Something went wrong at CustomerMailSender");
        }










    }
}