using Microsoft.EntityFrameworkCore;
using TicketCRM.Data;
using TicketCRM.Models;

namespace TicketCRM.Services;

public class StatisticsService
{
    private readonly ResolveDbContext _context;

    public StatisticsService(ResolveDbContext context)
    {
        _context = context;
    }

    public async Task<StatsDto> GetStatsAsync()
    {
        var tickets = await _context.Tickets.ToListAsync();

        var stats = new StatsDto
        {
            StatusCounts = new Dictionary<string, int>(),
            PriorityCounts = new Dictionary<string, int>()
        };

        // Count by status
        foreach (var status in Enum.GetValues(typeof(TicketStatus)).Cast<TicketStatus>())
        {
            var count = tickets.Count(t => t.Status == status);
            stats.StatusCounts[StatusMachine.StatusToString(status)] = count;
        }

        // Count by priority
        foreach (var priority in Enum.GetValues(typeof(TicketPriority)).Cast<TicketPriority>())
        {
            var count = tickets.Count(t => t.Priority == priority);
            stats.PriorityCounts[priority.ToString().ToLower()] = count;
        }

        // Calculate average resolution time
        var resolvedTickets = tickets
            .Where(t => t.Status == TicketStatus.Resolved && t.ResolvedAt.HasValue)
            .ToList();

        if (resolvedTickets.Any())
        {
            var totalTime = resolvedTickets.Sum(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalSeconds);
            stats.AverageResolutionTimeSeconds = totalTime / resolvedTickets.Count;
        }
        else
        {
            stats.AverageResolutionTimeSeconds = 0;
        }

        return stats;
    }
}

public class StatsDto
{
    public Dictionary<string, int> StatusCounts { get; set; } = new();
    public Dictionary<string, int> PriorityCounts { get; set; } = new();
    public double AverageResolutionTimeSeconds { get; set; }
}