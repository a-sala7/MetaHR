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
        
        //can post announcements
        public const string HRSenior = "HR_Senior"; //can set departments
        public const string HRJunior = "HR_Junior"; 

        public const string Employee = "Employee";
        public const string AdminsAndHR = "Admin,HR_Senior,HR_Junior";

        //can post announcements for his department
        public const string DepartmentDirector = "DepartmentDirector";

        public static string[] RolesList = new string[] { Admin, HRSenior, HRJunior, Employee, DepartmentDirector };
    }
}
