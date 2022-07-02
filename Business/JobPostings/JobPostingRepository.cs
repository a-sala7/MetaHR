using AutoMapper;
using DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Models.Commands.JobPostings;
using Models.DTOs;
using Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.JobPostings
{
    public class JobPostingRepository : IJobPostingRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public JobPostingRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<IEnumerable<JobPostingDTO>> GetAll()
        {
            var jobPostings = await _db.JobPostings.ToListAsync();
            return 
                _mapper.Map
                <IEnumerable<JobPosting>, IEnumerable<JobPostingDTO>>
                (jobPostings);
        }

        public async Task<JobPostingDTO> GetById(int id)
        {
            JobPosting jp = await _db.JobPostings.FirstOrDefaultAsync(x => x.Id == id);
            return _mapper.Map<JobPosting, JobPostingDTO>(jp);
        }

        public async Task<CommandResult> Add(CreateJobPostingCommand cmd)
        {
            var newJp = new JobPosting()
            {
                Title = cmd.Title,
                DescriptionHtml = cmd.DescriptionHtml,
                Category = cmd.Category
            };
            _db.JobPostings.Add(newJp);
            if (await _db.SaveChangesAsync() > 0)
            {
                return CommandResult.SuccessResult;
            }
            return CommandResult.UnknownInternalErrorResult;
        }

        public async Task<CommandResult> Update(int id, UpdateJobPostingCommand cmd)
        {
            JobPosting jpInDb = await _db.JobPostings.FirstOrDefaultAsync(x => x.Id == id);
            if(jpInDb == null)
            {
                return CommandResult.GetNotFoundResult
                    ("JobPosting", id);
            }

            _mapper.Map(cmd, jpInDb);
            _db.JobPostings.Update(jpInDb);

            if (await _db.SaveChangesAsync() > 0)
            {
                return CommandResult.SuccessResult;
            }
            return CommandResult.UnknownInternalErrorResult;
        }

        public async Task<CommandResult> Delete(int id)
        {
            JobPosting jpInDb = await _db.JobPostings.FirstOrDefaultAsync(x => x.Id == id);
            if (jpInDb == null)
            {
                return CommandResult.GetNotFoundResult
                    ("JobPosting", id);
            }

            if(await _db.JobApplications.AnyAsync(ja => ja.JobPostingId == id))
            {
                return CommandResult.GetErrorResult
                    ("Can't delete a Job Posting which has applications.");
            }

            _db.JobPostings.Remove(jpInDb);

            if (await _db.SaveChangesAsync() > 0)
            {
                return CommandResult.SuccessResult;
            }
            return CommandResult.UnknownInternalErrorResult;
        }
    }
}
