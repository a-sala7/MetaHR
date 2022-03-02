using DataAccess.Data;
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

        public JobPostingRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public Task<IEnumerable<JobPostingDTO>> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task<JobPostingDTO> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public Task<CommandResult> Add(AddJobPostingCommand cmd)
        {
            throw new NotImplementedException();
        }

        public Task<CommandResult> Update(UpdateJobPostingCommand cmd)
        {
            throw new NotImplementedException();
        }

        public Task<CommandResult> Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
