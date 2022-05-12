using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Commands.Employees
{
    public class ChangePfpCommand
    {
        public string EmployeeId { get; set; }
        public IFormFile Picture { get; set; }
    }
}
