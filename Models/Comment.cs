namespace TicketCRM.Models;

public class Comment
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public Ticket? Ticket { get; set; }
    public string Author { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool Internal { get; set; }
    public DateTime CreatedAt { get; set; }
}