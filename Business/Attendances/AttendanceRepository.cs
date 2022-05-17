using AutoMapper;
using Common;
using DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Models.Commands.Attendances;
using Models.DTOs;
using Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Attendances
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public AttendanceRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<PagedResult<AttendanceDTO>> GetByEmployeeId(string employeeId,
            int pageNumber, int pageSize = 10)
        {
            List<Attendance>? attendances = await _db
                .Attendances
                .Where(a => a.EmployeeId == employeeId)
                .Paginate(pageNumber: pageNumber, pageSize: pageSize)
                .ToListAsync();

            int totalCount = await _db
                .Attendances
                .Where(a => a.EmployeeId == employeeId)
                .CountAsync();

            IEnumerable<AttendanceDTO>? dtos = _mapper
                .Map<IEnumerable<Attendance>,
                IEnumerable<AttendanceDTO>>
                (attendances);

            return dtos.GetPagedResult(totalCount);
        }

        public async Task<CommandResult> Create(CreateAttendanceCommand cmd)
        {
            if(cmd.Date > DateTime.Now)
            {
                return CommandResult.GetErrorResult("Can't log attendance for future date!");
            }

            bool employeeExists = await _db.Employees
                .AnyAsync(e => e.Id == cmd.EmployeeId);
            if(employeeExists == false)
            {
                return CommandResult.GetNotFoundResult("Employee", cmd.EmployeeId);
            }

            bool employeeAlreadyLoggedToday = await _db
                .Attendances
                .AnyAsync(a => a.EmployeeId == cmd.EmployeeId &&
                a.Date.Date == cmd.Date.Date);

            if (employeeAlreadyLoggedToday)
            {
                return CommandResult.GetErrorResult("This employee's attendance has already been logged today.");
            }

            Attendance? att = _mapper.Map<CreateAttendanceCommand, Attendance>(cmd);

            _db.Attendances.Add(att);

            await _db.SaveChangesAsync();

            return CommandResult.SuccessResult;
        }

        public async Task<CommandResult> Delete(int id)
        {
            Attendance? attInDb = await _db.Attendances.FirstOrDefaultAsync(a => a.Id == id);

            if(attInDb == null)
            {
                return CommandResult.GetNotFoundResult("Attendance", id);
            }

            _db.Attendances.Remove(attInDb);

            await _db.SaveChangesAsync();
            return CommandResult.SuccessResult;
        }
    }
}
