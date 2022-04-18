using Common.Constants;
using DataAccess.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Test
{
    public class TestRepository : ITestRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public TestRepository(ApplicationDbContext db,
            UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<CommandResult> Test(string userId)
        {
            using (IDbContextTransaction transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    await _userManager.RemoveFromRoleAsync(user, Roles.HRJunior);
                    await _userManager.RemoveFromRoleAsync(user, Roles.HRSenior);
                    await _userManager.RemoveFromRoleAsync(user, Roles.Admin);
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
    }
}
