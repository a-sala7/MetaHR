using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Common.ConfigurationClasses;
using Models.DTOs;
using Models.Commands;
using Models.Commands.Accounts;
using System.IdentityModel.Tokens.Jwt;
using Models.Responses;
using Business.Email;
using Business.Email.Models;
using Common.Constants;
using Microsoft.EntityFrameworkCore;

namespace Business.Accounts
{
    public class AccountService : IAccountService
    {
        private readonly ApiConfiguration _apiConfiguration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _db;
        public AccountService(ApiConfiguration apiConfiguration,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender, ApplicationDbContext db)
        {
            _apiConfiguration = apiConfiguration;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _db = db;
        }

        public async Task<LoginResponse> Login(LoginCommand cmd)
        {
            var user = await _userManager.FindByEmailAsync(cmd.Email);
            if (user == null)
            {
                return LoginResponse.ErrorResponse($"User with email {cmd.Email} not found.");
            }
            var signInResult = await _signInManager.PasswordSignInAsync
                (user, cmd.Password, isPersistent: true, lockoutOnFailure: true);

            if (signInResult.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var token = await GetToken(user);
                Employee? emp = await _db.Employees.FirstOrDefaultAsync(e => e.Email == cmd.Email);
                var userInfo = new LocalUserInfo
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Token = token,
                    ProfilePictureURL = emp?.ProfilePictureURL,
                    Roles = roles
                };
                return LoginResponse.SuccessResponse(userInfo);
            }
            else if (signInResult.IsNotAllowed)
            {
                //We can return this if we want depending on email/phone number
                //confirmation/manual account confirmation by HR staff etc. with different
                //error messages for each case
                return LoginResponse.ErrorResponse("Your account is not yet confirmed.");
            }
            else if (signInResult.IsLockedOut)
            {
                return LoginResponse.ErrorResponse("You have entered an invalid password too many times. Please try again in 5 minutes.");
            }
            else
            {
                return LoginResponse.ErrorResponse($"Invalid password, you have {5 - user.AccessFailedCount} attempts remaining.");
            }
        }

        private async Task<List<Claim>> GetUserClaims(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
            };

            IList<string> roles = await _userManager.GetRolesAsync(user);

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }

        private SigningCredentials GetSigningCredentials()
        {
            SymmetricSecurityKey secret = new(Encoding.UTF8.GetBytes(_apiConfiguration.SecretKey));
            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }

        private async Task<string> GetToken(ApplicationUser user)
        {
            var signingCredentials = GetSigningCredentials();
            var claims = await GetUserClaims(user);
            var token = new JwtSecurityToken(
                signingCredentials: signingCredentials,
                issuer: _apiConfiguration.ValidIssuer,
                audience: _apiConfiguration.ValidAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(14)
            );

            var tokenAsString = new JwtSecurityTokenHandler().WriteToken(token);
            return tokenAsString;
        }

        public async Task<CommandResult> ForgotPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return CommandResult.GetNotFoundResult("User", email);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var model = new PasswordResetEmailModel
            {
                Email = email,
                UserId = user.Id,
                FirstName = user.FirstName,
                PasswordResetToken = token
            };
            await _emailSender.SendPasswordResetEmail(model);
            return CommandResult.SuccessResult;
        }

        public async Task<CommandResult> ChangePassword(ChangePasswordCommand cmd)
        {
            var user = await _userManager.FindByIdAsync(cmd.UserId);
            if (user == null)
            {
                return CommandResult.GetNotFoundResult("User", cmd.UserId);
            }

            var changeResult = await _userManager.ChangePasswordAsync(user, cmd.OldPassword, cmd.NewPassword);

            if (changeResult.Succeeded)
            {
                return CommandResult.SuccessResult;
            }
            IEnumerable<string> errors = changeResult.Errors.Select(e => e.Description);
            return CommandResult.GetErrorResult(errors);
        }

        public async Task<CommandResult> ResetPassword(ResetPasswordCommand cmd)
        {
            var user = await _userManager.FindByIdAsync(cmd.UserId);
            if (user == null)
            {
                return CommandResult.GetNotFoundResult("User", cmd.UserId);
            }
            var identityResult = await _userManager.ResetPasswordAsync(user, cmd.ResetPasswordToken, cmd.Password);
            if (identityResult.Succeeded)
            {
                return CommandResult.SuccessResult;
            }
            return CommandResult.GetErrorResult(identityResult.Errors.Select(e => e.Description));
        }

        public async Task<CommandResult> RegisterAttendanceLogger(RegisterAttendanceLoggerCommand cmd)
        {
            var user = new ApplicationUser
            {
                FirstName = cmd.FirstName,
                LastName = cmd.LastName,
                Email = cmd.Email.ToLower(),
                UserName = cmd.Email.ToLower(),
                DateRegisteredUtc = DateTime.UtcNow
            };
            var identityResult = await _userManager.CreateAsync(user, cmd.Password);
            if (identityResult.Succeeded == false)
            {
                var errors = identityResult.Errors.Select(e => e.Description);
                return CommandResult.GetErrorResult(errors);
            }
            var roleResult = await _userManager.AddToRoleAsync(user, Roles.AttendanceLogger);
            if (roleResult.Succeeded == false)
            {
                var errors = identityResult.Errors.Select(e => e.Description);
                return CommandResult.GetErrorResult(errors);
            }
            return CommandResult.SuccessResult;
        }

        public async Task<CommandResult> VerifyResetPwdToken(VerifyTokenCommand cmd)
        {
            var user = await _userManager.FindByIdAsync(cmd.UserId);
            if (user == null)
            {
                return CommandResult.GetNotFoundResult("User", cmd.UserId);
            }

            string purpose = UserManager<ApplicationUser>.ResetPasswordTokenPurpose;
            bool success = await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultProvider, purpose, cmd.Token);
            if (success)
            {
                return CommandResult.SuccessResult;
            }
            return CommandResult.GetErrorResult("Invalid token");
        }
    }
}
