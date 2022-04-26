using DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Models.Commands.Tickets;
using Models.DTOs;
using Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Business.Tickets
{
    public class TicketRepository : ITicketRepository
    {
        private readonly ApplicationDbContext _db;

        public TicketRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<TicketDTO> GetById(int id)
        {
            var ticket = await _db
                .Tickets
                .Include(t => t.Creator)
                .ThenInclude(e => e.Department)
                .FirstOrDefaultAsync(t => t.Id == id);

            if(ticket == null)
            {
                return null;
            }
            
            var ticketDTO = TicketToTicketDTOExpression.Compile().Invoke(ticket);

            return ticketDTO;
        }

        public async Task<CommandResult> CreateTicket(string creatorId, CreateTicketCommand cmd)
        {
            var utcnow = DateTime.UtcNow;
            var ticket = new Ticket()
            {
                Subject = cmd.Subject,
                CreatorId = creatorId,
                Messages = new List<TicketMessage>()
                {
                    new TicketMessage
                    {
                        SenderId = creatorId,
                        Content = cmd.Message,
                        IsInternalNote = false,
                        TimestampUtc = utcnow
                    }
                },
                CreatedAt = utcnow,
                IsOpen = true,
            };
            _db.Add(ticket);
            await _db.SaveChangesAsync();
            return CommandResult.SuccessResult;
        }

        public async Task<CommandResult> CloseOrOpenTicket(int ticketId, bool isOpen)
        {
            Ticket? ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null)
            {
                return CommandResult.GetNotFoundResult("Ticket", ticketId);
            }
            ticket.IsOpen = isOpen;
            _db.Tickets.Update(ticket);
            await _db.SaveChangesAsync();
            return CommandResult.SuccessResult;
        }

        public async Task<IEnumerable<TicketDTO>> GetAll(int pageNumber)
        {
            var skip = (pageNumber - 1)*10;
            var tickets = await _db
                .Tickets
                .Include(t => t.Creator)
                .ThenInclude(e => e.Department)
                .OrderByDescending(t => t.CreatedAt)
                .Skip(skip)
                .Take(10)
                .Select(TicketToTicketDTOExpression)
                .ToListAsync();
            return tickets;
        }

        public async Task<IEnumerable<TicketDTO>> GetByCreator(string creatorId)
        {
            var tickets = await _db
                .Tickets
                .Include(t => t.Creator)
                .ThenInclude(s => s.Department)
                .Where(t => t.CreatorId == creatorId)
                .Select(TicketToTicketDTOExpression)
                .ToListAsync();
            return tickets;
        }

        public async Task<IEnumerable<TicketMessageDTO>> GetMessages(int ticketId)
        {
            var msgs = await _db
                .TicketMessages
                .Include(m => m.Sender)
                .ThenInclude(s => s.Department)
                .Where(m => m.TicketId == ticketId)
                .OrderBy(m => m.TimestampUtc)
                .Select(TicketMessageToTicketMessageDTOExpression)
                .ToListAsync();

            return msgs;
        }

        public async Task<CommandResult> CreateTicketMessage(CreateTicketMessageCommand cmd, bool isInternalNote)
        {
            var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == cmd.TicketId);
            if(ticket == null)
            {
                return CommandResult.GetNotFoundResult("Ticket", cmd.TicketId);
            }
            if(ticket.IsOpen == false)
            {
                return CommandResult.GetErrorResult("Can't send a message in a closed ticket, open it first!");
            }
            TicketMessage msg = new TicketMessage
            {
                TicketId = cmd.TicketId,
                Content = cmd.Content,
                SenderId = cmd.SenderId,
                IsInternalNote = isInternalNote,
                TimestampUtc = DateTime.UtcNow
            };
            _db.TicketMessages.Add(msg);
            await _db.SaveChangesAsync();
            return CommandResult.SuccessResult;
        }

        private readonly Expression<Func<Ticket, TicketDTO>> TicketToTicketDTOExpression
           = t => new TicketDTO
           {
               Id = t.Id,
               Subject = t.Subject,
               CreatorId = t.CreatorId,
               CreatorName = t.Creator.FirstName + " " + t.Creator.LastName,
               CreatorDepartmentName = t.Creator.Department.Name,
               CreatedAt = t.CreatedAt,
               IsOpen = t.IsOpen
           };

        private readonly Expression<Func<TicketMessage, TicketMessageDTO>> TicketMessageToTicketMessageDTOExpression
           = m => new TicketMessageDTO
           {
               Id = m.Id,
               TicketId = m.TicketId,
               Content = m.Content,
               IsInternalNote = m.IsInternalNote,
               SenderId = m.SenderId,
               SenderName = m.Sender.FirstName + " " + m.Sender.LastName,
               TimestampUtc = m.TimestampUtc
           };
    }
}
