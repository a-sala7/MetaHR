using DataAccess.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Responses;
using Models.Commands;
using Models.Commands.Account;
using Business.Accounts;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MetaHR_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        // GET: api/<AccountController>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            var user = await _accountService.GetUserByEmail(email);
            if(user is null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterCommand cmd)
        {
            var result = await _accountService.Register(cmd);
            if (result.IsSuccessful)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginCommand cmd)
        {
            var loginResponse = await _accountService.Login(cmd);
            if (loginResponse.IsSuccessful)
            {
                return Ok(loginResponse);
            }
            return BadRequest(loginResponse);
        }
    }
}
