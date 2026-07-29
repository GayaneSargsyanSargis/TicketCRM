using Microsoft.AspNetCore.Mvc;
using TicketCRM.Services;

namespace TicketCRM.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatsController : ControllerBase
{
    private readonly StatisticsService _statsService;

    public StatsController(StatisticsService statsService)
    {
        _statsService = statsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _statsService.GetStatsAsync();
        return Ok(stats);
    }
}