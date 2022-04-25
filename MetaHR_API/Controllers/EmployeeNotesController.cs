using Business.EmployeeNotes;
using Common.Constants;
using MetaHR_API.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Commands.Notes;
using Models.DTOs;
using Models.Responses;
using System.Security.Claims;

namespace MetaHR_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeNotesController : ControllerBase
    {
        private readonly IEmployeeNoteRepository _repo;

        public EmployeeNotesController(IEmployeeNoteRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("{id}")]
        [Authorize(Roles = Roles.HRJunior + "," + Roles.HRSenior)]
        public async Task<IActionResult> GetById(int id)
        {
            EmployeeNoteDTO note = await _repo.GetById(id);
            if (note == null)
            {
                return NotFound($"Note with ID {id} not found.");
            }
            if (note.AuthorId != User.GetId())
            {
                return Unauthorized();
            }
            return Ok(note);
        }

        [HttpGet("mynotes")]
        [Authorize(Roles = Roles.HRJunior + "," + Roles.HRSenior)]
        public async Task<IActionResult> GetMyNotes()
        {
            var notes = await _repo.GetAllByAuthor(User.GetId());
            return Ok(notes);
        }

        [HttpGet("allAboutEmployee/{employeeId}")]
        [Authorize(Roles = Roles.HRJunior + "," + Roles.HRSenior)]
        public async Task<IActionResult> GetAllAboutEmployee(string employeeId)
        {
            var notes = await _repo
                .GetAllAboutEmployee(authorId: User.GetId(), employeeId);
            return Ok(notes);
        }

        [HttpPost]
        [Authorize(Roles = Roles.HRJunior + "," + Roles.HRSenior)]
        public async Task<IActionResult> Create(CreateEmployeeNoteCommand cmd)
        {
            if(cmd.EmployeeId == User.GetId())
            {
                return BadRequest(CommandResult.GetErrorResult("Can't make a note about yourself."));
            }
            var result = await _repo.Create(User.GetId(), cmd);
            return CommandResultResolver.Resolve(result);
        }

        [HttpPost("{noteId}")]
        [Authorize(Roles = Roles.HRJunior + "," + Roles.HRSenior)]
        public async Task<IActionResult> Update(int noteId, string newContent)
        {
            var note = await _repo.GetById(noteId);
            if(note == null)
            {
                return NotFound(CommandResult.GetNotFoundResult("Note", noteId));
            }
            if(note.AuthorId != User.GetId())
            {
                return Unauthorized();
            }
            var result = await _repo.Update(noteId, newContent);
            return CommandResultResolver.Resolve(result);
        }

        [HttpDelete]
        [Authorize(Roles = Roles.HRJunior + "," + Roles.HRSenior)]
        public async Task<IActionResult> Delete(int noteId)
        {
            var note = await _repo.GetById(noteId);
            if (note == null)
            {
                return NotFound(CommandResult.GetNotFoundResult("Note", noteId));
            }
            if (note.AuthorId != User.GetId())
            {
                return Unauthorized();
            }
            var result = await _repo.Delete(noteId);
            return CommandResultResolver.Resolve(result);
        }
    }
}
