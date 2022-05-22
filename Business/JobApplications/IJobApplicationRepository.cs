using Common;
using Models.Commands.JobApplications;
using Models.DTOs;
using Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.JobApplications
{
    public interface IJobApplicationRepository
    {
        Task<PagedResult<JobApplicationDTO>> GetAll(int pageNumber, int pageSize = 10);
        Task<PagedResult<JobApplicationDTO>> GetUnread(int pageNumber, int pageSize = 10);
        Task<PagedResult<JobApplicationDTO>> GetInProgress(int pageNumber, int pageSize = 10);
        Task<PagedResult<JobApplicationDTO>> GetCompleted(int pageNumber, int pageSize = 10);
        Task<JobApplicationDTO?> GetById(int id);
        Task<CommandResult> Create(CreateJobApplicationCommand cmd);
        Task<CommandResult> ChangeStage(int id, JobApplicationStage newStage);
        Task<CommandResult> Delete(int id);
        Task<string> GetCvURL(int jobApplicationId);
        // NOTES
        Task<JobApplicationNoteDTO> GetNoteById(int noteId);
        Task<IEnumerable<JobApplicationNoteDTO>> GetNotes(int jobApplicationId, string authorId);
        Task<CommandResult> CreateNote(string authorId, CreateJobApplicationNoteCommand cmd);
        Task<CommandResult> UpdateNote(UpdateJobApplicationNoteCommand cmd);
        Task<CommandResult> DeleteNote(int noteId);
    }
}
