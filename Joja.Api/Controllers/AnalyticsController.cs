using Microsoft.AspNetCore.Mvc;
using Joja.Api.Data;
using Joja.Api.Models;

namespace Joja.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AnalyticsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AnalyticsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("LogEvent")]
    public async Task<IActionResult> LogEvent([FromBody] AnalyticsLog log)
    {
        try 
        {
            if (log == null) return BadRequest();
            
            log.Timestamp = DateTime.Now;
            _context.AnalyticsLogs.Add(log);
            await _context.SaveChangesAsync();
            
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Analytics Error: {ex.Message}");
            return StatusCode(500);
        }
    }
}
