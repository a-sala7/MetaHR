using Business.Announcements;
using Business.Employees;
using Common.Constants;
using DataAccess.Data;
using MetaHR_API.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.Commands.Announcements;
using Models.DTOs;
using Models.Responses;

namespace MetaHR_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnnouncementsController : ControllerBase
    {
        private readonly IAnnouncementRepository _announcementRepo;
        private readonly IEmployeeRepository _employeeRepository;

        public AnnouncementsController(IAnnouncementRepository announcementRepo,
            IEmployeeRepository employeeRepository)
        {
            _announcementRepo = announcementRepo;
            _employeeRepository = employeeRepository;
        }

        [HttpGet("{announcementId}")]
        [Authorize(Roles = Roles.AdminsAndHR + "," + Roles.Employee)]
        public async Task<IActionResult> GetById(int announcementId)
        {
            AnnouncementDTO announcement = await _announcementRepo.GetById(announcementId);
            if(announcement == null)
            {
                return NotFound($"Announcement with ID {announcementId} not found.");
            }
            if (User.IsInRole(Roles.Employee))
            {
                var emp = await _employeeRepository.GetById(User.GetId());
                if (emp.DepartmentId != announcement.DepartmentId)
                {
                    return Unauthorized();
                }
            }
            return Ok(announcement);
        }

        [HttpGet]
        [Authorize(Roles = Roles.AdminsAndHR + "," + Roles.Employee)]
        public async Task<IActionResult> GetAll(int pageNumber)
        {
            if (User.IsInRole(Roles.Employee))
            {
                //only get ones visible to him
                EmployeeDTO emp = await _employeeRepository.GetById(User.GetId());

                IEnumerable<AnnouncementDTO> announcements = await
                    _announcementRepo.GetGlobalAndFromDepartment(emp.DepartmentId, pageNumber: pageNumber);

                return Ok(announcements);
            }
            else
            {
                IEnumerable<AnnouncementDTO> announcements = await _announcementRepo.GetAll(pageNumber);
                return Ok(announcements);
            }
        }

        [HttpPost("createGlobalAnnouncement")]
        [Authorize(Roles = Roles.HRJunior + "," + Roles.HRSenior)]
        public async Task<IActionResult> CreateGlobalAnnouncement(CreateAnnouncementCommand cmd)
        {
            CommandResult res = await _announcementRepo
                .Create(cmd, 
                departmentId: null, 
                authorId: User.GetId());

            return CommandResultResolver.Resolve(res);
        }

        [HttpPost("createAnnouncement")]
        [Authorize(Roles = Roles.HRJunior + "," + Roles.HRSenior + "," + Roles.DepartmentDirector)]
        public async Task<IActionResult> CreateAnnouncement(CreateAnnouncementCommand cmd, int departmentId)
        {
            if (User.IsInRole(Roles.DepartmentDirector))
            {
                EmployeeDTO emp = await _employeeRepository.GetById(User.GetId());
                if(departmentId != emp.DepartmentId)
                {
                    var cr = CommandResult.GetErrorResult("You are not authorized to post announcements to this department.");
                    return Unauthorized(cr);
                }
            }

            CommandResult res = await _announcementRepo
                .Create(cmd,
                departmentId: departmentId,
                authorId: User.GetId());

            return CommandResultResolver.Resolve(res);
        }

        [HttpPost("{announcementId}/update")]
        [Authorize(Roles = Roles.HRJunior + "," + Roles.HRSenior + "," + Roles.DepartmentDirector)]
        public async Task<IActionResult> UpdateAnnouncement(UpdateAnnouncementCommand cmd, int announcementId)
        {
            EmployeeDTO emp = await _employeeRepository.GetById(User.GetId());
            var an = await _announcementRepo.GetById(announcementId);
            if(emp.Id != an.AuthorId)
            {
                var cr = CommandResult.GetErrorResult("You are not authorized to edit this announcement.");
                return Unauthorized(cr);
            }

            CommandResult res = await _announcementRepo.Update(announcementId, cmd);

            return CommandResultResolver.Resolve(res);
        }

        [HttpDelete("{announcementId}")]
        [Authorize(Roles = Roles.AdminsAndHR + "," + Roles.DepartmentDirector)]
        public async Task<IActionResult> DeleteAnnouncement(int announcementId)
        {
            if(User.IsInRole(Roles.Admin) || User.IsInRole(Roles.HRSenior))
            {
                var res1 = await _announcementRepo.Delete(announcementId);
                return CommandResultResolver.Resolve(res1);
            }
            else
            {
                EmployeeDTO emp = await _employeeRepository.GetById(User.GetId());
                var an = await _announcementRepo.GetById(announcementId);
                if (emp.Id != an.AuthorId)
                {
                    var cr = CommandResult.GetErrorResult("You are not authorized to delete this announcement.");
                    return Unauthorized(cr);
                }
                else
                {
                    CommandResult res2 = await _announcementRepo.Delete(announcementId);

                    return CommandResultResolver.Resolve(res2);
                }
            }
        }
    }
}
