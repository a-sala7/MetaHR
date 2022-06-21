using Common;
using Models.Commands.VacationRequests;
using Models.DTOs;
using Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.VacationRequests
{
    public interface IVacationRequestRepository
    {
        Task<PagedResult<VacationRequestDTO>> GetAll(int pageNumber, int pageSize);
        Task<PagedResult<VacationRequestDTO>> GetByEmployee(string employeeId, int pageNumber, int pageSize);
        Task<CommandResult> Create(string employeeId, CreateVacationRequestCommand cmd);
        Task<CommandResult> Update(int id, string reviewerId, UpdateVacationRequestCommand cmd);
        Task<CommandResult> Delete(int id);
    }
}
