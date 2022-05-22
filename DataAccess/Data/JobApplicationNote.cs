using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Data
{
    public class JobApplicationNote : NoteBase
    {
        [Required]
        public int JobApplicationId { get; set; }
        public virtual JobApplication JobApplication { get; set; }
    }
}
