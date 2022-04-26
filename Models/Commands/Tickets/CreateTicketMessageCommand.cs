using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Commands.Tickets
{
    public class CreateTicketMessageCommand
    {
        [Required]
        public int TicketId { get; set; }
        [Required]
        [MaxLength(1000)]
        public string Content { get; set; }
        public string SenderId { get; set; }
    }
}
