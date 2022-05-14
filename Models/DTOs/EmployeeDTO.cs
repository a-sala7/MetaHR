using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs
{
    public class EmployeeDTO
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Title { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public bool IsDirector { get; set; }
        //hidden from other employees
        public DateTime? DateHired { get; set; }
        //hidden from other employees
        public DateTime? DateOfBirth { get; set; }
        public string? ProfilePictureURL { get; set; }
        public string? LinkedInURL { get; set; }
        public string? GitHubURL { get; set; }
        public string? PersonalWebsite { get; set; }
    }
}
