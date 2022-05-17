using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Commands.JobApplications
{
    public class CreateJobApplicationCommand
    {
        public int? JobPostingId { get; set; }

        //applicant info
        [Required]
        [MinLength(2)]
        [MaxLength(60)]
        public string FirstName { get; set; }

        [Required]
        [MinLength(2)]
        [MaxLength(60)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Phone { get; set; }

        //optional stuff
        public string? LinkedInURL { get; set; }
        public string? GitHubURL { get; set; }
        public string? PersonalWebsite { get; set; }

        //cv
        [Required]
        IFormFile CvFile { get; set; }
    }
}
