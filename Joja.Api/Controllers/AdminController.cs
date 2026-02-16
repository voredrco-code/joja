using Microsoft.AspNetCore.Mvc;
using Joja.Api.Data;
using Joja.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Joja.Api.Controllers;

public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Seed Translations - One-time action
    [HttpPost]
    public async Task<IActionResult> SeedTranslations()
    {
        // Check if already seeded
        var existing = await _context.UiTranslations.AnyAsync();
        if (existing)
        {
            return Json(new { success = false, message = "Translations already exist!" });
        }

        // Add UI Translations
        _context.UiTranslations.AddRange(
            new UiTranslation { Key = "HeroTitle", Language = "ar", Value = "جمال طبيعي من أجلك" },
            new UiTranslation { Key = "HeroSubtitle", Language = "ar", Value = "اكتشفي مجموعتنا العضوية" },
            new UiTranslation { Key = "ShopNow", Language = "ar", Value = "تسوقي الآن" },
            new UiTranslation { Key = "JojaMoments", Language = "ar", Value = "لحظات جوجا" },
            new UiTranslation { Key = "BestSellers", Language = "ar", Value = "الأكثر مبيعاً" },
            new UiTranslation { Key = "FilterAll", Language = "ar", Value = "الكل" },
            new UiTranslation { Key = "AddToCart", Language = "ar", Value = "أضف للسلة" },
            
            new UiTranslation { Key = "HeroTitle", Language = "en", Value = "Natural Beauty for You" },
            new UiTranslation { Key = "HeroSubtitle", Language = "en", Value = "Discover our organic collection" },
            new UiTranslation { Key = "ShopNow", Language = "en", Value = "Shop Now" },
            new UiTranslation { Key = "JojaMoments", Language = "en", Value = "Joja Moments" },
            new UiTranslation { Key = "BestSellers", Language = "en", Value = "Best Sellers" },
            new UiTranslation { Key = "FilterAll", Language = "en", Value = "All" },
            new UiTranslation { Key = "AddToCart", Language = "en", Value = "Add to Cart" }
        );

        // Add Category Translations
        _context.CategoryTranslations.AddRange(
            new CategoryTranslation { CategoryId = 1, Language = "en", Name = "Skin Care" },
            new CategoryTranslation { CategoryId = 2, Language = "en", Name = "Hair Care" },
            new CategoryTranslation { CategoryId = 3, Language = "en", Name = "Natural Oils" }
        );

        // Add Product Translations
        _context.ProductTranslations.AddRange(
            new ProductTranslation { ProductId = 1, Language = "en", Name = "Organic Jojoba Oil", Description = "Pure 100% jojoba oil for skin and hair" },
            new ProductTranslation { ProductId = 2, Language = "en", Name = "Natural Shea Butter", Description = "Natural shea butter for deep moisturizing" },
            new ProductTranslation { ProductId = 3, Language = "en", Name = "Moroccan Argan Oil", Description = "Authentic argan oil from Morocco for hair" },
            new ProductTranslation { ProductId = 4, Language = "en", Name = "Aloe Vera Cream", Description = "Natural aloe cream for skin moisturizing" }
        );

        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Translations seeded successfully!" });
    }

    // Manage Translations
    public async Task<IActionResult> ManageTranslations()
    {
        var translations = await _context.UiTranslations
            .OrderBy(t => t.Key)
            .ThenBy(t => t.Language)
            .ToListAsync();
            
        return View(translations);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateTranslation(int id, string value)
    {
        var translation = await _context.UiTranslations.FindAsync(id);
        if (translation == null) return Json(new { success = false, message = "Not found" });

        translation.Value = value;
        await _context.SaveChangesAsync();
        
        return Json(new { success = true });
    }

    // Dashboard Home
    public async Task<IActionResult> Index()
    {
        ViewBag.TotalProducts = await _context.Products.CountAsync();
        ViewBag.TotalCategories = await _context.Categories.CountAsync();
        ViewBag.HasTranslations = await _context.UiTranslations.AnyAsync();
        
        return View();
    }

    // Analytics Dashboard
    public async Task<IActionResult> Analytics(DateTime? startDate, DateTime? endDate)
    {
        // Default: Last 30 Days
        var end = endDate ?? DateTime.Now;
        var start = startDate ?? DateTime.Now.AddDays(-30);
        
        ViewBag.StartDate = start.ToString("yyyy-MM-dd");
        ViewBag.EndDate = end.ToString("yyyy-MM-dd");

        // Current Period Data
        var currentPeriodLogs = _context.AnalyticsLogs
            .Where(l => l.Timestamp >= start && l.Timestamp <= end);

        var visitCount = await currentPeriodLogs.CountAsync(l => l.EventType == "Visit");
        var cartCount = await currentPeriodLogs.CountAsync(l => l.EventType == "AddToCart");

        // Previous Period Data (for Growth Rate)
        var periodDuration = end - start;
        var prevEnd = start;
        var prevStart = start - periodDuration;

        var prevPeriodLogs = _context.AnalyticsLogs
            .Where(l => l.Timestamp >= prevStart && l.Timestamp < prevEnd);
            
        var prevVisitCount = await prevPeriodLogs.CountAsync(l => l.EventType == "Visit");
        var prevCartCount = await prevPeriodLogs.CountAsync(l => l.EventType == "AddToCart");

        // Calculate Growth Rates
        double visitGrowth = 0;
        if (prevVisitCount > 0) visitGrowth = ((double)(visitCount - prevVisitCount) / prevVisitCount) * 100;
        else if (visitCount > 0) visitGrowth = 100; // 0 to something is 100% growth (effectively infinite but capped for UI)

        double cartGrowth = 0;
        if (prevCartCount > 0) cartGrowth = ((double)(cartCount - prevCartCount) / prevCartCount) * 100;
        else if (cartCount > 0) cartGrowth = 100;

        ViewBag.TotalVisits = visitCount;
        ViewBag.TotalAddToCart = cartCount;
        ViewBag.VisitGrowth = visitGrowth;
        ViewBag.CartGrowth = cartGrowth;

        // Group by Country (Current Period)
        var countryStats = await currentPeriodLogs
            .Where(l => l.Country != "Unknown")
            .GroupBy(l => l.Country)
            .Select(g => new { Country = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();
            
        ViewBag.TopCountries = countryStats;

        var logs = await currentPeriodLogs.OrderByDescending(l => l.Timestamp).Take(100).ToListAsync();
        return View(logs);
    }
}
