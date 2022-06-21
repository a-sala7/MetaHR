using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Commands.VacationRequests
{
    public class UpdateVacationRequestCommand
    {
        public VacationRequestState State { get; set; }
        public string? DenialReason { get; set; }
    }
}
