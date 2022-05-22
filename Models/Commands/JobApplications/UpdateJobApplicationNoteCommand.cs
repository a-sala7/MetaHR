using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Commands.JobApplications
{
    public class UpdateJobApplicationNoteCommand
    {
        [Required]
        public int NoteId { get; set; }
        [Required]
        [MaxLength(1000)]
        public string Content { get; set; }
    }
}
