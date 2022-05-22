using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Commands.JobApplications
{
    public class CreateJobApplicationNoteCommand
    {
        [Required]
        public int JobApplicationId { get; set; }
        [Required]
        [MaxLength(1000)]
        public string Content { get; set; }
    }
}
