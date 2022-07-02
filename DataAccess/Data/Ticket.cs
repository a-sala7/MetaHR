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
    public class Ticket
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Subject { get; set; }
        [Required]
        public string CreatorId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public virtual Employee Creator { get; set; }
        public bool IsOpen { get; set; } = true;
        public bool IsAwaitingResponse { get; set; } = true;
        public virtual ICollection<TicketMessage> Messages { get; set; }
        public DateTime LastMessageAtUtc { get; set; }
    }
}
