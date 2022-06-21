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
    [Index(nameof(CreatedAt))]
    public class VacationRequest
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        [Required]
        public string EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public VacationRequestState State { get; set; }
        public string? ReviewerId { get; set; }
        public virtual Employee? Reviewer { get; set; }
        [MaxLength(200)]
        public string? DenialReason { get; set; }
    }
}
