using Models.Commands.Notes;
using Models.DTOs;
using Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.EmployeeNotes
{
    public interface IEmployeeNoteRepository
    {
        Task<IEnumerable<EmployeeNoteDTO>> GetAllWrittenBy(string id);
        Task<EmployeeNoteDTO> GetById(int id);
        Task<CommandResult> Create(string writtenById, CreateEmployeeNoteCommand cmd);
        Task<CommandResult> Update(int id, string content);
        Task<CommandResult> Delete(int id);
    }
}
