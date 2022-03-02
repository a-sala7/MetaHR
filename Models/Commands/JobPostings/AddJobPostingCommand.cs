using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Commands.JobPostings
{
    public class AddJobPostingCommand
    {
        public string Title { get; set; }
        public string DescriptionHtml { get; set; }
        public string Category { get; set; }
    }
}
