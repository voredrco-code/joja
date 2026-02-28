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
        try
        {
            // 1. إضافة ترجمات الواجهة (UI) بس لأنها مش مربوطة بمنتجات
            if (!await _context.UiTranslations.AnyAsync())
            {
                _context.UiTranslations.AddRange(
                    new UiTranslation { Key = "Home", Language = "en", Value = "Home" },
                    new UiTranslation { Key = "Home", Language = "ar", Value = "الرئيسية" },
                    new UiTranslation { Key = "Products", Language = "en", Value = "Products" },
                    new UiTranslation { Key = "Products", Language = "ar", Value = "المنتجات" },
                    new UiTranslation { Key = "Bundles", Language = "en", Value = "Bundles" },
                    new UiTranslation { Key = "Bundles", Language = "ar", Value = "عروض" }
                    // تقدر تزود أي كلمات تانية بتستخدمها في الموقع هنا
                );
                await _context.SaveChangesAsync();
            }

            // ملاحظة: تم إيقاف الترجمات التجريبية للمنتجات والأقسام عشان متعملش مشكلة Foreign Key
            // لو عايز تترجم منتج، الأفضل تعملها من لوحة التحكم في صفحة "تعديل المنتج"

            return Json(new { success = true, message = "تم إضافة ترجمات الواجهة بنجاح!" });
        }
        catch (Exception ex)
        {
            // السطر ده هيخلي المتصفح يفهم الخطأ ومايطلعش رسالة Unexpected token M
            return Json(new { success = false, message = "حدث خطأ في الداتابيز: " + (ex.InnerException?.Message ?? ex.Message) });
        }
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
        ViewBag.HeroBanner = await _context.Banners
            .OrderBy(b => b.DisplayOrder).ThenBy(b => b.Id)
            .FirstOrDefaultAsync();
        ViewBag.TotalBanners = await _context.Banners.CountAsync();
        
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
