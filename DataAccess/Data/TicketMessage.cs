using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Data
{
    [Index(nameof(TimestampUtc))]
    public class TicketMessage
    {
        public int Id { get; set; }
        
        [Required]
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }
        public string SenderId { get; set; }
        public Employee Sender { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Content { get; set; }
        public bool IsInternalNote { get; set; }
        public DateTime TimestampUtc { get; set; }

    }
}
