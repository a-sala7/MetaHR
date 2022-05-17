using Common;
using Models.Commands.Tickets;
using Models.DTOs;
using Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Tickets
{
    public interface ITicketRepository
    {
        Task<TicketDTO> GetById(int ticketId);
        Task<CommandResult> CreateTicket(string creatorId, CreateTicketCommand cmd);
        Task<CommandResult> CloseOrOpenTicket(int ticketId, bool isOpen);
        Task<PagedResult<TicketDTO>> GetAll(int pageNumber, int pageSize = 10);
        Task<IEnumerable<TicketDTO>> GetByCreator(string creatorId);
        Task<IEnumerable<TicketDTO>> GetTicketsAwaitingResponse();
        Task<IEnumerable<TicketMessageDTO>> GetMessages(int ticketId);
        Task<CommandResult> CreateTicketMessage(CreateTicketMessageCommand cmd, bool isInternalNote);
    }
}
