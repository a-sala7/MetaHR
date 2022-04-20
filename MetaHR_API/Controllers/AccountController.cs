using DataAccess.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Responses;
using Models.Commands;
using Models.Commands.Accounts;
using Business.Accounts;
using Microsoft.AspNetCore.Authorization;
using Common.Constants;
using MetaHR_API.Utility;

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

        [HttpPost]
        public async Task<IActionResult> Register(RegisterCommand cmd)
        {
            var result = await _accountService.Register(cmd);
            return CommandResultResolver.Resolve(result);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginCommand cmd)
        {
            var loginResponse = await _accountService.Login(cmd);
            if (loginResponse.IsSuccessful)
            {
                return Ok(loginResponse);
            }
            return BadRequest(loginResponse);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordCommand cmd)
        {
            var result = await _accountService.ResetPassword(cmd);
            return CommandResultResolver.Resolve(result);
        }
    }
}
