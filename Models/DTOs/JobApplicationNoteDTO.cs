using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs
{
    public class JobApplicationNoteDTO
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string AuthorId { get; set; }
        public string JobApplicationId { get; set; }
    }
}
