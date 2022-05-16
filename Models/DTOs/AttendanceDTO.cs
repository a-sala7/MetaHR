using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs
{
    public class AttendanceDTO
    {
        public int Id { get; set; }
        public string EmployeeId { get; set; }
        public DateTime Date { get; set; }
    }
}
