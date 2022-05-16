using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Constants
{
    public static class Roles
    {
        public const string Admin = "Admin";
        
        //can post global/department-specific announcements
        public const string HRSenior = "HR_Senior"; //can create departments
        public const string HRJunior = "HR_Junior";

        public const string Employee = "Employee";
        public const string AdminsAndHR = "Admin,HR_Senior,HR_Junior";
        public const string AdminsAndSeniors = "Admin,HR_Senior";

        //can post department-specific announcements
        public const string DepartmentDirector = "DepartmentDirector";

        public const string AttendanceLogger = "AttendanceLogger";

        public static string[] RolesList = new string[] { Admin, HRSenior, HRJunior, Employee, DepartmentDirector, AttendanceLogger };
    }
}
