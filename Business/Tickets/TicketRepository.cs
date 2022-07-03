using Common;
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
                .Include(t => t.Messages)
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
                CreatedAtUtc = utcnow,
                IsOpen = true,
                IsAwaitingResponse = true,
                LastMessageAtUtc = utcnow
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
            if(ticket.IsOpen == isOpen)
            {
                return CommandResult.SuccessResult;
            }
            ticket.IsOpen = isOpen;
            if(isOpen == false)
            {
                ticket.IsAwaitingResponse = false;
            }
            else
            {
                var lastMsg = _db.TicketMessages
                    .Where(t => t.TicketId == ticketId)
                    .OrderByDescending(t => t.TimestampUtc)
                    .FirstOrDefault();
                ticket.IsAwaitingResponse = (ticket.CreatorId == lastMsg.SenderId);
            }
            _db.Tickets.Update(ticket);
            await _db.SaveChangesAsync();
            return CommandResult.SuccessResult;
        }

        public async Task<PagedResult<TicketDTO>> GetAll(int pageNumber, int pageSize = 10)
        {
            await _db.SaveChangesAsync();

            var tickets = await _db
                .Tickets
                .Include(t => t.Messages)
                .Include(t => t.Creator)
                .ThenInclude(e => e.Department)
                .OrderByDescending(t => t.LastMessageAtUtc)
                .Paginate(pageNumber: pageNumber, pageSize: pageSize)
                .Select(TicketToTicketDTOExpression)
                .ToListAsync();

            var totalCount = await _db.
                Tickets.CountAsync();

            return tickets.GetPagedResult(totalCount);
        }

        public async Task<IEnumerable<TicketDTO>> GetByCreator(string creatorId)
        {
            var tickets = await _db
                .Tickets
                .Include(t => t.Messages)
                .Include(t => t.Creator)
                .ThenInclude(s => s.Department)
                .OrderByDescending(t => t.LastMessageAtUtc)
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
            if(ticket.CreatorId == cmd.SenderId)
            {
                ticket.IsAwaitingResponse = true;
            }
            else
            {
                if(msg.IsInternalNote == false)
                {
                    ticket.IsAwaitingResponse = false;
                }
            }
            if (msg.IsInternalNote == false)
            {
                ticket.LastMessageAtUtc = msg.TimestampUtc;
            }
            _db.Tickets.Update(ticket);
            _db.TicketMessages.Add(msg);
            await _db.SaveChangesAsync();
            return CommandResult.SuccessResult;
        }

        public async Task<IEnumerable<TicketDTO>> GetTicketsAwaitingResponse()
        {
            var tickets = await _db
                .Tickets
                .Include(t => t.Messages)
                .Include(t => t.Creator)
                .ThenInclude(e => e.Department)
                .Where(t => t.IsAwaitingResponse)
                .OrderByDescending(t => t.CreatedAtUtc)
                .Select(TicketToTicketDTOExpression)
                .ToListAsync();

            return tickets;
        }

        private readonly Expression<Func<Ticket, TicketDTO>> TicketToTicketDTOExpression
           = t => new TicketDTO
           {
               Id = t.Id,
               Subject = t.Subject,
               LastMessage = t.Messages.OrderByDescending(tm => tm.TimestampUtc).Select(tm => tm.Content).First(),
               LastMessageAtUtc = t.LastMessageAtUtc,
               CreatorId = t.CreatorId,
               CreatorName = t.Creator.FirstName + " " + t.Creator.LastName,
               CreatorDepartmentName = t.Creator.Department.Name,
               CreatorPfpURL = t.Creator.ProfilePictureURL,
               CreatedAt = t.CreatedAtUtc,
               IsOpen = t.IsOpen,
               IsAwaitingResponse = t.IsAwaitingResponse
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
               SenderPfpUrl = m.Sender.ProfilePictureURL,
               TimestampUtc = m.TimestampUtc
           };
    }
}
