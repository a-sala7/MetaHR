using Business.Departments;
using Common.Constants;
using MetaHR_API.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Commands.Departments;
using Models.DTOs;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MetaHR_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentRepository _departmentRepository;

        public DepartmentsController(IDepartmentRepository departmentRepository)
        {
            _departmentRepository = departmentRepository;
        }

        // GET: api/<DepartmentsController>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get()
        {
            IEnumerable<DepartmentDTO> departments = await _departmentRepository.GetAll();
            return Ok(departments);
        }

        // GET api/<DepartmentsController>/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id)
        {
            DepartmentDTO department = await _departmentRepository.GetById(id);
            if(department == null)
            {
                return NotFound($"Department with ID {id} not found."):
            }
            return Ok(department);
        }

        // POST api/<DepartmentsController>
        [HttpPost]
        [Authorize(Roles = Roles.AdminsAndSeniors)]
        public async Task<IActionResult> Create(CreateDepartmentCommand cmd)
        {
            var cmdResult = await _departmentRepository.Create(cmd);
            return CommandResultResolver.Resolve(cmdResult);
        }

        // POST api/<DepartmentsController>/5
        [HttpPost("{id}")]
        public async Task<IActionResult> Update(int id, UpdateDepartmentCommand cmd)
        {
            var cmdResult = await _departmentRepository.Update(id, cmd);
            return CommandResultResolver.Resolve(cmdResult);
        }
    }
}
