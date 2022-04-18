using Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Test
{
    interface ITestRepository
    {
        Task<CommandResult> Test(string userId);
    }
}
