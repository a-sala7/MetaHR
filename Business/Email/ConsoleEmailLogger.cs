using Business.Email.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Email
{
    public class ConsoleEmailLogger : IEmailSender
    {
        public async Task SendEmailToNewUser(NewUserEmailModel model)
        {
            Console.WriteLine();
            Console.WriteLine("New user registered!");
            Console.WriteLine("First name: {0}", model.FirstName);
            Console.WriteLine("Email: {0}", model.Email);
            Console.WriteLine("User Id: {0}", model.UserId);
            Console.WriteLine("Password Reset Token: {0}", model.PasswordResetToken);
            Console.WriteLine();
        }

        public async Task SendPasswordResetEmail(PasswordResetEmailModel model)
        {
            Console.WriteLine();
            Console.WriteLine("User resetting their password!");
            Console.WriteLine("User Id: {0}", model.UserId);
            Console.WriteLine("Password Reset Token: {0}", model.PasswordResetToken);
            Console.WriteLine();
        }
    }
}
