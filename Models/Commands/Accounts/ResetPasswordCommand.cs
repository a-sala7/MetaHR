using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Commands.Accounts
{
    public class ResetPasswordCommand
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        [MinLength(6)]
        public string Password { get; set; }
        [Required]
        public string ResetPasswordToken { get; set; }
    }
}
