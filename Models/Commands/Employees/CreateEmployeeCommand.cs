using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Commands.Employees
{
    public class CreateEmployeeCommand
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
        [MaxLength(60)]
        public string Title { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [Required]
        public string Role { get; set; }
        
        [MaxLength(100)]
        public string? GitHubURL { get; set; }
        
        [MaxLength(100)]
        public string? LinkedInURL { get; set; }
        
        [Url]
        [MaxLength(100)]
        public string? PersonalWebsite { get; set; }
    }
}
