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
        public async Task<IActionResult> GetByType(string type, int pageNumber, int pageSize = 10)
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

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateJobApplicationCommand cmd)
        {
            var verifyRes = FileVerifier.VerifyPdf(cmd.CvFile);
            if(verifyRes.IsSuccessful == false)
            {
                return CommandResultResolver.Resolve(verifyRes);
            }
            var cmdResult = await _repo.Create(cmd);
            return CommandResultResolver.Resolve(cmdResult);
        }

        [HttpPost("changeStage/{id}")]
        [Authorize(Roles = Roles.AdminsAndHR)]
        public async Task<IActionResult> ChangeStage(int id, JobApplicationStage stage)
        {
            CommandResult cmdResult = await _repo.ChangeStage(id, stage);

            return CommandResultResolver.Resolve(cmdResult);
        }

        [HttpGet("{id}/cvURL")]
        [Authorize(Roles = Roles.AdminsAndHR)]
        public async Task<IActionResult> GetJobApplicationCvURL(int id)
        {
                return Ok(await _repo.GetCvURL(id));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.HRJunior + "," + Roles.HRSenior)]
        public async Task<IActionResult> Delete(int id)
        {
            CommandResult cmdResult = await _repo.Delete(id);

            return CommandResultResolver.Resolve(cmdResult);
        }

        // NOTES
        /// <summary>
        /// Gets notes written by current user for the given JobApplicationId
        /// </summary>
        /// <remarks>
        /// Roles: HR_Junior, HR_Senior
        /// 
        /// Example request:
        /// 
        ///     GET /api/jobApplications/5/notes
        ///     
        /// Example response:
        /// 
        ///     [
        ///         {
        ///            "id":141,
        ///            "content":"test note 1"
        ///            "createdAtUtc":"2022-05-22T00:00:00.000Z",
        ///            "authorId":"4a9a8d3e-69f7-4a7e-9528-12eb3b297dec",
        ///            "jobApplicationId":5
        ///         },
        ///         {
        ///            "id":142,
        ///            "content":"test note 2"
        ///            "createdAtUtc":"2022-05-22T00:00:00.000Z",
        ///            "authorId":"4a9a8d3e-69f7-4a7e-9528-12eb3b297dec",
        ///            "jobApplicationId":5
        ///         }
        ///     ]
        /// 
        /// </remarks>
        [HttpGet("{jobApplicationId}/notes/")]
        [Authorize(Roles = Roles.HRJunior + "," + Roles.HRSenior)]
        public async Task<IActionResult> GetNotes(int jobApplicationId)
        {
            var notes = await _repo.GetNotes(jobApplicationId, User.GetId());
            return Ok(notes);
        }
        /// <summary>
        /// Create a new note for a job application
        /// </summary>
        /// <remarks>
        /// Roles: HR_Junior, HR_Senior
        /// 
        /// Example request:
        /// 
        ///     POST /api/jobApplications/notes
        ///     body: {
        ///         "jobApplicationId": 123,
        ///         "content": "test note 12345"
        ///     }
        ///     
        /// Example response 1:
        /// 
        ///     {
        ///         "isSuccessful":true,
        ///         "notFound":false,
        ///         "internalError":false,
        ///         "errors":null
        ///     }
        ///     
        /// Example response 2:
        /// 
        ///     {
        ///         "isSuccessful":false,
        ///         "notFound":true,
        ///         "internalError":false,
        ///         "errors":["Job Application with ID: 123 not found."]
        ///     }
        /// 
        /// </remarks>
        /// <response code="200">OK</response>
        /// <response code="404">Not Found (Job Application with given ID not found)</response>
        [HttpPost("notes")]
        [Authorize(Roles = Roles.HRJunior + "," + Roles.HRSenior)]
        public async Task<IActionResult> CreateNote(CreateJobApplicationNoteCommand cmd)
        {
            var result = await _repo.CreateNote(User.GetId(), cmd);
            return CommandResultResolver.Resolve(result);
        }

        /// <summary>
        /// Deletes a job application note
        /// </summary>
        /// <remarks>
        /// Roles: HR_Junior, HR_Senior
        /// 
        /// Users can't delete a note if they did not write it themselves.
        /// 
        /// Example request:
        /// 
        ///     DELETE /api/jobApplications/notes/150
        ///     
        /// </remarks>
        /// <response code="200">OK</response>
        /// <response code="404">Not Found (Job Application Note with given ID not found)</response>
        /// <response code="401">Unauthorized</response>
        [HttpDelete("notes/{noteId}")]
        [Authorize(Roles = Roles.HRJunior + "," + Roles.HRSenior)]
        public async Task<IActionResult> DeleteNote(int noteId)
        {
            var note = await _repo.GetNoteById(noteId);
            if(note == null)
            {
                return NotFound(CommandResult.GetNotFoundResult("Job Application Note", noteId));
            }

            if(note.AuthorId != User.GetId())
            {
                return Unauthorized(CommandResult.GetErrorResult("You are not authorized to delete that note."));
            }

            var result = await _repo.DeleteNote(noteId);
            return CommandResultResolver.Resolve(result);
        }

        /// <summary>
        /// Updates a job application note
        /// </summary>
        /// <remarks>
        /// Roles: HR_Junior, HR_Senior
        /// 
        /// Users can't update a note if they did not write it themselves.
        /// 
        /// Example request:
        /// 
        ///     POST /api/jobApplications/notes/update
        ///     body {
        ///         "noteId":123,
        ///         "content":"updated note text"
        ///     }
        ///     
        /// </remarks>
        /// <response code="200">OK</response>
        /// <response code="404">Not Found (Job Application Note with given ID not found)</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost("notes/update")]
        [Authorize(Roles = Roles.HRJunior + "," + Roles.HRSenior)]
        public async Task<IActionResult> UpdateNote(UpdateJobApplicationNoteCommand cmd)
        {
            var note = await _repo.GetNoteById(cmd.NoteId);
            if (note == null)
            {
                return NotFound(CommandResult.GetNotFoundResult("Job Application Note", cmd.NoteId));
            }

            if (note.AuthorId != User.GetId())
            {
                return Unauthorized(CommandResult.GetErrorResult("You are not authorized to update that note."));
            }

            var result = await _repo.UpdateNote(cmd);
            return CommandResultResolver.Resolve(result);
        }
    }
}
