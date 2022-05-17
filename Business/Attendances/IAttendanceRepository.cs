using Common;
using Models.Commands.Attendances;
using Models.DTOs;
using Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Attendances
{
    public interface IAttendanceRepository
    {
        Task<PagedResult<AttendanceDTO>> GetByEmployeeId(string employeeId, int pageNumber, int pageSize = 10);
        Task<CommandResult> Create(CreateAttendanceCommand cmd);
        Task<CommandResult> Delete(int id);
    }
}
