using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Data
{
    public class JobPosting
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string DescriptionHtml { get; set; }
        [Required]
        public string Category { get; set; }
    }
}
