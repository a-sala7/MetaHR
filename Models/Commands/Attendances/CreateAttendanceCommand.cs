using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Commands.Attendances
{
    public class CreateAttendanceCommand
    {
        [Required]
        public string EmployeeId { get; set; }
        [Required]
        public DateTime Date { get; set; }
    }
}
