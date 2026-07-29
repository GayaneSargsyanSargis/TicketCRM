using Microsoft.EntityFrameworkCore;
using TicketCRM.Data;
using TicketCRM.DTOs;
using TicketCRM.Models;
using System.Text.RegularExpressions;

namespace TicketCRM.Services;

public class TicketService
{
    private readonly ResolveDbContext _context;
    private readonly AuditService _auditService;

    public TicketService(ResolveDbContext context, AuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public (bool isValid, string? errorMessage, string? errorField) ValidateCreateTicket(CreateTicketDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Subject))
            return (false, "Subject cannot be empty", "subject");

        if (string.IsNullOrWhiteSpace(dto.Description))
            return (false, "Description cannot be empty", "description");

        if (string.IsNullOrWhiteSpace(dto.CustomerEmail) || !IsValidEmail(dto.CustomerEmail))
            return (false, "Invalid email format", "customerEmail");

        if (!IsValidPriority(dto.Priority))
            return (false, "Priority must be one of: low, normal, high, urgent", "priority");

        return (true, null, null);
    }

    public async Task<(bool success, Ticket? ticket, string? error)> CreateTicketAsync(CreateTicketDto dto, string actor)
    {
        var (isValid, errorMessage, _) = ValidateCreateTicket(dto);
        if (!isValid)
            return (false, null, errorMessage);

        var ticket = new Ticket
        {
            Subject = dto.Subject.Trim(),
            Description = dto.Description.Trim(),
            CustomerEmail = dto.CustomerEmail.Trim(),
            Priority = ParsePriority(dto.Priority),
            Status = TicketStatus.New,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(actor, AuditAction.Created, ticket.Id, $"Ticket created: {ticket.Subject}");

        return (true, ticket, null);
    }

    public async Task<Ticket?> GetTicketAsync(int id)
    {
        return await _context.Tickets
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<Ticket>> ListTicketsAsync(string? status = null, string? priority = null)
    {
        var query = _context.Tickets
            .Include(t => t.Comments)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            try
            {
                var parsedStatus = StatusMachine.ParseStatus(status);
                query = query.Where(t => t.Status == parsedStatus);
            }
            catch
            {
                // Invalid status, ignore filter
            }
        }

        if (!string.IsNullOrEmpty(priority))
        {
            if (Enum.TryParse<TicketPriority>(priority, ignoreCase: true, out var parsedPriority))
            {
                query = query.Where(t => t.Priority == parsedPriority);
            }
        }

        return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
    }

    public async Task<(bool success, string? error, List<string>? allowedStatuses)> ChangeStatusAsync(
        int ticketId, string newStatusString, string actor)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null)
            return (false, "Ticket not found", null);

        try
        {
            var newStatus = StatusMachine.ParseStatus(newStatusString);

            if (!StatusMachine.CanTransition(ticket.Status, newStatus))
            {
                var allowed = StatusMachine.GetAllowedTransitions(ticket.Status)
                    .Select(StatusMachine.StatusToString)
                    .ToList();
                return (false, $"Cannot transition from {StatusMachine.StatusToString(ticket.Status)} to {newStatusString}", allowed);
            }

            var oldStatus = ticket.Status;
            ticket.Status = newStatus;

            if (newStatus == TicketStatus.Resolved)
                ticket.ResolvedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(actor, AuditAction.StatusChanged, ticketId,
                $"Status changed from {StatusMachine.StatusToString(oldStatus)} to {newStatusString}");

            return (true, null, null);
        }
        catch (ArgumentException)
        {
            return (false, "Invalid status", null);
        }
    }

    public async Task<(bool success, Comment? comment, string? error)> AddCommentAsync(
        int ticketId, CreateCommentDto dto, string actor)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null)
            return (false, null, "Ticket not found");

        if (string.IsNullOrWhiteSpace(dto.Author))
            return (false, null, "Author cannot be empty");

        if (string.IsNullOrWhiteSpace(dto.Body))
            return (false, null, "Body cannot be empty");

        var comment = new Comment
        {
            TicketId = ticketId,
            Author = dto.Author.Trim(),
            Body = dto.Body.Trim(),
            Internal = dto.Internal,
            CreatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(actor, AuditAction.CommentAdded, ticketId,
            $"Comment added by {comment.Author}" + (comment.Internal ? " (internal)" : ""));

        return (true, comment, null);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidPriority(string priority)
    {
        return priority.ToLower() is "low" or "normal" or "high" or "urgent";
    }

    private static TicketPriority ParsePriority(string priority)
    {
        return priority.ToLower() switch
        {
            "low" => TicketPriority.Low,
            "normal" => TicketPriority.Normal,
            "high" => TicketPriority.High,
            "urgent" => TicketPriority.Urgent,
            _ => TicketPriority.Normal
        };
    }
}