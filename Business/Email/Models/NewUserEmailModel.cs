using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Email.Models
{
    public class NewUserEmailModel
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string UserId { get; set; }
        public string PasswordResetToken { get; set; }
    }
}
