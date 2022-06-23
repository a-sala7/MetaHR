using Business.Employees;
using Business.VacationRequests;
using Common;
using Common.Constants;
using MetaHR_API.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Commands.VacationRequests;
using Models.DTOs;
using Models.Responses;

namespace MetaHR_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VacationRequestsController : ControllerBase
    {
        private readonly IVacationRequestRepository _repo;
        private readonly IEmployeeRepository _empRepo;

        public VacationRequestsController(IVacationRequestRepository repo, IEmployeeRepository empRepo)
        {
            _repo = repo;
            _empRepo = empRepo;
        }

        [HttpGet]
        [Authorize(Roles = Roles.HRJunior + "," + Roles.HRSenior)]
        public async Task<IActionResult> GetAll(int pageNumber, int pageSize = 10)
        {
            PagedResult<VacationRequestDTO> result = await _repo.GetAll(pageNumber, pageSize);
            return Ok(result);
        }

        [HttpGet("byEmployee")]
        [Authorize(Roles = Roles.HRJunior + "," + Roles.HRSenior + "," + Roles.Employee)]
        public async Task<IActionResult> GetByEmployee(string employeeId, int pageNumber, int pageSize = 10)
        {
            var emp = await _empRepo.GetById(employeeId);
            if(emp == null)
            {
                return NotFound($"Employee with ID {employeeId} not found.");
            }
            if(User.IsInRole(Roles.Employee) && User.GetId() != employeeId)
            {
                return Unauthorized("You can't view another employee's VacationRequests.");
            }
            PagedResult<VacationRequestDTO> result = await _repo.GetByEmployee(employeeId, pageNumber, pageSize);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = Roles.HRJunior + "," + Roles.HRSenior + "," + Roles.Employee)]
        public async Task<IActionResult> Create(CreateVacationRequestCommand cmd)
        {
            var res = await _repo.Create(User.GetId(), cmd);
            return CommandResultResolver.Resolve(res);
        }

        [HttpPost("{id}")]
        [Authorize(Roles = Roles.HRSenior + "," + Roles.HRJunior)]
        public async Task<IActionResult> Update(int id, UpdateVacationRequestCommand cmd)
        {
            CommandResult? res = await _repo.Update(id, User.GetId(), cmd);
            return CommandResultResolver.Resolve(res);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.HRSenior + "," + Roles.HRJunior)]
        public async Task<IActionResult> Delete(int id)
        {
            CommandResult? res = await _repo.Delete(id);
            return CommandResultResolver.Resolve(res);
        }
    }
}
