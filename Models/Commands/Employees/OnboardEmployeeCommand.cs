using Microsoft.AspNetCore.Http;
using Models.Commands.Accounts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Commands.Employees
{
    public class OnboardEmployeeCommand : ResetPasswordCommand
    {
        public IFormFile? ProfilePicture { get; set; }

        [Url]
        [MaxLength(100)]
        public string? GitHubURL { get; set; }
        
        [Url]
        [MaxLength(100)]
        public string? LinkedInURL { get; set; }
        
        [Url]
        [MaxLength(100)]
        public string? PersonalWebsite { get; set; }
    }
}
