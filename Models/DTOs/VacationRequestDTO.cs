using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs
{
    public class VacationRequestDTO
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeFirstName { get; set; }
        public string EmployeeLastName { get; set; }
        public string EmployeeEmail { get; set; }
        public string DepartmentName { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string State { get; set; }
        public string? ReviewerId { get; set; }
        public string? ReviewerFirstName { get; set; }
        public string? ReviewerLastName { get; set; }
        public string? DenialReason { get; set; }
    }
}
