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
    public class Employee : ApplicationUser
    {
        [Required]
        public string Title { get; set; }

        public int DepartmentId { get; set; }
        public virtual Department Department { get; set; }

        [Required]
        public DateTime? DateHired { get; set; }
    }
}
