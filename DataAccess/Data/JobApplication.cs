using Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Data
{
    [Index(nameof(ReceivedOnUtc))]
    public class JobApplication
    {
        public int Id { get; set; }
        public int? JobPostingId { get; set; }
        public virtual JobPosting? JobPosting { get; set; }
        public DateTime ReceivedOnUtc { get; set; }
        public JobApplicationStage Stage { get; set; }

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
        public string CvURL { get; set; }
    }
}
