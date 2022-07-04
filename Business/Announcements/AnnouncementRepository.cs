using Common;
using DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Models.Commands.Announcements;
using Models.DTOs;
using Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Business.Announcements
{
    public class AnnouncementRepository : IAnnouncementRepository
    {
        private readonly ApplicationDbContext _db;

        public AnnouncementRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<AnnouncementDTO?> GetById(int announcementId)
        {
            var announcement = await _db
                .Announcements
                .Include(a => a.Author)
                .Include(a => a.Department)
                .Select(AnnouncementToAnnouncementDTOExpression)
                .FirstOrDefaultAsync(a => a.Id == announcementId);

            return announcement;
        }

        public async Task<PagedResult<AnnouncementDTO>> GetAll(int pageNumber, int pageSize = 10)
        {
            var announcements = await _db
                .Announcements
                .Include(a => a.Author)
                .Include(a => a.Department)
                .OrderByDescending(a => a.CreatedAtUtc)
                .Paginate(pageNumber: pageNumber, pageSize: pageSize)
                .Select(AnnouncementToAnnouncementDTOExpression)
                .ToListAsync();

            int totalCount = await _db
                .Announcements
                .CountAsync();

            return announcements.GetPagedResult(totalCount);
        }

        public async Task<PagedResult<AnnouncementDTO>> GetGlobalAndFromDepartment(int departmentId, int pageNumber, int pageSize = 10)
        {
            var announcements = await _db
                .Announcements
                .Include(a => a.Author)
                .Include(a => a.Department)
                .Where(a => a.DepartmentId == departmentId || a.DepartmentId == null)
                .OrderByDescending(a => a.CreatedAtUtc)
                .Paginate(pageNumber: pageNumber, pageSize: pageSize)
                .Select(AnnouncementToAnnouncementDTOExpression)
                .ToListAsync();

            int totalCount = await _db
                .Announcements
                .Where(a => a.DepartmentId == departmentId || a.DepartmentId == null)
                .CountAsync();

            return announcements.GetPagedResult(totalCount);
        }

        public async Task<CommandResult> Create(CreateAnnouncementCommand cmd, int? departmentId, string authorId)
        {
            if(departmentId != null)
            {
                if(await _db.Departments.AnyAsync(d => d.Id == departmentId.Value) == false)
                {
                    return CommandResult.GetNotFoundResult("Department", departmentId.Value);
                }
            }
            var an = new Announcement
            {
                Title = cmd.Title,
                Content = cmd.Content,
                AuthorId = authorId,
                CreatedAtUtc = DateTime.UtcNow,
                DepartmentId = departmentId
            };
            _db.Announcements.Add(an);
            await _db.SaveChangesAsync();
            return CommandResult.SuccessResult;
        }

        public async Task<CommandResult> Update(int announcementId, UpdateAnnouncementCommand cmd)
        {
            var anInDb = await _db.Announcements.FirstOrDefaultAsync(a => a.Id == announcementId);
            if (anInDb == null)
            {
                return CommandResult.GetNotFoundResult("Announcement", announcementId);
            }
            anInDb.Title = cmd.Title;
            anInDb.Content = cmd.Content;
            _db.Announcements.Update(anInDb);
            await _db.SaveChangesAsync();
            return CommandResult.SuccessResult;
        }

        public async Task<CommandResult> Delete(int announcementId)
        {
            var anInDb = await _db.Announcements.FirstOrDefaultAsync(a => a.Id == announcementId);
            if (anInDb == null)
            {
                return CommandResult.GetNotFoundResult("Announcement", announcementId);
            }
            _db.Announcements.Remove(anInDb);
            await _db.SaveChangesAsync();
            return CommandResult.SuccessResult;
        }

        private readonly Expression<Func<Announcement, AnnouncementDTO>> AnnouncementToAnnouncementDTOExpression
           = a => new AnnouncementDTO
           {
               Id = a.Id,
               Title = a.Title,
               Content = a.Content,
               AuthorId = a.AuthorId,
               AuthorName = a.Author.FirstName + " " + a.Author.LastName,
               AuthorPfpURL = a.Author.ProfilePictureURL,
               CreatedAt = a.CreatedAtUtc,
               DepartmentId = a.DepartmentId,
               DepartmentName = a.Department.Name
           };
    }
}
