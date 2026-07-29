using TicketCRM.Models;

namespace TicketCRM.Services;

public class StatusMachine
{
    private static readonly Dictionary<TicketStatus, HashSet<TicketStatus>> AllowedTransitions = new()
    {
        { TicketStatus.New, new() { TicketStatus.Open } },
        { TicketStatus.Open, new() { TicketStatus.InProgress } },
        { TicketStatus.InProgress, new() { TicketStatus.WaitingCustomer, TicketStatus.Resolved } },
        { TicketStatus.WaitingCustomer, new() { TicketStatus.InProgress } },
        { TicketStatus.Resolved, new() { TicketStatus.Closed } },
        { TicketStatus.Closed, new() }
    };

    public static bool CanTransition(TicketStatus from, TicketStatus to)
    {
        return AllowedTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
    }

    public static List<TicketStatus> GetAllowedTransitions(TicketStatus from)
    {
        return AllowedTransitions.TryGetValue(from, out var allowed)
            ? allowed.ToList()
            : new List<TicketStatus>();
    }

    public static TicketStatus ParseStatus(string statusString)
    {
        return statusString.ToLower() switch
        {
            "new" => TicketStatus.New,
            "open" => TicketStatus.Open,
            "in_progress" => TicketStatus.InProgress,
            "waiting_customer" => TicketStatus.WaitingCustomer,
            "resolved" => TicketStatus.Resolved,
            "closed" => TicketStatus.Closed,
            _ => throw new ArgumentException($"Invalid status: {statusString}")
        };
    }

    public static string StatusToString(TicketStatus status)
    {
        return status switch
        {
            TicketStatus.New => "new",
            TicketStatus.Open => "open",
            TicketStatus.InProgress => "in_progress",
            TicketStatus.WaitingCustomer => "waiting_customer",
            TicketStatus.Resolved => "resolved",
            TicketStatus.Closed => "closed",
            _ => throw new ArgumentException($"Invalid status: {status}")
        };
    }
}