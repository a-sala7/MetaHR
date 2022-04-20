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

            _mapper.Map<UpdateDepartmentCommand, Department>(cmd, depInDb);
            _db.Departments.Update(depInDb);
            if(await _db.SaveChangesAsync() > 0)
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
