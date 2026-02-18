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
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Group by Date
        var dailyVisits = await _context.AnalyticsLogs
            .GroupBy(l => l.Timestamp.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderByDescending(d => d.Date)
            .Take(30)
            .ToListAsync();

        // Top Products
        var topProducts = await _context.AnalyticsLogs
            .Where(l => !string.IsNullOrEmpty(l.Product))
            .GroupBy(l => l.Product)
            .Select(g => new { Product = g.Key, Count = g.Count() })
            .OrderByDescending(p => p.Count)
            .Take(10)
            .ToListAsync();
            
        // Top Cities
        var topCities = await _context.AnalyticsLogs
            .Where(l => !string.IsNullOrEmpty(l.City) && l.City != "Unknown")
            .GroupBy(l => l.City)
            .Select(g => new { City = g.Key, Count = g.Count() })
            .OrderByDescending(c => c.Count)
            .Take(10)
            .ToListAsync();

        ViewBag.DailyVisits = dailyVisits;
        ViewBag.TopProducts = topProducts;
        ViewBag.TopCities = topCities;
        
        return View(await _context.AnalyticsLogs.OrderByDescending(l => l.Timestamp).Take(100).ToListAsync());
    }
}
