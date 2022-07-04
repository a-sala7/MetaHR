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
        public async Task SendOnboardEmail(NewUserEmailModel model)
        {
            var link = $"http://localhost:3000/onboard?userId={model.UserId}&token={model.PasswordResetToken}";
            link = link.Replace("+", "%2B");
            Console.WriteLine();
            Console.WriteLine($"New user ({model.Email}) registered!");
            Console.WriteLine($"Link: {link}");
            Console.WriteLine();
        }

        public async Task SendPasswordResetEmail(PasswordResetEmailModel model)
        {
            var link = $"http://localhost:3000/account/resetPassword?userId={model.UserId}&token={model.PasswordResetToken}";
            link = link.Replace("+", "%2B");
            Console.WriteLine();
            Console.WriteLine($"User {model.Email} resetting their password!");
            Console.WriteLine($"Link: {link}");
            Console.WriteLine();
        }

        public Task SendTestMail(string to, string subject, string message)
        {
            throw new NotImplementedException();
        }
    }
}
