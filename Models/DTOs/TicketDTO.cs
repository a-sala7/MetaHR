﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs
{
    public class TicketDTO
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatorId { get; set; }
        public string CreatorName { get; set; }
        public string CreatorDepartmentName { get; set; }
        public bool IsOpen { get; set; }
    }
}
