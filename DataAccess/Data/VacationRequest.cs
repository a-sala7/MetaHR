using Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Data
{
    [Index(nameof(CreatedAtUtc))]
    public class VacationRequest
    {
        public int Id { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        [Required]
        public string EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int NumberOfDays { get; set; }
        public VacationRequestState State { get; set; }
        public string? ReviewerId { get; set; }
        public virtual Employee? Reviewer { get; set; }
        [MaxLength(200)]
        public string? DenialReason { get; set; }
    }
}
