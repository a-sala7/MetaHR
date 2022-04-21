using Models.Commands.Departments;
using Models.DTOs;
using Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Departments
{
    public interface IDepartmentRepository
    {
        Task<DepartmentDTO> GetById(int id);
        Task<IEnumerable<DepartmentDTO>> GetAll();
        Task<CommandResult> Create(CreateDepartmentCommand cmd);
        Task<CommandResult> Update(int id, UpdateDepartmentCommand cmd);
        Task<CommandResult> Delete(int id);
        Task<CommandResult> AssignDirector(AssignDirectorCommand cmd);
    }
}
