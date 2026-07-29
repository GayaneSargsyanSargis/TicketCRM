namespace TicketCRM.DTOs;

public class CreateCommentDto
{
    public string Author { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool Internal { get; set; }
}