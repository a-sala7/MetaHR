using AutoMapper;
using Business.FileManager;
using Common;
using Common.Constants;
using DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Models.Commands.JobApplications;
using Models.DTOs;
using Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Business.JobApplications
{
    public class JobApplicationRepository : IJobApplicationRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileManager _fileManager;
        private readonly IMapper _mapper;
        public JobApplicationRepository(ApplicationDbContext db,
            IFileManager fileManager, IMapper mapper)
        {
            _db = db;
            _fileManager = fileManager;
            _mapper = mapper;
        }

        public async Task<PagedResult<JobApplicationDTO>> GetAll(int pageNumber, int pageSize = 10)
        {
            var apps = await _db.JobApplications
                .OrderByDescending(ja => ja.ReceivedOnUtc)
                .Paginate(pageNumber: pageNumber, pageSize: pageSize)
                .Include(ja => ja.JobPosting)
                .Select(JAToJADTOExpression)
                .ToListAsync();

            var totalCount = await _db
                .JobApplications
                .CountAsync();

            return apps.GetPagedResult(totalCount);
        }

        public async Task<JobApplicationDTO?> GetById(int id)
        {
            var app = await _db.JobApplications
                .Select(JAToJADTOExpression)
                .FirstOrDefaultAsync(ja => ja.Id == id);

            return app;
        }

        public async Task<PagedResult<JobApplicationDTO>> GetUnread(int pageNumber, int pageSize = 10)
        {
            var apps = await _db.JobApplications
                .OrderByDescending(ja => ja.ReceivedOnUtc)
                .Where(ja => ja.Stage == JobApplicationStage.Unread)
                .Paginate(pageNumber: pageNumber, pageSize: pageSize)
                .Include(ja => ja.JobPosting)
                .Select(JAToJADTOExpression)
                .ToListAsync();

            int totalCount = await _db
                .JobApplications
                .Where(ja => ja.Stage == JobApplicationStage.Unread)
                .CountAsync();

            return apps.GetPagedResult(totalCount);
        }

        public async Task<PagedResult<JobApplicationDTO>> GetCompleted(int pageNumber, int pageSize = 10)
        {
            var apps = await _db.JobApplications
                .OrderByDescending(ja => ja.ReceivedOnUtc)
                .Where(ja => ja.Stage >= JobApplicationStage.Accepted)
                .Paginate(pageNumber: pageNumber, pageSize: pageSize)
                .Include(ja => ja.JobPosting)
                .Select(JAToJADTOExpression)
                .ToListAsync();

            var totalCount = await _db
                .JobApplications
                .Where(ja => ja.Stage >= JobApplicationStage.Accepted)
                .CountAsync();

            return apps.GetPagedResult(totalCount);
        }

        public async Task<PagedResult<JobApplicationDTO>> GetInProgress(int pageNumber, int pageSize = 10)
        {
            var apps = await _db.JobApplications
                .OrderByDescending(ja => ja.ReceivedOnUtc)
                .Where(ja => ja.Stage >= JobApplicationStage.PendingInterview
                && ja.Stage <= JobApplicationStage.PendingDecision)
                .Paginate(pageNumber: pageNumber, pageSize: pageSize)
                .Include(ja => ja.JobPosting)
                .Select(JAToJADTOExpression)
                .ToListAsync();

            int totalCount = await _db.
                JobApplications
                .Where(ja => ja.Stage >= JobApplicationStage.PendingInterview
                && ja.Stage <= JobApplicationStage.PendingDecision)
                .CountAsync();

            return apps.GetPagedResult(totalCount);
        }
        
        public async Task<CommandResult> Create(CreateJobApplicationCommand cmd)
        {
            JobPosting? jp = await _db.JobPostings
                .FirstOrDefaultAsync(jp => jp.Id == cmd.JobPostingId);
            if(jp == null)
            {
                return CommandResult.GetNotFoundResult("Job Posting", cmd.JobPostingId);
            }
            var ext = Path.GetExtension(cmd.CvFile.FileName);
            string newFileName = Guid.NewGuid().ToString() + ext;
            string cvUrl = await _fileManager.UploadFile(
                fileName: newFileName,
                stream: cmd.CvFile.OpenReadStream(),
                contentType: cmd.CvFile.ContentType,
                folder: Folders.Cvs
                );
            
            JobApplication ja = _mapper.Map<CreateJobApplicationCommand, JobApplication>(cmd);
            ja.ReceivedOnUtc = DateTime.UtcNow;
            ja.CvURL = cvUrl;
            _db.JobApplications.Add(ja);
            
            await _db.SaveChangesAsync();
            return CommandResult.SuccessResult;
        }

        public async Task<CommandResult> ChangeStage(int id, JobApplicationStage newStage)
        {
            var app = await _db.JobApplications
                .FirstOrDefaultAsync(ja => ja.Id == id);
            
            if (app == null)
                return CommandResult.GetNotFoundResult("Job Application", id);


            app.Stage = newStage;
            _db.JobApplications.Update(app);

            await _db.SaveChangesAsync();
            return CommandResult.SuccessResult;
        }

        public async Task<CommandResult> Delete(int id)
        {
            var app = await _db.JobApplications
                .FirstOrDefaultAsync(ja => ja.Id == id);

            if (app == null)
                return CommandResult.GetNotFoundResult("Job Application", id);

            if(string.IsNullOrWhiteSpace(app.CvURL) == false)
            {
                var cvFileName = Path.GetFileName(app.CvURL);
                await _fileManager.DeleteFile(cvFileName, Folders.Cvs);
            }

            _db.JobApplications.Remove(app);
            
            await _db.SaveChangesAsync();
            return CommandResult.SuccessResult;
        }

        public async Task<string> GetCvURL(int jobApplicationId)
        {
            JobApplication? ja = await _db
                .JobApplications
                .FirstOrDefaultAsync(ja => ja.Id == jobApplicationId);

            if (ja == null)
                throw new Exception($"Job application with id {jobApplicationId} not found");


            return await _fileManager.GetPreSignedURL(fileName: Path.GetFileName(ja.CvURL), folder: Folders.Cvs);
        }

        //NOTES

        public async Task<JobApplicationNoteDTO> GetNoteById(int noteId)
        {
            JobApplicationNote note = await _db.JobApplicationNotes
                .FirstOrDefaultAsync(n => n.Id == noteId);

            return _mapper.Map<JobApplicationNote, JobApplicationNoteDTO>(note);
        }

        public async Task<IEnumerable<JobApplicationNoteDTO>> GetNotes(int jobApplicationId, string authorId)
        {
            IEnumerable<JobApplicationNote> notes = await _db.JobApplicationNotes
                .OrderByDescending(n => n.CreatedAtUtc)
                .Where(n => n.JobApplicationId == jobApplicationId
                && n.AuthorId == authorId)
                .ToListAsync();
            
            var noteDtos = _mapper.Map<IEnumerable<JobApplicationNote>,
                IEnumerable<JobApplicationNoteDTO>>(notes);

            return noteDtos;
        }

        public async Task<CommandResult> CreateNote(string authorId, CreateJobApplicationNoteCommand cmd)
        {
            JobApplication? ja = await _db.JobApplications
                .FirstOrDefaultAsync(ja => ja.Id == cmd.JobApplicationId);

            if(ja == null)
            {
                return CommandResult.GetNotFoundResult("Job Application", cmd.JobApplicationId);
            }

            var emp = await _db.Employees.FirstOrDefaultAsync(emp => emp.Id == authorId);

            if(emp == null)
            {
                return CommandResult.GetNotFoundResult("Employee", authorId);
            }

            JobApplicationNote note = _mapper.Map<CreateJobApplicationNoteCommand, 
                JobApplicationNote>(cmd);
            note.CreatedAtUtc = DateTime.UtcNow;
            note.AuthorId = authorId;

            _db.JobApplicationNotes.Add(note);
            await _db.SaveChangesAsync();
            
            return CommandResult.SuccessResult;
        }

        public async Task<CommandResult> UpdateNote(UpdateJobApplicationNoteCommand cmd)
        {
            JobApplicationNote note = await _db.JobApplicationNotes
                .FirstOrDefaultAsync(n => n.Id == cmd.NoteId);

            if(note == null)
            {
                return CommandResult.GetNotFoundResult("Job Application Note", cmd.NoteId);
            }

            note.Content = cmd.Content;
            _db.JobApplicationNotes.Update(note);
            await _db.SaveChangesAsync();

            return CommandResult.SuccessResult;
        }

        public async Task<CommandResult> DeleteNote(int noteId)
        {
            JobApplicationNote note = await _db.JobApplicationNotes
                .FirstOrDefaultAsync(n => n.Id == noteId);

            if (note == null)
            {
                return CommandResult.GetNotFoundResult("Job Application Note", noteId);
            }

            _db.JobApplicationNotes.Remove(note);
            await _db.SaveChangesAsync();
            return CommandResult.SuccessResult;
        }

        private readonly Expression<Func<JobApplication, JobApplicationDTO>> JAToJADTOExpression
           = ja => new JobApplicationDTO
           {
               Id = ja.Id,
               JobTitle = ja.JobPosting.Title,
               JobPostingId = ja.JobPosting.Id,
               ReceivedOnUtc = ja.ReceivedOnUtc,
               Email = ja.Email,
               FirstName = ja.FirstName,
               LastName = ja.LastName,
               Phone = ja.Phone,
               Stage = ja.Stage.ToString(),
               GitHubURL = ja.GitHubURL,
               LinkedInURL = ja.LinkedInURL,
               PersonalWebsite = ja.PersonalWebsite
           };
    }
}
