using DataAccess.Data;
using Models.Commands.Accounts;
using Models.DTOs;
using Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Accounts
{
    public interface IAccountService
    {
        Task<LoginResponse> Login(LoginCommand cmd);
        Task<CommandResult> ForgotPassword(string email);
        Task<CommandResult> ResetPassword(ResetPasswordCommand cmd);
        Task<CommandResult> ChangePassword(ChangePasswordCommand cmd);
        Task<CommandResult> RegisterAttendanceLogger(RegisterAttendanceLoggerCommand cmd);
        Task<CommandResult> VerifyResetPwdToken(VerifyTokenCommand cmd);
    }
}
