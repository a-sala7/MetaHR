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
        Task<IEnumerable<EmployeeNoteDTO>> GetAllByAuthor(string authorId);
        Task<IEnumerable<EmployeeNoteDTO>> GetAllAboutEmployee(string authorId, string employeeId);
        Task<EmployeeNoteDTO> GetById(int id);
        Task<CommandResult> Create(string writtenById, CreateEmployeeNoteCommand cmd);
        Task<CommandResult> Update(int id, string content);
        Task<CommandResult> Delete(int id);
    }
}
