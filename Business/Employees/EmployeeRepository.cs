using AutoMapper;
using Business.Accounts;
using Business.Email;
using Business.Email.Models;
using DataAccess.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models.Commands.Accounts;
using Models.Commands.Employees;
using Models.DTOs;
using Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Business.Employees
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IAccountService _accountService;

        public EmployeeRepository(ApplicationDbContext db, IMapper mapper,
            UserManager<ApplicationUser> userManager, IEmailSender emailSender,
            IAccountService accountService)
        {
            _db = db;
            _mapper = mapper;
            _userManager = userManager;
            _emailSender = emailSender;
            _accountService = accountService;
        }

        public async Task<IEnumerable<EmployeeDTO>> GetAll()
        {
            IEnumerable<EmployeeDTO> employees = await _db
                .Employees
                .Select(EmployeeToEmployeeDTOExpression)
                .ToListAsync();
            return employees;
        }

        public async Task<EmployeeDTO> GetById(string employeeId)
        {
            Employee emp = await _db.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);
            if(emp == null)
            {
                return null;
            }
            
            EmployeeDTO empDto = EmployeeToEmployeeDTOExpression.Compile().Invoke(emp);
            return empDto;
        }

        public async Task<CommandResult> Create(CreateEmployeeCommand cmd)
        {
            if(_db.Departments.Any(d => d.Id == cmd.DepartmentId) == false)
            {
                return CommandResult.GetErrorResult($"Department with ID {cmd.DepartmentId} not found.");
            }
            Employee emp = _mapper.Map<CreateEmployeeCommand, Employee>(cmd);
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    emp.Email = cmd.Email.ToLower();
                    emp.UserName = cmd.Email.ToLower();
                    emp.DateRegisteredUtc = DateTime.UtcNow;
                    var pwd = Guid.NewGuid().ToString();
                    var res = await _userManager.CreateAsync(emp, password: pwd);
                    await _userManager.AddToRoleAsync(emp, cmd.Role);
                    string token = await _userManager.GeneratePasswordResetTokenAsync(emp);

                    transaction.Commit();

                    var emailModel = new NewUserEmailModel()
                    {
                        Email = cmd.Email,
                        FirstName = cmd.FirstName,
                        UserId = emp.Id,
                        PasswordResetToken = token
                    };
                    await _emailSender.SendEmailToNewUser(emailModel);

                    return CommandResult.SuccessResult;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }

        public async Task<CommandResult> Update(string employeeId, UpdateEmployeeCommand cmd)
        {
            Employee? empInDb = await _db.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);
            if(empInDb == null)
            {
                return CommandResult.GetNotFoundResult("Employee", employeeId);
            }

            empInDb = _mapper.Map<UpdateEmployeeCommand, Employee>(cmd);
            _db.Employees.Update(empInDb);

            if(await _db.SaveChangesAsync() > 0)
            {
                return CommandResult.SuccessResult;
            }
            return CommandResult.UnknownInternalErrorResult;
        }

        public async Task<CommandResult> ChangeRole(ChangeRoleCommand cmd)
        {
            ApplicationUser? emp = await _userManager.FindByIdAsync(cmd.EmployeeId);
            if(emp == null)
            {
                return CommandResult.GetNotFoundResult("Employee", cmd.EmployeeId);
            }
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    IList<string>? currRoles = await _userManager.GetRolesAsync(emp);
                    if(currRoles != null)
                    {
                        await _userManager.RemoveFromRolesAsync(emp, currRoles);
                    }
                    await _userManager.AddToRoleAsync(emp, cmd.RoleName);

                    transaction.Commit();
                    return CommandResult.SuccessResult;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }

        public async Task<CommandResult> OnboardEmployee(OnboardEmployeeCommand cmd)
        {
            Employee? empInDb = await _db.Employees.FirstOrDefaultAsync(e => e.Id == cmd.UserId);
            if(empInDb == null)
            {
                return CommandResult.GetNotFoundResult("Employee", cmd.UserId);
            }
            var resetPwdCmd = new ResetPasswordCommand
            {
                UserId = cmd.UserId,
                Password = cmd.Password,
                ResetPasswordToken = cmd.ResetPasswordToken
            };
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var resetPwdResult = await _accountService.ResetPassword(cmd);
                    if(resetPwdResult.IsSuccessful == false)
                    {
                        return resetPwdResult;
                    }
                    //TODO: code for changing profile picture
                    empInDb.GithubUsername = cmd.GithubUsername;
                    empInDb.LinkedInUsername = cmd.LinkedInUsername;
                    empInDb.PersonalWebsite = cmd.PersonalWebsite;
                    _db.Employees.Update(empInDb);
                    await _db.SaveChangesAsync();
                    transaction.Commit();
                    return CommandResult.SuccessResult;
                }
                catch(Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }

        public async Task<CommandResult> ChangeProfilePicture(string employeeId, IFormFile picture)
        {
            throw new NotImplementedException();
        }

        private readonly Expression<Func<Employee, EmployeeDTO>> EmployeeToEmployeeDTOExpression
           = e => new EmployeeDTO
           {
               Id = e.Id,
               Email = e.Email,
               Title = e.Title,
               FirstName = e.FirstName,
               LastName = e.LastName,
               DepartmentName = e.Department.Name,
               DateHired = e.DateHired,
               DateOfBirth = e.DateOfBirth,
               ProfilePictureUrl = e.ProfilePictureUrl,
               GithubUsername = e.GithubUsername,
               LinkedInUsername = e.LinkedInUsername,
               PersonalWebsite = e.PersonalWebsite
           };
    }
}
