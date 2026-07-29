namespace TicketCRM.Models;

public enum TicketStatus
{
    New,
    Open,
    InProgress,
    WaitingCustomer,
    Resolved,
    Closed
}

public enum TicketPriority
{
    Low,
    Normal,
    High,
    Urgent
}

public enum AuditAction
{
    Created,
    StatusChanged,
    CommentAdded
}