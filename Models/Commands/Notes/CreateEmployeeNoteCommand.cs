using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Commands.Notes
{
    public class CreateEmployeeNoteCommand
    {
        [Required]
        public string EmployeeId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Content { get; set; }
    }
}
