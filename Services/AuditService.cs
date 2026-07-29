using TicketCRM.Data;
using TicketCRM.Models;
using Microsoft.EntityFrameworkCore; // Add this using directive

namespace TicketCRM.Services;

public class AuditService
{
    private readonly ResolveDbContext _context;

    public AuditService(ResolveDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(string actor, AuditAction action, int? ticketId, string details)
    {
        var entry = new AuditEntry
        {
            Actor = actor,
            Action = action,
            TicketId = ticketId,
            At = DateTime.UtcNow,
            Details = details
        };

        _context.AuditEntries.Add(entry);
        await _context.SaveChangesAsync();
    }

    public IQueryable<AuditEntry> GetAll()
    {
        return _context.AuditEntries.OrderByDescending(e => e.At);
    }
}