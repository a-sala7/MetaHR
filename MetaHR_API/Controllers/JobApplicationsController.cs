using Business.JobApplications;
using Common;
using Common.Constants;
using MetaHR_API.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Commands.JobApplications;
using Models.DTOs;
using Models.Responses;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MetaHR_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobApplicationsController : ControllerBase
    {
        private readonly IJobApplicationRepository _repo;

        public JobApplicationsController(IJobApplicationRepository repo)
        {
            _repo = repo;
        }

        // GET: api/<JobApplicationsController>
        [HttpGet]
        [Authorize(Roles = Roles.AdminsAndHR)]
        public async Task<IActionResult> GetAll(int pageNumber, int pageSize = 10)
        {
            PagedResult<JobApplicationDTO>? apps = await _repo
                .GetAll(pageNumber: pageNumber, pageSize: pageSize);

            return Ok(apps);
        }

        [HttpGet("byType/{type}")]
        [Authorize(Roles = Roles.AdminsAndHR)]
        public async Task<IActionResult> GetAll(string type, int pageNumber, int pageSize = 10)
        {
            PagedResult<JobApplicationDTO>? apps;
            if(type.ToLower() == "completed")
            {
                apps = await _repo
                    .GetCompleted(pageNumber: pageNumber, pageSize: pageSize);
            }
            else if(type.ToLower() == "unread")
            {
                apps = await _repo
                    .GetUnread(pageNumber: pageNumber, pageSize: pageSize);
            }
            else if(type.ToLower() == "inprogress")
            {
                apps = await _repo
                    .GetInProgress(pageNumber: pageNumber, pageSize: pageSize);
            }
            else
            {
                return BadRequest("Invalid type!");
            }

            return Ok(apps);
        }

        // GET api/<JobApplicationsController>/5
        [HttpGet("{id}")]
        [Authorize(Roles = Roles.AdminsAndHR)]
        public async Task<IActionResult> Get(int id)
        {
            JobApplicationDTO? app = await _repo
                .GetById(id);

            if(app == null)
            {
                return NotFound();
            }

            return Ok(app);
        }

        // TODO
        // POST api/<JobApplicationsController>
        [HttpPost]
        [Authorize(Roles = Roles.AdminsAndHR)]
        public async Task<IActionResult> Create(CreateJobApplicationCommand cmd)
        {
            throw new NotImplementedException();
        }

        // POST api/<JobApplicationsController>/changeStage/5
        [HttpPost("changeStage/{id}")]
        [Authorize(Roles = Roles.AdminsAndHR)]
        public async Task<IActionResult> ChangeStage(int id, JobApplicationStage stage)
        {
            CommandResult cmdResult = await _repo.ChangeStage(id, stage);

            return CommandResultResolver.Resolve(cmdResult);
        }

        // DELETE api/<JobApplicationsController>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.AdminsAndHR)]
        public async Task<IActionResult> Delete(int id)
        {
            CommandResult cmdResult = await _repo.Delete(id);

            return CommandResultResolver.Resolve(cmdResult);
        }
    }
}
