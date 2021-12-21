using DataAccess.Data;
using Models.Commands.Account;
using Models.DTOs;
using Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Account
{
    public interface IAccountService
    {
        Task<ApplicationUser> GetUserByEmail(string email);
        Task<LoginResponse> Login(LoginCommand cmd);
        Task<CommandResult> Register(RegisterCommand cmd);
    }
}
