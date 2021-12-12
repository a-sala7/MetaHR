using DataAccess.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Commands;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MetaHR_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: api/<AccountController>
        [HttpGet]
        public async Task<IActionResult> Get(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterCommand cmd)
        {
            var user = new ApplicationUser
            {
                FirstName = cmd.FirstName,
                LastName = cmd.LastName,
                UserName = cmd.Email,
                Email = cmd.Email,
            };
            var identityResult = await _userManager.CreateAsync(user, cmd.Password);
            if (identityResult.Succeeded)
            {
                return Ok(CommandResult.SuccessResult);
            }
            var identityErrors = identityResult.Errors.Select(e => e.Description);
            return BadRequest(CommandResult.GetErrorResult(identityErrors));
        }
    }
}
