using Common;
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

        public JobApplicationRepository(ApplicationDbContext db)
        {
            _db = db;
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
                .FirstOrDefaultAsync(ja => ja.Id == id); ;

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
        //TODO
        public Task<CommandResult> Create(CreateJobApplicationCommand cmd)
        {
            throw new NotImplementedException();
        }
        //TODO
        public Task<CommandResult> ChangeStage(int id, JobApplicationStage newStage)
        {
            throw new NotImplementedException();
        }
        //TODO
        public Task<CommandResult> Delete(int id)
        {
            throw new NotImplementedException();
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
               CvURL = ja.CvURL,
               Stage = ja.Stage.ToString(),
               GitHubURL = ja.GitHubURL,
               LinkedInURL = ja.LinkedInURL,
               PersonalWebsite = ja.PersonalWebsite
           };
    }
}
