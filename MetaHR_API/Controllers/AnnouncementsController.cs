using Business.Announcements;
using Business.Employees;
using Common;
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

        /// <summary>
        /// Gets an Announcement by ID
        /// </summary>
        /// <returns>The requested AnnouncementDTO</returns>
        /// <remarks>
        /// Roles: Admin, HR_Junior, HR_Senior, Employee
        /// 
        /// Employee can only request an Announcement from his department or a global announcement (global announcements have announcementId = null)
        /// 
        /// Sample request:
        ///
        ///     GET /api/announcements/7
        ///     
        /// Sample response:
        /// 
        ///     {
        ///         "id": 7,
        ///         "authorId": "d2264968-a1e6-432c-b178-2a4e7627c289",
        ///         "authorName": "Ahmed Salah",
        ///         "departmentId": null,
        ///         "departmentName": null,
        ///         "title": "Hello",
        ///         "content": "This is a global announcement",
        ///         "createdAt": "2022-04-27T00:07:46.6979196"
        ///     }
        /// </remarks>
        /// <response code="200">OK: AnnouncementDTO in response body</response>
        /// <response code="404">Not Found</response>
        /// <response code="401">Unauthorized</response>
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

        /// <summary>
        /// Gets all Announcements visible to the current user
        /// </summary>
        /// <remarks>
        /// Roles: Admin, HR_Junior, HR_Senior, Employee
        /// 
        /// When an employee uses this, only Announcements from his department and global announcements are returned (global announcements have announcementId = null)
        /// 
        /// Sample request:
        ///
        ///     GET /api/announcements?pageNumber=1&amp;pageSize=10
        /// </remarks>
        /// <response code="200">Array of AnnouncementDTO</response>
        [HttpGet]
        [Authorize(Roles = Roles.AdminsAndHR + "," + Roles.Employee)]
        public async Task<IActionResult> GetAll(int pageNumber, int pageSize = 10)
        {
            if (User.IsInRole(Roles.Employee))
            {
                //only get ones visible to him
                EmployeeDTO emp = await _employeeRepository.GetById(User.GetId());

                PagedResult<AnnouncementDTO> announcements = await
                    _announcementRepo
                    .GetGlobalAndFromDepartment(emp.DepartmentId, 
                    pageNumber: pageNumber, pageSize: pageSize);

                return Ok(announcements);
            }
            else
            {
                PagedResult<AnnouncementDTO> announcements = await _announcementRepo
                    .GetAll(pageNumber: pageNumber, pageSize: pageSize);
                return Ok(announcements);
            }
        }
        /// <summary>
        /// Create a global announcement (visible to all users)
        /// </summary>
        /// <remarks>
        /// Roles: HR_Junior, HR_Senior
        /// 
        /// Sample request:
        /// 
        ///     POST /api/announcements/createGlobalAnnouncement
        ///     requestBody:
        ///     {
        ///         "title": "Title of the announcement"
        ///         "content": "Content of the announcement"
        ///     }
        /// </remarks>
        /// <response code="200">OK</response>
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


        /// <summary>
        /// Creates a department-specific announcement (only visible to employees of that department &amp; HR &amp; Admins)
        /// </summary>
        /// <remarks>
        /// Roles: HR_Junior, HR_Senior, DepartmentDirector
        /// 
        /// DepartmentDirector can only make announcements to his department
        /// 
        /// Sample request:
        /// 
        ///     POST /api/announcements/createAnnouncement?departmentId=123
        ///     requestBody:
        ///     {
        ///         "title": "Title of the announcement"
        ///         "content": "Content of the announcement"
        ///     }
        /// </remarks>
        /// <response code="200">OK</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="404">Not Found (Department with given ID not found)</response>
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


        /// <summary>
        /// Updates an announcement
        /// </summary>
        /// <remarks>
        /// Roles: HR_Junior, HR_Senior, DepartmentDirector
        /// 
        /// Users can only update announcements they wrote themselves
        /// 
        /// Sample request:
        /// 
        ///     POST /api/announcements/5
        ///     requestBody:
        ///     {
        ///         "title": "New title"
        ///         "content": "New content"
        ///     }
        /// </remarks>
        /// <response code="200">OK</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost("{announcementId}")]
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

        /// <summary>
        /// Deletes an announcement
        /// </summary>
        /// <remarks>
        /// Roles: Admin, HR_Junior, HR_Senior, DepartmentDirector
        /// 
        /// Admins and HR Seniors can delete any announcement
        /// DepartmentDirectors and HR Juniors can only delete announcements which they wrote themselves
        /// 
        /// Sample request:
        ///
        ///     DELETE /api/announcements/5
        /// </remarks>
        /// <response code="200">OK</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="404">Not Found</response>
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
