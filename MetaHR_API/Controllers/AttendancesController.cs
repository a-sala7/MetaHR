using Business.Attendances;
using Common;
using Common.Constants;
using MetaHR_API.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Commands.Attendances;
using Models.DTOs;
using Models.Responses;

namespace MetaHR_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendancesController : ControllerBase
    {
        private readonly IAttendanceRepository _repo;

        public AttendancesController(IAttendanceRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("getByEmployeeId/{employeeId}")]
        [Authorize(Roles = Roles.HRJunior + "," + Roles.HRSenior)]
        public async Task<IActionResult> GetByEmployeeId(string employeeId, int pageNumber, int pageSize = 10)
        {
            PagedResult<AttendanceDTO>? atts = await _repo
                .GetByEmployeeId(employeeId, pageNumber: pageNumber, pageSize: pageSize);

            return Ok(atts);
        }

        [HttpPost]
        [Authorize(Roles = Roles.AttendanceLogger)]
        public async Task<IActionResult> Create(CreateAttendanceCommand cmd)
        {
            CommandResult res = await _repo.Create(cmd);
            return CommandResultResolver.Resolve(res);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.HRJunior + "," + Roles.HRSenior)]
        public async Task<IActionResult> Delete(int id)
        {
            CommandResult res = await _repo.Delete(id);
            return CommandResultResolver.Resolve(res);
        }
    }
}
