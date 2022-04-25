using AutoMapper;
using DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Models.Commands.Notes;
using Models.DTOs;
using Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Business.EmployeeNotes
{
    public class EmployeeNoteRepository : IEmployeeNoteRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public EmployeeNoteRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<CommandResult> Create(string writtenById, CreateEmployeeNoteCommand cmd)
        {
            bool byExists = await _db.Employees.AnyAsync(e => e.Id == writtenById);
            if (!byExists)
            {
                return CommandResult.GetNotFoundResult("Employee", writtenById);
            }

            bool aboutExists = await _db.Employees.AnyAsync(e => e.Id == cmd.EmployeeId);
            if (!aboutExists)
            {
                return CommandResult.GetNotFoundResult("Employee", cmd.EmployeeId);
            }


            var newNote = _mapper.Map<CreateEmployeeNoteCommand, EmployeeNote>(cmd);
            newNote.AuthorId = writtenById;
            
            _db.Add(newNote);
            
            await _db.SaveChangesAsync();
            return CommandResult.SuccessResult;
        }

        public async Task<CommandResult> Delete(int id)
        {
            var noteInDb = await _db.EmployeeNotes.FirstOrDefaultAsync(n => n.Id == id);
            if (noteInDb == null)
            {
                return CommandResult.GetNotFoundResult("Note", id);
            }

            _db.EmployeeNotes.Remove(noteInDb);
            await _db.SaveChangesAsync();
            return CommandResult.SuccessResult;
        }

        public async Task<IEnumerable<EmployeeNoteDTO>> GetAllAboutEmployee(string authorId, string employeeId)
        {
            IEnumerable<EmployeeNoteDTO> notesAbout = await _db
                .EmployeeNotes
                .Include(n => n.Employee)
                .Include(n => n.Author)
                .Select(NoteToNoteDTOExpression)
                .Where(n => n.EmployeeId == employeeId && n.AuthorId == authorId)
                .ToListAsync();

            return notesAbout;
        }

        public async Task<IEnumerable<EmployeeNoteDTO>> GetAllByAuthor(string authorId)
        {
            IEnumerable<EmployeeNoteDTO> notesBy = await _db
                .EmployeeNotes
                .Include(n => n.Employee)
                .Include(n => n.Author)
                .Select(NoteToNoteDTOExpression)
                .Where(n => n.AuthorId == authorId)
                .ToListAsync();

            return notesBy;
        }

        public async Task<EmployeeNoteDTO> GetById(int id)
        {
            var note = await _db
                .EmployeeNotes
                .Include(n => n.Employee)
                .Include(n => n.Author)
                .FirstOrDefaultAsync(n => n.Id == id);

            if(note is null)
                return null;
            
            var noteDto = NoteToNoteDTOExpression.Compile().Invoke(note);
            return noteDto;
        }

        public async Task<CommandResult> Update(int id, string content)
        {
            var noteInDb = await _db.EmployeeNotes.FirstOrDefaultAsync(n => n.Id == id);
            if (noteInDb == null)
            {
                return CommandResult.GetNotFoundResult("Note", id);
            }
            noteInDb.Content = content;
            _db.EmployeeNotes.Update(noteInDb);
            await _db.SaveChangesAsync();
            return CommandResult.SuccessResult;
        }

        private readonly Expression<Func<EmployeeNote, EmployeeNoteDTO>> NoteToNoteDTOExpression
           = n => new EmployeeNoteDTO
           {
               Id = n.Id,
               AuthorId = n.AuthorId,
               AuthorName = n.Author.FirstName + " " + n.Author.LastName,
               EmployeeId = n.EmployeeId,
               EmployeeName = n.Employee.FirstName + " " + n.Employee.LastName,
               Content = n.Content
           };
    }
}
