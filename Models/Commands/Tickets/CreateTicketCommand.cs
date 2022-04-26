using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Commands.Tickets
{
    public class CreateTicketCommand
    {
        [Required]
        [MaxLength(100)]
        public string Subject { get; set; }
        [Required]
        [MaxLength(1000)]
        public string Message { get; set; }
    }
}
