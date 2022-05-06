using Business.Employees;
using Business.Files;
using Common.Constants;
using MetaHR_API.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Commands.Employees;
using Models.Responses;
using System.Security.Claims;

namespace MetaHR_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IFileManager _fileManager;

        public EmployeesController(IEmployeeRepository employeeRepository,
            IFileManager fileManager)
        {
            _employeeRepository = employeeRepository;
            _fileManager = fileManager;
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
            if (User.IsInRole(Roles.Employee))
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
        public async Task<IActionResult> OnboardEmployee(OnboardEmployeeCommand cmd)
        {
            var res = await _employeeRepository.OnboardEmployee(cmd);
            return CommandResultResolver.Resolve(res);
        }

        [HttpPost("test")]
        public async Task<IActionResult> Test([FromForm] TestCmd cmd)
        {
            var length = cmd.ProfilePicture.Length;
            if(length > Sizes.MaxPfpSizeBytes)
            {
                var sizeInMB = (int)(Sizes.MaxPfpSizeBytes / (1024 * 1024));
                return BadRequest($"File must be less than {sizeInMB}MB");
            }
            
            var ext = Path.GetExtension(cmd.ProfilePicture.FileName).ToLower();
            if (ext == ".jpg" || ext == ".jpeg")
            {
                if(FileSignatureVerifier.IsJpeg(cmd.ProfilePicture.OpenReadStream(), length) == false)
                {
                    return BadRequest("Not a valid .JPEG image.");
                }
            }
            else if (ext == ".png")
            {
                if (FileSignatureVerifier.IsPng(cmd.ProfilePicture.OpenReadStream(), length) == false)
                {
                    return BadRequest("Not a valid .PNG image.");
                }
            }
            else
            {
                return BadRequest("Must be a .JPEG or .PNG image.");
            }

            var url = await _fileManager
                .UploadFile(
                fileName: Guid.NewGuid().ToString() + ext,
                stream: cmd.ProfilePicture.OpenReadStream(),
                contentType: cmd.ProfilePicture.ContentType,
                folder: "profile-pictures"
                );

            return Ok(url);
        }
        [HttpPost("testDelete")]
        public async Task<IActionResult> TestDelete(string fileName, string? folder = null)
        {
            await _fileManager.DeleteFile(fileName, folder);

            return Ok(CommandResult.SuccessResult);
        }
        public class TestCmd
        {
            public string EmployeeId { get; set; }
            public IFormFile ProfilePicture { get; set; }
        }
    }
}
