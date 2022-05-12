using Business.Email.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Email
{
    public interface IEmailSender
    {
        Task SendEmailToNewUser(NewUserEmailModel model);
        Task SendPasswordResetEmail(PasswordResetEmailModel model);
    }
}
