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
using System.Linq.Expressions;

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
            var depDtos = _mapper.Map<IEnumerable<DepartmentDTO>>(deps);

            var directors = await _db.Employees.Where(e => e.IsDirector).ToListAsync();
            foreach(var dir in directors)
            {
                var dep = depDtos.FirstOrDefault(dep => dep.Id == dir.DepartmentId);
                dep.DirectorId = dir.Id;
                dep.DirectorName = $"{dir.FirstName} {dir.LastName}";
            }
            return depDtos;
        }

        public async Task<DepartmentDTO> GetById(int departmentId)
        {
            Department dep = await _db.Departments.FirstOrDefaultAsync(d => d.Id == departmentId);
            var depDto = _mapper.Map<DepartmentDTO>(dep);

            var directors = await _db.Employees.Where(e => e.IsDirector).ToListAsync();
            var dir = directors.FirstOrDefault(e => e.DepartmentId == departmentId);
            depDto.DirectorId = dir.Id;
            depDto.DirectorName = $"{dir.FirstName} {dir.LastName}";

            return depDto;
        }

        public async Task<CommandResult> Create(CreateDepartmentCommand cmd)
        {
            bool dpExistsWithSameName = await _db.Departments.AnyAsync(d => d.Name == cmd.Name);
            if (dpExistsWithSameName)
            {
                return CommandResult.GetErrorResult("A department with that name already exists!");
            }


            var department = _mapper.Map<CreateDepartmentCommand, Department>(cmd);
            _db.Departments.Add(department);

            if (await _db.SaveChangesAsync() > 0)
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
                return CommandResult.GetNotFoundResult("Department", departmentId);
            }

            _mapper.Map<UpdateDepartmentCommand, Department>(cmd, depInDb);
            _db.Departments.Update(depInDb);
            if (await _db.SaveChangesAsync() > 0)
            {
                return CommandResult.SuccessResult;
            }
            return CommandResult.UnknownInternalErrorResult;
        }

        public async Task<CommandResult> Delete(int id)
        {
            Department dep = await _db.Departments
                .Include(x => x.Employees)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (dep == null)
            {
                return CommandResult.GetNotFoundResult("Department", id);
            }

            bool departmentHasEmployees = dep.Employees.Any();
            if (departmentHasEmployees)
            {
                return CommandResult.GetErrorResult("Can't delete department with employees." +
                    " Transfer the employees to another department first.");
            }

            _db.Departments.Remove(dep);
            if (await _db.SaveChangesAsync() > 0)
            {
                return CommandResult.SuccessResult;
            }
            return CommandResult.UnknownInternalErrorResult;
        }

        public async Task<CommandResult> AssignDirector(AssignDirectorCommand cmd)
        {
            Department dep = await _db.Departments
                .Include(x => x.Employees)
                .FirstOrDefaultAsync(x => x.Id == cmd.DepartmentId);
            if (dep == null)
            {
                return CommandResult.GetNotFoundResult("Department", cmd.DepartmentId);
            }
            Employee newDir = await _db.Employees
                .FirstOrDefaultAsync(e => e.Id == cmd.DirectorId);
            if (newDir == null)
            {
                return CommandResult.GetNotFoundResult("Employee", cmd.DirectorId);
            }

            if (newDir.DepartmentId != cmd.DepartmentId)
            {
                return CommandResult.GetErrorResult($"This employee is not part of that department!");
            }

            var directors = await _userManager.GetUsersInRoleAsync(Roles.DepartmentDirector);
            var directorIds = directors.Select(x => x.Id).ToList();

            //given user is already a director
            if (directors.Contains(newDir) && newDir.IsDirector)
            {
                return CommandResult.SuccessResult;
            }

            //given user is not a director
            //he is now director of given department, if a director exists for this department, he replaces him
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var existingDirector = await _db
                    .Employees
                    .FirstOrDefaultAsync
                    (e => directorIds.Contains(e.Id) && e.DepartmentId == cmd.DepartmentId);
                    if (existingDirector != null)
                    {
                        await _userManager.RemoveFromRoleAsync(existingDirector, Roles.DepartmentDirector);
                        existingDirector.IsDirector = false;
                        _db.Employees.Update(existingDirector);
                    }
                    await _userManager.AddToRoleAsync(newDir, Roles.DepartmentDirector);
                    newDir.IsDirector = true;
                    _db.Employees.Update(newDir);
                    await _db.SaveChangesAsync();
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
    }
}
