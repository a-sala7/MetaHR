﻿using System;
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

namespace Business.Accounts
{
    public class AccountService : IAccountService
    {
        private readonly ApiConfiguration _apiConfiguration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public AccountService(ApiConfiguration apiConfiguration,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _apiConfiguration = apiConfiguration;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<LoginResponse> Login(LoginCommand cmd)
        {
            var user = await _userManager.FindByEmailAsync(cmd.Email);
            if(user == null)
            {
                return LoginResponse.ErrorResponse($"User with email {cmd.Email} not found.");
            }
            var signInResult = await _signInManager.PasswordSignInAsync
                (user, cmd.Password, isPersistent: true, lockoutOnFailure: true);

            if(signInResult.Succeeded)
            {
                var token = await GetToken(user);
                var userInfo = new LocalUserInfo
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Token = token
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

        public async Task<CommandResult> Register(RegisterCommand cmd)
        {
            var user = new ApplicationUser
            {
                FirstName = cmd.FirstName,
                LastName = cmd.LastName,
                UserName = cmd.Email.ToLower(),
                Email = cmd.Email.ToLower(),
                DateRegisteredUtc = DateTime.UtcNow
            };
            var identityResult = await _userManager.CreateAsync(user, cmd.Password);
            if (identityResult.Succeeded)
            {
                return CommandResult.SuccessResult;
            }
            var identityErrors = identityResult.Errors.Select(e => e.Description);
            return CommandResult.GetErrorResult(identityErrors);
        }

        public async Task<ApplicationUser> GetUserByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                return null;

            return user;

            //return new ApplicationUserDTO
            //{
            //    Id = user.Id,
            //    Email = user.Email,
            //    FirstName = user.FirstName,
            //    LastName = user.LastName,
            //    EmailConfirmed = user.EmailConfirmed
            //};
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

            foreach(var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }

        private SigningCredentials GetSigningCredentials()
        {
            SymmetricSecurityKey secret = new (Encoding.UTF8.GetBytes(_apiConfiguration.SecretKey));
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

        public async Task<CommandResult> ResetPassword(ResetPasswordCommand cmd)
        {
            var user = await _userManager.FindByIdAsync(cmd.UserId);
            if (user == null)
            {
                return CommandResult.GetNotFoundResult($"User with ID {cmd.UserId} not found.");
            }
            var identityResult = await _userManager.ResetPasswordAsync(user, cmd.ResetPasswordToken, cmd.Password);
            if (identityResult.Succeeded)
            {
                return CommandResult.SuccessResult;
            }
            return CommandResult.GetErrorResult(identityResult.Errors.Select(e => e.Description));
        }
    }
}
