using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketCRM.Services;

namespace TicketCRM.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly AuditService _auditService;

    public AuditController(AuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuditLog()
    {
        var entries = await _auditService.GetAll()
            .Select(e => new
            {
                e.Id,
                e.Actor,
                action = e.Action.ToString(),
                e.TicketId,
                at = e.At,
                e.Details
            }).ToListAsync();

        return Ok(entries);
    }
}