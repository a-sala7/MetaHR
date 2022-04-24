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
            if (note.EmployeeWrittenById != User.GetId())
            {
                return Unauthorized();
            }
            return Ok(note);
        }

        [HttpGet("allWrittenBy/{id}")]
        [Authorize(Roles = Roles.HRJunior + "," + Roles.HRSenior)]
        public async Task<IActionResult> GetAllWrittenBy(string id)
        {
            if (id != User.GetId())
            {
                return Unauthorized();
            }
            var notes = await _repo.GetAllWrittenBy(id);
            return Ok(notes);
        }

        [HttpPost]
        [Authorize(Roles = Roles.HRJunior + "," + Roles.HRSenior)]
        public async Task<IActionResult> Create(CreateEmployeeNoteCommand cmd)
        {
            var result = await _repo.Create(User.GetId(), cmd);
            return CommandResultResolver.Resolve(result);
        }

        [HttpPost("{id}")]
        [Authorize(Roles = Roles.HRJunior + "," + Roles.HRSenior)]
        public async Task<IActionResult> Update(int noteId, string newContent)
        {
            var note = await _repo.GetById(noteId);
            if(note == null)
            {
                return NotFound(CommandResult.GetNotFoundResult("Note", noteId));
            }
            if(note.EmployeeWrittenById != User.GetId())
            {
                return Unauthorized();
            }
            var result = await _repo.Update(noteId, newContent);
            return CommandResultResolver.Resolve(result);
        }

        [HttpDelete]
        [Authorize(Roles = Roles.HRJunior + "," + Roles.HRSenior)]
        public async Task<IActionResult> Delete(int noteId, string newContent)
        {
            var note = await _repo.GetById(noteId);
            if (note == null)
            {
                return NotFound(CommandResult.GetNotFoundResult("Note", noteId));
            }
            if (note.EmployeeWrittenById != User.GetId())
            {
                return Unauthorized();
            }
            var result = await _repo.Delete(noteId);
            return CommandResultResolver.Resolve(result);
        }
    }
}
