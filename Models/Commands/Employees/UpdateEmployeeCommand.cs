using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Commands.Employees
{
    public class UpdateEmployeeCommand
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public DateTime DateHired { get; set; }
        
        [Required]
        public string Title { get; set; }

        [Required]
        public int DepartmentId { get; set; }

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
