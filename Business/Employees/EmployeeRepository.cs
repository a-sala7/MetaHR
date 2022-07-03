using AutoMapper;
using Business.Accounts;
using Business.Email;
using Business.Email.Models;
using Business.FileManager;
using Common.Constants;
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
using System.Diagnostics;
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
        private readonly IFileManager _fileManager;

        public EmployeeRepository(ApplicationDbContext db, IMapper mapper,
            UserManager<ApplicationUser> userManager, IEmailSender emailSender,
            IAccountService accountService, IFileManager fileManager)
        {
            _db = db;
            _mapper = mapper;
            _userManager = userManager;
            _emailSender = emailSender;
            _accountService = accountService;
            _fileManager = fileManager;
        }

        public async Task<IEnumerable<EmployeeDTO>> GetAll()
        {
            IEnumerable<EmployeeDTO> employees = await _db
                .Employees
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .Select(EmployeeToEmployeeDTOExpression)
                .ToListAsync();
            return employees;
        }

        public async Task<EmployeeDTO> GetById(string employeeId)
        {
            Employee emp = await _db
                .Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == employeeId);
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
            if(cmd.Role == Roles.DepartmentDirector)
            {
                return CommandResult.GetErrorResult("Accounts cannot be created with the role DepartmentDirector. Please create an employee first then assign him as a director from departments list.");
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
                    if (res.Succeeded)
                    {
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
                        await _emailSender.SendOnboardEmail(emailModel);

                        return CommandResult.SuccessResult;
                    }
                    else
                    {
                        var errors = res.Errors.Select(e => e.Description);
                        return CommandResult.GetErrorResult(errors);
                    }
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
            
            using (var transaction = _db.Database.BeginTransaction())
            {
                if(cmd.ProfilePicture != null)
                {
                    await ChangeProfilePicture(
                        new ChangePfpCommand 
                        { 
                            EmployeeId = empInDb.Id,
                            Picture = cmd.ProfilePicture 
                        });
                }
                try
                {
                    var resetPwdCommand = cmd as ResetPasswordCommand;
                    var resetPwdResult = await _accountService.ResetPassword(resetPwdCommand);
                    if(resetPwdResult.IsSuccessful == false)
                    {
                        return resetPwdResult;
                    }
                    empInDb.GitHubURL = cmd.GitHubURL;
                    empInDb.LinkedInURL = cmd.LinkedInURL;
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

        public async Task<CommandResult> ChangeProfilePicture(ChangePfpCommand cmd)
        {
            Employee? empInDb = await _db.Employees.FirstOrDefaultAsync(e => e.Id == cmd.EmployeeId);
            if(empInDb == null)
            {
                return CommandResult.GetNotFoundResult("Employee", cmd.EmployeeId);
            }
            var oldPfpUrl = empInDb.ProfilePictureURL;
            string newPfpUrl;
            if(string.IsNullOrWhiteSpace(oldPfpUrl) == false)
            {
                //upload first before deleting
                var ext = Path.GetExtension(cmd.Picture.FileName);
                string newFileName = Guid.NewGuid().ToString() + ext;
                newPfpUrl = await _fileManager.UploadFile(
                    fileName: newFileName,
                    stream: cmd.Picture.OpenReadStream(),
                    contentType: cmd.Picture.ContentType,
                    folder: Folders.ProfilePictures
                    );
                
                var oldPfpFileName = Path.GetFileName(oldPfpUrl);
                try
                {
                    await _fileManager.DeleteFile(oldPfpFileName, Folders.ProfilePictures);
                }
                catch (Exception ex)
                {
                    //fail gracefully if delete doesn't work (maybe file was already deleted before)
                    //if I don't do this, in the case that a file has already been deleted, then
                    //the user would never be able to finish uploading a new one
                    Debug.WriteLine(ex);
                }
            }
            else
            {
                var ext = Path.GetExtension(cmd.Picture.FileName);
                string newFileName = Guid.NewGuid().ToString() + ext;
                newPfpUrl = await _fileManager.UploadFile(
                    fileName: newFileName,
                    stream: cmd.Picture.OpenReadStream(),
                    contentType: cmd.Picture.ContentType,
                    folder: Folders.ProfilePictures
                    );
            }
            empInDb.ProfilePictureURL = newPfpUrl;
            _db.Employees.Update(empInDb);
            await _db.SaveChangesAsync();
            return CommandResult.SuccessResult;
        }

        public async Task<CommandResult> DeleteProfilePicture(string employeeId)
        {
            Employee? empInDb = await _db.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);
            if (empInDb == null)
            {
                return CommandResult.GetNotFoundResult("Employee", employeeId);
            }
            if(string.IsNullOrWhiteSpace(empInDb.ProfilePictureURL))
            {
                return CommandResult.GetErrorResult("No profile picture to delete.");
            }
            var key = Path.GetFileName(empInDb.ProfilePictureURL);
            await _fileManager.DeleteFile(key, Folders.ProfilePictures);
            empInDb.ProfilePictureURL = null;
            _db.Employees.Update(empInDb);
            await _db.SaveChangesAsync();
            return CommandResult.SuccessResult;
        }

        public async Task<CommandResult> UpdateProfile(string employeeId, UpdateProfileCommand cmd)
        {
            var emp = await _db.Employees.FirstOrDefaultAsync(e => e.Id==employeeId);
            if(emp == null)
            {
                return CommandResult.GetNotFoundResult("Employee", employeeId);
            }

            emp.GitHubURL = cmd.GitHubURL;
            emp.LinkedInURL = cmd.LinkedInURL;
            emp.PersonalWebsite = cmd.PersonalWebsite;

            await _db.SaveChangesAsync();
            return CommandResult.SuccessResult;
        }

        private readonly Expression<Func<Employee, EmployeeDTO>> EmployeeToEmployeeDTOExpression
           = e => new EmployeeDTO
           {
               Id = e.Id,
               Email = e.Email,
               Title = e.Title,
               FirstName = e.FirstName,
               LastName = e.LastName,
               DepartmentId = e.DepartmentId,
               DepartmentName = e.Department.Name,
               IsDirector = e.IsDirector,
               DateHired = e.DateHired,
               DateOfBirth = e.DateOfBirth,
               ProfilePictureURL = e.ProfilePictureURL,
               GitHubURL = e.GitHubURL,
               LinkedInURL = e.LinkedInURL,
               PersonalWebsite = e.PersonalWebsite
           };
    }
}
