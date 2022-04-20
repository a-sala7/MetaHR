using Business.Employees;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Commands.Employees;

namespace MetaHR_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IEmployeeRepository _er;

        public TestController(IEmployeeRepository er)
        {
            _er = er;
        }

        [HttpPost]
        public async Task<IActionResult> Test(CreateEmployeeCommand cmd)
        {
            var res = await _er.Create(cmd);
            return Ok(res);
        }
    }
}
