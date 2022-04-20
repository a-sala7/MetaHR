using AutoMapper;
using DataAccess.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models.Commands.Departments;
using Models.DTOs;
using Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Constants;
using Microsoft.EntityFrameworkCore.Storage;

namespace Business.Departments
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        public DepartmentRepository(ApplicationDbContext db, IMapper mapper,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _mapper = mapper;
            _userManager = userManager;
        }
        public async Task<IEnumerable<DepartmentDTO>> GetAll()
        {
            List<Department> deps = await _db.Departments.ToListAsync();
            return _mapper.Map<IEnumerable<DepartmentDTO>>(deps);
        }

        public async Task<DepartmentDTO> GetById(int departmentId)
        {
            Department dep = await _db.Departments.FirstOrDefaultAsync(d => d.Id == departmentId);
            return _mapper.Map<DepartmentDTO>(dep);
        }

        public async Task<CommandResult> Create(CreateDepartmentCommand cmd)
        {
            if (await EmployeeIdExists(cmd.DirectorId) == false)
            {
                return CommandResult.GetErrorResult($"Director with ID {cmd.DirectorId} not found.");
            }
            if(await EmployeeManagesAnotherDepartment(cmd.DirectorId, departmentId: 0))
            {
                return CommandResult.GetErrorResult($"This employee is already a director of another department.");
            }

            var department = _mapper.Map<CreateDepartmentCommand, Department>(cmd);
            _db.Departments.Add(department);

            if(await _db.SaveChangesAsync() > 0)
            {
                return CommandResult.SuccessResult;
            }
            return CommandResult.UnknownInternalErrorResult;
        }

        public async Task<CommandResult> Update(int departmentId, UpdateDepartmentCommand cmd)
        {
            Department depInDb = await _db.Departments.FirstOrDefaultAsync(x => x.Id == departmentId);
            
            if (depInDb == null)
            {
                return CommandResult.GetNotFoundResult($"Department with ID {departmentId} not found.");
            }
            using (IDbContextTransaction transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    if (depInDb.DirectorId != cmd.DirectorId)
                    {
                        if (await EmployeeIdExists(cmd.DirectorId) == false)
                        {
                            return CommandResult.GetErrorResult($"Director with ID {cmd.DirectorId} not found.");
                        }
                        if (await EmployeeManagesAnotherDepartment(cmd.DirectorId, departmentId))
                        {
                            return CommandResult.GetErrorResult($"This employee is already a director of another department.");
                        }

                        var oldDirector = await _userManager.FindByIdAsync(depInDb.DirectorId);
                        var newDirector = await _userManager.FindByIdAsync(cmd.DirectorId);
                        await _userManager.RemoveFromRoleAsync(oldDirector, Roles.DepartmentDirector);
                        await _userManager.AddToRoleAsync(newDirector, Roles.DepartmentDirector);
                    }


                    _mapper.Map<UpdateDepartmentCommand, Department>(cmd, depInDb);
                    _db.Departments.Update(depInDb);

                    transaction.Commit();
                    return CommandResult.SuccessResult;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return CommandResult.GetInternalErrorResult(ex.Message);
                }
            }
        }

        private async Task<bool> EmployeeIdExists(string employeeId)
        {
            var emp = await _db.Employees.FirstOrDefaultAsync(e => e.Id == employeeId);
            if (emp == null)
            {
                return false;
            }
            return true;
        }

        private async Task<bool> EmployeeManagesAnotherDepartment(string employeeId, int departmentId)
        {
            Department dep = await _db.Departments.FirstOrDefaultAsync(d => d.DirectorId == employeeId);
            if(dep != null && dep.Id != departmentId)
            {
                return true;
            }
            return false;
        }

        public async Task<CommandResult> Delete(int id)
        {
            Department dep = await _db.Departments
                .Include(x => x.Employees)
                .FirstOrDefaultAsync(x => x.Id == id);
            if(dep == null)
            {
                return CommandResult.GetNotFoundResult($"Department with id {id} not found.");
            }

            bool departmentHasEmployees = dep.Employees.Any();
            if (departmentHasEmployees)
            {
                return CommandResult.GetErrorResult("Can't delete department with employees." +
                    " Transfer the employees to another department first.");
            }

            _db.Departments.Remove(dep);
            if(await _db.SaveChangesAsync() > 0)
            {
                return CommandResult.SuccessResult;
            }
            return CommandResult.UnknownInternalErrorResult;
        }
    }
}
