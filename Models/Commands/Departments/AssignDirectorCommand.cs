using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Commands.Departments
{
    public class AssignDirectorCommand
    {
        public int DepartmentId { get; set; }
        public string DirectorId { get; set; }
    }
}
