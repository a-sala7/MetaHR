using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Commands.VacationRequests
{
    public class CreateVacationRequestCommand
    {
        public DateTime From { get; set; }
        [Range(1,14)]
        public int NumberOfDays { get; set; }
    }
}
