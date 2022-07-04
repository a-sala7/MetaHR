using Business.Email.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Model;
using Task = System.Threading.Tasks.Task;

namespace Business.Email
{
    public class SendinblueEmailSender : IEmailSender
    {
        private readonly TransactionalEmailsApi _api = new();
        private readonly SendSmtpEmailSender _sender = new()
        {
            Email = "no-reply@metahr.live",
            Name = "MetaHR"
        };

        public async Task SendOnboardEmail(NewUserEmailModel model)
        {
            var link = $"https://internal.metahr.live/onboard?userId={model.UserId}&token={model.PasswordResetToken}";
            //+ is special character in query strings, representing a space
            //use proper encoding of + character
            //otherwise it will be converted to a space
            link = link.Replace("+", "%2B");
            var mail = new SendSmtpEmail()
            {
                Sender = _sender,
                To = GetListWithSingleRecipient(model.Email, model.FirstName),
                Subject = "Welcome to MetaHR",
                HtmlContent = $"Hello, {model.FirstName}.<br>" +
                "Welcome to MetaHR!<br>" +
                "Please follow the link below to create a password for your new account and complete the onboarding process<br>" +
                $"<a href=\"{link}\">{link}</a>"
            };

            await _api.SendTransacEmailAsync(mail);
        }

        public async Task SendPasswordResetEmail(PasswordResetEmailModel model)
        {
            var link = $"https://internal.metahr.live/account/resetPassword?userId={model.UserId}&token={model.PasswordResetToken}";
            link = link.Replace("+", "%2B");
            var mail = new SendSmtpEmail()
            {
                Sender = _sender,
                To = GetListWithSingleRecipient(model.Email, model.FirstName),
                Subject = "Reset your MetaHR password",
                HtmlContent = $"Hello, {model.FirstName}." +
                "Forgot your password? No problem!<br>" +
                "Follow this link to reset your password:</br>" +
                $"<a href=\"{link}\">{link}</a> <br><br>" +
                "If you didn't request to reset your password, you can safely ignore this email."
            };

            await _api.SendTransacEmailAsync(mail);
        }

        public async Task SendTestMail(string to, string subject, string message)
        {
            var mail = new SendSmtpEmail()
            {
                Sender = _sender,
                To = GetListWithSingleRecipient(to),
                Subject = subject,
                HtmlContent = message
            };

            await _api.SendTransacEmailAsync(mail);
        }

        private List<SendSmtpEmailTo> GetListWithSingleRecipient(string email, string name = null)
        {
            return new List<SendSmtpEmailTo>() { new SendSmtpEmailTo(email, name) };
        }
    }
}
