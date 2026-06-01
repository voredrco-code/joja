using Microsoft.AspNetCore.Mvc;
using Joja.Api.Data;
using Joja.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Joja.Api.Controllers;

[Microsoft.AspNetCore.Authorization.Authorize]
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
                var seedData = new List<UiTranslation>
                {
                    new UiTranslation { Key = "Home", Language = "en", Value = "Home" },
                    new UiTranslation { Key = "Home", Language = "ar", Value = "الرئيسية" },
                    new UiTranslation { Key = "Products", Language = "en", Value = "Products" },
                    new UiTranslation { Key = "Products", Language = "ar", Value = "المنتجات" },
                    new UiTranslation { Key = "Bundles", Language = "en", Value = "Bundles" },
                    new UiTranslation { Key = "Bundles", Language = "ar", Value = "عروض" },
                    new UiTranslation { Key = "HeroTitle", Language = "en", Value = "Natural Beauty for You" },
                    new UiTranslation { Key = "HeroTitle", Language = "ar", Value = "جمال طبيعي عشانك" },
                    new UiTranslation { Key = "HeroSubtitle", Language = "en", Value = "Discover our natural collection" },
                    new UiTranslation { Key = "HeroSubtitle", Language = "ar", Value = "اكتشفي مجموعتنا الطبيعية" },
                    new UiTranslation { Key = "ShopNow", Language = "en", Value = "Shop Now" },
                    new UiTranslation { Key = "ShopNow", Language = "ar", Value = "تسوقي الآن" },
                    new UiTranslation { Key = "JojaMoments", Language = "en", Value = "Joja Moments" },
                    new UiTranslation { Key = "JojaMoments", Language = "ar", Value = "لحظات جوجا" },
                    new UiTranslation { Key = "BestSellers", Language = "en", Value = "Best Sellers" },
                    new UiTranslation { Key = "BestSellers", Language = "ar", Value = "الأكثر مبيعاً" },
                    new UiTranslation { Key = "FilterAll", Language = "en", Value = "All" },
                    new UiTranslation { Key = "FilterAll", Language = "ar", Value = "الكل" },
                    new UiTranslation { Key = "AddToCart", Language = "en", Value = "Add To Cart" },
                    new UiTranslation { Key = "AddToCart", Language = "ar", Value = "أضف للسلة 🛒" },
                    new UiTranslation { Key = "BuyNow", Language = "en", Value = "Buy Now ⚡" },
                    new UiTranslation { Key = "BuyNow", Language = "ar", Value = "اشتري الآن ⚡" },
                    new UiTranslation { Key = "Checkout", Language = "en", Value = "Checkout" },
                    new UiTranslation { Key = "Checkout", Language = "ar", Value = "إتمام الطلب" },
                    new UiTranslation { Key = "NavHome", Language = "en", Value = "Home" },
                    new UiTranslation { Key = "NavHome", Language = "ar", Value = "الرئيسية" },
                    new UiTranslation { Key = "NavShop", Language = "en", Value = "Shop" },
                    new UiTranslation { Key = "NavShop", Language = "ar", Value = "المتجر" },
                    new UiTranslation { Key = "NavHairCare", Language = "en", Value = "Hair Care" },
                    new UiTranslation { Key = "NavHairCare", Language = "ar", Value = "العناية بالشعر" },
                    new UiTranslation { Key = "NavSkinCare", Language = "en", Value = "Skin Care" },
                    new UiTranslation { Key = "NavSkinCare", Language = "ar", Value = "العناية بالبشرة" },
                    new UiTranslation { Key = "NavAboutUs", Language = "en", Value = "About Us" },
                    new UiTranslation { Key = "NavAboutUs", Language = "ar", Value = "من نحن" },
                    new UiTranslation { Key = "NavContactUs", Language = "en", Value = "Contact Us" },
                    new UiTranslation { Key = "NavContactUs", Language = "ar", Value = "تواصل معنا" },
                    new UiTranslation { Key = "QuickLinks", Language = "en", Value = "Quick Links" },
                    new UiTranslation { Key = "QuickLinks", Language = "ar", Value = "روابط سريعة" },
                    new UiTranslation { Key = "FollowUs", Language = "en", Value = "Follow Us" },
                    new UiTranslation { Key = "FollowUs", Language = "ar", Value = "تابعنا" },
                    new UiTranslation { Key = "AboutJoja", Language = "en", Value = "About JOJA" },
                    new UiTranslation { Key = "AboutJoja", Language = "ar", Value = "عن جوجا" },
                    new UiTranslation { Key = "SelectOption", Language = "en", Value = "Select Option:" },
                    new UiTranslation { Key = "SelectOption", Language = "ar", Value = "اختر:" },
                    new UiTranslation { Key = "IngredientsLbl", Language = "en", Value = "🌿 Ingredients" },
                    new UiTranslation { Key = "IngredientsLbl", Language = "ar", Value = "🌿 المكونات" },
                    new UiTranslation { Key = "UsageInstructionsLbl", Language = "en", Value = "✨ Usage Instructions" },
                    new UiTranslation { Key = "UsageInstructionsLbl", Language = "ar", Value = "✨ طريقة الاستخدام" },
                    new UiTranslation { Key = "CustomerReviews", Language = "en", Value = "Customer Reviews" },
                    new UiTranslation { Key = "CustomerReviews", Language = "ar", Value = "تقييمات العملاء" },
                    new UiTranslation { Key = "ShowReviews", Language = "en", Value = "Show Reviews" },
                    new UiTranslation { Key = "ShowReviews", Language = "ar", Value = "عرض التقييمات" },
                    new UiTranslation { Key = "HideReviews", Language = "en", Value = "Hide Reviews" },
                    new UiTranslation { Key = "HideReviews", Language = "ar", Value = "إخفاء التقييمات" },
                    new UiTranslation { Key = "NoReviews", Language = "en", Value = "No reviews yet. Be the first!" },
                    new UiTranslation { Key = "NoReviews", Language = "ar", Value = "لا توجد تقييمات بعد. كن الأول!" },
                    new UiTranslation { Key = "LeaveReview", Language = "en", Value = "Leave a Review" },
                    new UiTranslation { Key = "LeaveReview", Language = "ar", Value = "أضف تقييمك" },
                    new UiTranslation { Key = "YourName", Language = "en", Value = "Your Name" },
                    new UiTranslation { Key = "YourName", Language = "ar", Value = "اسمك" },
                    new UiTranslation { Key = "RatingLbl", Language = "en", Value = "Rating" },
                    new UiTranslation { Key = "RatingLbl", Language = "ar", Value = "التقييم" },
                    new UiTranslation { Key = "CommentOptional", Language = "en", Value = "Comment (Optional)" },
                    new UiTranslation { Key = "CommentOptional", Language = "ar", Value = "تعليق (اختياري)" },
                    new UiTranslation { Key = "SubmitReview", Language = "en", Value = "Submit Review" },
                    new UiTranslation { Key = "SubmitReview", Language = "ar", Value = "إرسال التقييم" }
                };
                
                _context.UiTranslations.AddRange(seedData);
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
