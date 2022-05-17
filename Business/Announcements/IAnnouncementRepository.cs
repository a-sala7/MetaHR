using Common;
using Models.Commands.Announcements;
using Models.DTOs;
using Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Announcements
{
    public interface IAnnouncementRepository
    {
        Task<PagedResult<AnnouncementDTO>> GetAll(int pageNumber, int pageSize = 10);
        Task<PagedResult<AnnouncementDTO>> GetGlobalAndFromDepartment(int departmentId, int pageNumber, int pageSize = 10);
        Task<CommandResult> Create(CreateAnnouncementCommand cmd, int? departmentId, string authorId);
        Task<CommandResult> Update(int announcementId, UpdateAnnouncementCommand cmd);
        Task<CommandResult> Delete(int announcementId);
        Task<AnnouncementDTO?> GetById(int announcementId);
    }
}
