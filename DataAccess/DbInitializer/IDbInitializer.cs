using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DbInitializer
{
    public interface IDbInitializer
    {
        Task InitializeRoles();
        Task SeedAdminUser();
        Task Migrate();
    }
}
