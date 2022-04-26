﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Data
{
    public class Ticket
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Subject { get; set; }
        [Required]
        public string CreatorId { get; set; }
        public DateTime CreatedAt { get; set; }
        public virtual Employee Creator { get; set; }
        public bool IsOpen { get; set; } = true;
        public virtual ICollection<TicketMessage> Messages { get; set; }
    }
}
