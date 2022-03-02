using Business.JobPostings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Commands.JobPostings;
using Models.DTOs;
using Models.Responses;
using MetaHR_API.Utility;

namespace MetaHR_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobPostingsController : ControllerBase
    {
        IJobPostingRepository _jobPostingRepo;

        public JobPostingsController(IJobPostingRepository jobPostingRepo)
        {
            _jobPostingRepo = jobPostingRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            IEnumerable<JobPostingDTO> jobPostings = await _jobPostingRepo.GetAll();
            return Ok(jobPostings);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            JobPostingDTO jobPosting = await _jobPostingRepo.GetById(id);
            if(jobPosting == null)
            {
                return NotFound();
            }
            return Ok(jobPosting);
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddJobPostingCommand cmd)
        {
            CommandResult cmdResult = await _jobPostingRepo.Add(cmd);
            var actionResult = CommandResultResolver.Resolve(cmdResult);
            return actionResult;
        }

        [HttpPost("update/{id}")]
        public async Task<IActionResult> Update(int id, UpdateJobPostingCommand cmd)
        {
            CommandResult cmdResult = await _jobPostingRepo.Update(id, cmd);
            var actionResult = CommandResultResolver.Resolve(cmdResult);
            return actionResult;
        }
    }
}
