namespace TicketCRM.Models;

public class AuditEntry
{
    public int Id { get; set; }
    public string Actor { get; set; } = string.Empty;
    public AuditAction Action { get; set; }
    public int? TicketId { get; set; }
    public DateTime At { get; set; }
    public string Details { get; set; } = string.Empty;
}