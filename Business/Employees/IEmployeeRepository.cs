using Models.Commands.Employees;
using Models.DTOs;
using Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Employees
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<EmployeeDTO>> GetAll();
        Task<EmployeeDTO> GetById(string employeeId);
        Task<CommandResult> Create(CreateEmployeeCommand cmd);
        Task<CommandResult> Update(string employeeId, UpdateEmployeeCommand cmd);
        Task<CommandResult> ChangeRole(ChangeRoleCommand cmd);
    }
}
