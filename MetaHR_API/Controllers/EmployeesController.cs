using Business.Employees;
using Business.FileManager;
using Common.Constants;
using MetaHR_API.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Commands.Employees;
using Models.Responses;

namespace MetaHR_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeesController(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllEmployees()
        {
            var employees = await _employeeRepository.GetAll();
            //hide DoB & DH from other employees
            if (User.IsInRole(Roles.Employee))
            {
                foreach(var e in employees)
                {
                    e.DateHired = null;
                    e.DateOfBirth = null;
                }
            }
            return Ok(employees);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(string id)
        {
            var employee = await _employeeRepository.GetById(id);
            if(employee == null)
            {
                return NotFound($"Employee with ID {id} not found.");
            }
            //hide DoB & DH from other employees
            if (User.IsInRole(Roles.Employee) && User.GetId() != id)
            {
                employee.DateHired = null;
                employee.DateOfBirth = null;
            }
            return Ok(employee);
        }

        [HttpPost]
        [Authorize(Roles = Roles.AdminsAndHR)]
        public async Task<IActionResult> CreateEmployee(CreateEmployeeCommand cmd)
        {
            var r = cmd.Role;
            if(User.IsInRole(Roles.Admin))
            {
                if(r != Roles.HRSenior && r != Roles.HRJunior && r != Roles.Employee)
                {
                    return BadRequest(CommandResult.GetErrorResult($"{r} is an invalid role choice."));
                }
            }
            if (User.IsInRole(Roles.HRSenior))
            {
                if (r != Roles.HRSenior && r != Roles.HRJunior && r != Roles.Employee)
                {
                    return BadRequest(CommandResult.GetErrorResult($"{r} is an invalid role " +
                        $"choice or you are not authorized to create such a user."));
                }
            }
            if (User.IsInRole(Roles.HRJunior))
            {
                if (r != Roles.HRJunior && r != Roles.Employee)
                {
                    return BadRequest(CommandResult.GetErrorResult($"{r} is an invalid role " +
                        $"choice or you are not authorized to create such a user."));
                }
            }

            var res = await _employeeRepository.Create(cmd);
            return CommandResultResolver.Resolve(res);
        }

        [HttpPost("changeRole")]
        [Authorize(Roles = Roles.AdminsAndHR)]
        public async Task<IActionResult> ChangeRole(ChangeRoleCommand cmd)
        {
            if(User.GetId() == cmd.EmployeeId)
            {
                return BadRequest("You can't change your own role.");
            }
            var r = cmd.RoleName;
            if (User.IsInRole(Roles.Admin))
            {
                if (r != Roles.HRSenior && r != Roles.HRJunior && r != Roles.Employee)
                {
                    return BadRequest(CommandResult.GetErrorResult($"{r} is an invalid role choice."));
                }
            }
            if (User.IsInRole(Roles.HRSenior))
            {
                if (r != Roles.HRSenior && r != Roles.HRJunior)
                {
                    return BadRequest(CommandResult.GetErrorResult($"{r} is an invalid role " +
                        $"choice or you are not authorized to grant this role."));
                }
            }
            if (User.IsInRole(Roles.HRJunior))
            {
                if (r != Roles.Employee)
                {
                    return BadRequest(CommandResult.GetErrorResult($"{r} is an invalid role " +
                        $"choice or you are not authorized to grant this role."));
                }
            }

            var res = await _employeeRepository.ChangeRole(cmd);
            return CommandResultResolver.Resolve(res);
        }

        [HttpPost("onboard")]
        public async Task<IActionResult> OnboardEmployee([FromForm] OnboardEmployeeCommand cmd)
        {
            if(cmd.ProfilePicture != null)
            {
                var verifyResult = FileVerifier.VerifyImage(cmd.ProfilePicture);
                if (verifyResult.IsSuccessful == false)
                {
                    return CommandResultResolver.Resolve(verifyResult);
                }
            }

            var res = await _employeeRepository.OnboardEmployee(cmd);
            return CommandResultResolver.Resolve(res);
        }

        [HttpPost("changepfp")]
        [Authorize]
        public async Task<IActionResult> ChangeProfilePicture([FromForm] ChangePfpCommand cmd)
        {
            if (User.IsInRole(Roles.Admin))
            {
                return BadRequest(CommandResult.GetErrorResult("Admin users can't have profile pictures."));
            }
            if(cmd.EmployeeId != User.GetId())
            {
                return BadRequest(CommandResult.GetErrorResult("You cannot change someone else's profile picture."));
            }
            var verifyResult = FileVerifier.VerifyImage(cmd.Picture);

            if(verifyResult.IsSuccessful == false)
            {
                return CommandResultResolver.Resolve(verifyResult);
            }

            var res = await _employeeRepository.ChangeProfilePicture(cmd);
            return CommandResultResolver.Resolve(res);
        }
        [HttpPost("deletepfp")]
        [Authorize]
        public async Task<IActionResult> DeleteProfilePicture(string employeeId)
        {
            if (User.IsInRole(Roles.Admin))
            {
                if(employeeId == User.GetId())
                {
                    return BadRequest(CommandResult.GetErrorResult("Admin users can't have profile pictures." +
                        " What are you trying to delete?"));
                }
            }
            else
            {
                if (employeeId != User.GetId())
                {
                    return BadRequest(CommandResult.GetErrorResult("You cannot delete someone else's profile picture."));
                }
            }
            

            var res = await _employeeRepository.DeleteProfilePicture(employeeId);
            return CommandResultResolver.Resolve(res);
        }
        [HttpPost("updateProfile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(UpdateProfileCommand cmd)
        {
            if(User.IsInRole(Roles.Admin) || User.IsInRole(Roles.AttendanceLogger))
            {
                return BadRequest();
            }
            var res = await _employeeRepository.UpdateProfile(User.GetId(), cmd);
            return CommandResultResolver.Resolve(res);
        }
    }
}
