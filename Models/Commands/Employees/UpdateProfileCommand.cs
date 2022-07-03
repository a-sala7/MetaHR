using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Commands.Employees
{
    public class UpdateProfileCommand
    {
        [Url]
        [MaxLength(200)]
        public string? GitHubURL { get; set; }
        [Url]
        [MaxLength(200)]
        public string? LinkedInURL { get; set; }
        [Url]
        [MaxLength(200)]
        public string? PersonalWebsite { get; set; }
    }
}
