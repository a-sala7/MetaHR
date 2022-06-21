using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTOs
{
    public class TicketMessageDTO
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string SenderPfpUrl { get; set; }
        public string Content { get; set; }
        public bool IsInternalNote { get; set; }
        public DateTime TimestampUtc { get; set; }
    }
}
