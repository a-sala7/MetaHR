using Common.Constants;
using DataAccess.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;

        public DbInitializer(RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _db = db;
        }

        public async Task InitializeRoles()
        {
            await _db.Database.MigrateAsync();
            foreach (var role in Roles.RolesList)
            {
                if(await _roleManager.RoleExistsAsync(role) == false)
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        public async Task Migrate()
        {
            await _db.Database.MigrateAsync();
        }

        public async Task SeedAdminUser()
        {
            var adminInDb = await _userManager.FindByEmailAsync("admin@test.com");
            if (adminInDb != null)
            {
                if(await _userManager.IsInRoleAsync(adminInDb, Roles.Admin) == true)
                {
                    return;
                }
                throw new Exception("admin@test.com user exists and is not in admin role!");
            }

            var admin = new ApplicationUser
            {
                UserName = "admin@test.com",
                Email = "admin@test.com",
                FirstName = "admin",
                LastName = ".",
                EmailConfirmed = true,
                DateRegisteredUtc = DateTime.UtcNow
            };
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    await _userManager.CreateAsync(admin, "admin123");
                    await _userManager.AddToRoleAsync(admin, Roles.Admin);
                    transaction.Commit();
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
