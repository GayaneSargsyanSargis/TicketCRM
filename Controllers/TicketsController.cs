using Microsoft.AspNetCore.Mvc;
using TicketCRM.DTOs;
using TicketCRM.Services;

namespace TicketCRM.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly TicketService _ticketService;

    public TicketsController(TicketService ticketService)
    {
        _ticketService = ticketService;
    }

    private string GetActor()
    {
        return Request.Headers.TryGetValue("X-Actor", out var actor) ? actor.ToString() : "api";
    }

    [HttpPost]
    public async Task<IActionResult> CreateTicket([FromBody] CreateTicketDto dto)
    {
        var (isValid, errorMessage, errorField) = _ticketService.ValidateCreateTicket(dto);
        if (!isValid)
        {
            return BadRequest(new { error = errorMessage, field = errorField });
        }

        var (success, ticket, error) = await _ticketService.CreateTicketAsync(dto, GetActor());
        if (!success)
            return BadRequest(new { error });

        return CreatedAtAction(nameof(GetTicket), new { id = ticket!.Id }, MapToResponseDto(ticket));
    }

    [HttpGet]
    public async Task<IActionResult> ListTickets([FromQuery] string? status, [FromQuery] string? priority)
    {
        var tickets = await _ticketService.ListTicketsAsync(status, priority);
        return Ok(tickets.Select(MapToResponseDto).ToList());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTicket(int id)
    {
        var ticket = await _ticketService.GetTicketAsync(id);
        if (ticket == null)
            return NotFound(new { error = "Ticket not found" });

        return Ok(MapToResponseDto(ticket));
    }

    [HttpPost("{id}/status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeStatusDto dto)
    {
        var (success, error, allowedStatuses) = await _ticketService.ChangeStatusAsync(id, dto.To, GetActor());
        if (!success)
        {
            if (allowedStatuses != null)
                return BadRequest(new { error, allowedStatuses });
            return BadRequest(new { error });
        }

        var ticket = await _ticketService.GetTicketAsync(id);
        return Ok(MapToResponseDto(ticket!));
    }

    [HttpPost("{id}/comments")]
    public async Task<IActionResult> AddComment(int id, [FromBody] CreateCommentDto dto)
    {
        var (success, comment, error) = await _ticketService.AddCommentAsync(id, dto, GetActor());
        if (!success)
            return BadRequest(new { error });

        return CreatedAtAction(nameof(GetTicket), new { id }, new { commentId = comment!.Id });
    }

    private static TicketResponseDto MapToResponseDto(Models.Ticket ticket)
    {
        return new TicketResponseDto
        {
            Id = ticket.Id,
            Subject = ticket.Subject,
            Description = ticket.Description,
            CustomerEmail = ticket.CustomerEmail,
            Priority = ticket.Priority.ToString().ToLower(),
            Status = Services.StatusMachine.StatusToString(ticket.Status),
            CreatedAt = ticket.CreatedAt,
            ResolvedAt = ticket.ResolvedAt,
            Comments = ticket.Comments
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    Author = c.Author,
                    Body = c.Body,
                    Internal = c.Internal,
                    CreatedAt = c.CreatedAt
                })
                .ToList()
        };
    }
}