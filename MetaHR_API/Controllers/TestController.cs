using Business.Test;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MetaHR_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly TestRepository _testRepository;

        public TestController(TestRepository testRepository)
        {
            _testRepository = testRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Test(string userId)
        {
            var res = await _testRepository.Test(userId);
            return Ok(res);
        }
    }
}
