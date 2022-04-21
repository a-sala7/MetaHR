using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Data
{
    public class EmployeeNote
    {
        public int Id { get; set; }
        
        [Required]
        public string? EmployeeWrittenById { get; set; }
        public virtual Employee EmployeeWrittenBy { get; set; }
        
        [Required]
        public string? EmployeeWrittenAboutId { get; set; }
        public virtual Employee EmployeeWrittenAbout { get; set; }
        
        [Required]
        [MaxLength(1000)]
        public string Content { get; set; }
    }
}
