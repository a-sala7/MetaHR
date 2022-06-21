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
    public class NoteBase
    {
        public int Id { get; set; }

        [Required]
        public string AuthorId { get; set; }
        public virtual Employee Author { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Content { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
