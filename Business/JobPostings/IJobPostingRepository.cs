﻿using Models.Commands.JobPostings;
using Models.DTOs;
using Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.JobPostings
{
    public interface IJobPostingRepository
    {
        Task<IEnumerable<JobPostingDTO>> GetAll();
        Task<JobPostingDTO> GetById(int id);
        Task<CommandResult> Add(AddJobPostingCommand cmd);
        Task<CommandResult> Update(UpdateJobPostingCommand cmd);
        Task<CommandResult> Delete(int id);
    }
}
