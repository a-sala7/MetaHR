﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Data
{
    public class EmployeeNote : NoteBase
    {
        [Required]
        public string EmployeeWrittenAboutId { get; set; }
        public virtual Employee EmployeeWrittenAbout { get; set; }
    }
}
