using Business.Tickets;
using Common;
using Common.Constants;
using MetaHR_API.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models.Commands.Tickets;
using Models.DTOs;
using Models.Responses;

namespace MetaHR_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketRepository _ticketRepository;

        public TicketsController(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        [HttpGet("{ticketId}")]
        [Authorize(Roles = Roles.Employee + "," + Roles.HRJunior + "," + Roles.HRSenior)]
        public async Task<IActionResult> GetTicket(int ticketId)
        {
            var ticket = await _ticketRepository.GetById(ticketId);
            if(ticket == null)
            {
                return NotFound();
            }
            if (User.IsInRole(Roles.Employee))
            {
                if(ticket.CreatorId != User.GetId())
                {
                    return Unauthorized();
                }
            }
            return Ok(ticket);
        }

        [HttpGet("myTickets")]
        [Authorize(Roles = Roles.Employee)]
        public async Task<IActionResult> GetMyTickets()
        {
            var tickets = await _ticketRepository.GetByCreator(User.GetId());
            return Ok(tickets);
        }

        [HttpGet]
        [Authorize(Roles = Roles.HRSenior + "," + Roles.HRJunior)]
        public async Task<IActionResult> GetAll(int pageNumber, int pageSize = 10)
        {
            PagedResult<TicketDTO> tickets = await _ticketRepository
                .GetAll(pageNumber: pageNumber, pageSize: pageSize);

            return Ok(tickets);
        }

        [HttpGet("awaitingResponse")]
        [Authorize(Roles = Roles.HRSenior + "," + Roles.HRJunior)]
        public async Task<IActionResult> GetTicketsAwaitingResponse()
        {
            var tickets = await _ticketRepository.GetTicketsAwaitingResponse();
            return Ok(tickets);
        }

        [HttpPost("createTicket")]
        [Authorize(Roles = Roles.Employee)]
        public async Task<IActionResult> CreateTicket(CreateTicketCommand cmd)
        {
            var result = await _ticketRepository.CreateTicket(User.GetId(), cmd);
            return CommandResultResolver.Resolve(result);
        }

        [HttpPost("createMessage")]
        [Authorize(Roles = Roles.Employee + "," + Roles.HRJunior + "," + Roles.HRSenior)]
        public async Task<IActionResult> CreateTicketMessage(int ticketId, string content, bool isInternalNote = false)
        {
            var ticket = await _ticketRepository.GetById(ticketId);
            if (ticket == null)
            {
                var res1 = CommandResult.GetNotFoundResult("Ticket", ticketId);
                return CommandResultResolver.Resolve(res1);
            }

            if(content.Length > 1000)
            {
                return BadRequest(CommandResult.GetErrorResult("Content is too long. Max 1000 characters"));
            }

            if (User.IsInRole(Roles.Employee))
            {
                if(isInternalNote)
                    return BadRequest();

                if (ticket.CreatorId != User.GetId())
                    return Unauthorized();
            }
            var cmd = new CreateTicketMessageCommand
            {
                TicketId = ticketId,
                Content = content,
                SenderId = User.GetId(),
            };
            var res2 = await _ticketRepository.CreateTicketMessage(cmd, isInternalNote);
            return CommandResultResolver.Resolve(res2);
        }

        [HttpGet("{ticketId}/messages")]
        [Authorize(Roles = Roles.Employee + "," + Roles.HRJunior + "," + Roles.HRSenior)]
        public async Task<IActionResult> GetTicketMessages(int ticketId)
        {
            var t = await _ticketRepository.GetById(ticketId);
            if(t == null)
            {
                return NotFound();
            }
            List<TicketMessageDTO> messages = (await _ticketRepository.GetMessages(ticketId)).ToList();
            if (User.IsInRole(Roles.Employee))
            {
                if(User.GetId() != t.CreatorId)
                {
                    return Unauthorized();
                }
                messages.RemoveAll(m => m.IsInternalNote);
                t.LastMessageAtUtc = messages.OrderByDescending(m => m.TimestampUtc).First().TimestampUtc;
            }
            return Ok(messages);
        }

        [HttpPost("closeOrOpenTicket")]
        [Authorize(Roles = Roles.Employee + "," + Roles.HRSenior + "," + Roles.HRJunior)]
        public async Task<IActionResult> CloseOrOpenTicket(int ticketId, bool isOpen)
        {
            TicketDTO? ticket = await _ticketRepository.GetById(ticketId);
            
            if(ticket == null)
            {
                var res = CommandResult.GetNotFoundResult("Ticket", ticketId);
                return CommandResultResolver.Resolve(res);
            }

            if (User.IsInRole(Roles.Employee))
            {
                if(ticket.CreatorId != User.GetId())
                {
                    return Unauthorized(CommandResult.GetErrorResult("You are not authorized to modify this ticket."));
                }
            }

            var result = await _ticketRepository.CloseOrOpenTicket(ticketId, isOpen);
            return CommandResultResolver.Resolve(result);
        }
    }
}
