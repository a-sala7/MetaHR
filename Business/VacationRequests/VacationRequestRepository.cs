using Common;
using DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Models.Commands.VacationRequests;
using Models.DTOs;
using Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Business.VacationRequests
{
    public class VacationRequestRepository : IVacationRequestRepository
    {
        private readonly ApplicationDbContext _db;

        public VacationRequestRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<VacationRequestDTO>> GetAll(int pageNumber, int pageSize = 10)
        {
            int skip = (pageNumber - 1) * pageSize;
            int count = await _db.VacationRequests.CountAsync();
            List<VacationRequestDTO>? reqDtos = await _db
                .VacationRequests
                .OrderByDescending(vr => vr.CreatedAtUtc)
                .Paginate(pageNumber: pageNumber, pageSize: pageSize)
                .Include(vr => vr.Employee)
                .ThenInclude(emp => emp.Department)
                .Select(VRtoVRDTOExpression)
                .ToListAsync();

            return new PagedResult<VacationRequestDTO>(reqDtos, count);
        }

        public async Task<PagedResult<VacationRequestDTO>> GetByEmployee(string employeeId, int pageNumber, int pageSize = 10)
        {
            int skip = (pageNumber - 1) * pageSize;
            int count = await _db.VacationRequests.CountAsync();
            List<VacationRequestDTO>? reqDtos = await _db
                .VacationRequests
                .Where(vr => vr.EmployeeId == employeeId)
                .OrderByDescending(vr => vr.CreatedAtUtc)
                .Paginate(pageNumber: pageNumber, pageSize: pageSize)
                .Include(vr => vr.Employee)
                .ThenInclude(emp => emp.Department)
                .Select(VRtoVRDTOExpression)
                .ToListAsync();

            return new PagedResult<VacationRequestDTO>(reqDtos, count);
        }

        public async Task<CommandResult> Create(string employeeId, CreateVacationRequestCommand cmd)
        {
            if (cmd.NumberOfDays < 1 || cmd.NumberOfDays > 14)
            {
                return CommandResult.GetErrorResult("NumberOfDays must be between 1 and 14");
            }
            Employee? emp = await _db.Employees.FirstAsync(e => e.Id == employeeId);
            //if employee hired less than 3 months ago
            //don't allow him to request more than 1 day in a row
            if (emp.DateHired.Date > DateTime.Now.Date.AddMonths(-3))
            {
                if (cmd.NumberOfDays > 1)
                {
                    return CommandResult.GetErrorResult("Employees hired less than 3 months ago can't request more than 1 consecutive vacation day.");
                }
                DateTime reqPlus1 = cmd.From.Date.AddDays(1);
                DateTime reqMinus1 = cmd.From.Date.AddDays(-1);
                bool consecutiveToAnExistingRequest = await _db.VacationRequests.Where(vr => vr.EmployeeId == employeeId)
                    .AnyAsync(vr => vr.FromDate == reqPlus1 || vr.FromDate == reqMinus1);

                if (consecutiveToAnExistingRequest)
                {
                    return CommandResult.GetErrorResult("Employees hired less than 3 months ago can't request more than 1 consecutive vacation day.");
                }
            }

            var to = cmd.From.Date.AddDays(cmd.NumberOfDays - 1);
            if (await _db.VacationRequests.Where(vr => vr.EmployeeId == employeeId)
                .AnyAsync(vr => vr.FromDate.Date == cmd.From.Date && vr.ToDate.Date == to))
            {
                return CommandResult.GetErrorResult("You've already made an identical request.");
            }

            if (
                //current from date is in range of a previous request
                await _db.VacationRequests.Where(vr => vr.EmployeeId == employeeId && vr.State == VacationRequestState.Approved)
                .AnyAsync(vr => vr.FromDate.Date <= cmd.From.Date && vr.ToDate.Date >= cmd.From.Date)
                ||
                //current to date is in range of a previous request
                await _db.VacationRequests.Where(vr => vr.EmployeeId == employeeId && vr.State == VacationRequestState.Approved)
                .AnyAsync(vr => vr.FromDate.Date <= to && vr.ToDate.Date >= to)
                )
            {
                return CommandResult.GetErrorResult("You've already made a similar request that was approved.");
            }

            var vacationRequest = new VacationRequest
            {
                EmployeeId = employeeId,
                CreatedAtUtc = DateTime.UtcNow,
                State = VacationRequestState.Pending,
                NumberOfDays = cmd.NumberOfDays,
                FromDate = cmd.From.Date,
                ToDate = to
            };
            _db.VacationRequests.Add(vacationRequest);
            await _db.SaveChangesAsync();
            return CommandResult.SuccessResult;
        }

        public async Task<CommandResult> Update(int id, string reviewerId, UpdateVacationRequestCommand cmd)
        {
            var req = await _db.VacationRequests.FirstOrDefaultAsync(vr => vr.Id == id);
            if(req == null)
            {
                return CommandResult.GetNotFoundResult("VacationRequest", id);
            }
            if(cmd.State == VacationRequestState.Pending)
            {
                return CommandResult.GetErrorResult("Can't manually set VacationRequestState to Pending");
            }
            if(req.EmployeeId == reviewerId)
            {
                return CommandResult.GetErrorResult("Nice try :) but you can't approve/deny your own vacation request.");
            }
            req.ReviewerId = reviewerId;
            req.State = cmd.State;
            if(req.State == VacationRequestState.Denied){
                req.DenialReason = cmd.DenialReason;
            }
            await _db.SaveChangesAsync();
            return CommandResult.SuccessResult;
        }

        public async Task<CommandResult> Delete(int id)
        {
            var req = await _db.VacationRequests.FirstOrDefaultAsync(vr => vr.Id == id);
            if (req == null)
            {
                return CommandResult.GetNotFoundResult("VacationRequest", id);
            }
            
            _db.VacationRequests.Remove(req);
            await _db.SaveChangesAsync();

            return CommandResult.SuccessResult;
        }

        private readonly Expression<Func<VacationRequest, VacationRequestDTO>> VRtoVRDTOExpression
           = vr => new VacationRequestDTO
           {
               Id = vr.Id,
               CreatedAt = vr.CreatedAtUtc,
               EmployeeId = vr.EmployeeId,
               EmployeeEmail = vr.Employee.Email,
               EmployeeFirstName = vr.Employee.FirstName,
               EmployeeLastName = vr.Employee.LastName,
               From = vr.FromDate,
               To = vr.ToDate,
               State = vr.State.ToString(),
               DepartmentName = vr.Employee.Department.Name,
               ReviewerId = vr.Reviewer.Id,
               ReviewerFirstName = vr.Reviewer.FirstName,
               ReviewerLastName = vr.Reviewer.LastName,
               DenialReason = vr.DenialReason
           };
    }
}
