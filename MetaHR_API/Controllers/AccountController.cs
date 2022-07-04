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
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var result = await _accountService.ForgotPassword(email);
            return CommandResultResolver.Resolve(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword)
        {
            var cmd = new ChangePasswordCommand
            {
                OldPassword = oldPassword,
                NewPassword = newPassword,
                UserId = User.GetId()
            };
            var result = await _accountService.ChangePassword(cmd);
            return CommandResultResolver.Resolve(result);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordCommand cmd)
        {
            var result = await _accountService.ResetPassword(cmd);
            return CommandResultResolver.Resolve(result);
        }

        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> RegisterAttendanceLogger(RegisterAttendanceLoggerCommand cmd)
        {
            CommandResult res = await _accountService.RegisterAttendanceLogger(cmd);
            return CommandResultResolver.Resolve(res);
        }

        [HttpPost]
        public async Task<IActionResult> VerifyToken(VerifyTokenCommand cmd)
        {
            CommandResult res = await _accountService.VerifyResetPwdToken(cmd);
            return CommandResultResolver.Resolve(res);
        }
    }
}
