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
            if(cmd.JobPostingId != null)
            {
                JobPosting? jp = await _db.JobPostings
                    .FirstOrDefaultAsync(jp => jp.Id == cmd.JobPostingId.Value);
                if(jp == null)
                {
                    return CommandResult.GetNotFoundResult("Job Posting", cmd.JobPostingId.Value);
                }
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
