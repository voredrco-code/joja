using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // دي كانت ناقصة وهتعمل Error
using Joja.Api.Data;
using Joja.Api.Models;

namespace Joja.Api.Controllers
{
    // شيلنا [Route] من هنا عشان الصفحة تفتح عادي على /Analytics
    public class AnalyticsController : Controller // غيرنا دي من ControllerBase لـ Controller عشان الـ View تشتغل
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // API Endpoint: ده الرابط اللي الكود هيبعت عليه الداتا
        // الرابط هيكون: /api/Analytics/LogEvent
        [HttpPost]
        [Route("api/Analytics/LogEvent")] 
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

        // Dashboard Page: دي صفحة الإحصائيات
        // الرابط هيكون: /Analytics
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // 1. الزيارات اليومية
            var dailyVisits = await _context.AnalyticsLogs
                .GroupBy(l => l.Timestamp.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderByDescending(d => d.Date)
                .Take(30)
                .ToListAsync();

            // 2. أكثر المنتجات زيارة
            var topProducts = await _context.AnalyticsLogs
                .Where(l => !string.IsNullOrEmpty(l.Product))
                .GroupBy(l => l.Product)
                .Select(g => new { Product = g.Key, Count = g.Count() })
                .OrderByDescending(p => p.Count)
                .Take(10)
                .ToListAsync();
                
            // 3. أكثر المدن
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
            
            // آخر 100 عملية
            return View(await _context.AnalyticsLogs.OrderByDescending(l => l.Timestamp).Take(100).ToListAsync());
        }
    }
}