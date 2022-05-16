using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Data
{
    [Table("Employees")]
    [Index(nameof(IsDirector))]
    public class Employee : ApplicationUser
    {
        [Required]
        [MaxLength(60)]
        public string Title { get; set; }

        public int DepartmentId { get; set; }
        public virtual Department Department { get; set; }
        public bool IsDirector { get; set; }

        [Required]
        public DateTime DateHired { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        public string? ProfilePictureURL { get; set; }
        
        [MaxLength(100)]
        public string? LinkedInURL { get; set; }
        [MaxLength(100)]
        public string? GitHubURL { get; set; }
        [MaxLength(100)]
        public string? PersonalWebsite { get; set; }

        public virtual ICollection<EmployeeNote> NotesWritten { get; set; }
        public virtual ICollection<EmployeeNote> NotesAbout { get; set; }
        public virtual ICollection<TicketMessage> TicketMessages { get; set; }
        public virtual ICollection<Announcement> AnnouncementsWritten { get; set; }
        public virtual ICollection<Attendance> Attendances { get; set; }
    }
}
