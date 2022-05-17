using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs
{
    public class JobApplicationDTO
    {
        public int Id { get; set; }
        public int? JobPostingId { get; set; }
        public string? JobTitle { get; set; }
        public DateTime ReceivedOnUtc { get; set; }
        public string Stage { get; set; }

        //applicant info
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string? LinkedInURL { get; set; }
        public string? GitHubURL { get; set; }
        public string? PersonalWebsite { get; set; }
    }
}
