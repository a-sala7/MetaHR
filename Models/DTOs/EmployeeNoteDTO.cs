using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs
{
    public class EmployeeNoteDTO
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string EmployeeWrittenById { get; set; }
        public string EmployeeWrittenByName { get; set; }
        public string EmployeeWrittenAboutId { get; set; }
        public string EmployeeWrittenAboutName { get; set; }
    }
}
