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
        [MaxLength(50)]
        public string? GithubUsername { get; set; }
        [MaxLength(50)]
        public string? LinkedInUsername { get; set; }
        [Url]
        [MaxLength(100)]
        public string? PersonalWebsite { get; set; }
    }
}
